using Microsoft.EntityFrameworkCore;
using HospitalAssetTracker.Data;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Services;

namespace HospitalAssetTracker.Services
{
    public class RequestService : IRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly IAssetService _assetService;
        private readonly IInventoryService _inventoryService;
        private readonly IProcurementService _procurementService;

        public RequestService(
            ApplicationDbContext context,
            IAuditService auditService,
            IAssetService assetService,
            IInventoryService inventoryService,
            IProcurementService procurementService)
        {
            _context = context;
            _auditService = auditService;
            _assetService = assetService;
            _inventoryService = inventoryService;
            _procurementService = procurementService;
        }

        public async Task<PagedResult<ITRequest>> GetRequestsAsync(RequestSearchModel searchModel)
        {
            var query = _context.ITRequests
                .Include(r => r.RequestedByUser)
                .Include(r => r.RelatedAsset)
                .Include(r => r.AssignedToUser)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchModel.SearchTerm))
            {
                query = query.Where(r => r.Title.Contains(searchModel.SearchTerm) ||
                                       r.Description.Contains(searchModel.SearchTerm) ||
                                       r.RequestNumber.Contains(searchModel.SearchTerm));
            }

            if (searchModel.RequestType.HasValue)
            {
                query = query.Where(r => r.RequestType == searchModel.RequestType.Value);
            }

            if (searchModel.Status.HasValue)
            {
                query = query.Where(r => r.Status == searchModel.Status.Value);
            }

            if (searchModel.Priority.HasValue)
            {
                query = query.Where(r => r.Priority == searchModel.Priority.Value);
            }

            if (!string.IsNullOrEmpty(searchModel.Department))
            {
                query = query.Where(r => r.Department == searchModel.Department);
            }

            if (searchModel.DateFrom.HasValue)
            {
                query = query.Where(r => r.CreatedDate >= searchModel.DateFrom.Value);
            }

            if (searchModel.DateTo.HasValue)
            {
                query = query.Where(r => r.CreatedDate <= searchModel.DateTo.Value);
            }

            // Apply sorting
            query = searchModel.SortBy?.ToLower() switch
            {
                "title" => searchModel.SortDesc ? query.OrderByDescending(r => r.Title) : query.OrderBy(r => r.Title),
                "priority" => searchModel.SortDesc ? query.OrderByDescending(r => r.Priority) : query.OrderBy(r => r.Priority),
                "status" => searchModel.SortDesc ? query.OrderByDescending(r => r.Status) : query.OrderBy(r => r.Status),
                "department" => searchModel.SortDesc ? query.OrderByDescending(r => r.Department) : query.OrderBy(r => r.Department),
                "createdate" => searchModel.SortDesc ? query.OrderByDescending(r => r.CreatedDate) : query.OrderBy(r => r.CreatedDate),
                _ => query.OrderByDescending(r => r.CreatedDate)
            };

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((searchModel.Page - 1) * searchModel.PageSize)
                .Take(searchModel.PageSize)
                .ToListAsync();

            return new PagedResult<ITRequest>
            {
                Items = items,
                TotalCount = totalItems,
                PageNumber = searchModel.Page,
                PageSize = searchModel.PageSize
            };
        }

        public async Task<ITRequest?> GetRequestByIdAsync(int requestId)
        {
            return await _context.ITRequests
                .Include(r => r.RequestedByUser)
                .Include(r => r.RelatedAsset)
                .Include(r => r.AssignedToUser)
                .FirstOrDefaultAsync(r => r.Id == requestId);
        }

        public async Task<ITRequest> CreateRequestAsync(ITRequest request, string userId)
        {
            // Generate request number
            var datePrefix = DateTime.Now.ToString("yyyyMM");
            var lastRequest = await _context.ITRequests
                .Where(r => r.RequestNumber.StartsWith($"REQ-{datePrefix}"))
                .OrderByDescending(r => r.RequestNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastRequest != null)
            {
                var lastSequence = lastRequest.RequestNumber.Split('-').Last();
                if (int.TryParse(lastSequence, out int lastNum))
                {
                    sequence = lastNum + 1;
                }
            }

            request.RequestNumber = $"REQ-{datePrefix}-{sequence:D4}";
            request.RequestedByUserId = userId;
            request.RequestDate = DateTime.UtcNow;
            request.CreatedDate = DateTime.UtcNow;
            request.Status = RequestStatus.Submitted;

            // Ensure all DateTime fields are UTC
            if (request.RequiredByDate.HasValue && request.RequiredByDate.Value.Kind != DateTimeKind.Utc)
            {
                request.RequiredByDate = DateTime.SpecifyKind(request.RequiredByDate.Value, DateTimeKind.Utc);
            }
            
            if (request.DueDate.HasValue && request.DueDate.Value.Kind != DateTimeKind.Utc)
            {
                request.DueDate = DateTime.SpecifyKind(request.DueDate.Value, DateTimeKind.Utc);
            }
            
            if (request.ApprovalDate.HasValue && request.ApprovalDate.Value.Kind != DateTimeKind.Utc)
            {
                request.ApprovalDate = DateTime.SpecifyKind(request.ApprovalDate.Value, DateTimeKind.Utc);
            }
            
            if (request.CompletedDate.HasValue && request.CompletedDate.Value.Kind != DateTimeKind.Utc)
            {
                request.CompletedDate = DateTime.SpecifyKind(request.CompletedDate.Value, DateTimeKind.Utc);
            }
            
            if (request.LastModifiedDate.HasValue && request.LastModifiedDate.Value.Kind != DateTimeKind.Utc)
            {
                request.LastModifiedDate = DateTime.SpecifyKind(request.LastModifiedDate.Value, DateTimeKind.Utc);
            }

            // Set automatic priority based on request type and asset criticality
            await SetAutomaticPriorityAsync(request);

            _context.ITRequests.Add(request);
            await _context.SaveChangesAsync();

            // Check if auto-approval is applicable
            if (await IsEligibleForAutoApprovalAsync(request))
            {
                await ProcessAutoApprovalAsync(request, userId);
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Create, "ITRequest", request.Id, userId, 
                $"IT request {request.RequestNumber} created");

            return request;
        }

        public async Task<ITRequest> UpdateRequestAsync(ITRequest request, string userId)
        {
            var existingRequest = await GetRequestByIdAsync(request.Id);
            if (existingRequest == null)
                throw new InvalidOperationException("Request not found");

            // Update fields
            existingRequest.Title = request.Title;
            existingRequest.Description = request.Description;
            existingRequest.Priority = request.Priority;
            existingRequest.RequiredByDate = request.RequiredByDate;
            existingRequest.BusinessJustification = request.BusinessJustification;
            existingRequest.LastUpdatedDate = DateTime.UtcNow;
            existingRequest.LastUpdatedByUserId = userId;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Update, "ITRequest", request.Id, userId, 
                $"IT request {existingRequest.RequestNumber} updated");

            return existingRequest;
        }

        public async Task<bool> AssignRequestAsync(int requestId, string assignedToUserId, string currentUserId)
        {
            var request = await GetRequestByIdAsync(requestId);
            if (request == null) return false;

            request.AssignedToUserId = assignedToUserId;
            request.Status = RequestStatus.InProgress;
            request.LastUpdatedDate = DateTime.UtcNow;
            request.LastUpdatedByUserId = currentUserId;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Update, "ITRequest", requestId, currentUserId, 
                $"Request {request.RequestNumber} assigned");

            return true;
        }

        public async Task<bool> ApproveRequestAsync(int requestId, string approverId, string? comments = null)
        {
            var request = await GetRequestByIdAsync(requestId);
            if (request == null) return false;

            // Create approval record
            var approval = new RequestApproval
            {
                ITRequestId = requestId,
                ApproverId = approverId,
                DecisionDate = DateTime.UtcNow,
                Status = ApprovalStatus.Approved,
                Comments = comments,
                CreatedDate = DateTime.UtcNow,
                ApprovalLevel = ApprovalLevel.Supervisor,
                Sequence = 1
            };
            _context.RequestApprovals.Add(approval);

            // Update request status based on approval workflow
            if (await IsFullyApprovedAsync(requestId))
            {
                request.Status = RequestStatus.Approved;
                
                // Trigger next phase based on request type
                await ProcessApprovedRequestAsync(request, approverId);
            }

            request.LastUpdatedDate = DateTime.UtcNow;
            request.LastUpdatedByUserId = approverId;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Update, "ITRequest", requestId, approverId, 
                $"Request {request.RequestNumber} approved");

            return true;
        }

        public async Task<bool> RejectRequestAsync(int requestId, string rejectionReason)
        {
            var request = await GetRequestByIdAsync(requestId);
            if (request == null) return false;

            // Create rejection record
            var rejection = new RequestApproval
            {
                ITRequestId = requestId,
                ApproverId = "system", // System rejection
                DecisionDate = DateTime.UtcNow,
                Status = ApprovalStatus.Rejected,
                Comments = rejectionReason,
                CreatedDate = DateTime.UtcNow,
                ApprovalLevel = ApprovalLevel.Supervisor,
                Sequence = 1
            };
            _context.RequestApprovals.Add(rejection);

            // Update request status
            request.Status = RequestStatus.Rejected;
            request.LastUpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Update, "ITRequest", requestId, "system", 
                $"Request {request.RequestNumber} rejected: {rejectionReason}");

            return true;
        }

        public async Task<bool> CompleteRequestAsync(int requestId, string completedById, string? completionNotes = null)
        {
            var request = await GetRequestByIdAsync(requestId);
            if (request == null) return false;

            request.Status = RequestStatus.Completed;
            request.CompletedDate = DateTime.UtcNow;
            request.CompletionNotes = completionNotes;
            request.CompletedByUserId = completedById;
            request.LastUpdatedDate = DateTime.UtcNow;
            request.LastUpdatedByUserId = completedById;

            // Update related asset status if applicable
            if (request.AssetId.HasValue)
            {
                await UpdateAssetStatusAfterCompletion(request, completedById);
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Update, "ITRequest", requestId, completedById, 
                $"Request {request.RequestNumber} completed");

            return true;
        }

        public async Task<RequestDashboardData> GetRequestDashboardDataAsync()
        {
            var totalRequests = await _context.ITRequests.CountAsync();
            var openRequests = await _context.ITRequests.CountAsync(r => 
                r.Status == RequestStatus.Submitted || 
                r.Status == RequestStatus.InProgress || 
                r.Status == RequestStatus.PendingApproval);
            var completedToday = await _context.ITRequests.CountAsync(r => 
                r.CompletedDate.HasValue && r.CompletedDate.Value.Date == DateTime.UtcNow.Date);
            var overdue = await _context.ITRequests.CountAsync(r => 
                r.RequiredByDate.HasValue && r.RequiredByDate.Value < DateTime.UtcNow && 
                r.Status != RequestStatus.Completed && r.Status != RequestStatus.Cancelled);

            // Get request by type counts
            var requestsByType = await _context.ITRequests
                .GroupBy(r => r.RequestType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();

            // Get request by priority counts
            var requestsByPriority = await _context.ITRequests
                .GroupBy(r => r.Priority)
                .Select(g => new { Priority = g.Key, Count = g.Count() })
                .ToListAsync();

            // Get recent requests
            var recentRequestsData = await _context.ITRequests
                .Include(r => r.RequestedByUser)
                .Include(r => r.AssignedToUser)
                .OrderByDescending(r => r.RequestDate)
                .Take(10)
                .ToListAsync();

            var recentRequests = recentRequestsData.Select(r => new RequestSummaryViewModel
            {
                Id = r.Id,
                RequestNumber = r.RequestNumber,
                Title = r.Title,
                RequestType = r.RequestType.ToString(),
                Priority = r.Priority,
                Status = r.Status,
                RequestDate = r.RequestDate,
                RequesterName = r.RequestedByUser?.FirstName + " " + r.RequestedByUser?.LastName,
                AssignedToName = r.AssignedToUser?.FirstName + " " + r.AssignedToUser?.LastName
            }).ToList();

            return new RequestDashboardData
            {
                TotalRequests = totalRequests,
                OpenRequests = openRequests,
                CompletedToday = completedToday,
                OverdueRequests = overdue,
                RequestsByType = requestsByType.ToDictionary(x => x.Type.ToString(), x => x.Count),
                RequestsByPriority = requestsByPriority.ToDictionary(x => x.Priority.ToString(), x => x.Count),
                RecentRequests = recentRequests
            };
        }

        public async Task<List<ITRequest>> GetOverdueRequestsAsync()
        {
            return await _context.ITRequests
                .Include(r => r.RequestedByUser)
                .Include(r => r.AssignedToUser)
                .Where(r => r.RequiredByDate.HasValue && 
                           r.RequiredByDate.Value < DateTime.UtcNow && 
                           r.Status != RequestStatus.Completed && 
                           r.Status != RequestStatus.Cancelled)
                .OrderBy(r => r.RequiredByDate)
                .ToListAsync();
        }

        public async Task<List<ITRequest>> GetMyRequestsAsync(string userId)
        {
            return await _context.ITRequests
                .Include(r => r.RelatedAsset)
                .Include(r => r.AssignedToUser)
                .Where(r => r.RequestedByUserId == userId)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<ITRequest>> GetAssignedRequestsAsync(string userId)
        {
            return await _context.ITRequests
                .Include(r => r.RequestedByUser)
                .Include(r => r.RelatedAsset)
                .Where(r => r.AssignedToUserId == userId && 
                           r.Status != RequestStatus.Completed && 
                           r.Status != RequestStatus.Cancelled)
                .OrderBy(r => r.Priority)
                .ThenBy(r => r.RequiredByDate)
                .ToListAsync();
        }

        // Private methods for business logic

        private async Task SetAutomaticPriorityAsync(ITRequest request)
        {
            // Set priority based on request type and asset criticality
            if (request.RelatedAssetId.HasValue)
            {
                var asset = await _assetService.GetAssetByIdAsync(request.RelatedAssetId.Value);
                if (asset != null && asset.IsCritical)
                {
                    request.Priority = RequestPriority.Critical;
                    return;
                }
            }

            // Default priorities by request type
            request.Priority = request.RequestType switch
            {
                RequestType.HardwareReplacement => RequestPriority.High,
                RequestType.NetworkConnectivity => RequestPriority.High,
                RequestType.MaintenanceService => RequestPriority.Medium,
                RequestType.NewEquipment => RequestPriority.Medium,
                RequestType.SoftwareInstallation => RequestPriority.Low,
                RequestType.UserAccessRights => RequestPriority.Low,
                RequestType.ITConsultation => RequestPriority.Low,
                RequestType.Training => RequestPriority.Low,
                _ => RequestPriority.Medium
            };
        }

        private async Task<bool> IsEligibleForAutoApprovalAsync(ITRequest request)
        {
            // Auto-approve standard consumables under certain value
            if (request.RequestType == RequestType.SoftwareInstallation && 
                request.EstimatedCost <= 500)
                return true;

            // Auto-approve like-for-like replacements for critical systems
            if (request.RequestType == RequestType.HardwareReplacement && 
                request.RelatedAssetId.HasValue)
            {
                var asset = await _assetService.GetAssetByIdAsync(request.RelatedAssetId.Value);
                return asset?.IsCritical == true;
            }

            return false;
        }

        private Task ProcessAutoApprovalAsync(ITRequest request, string userId)
        {
            request.Status = RequestStatus.Approved;
            request.ApprovedByUserId = userId;
            request.ApprovalDate = DateTime.UtcNow;
            
            var approval = new RequestApproval
            {
                ITRequestId = request.Id,
                ApproverId = userId,
                DecisionDate = DateTime.UtcNow,
                Status = ApprovalStatus.Approved,
                Comments = "Auto-approved based on system rules",
                CreatedDate = DateTime.UtcNow,
                ApprovalLevel = ApprovalLevel.Supervisor,
                Sequence = 1
            };
            _context.RequestApprovals.Add(approval);
            
            return Task.CompletedTask;
        }

        private async Task<bool> IsFullyApprovedAsync(int requestId)
        {
            var request = await GetRequestByIdAsync(requestId);
            if (request == null) return false;

            // Check if all required approvals are received based on request value and type
            var approvals = await _context.RequestApprovals
                .Where(a => a.ITRequestId == requestId && a.Status == ApprovalStatus.Approved)
                .CountAsync();

            // Simple approval logic - can be enhanced based on complex business rules
            return approvals >= GetRequiredApprovalCount(request);
        }

        private int GetRequiredApprovalCount(ITRequest request)
        {
            if (request.EstimatedCost <= 500) return 1;
            if (request.EstimatedCost <= 5000) return 2;
            return 3;
        }

        private async Task ProcessApprovedRequestAsync(ITRequest request, string approverId)
        {
            switch (request.RequestType)
            {
                case RequestType.HardwareReplacement:
                    await ProcessHardwareReplacementAsync(request, approverId);
                    break;
                case RequestType.NewEquipment:
                    await ProcessNewEquipmentAsync(request, approverId);
                    break;
                // Add other request type processing logic
            }
        }

        private async Task ProcessHardwareReplacementAsync(ITRequest request, string approverId)
        {
            if (!request.AssetId.HasValue) return;

            // Check inventory for replacement
            var replacementAvailable = await _inventoryService.CheckAvailabilityAsync(
                request.RequestedItemCategory ?? "Unknown", 1);

            if (replacementAvailable)
            {
                // Update asset status to maintenance pending
                await _assetService.UpdateAssetStatusAsync(request.AssetId.Value, 
                    AssetStatus.MaintenancePending, approverId);
            }
            else
            {
                // Trigger procurement
                await _procurementService.CreateProcurementFromRequestAsync(request.Id, approverId);
            }
        }

        private async Task ProcessNewEquipmentAsync(ITRequest request, string approverId)
        {
            // Check inventory first
            var available = await _inventoryService.CheckAvailabilityAsync(
                request.RequestedItemCategory ?? "Unknown", 1);

            if (!available)
            {
                // Trigger procurement
                await _procurementService.CreateProcurementFromRequestAsync(request.Id, approverId);
            }
        }

        private async Task UpdateAssetStatusAfterCompletion(ITRequest request, string completedById)
        {
            if (!request.AssetId.HasValue) return;

            switch (request.RequestType)
            {
                case RequestType.HardwareReplacement:
                    // Asset should be back in service or written off
                    await _assetService.UpdateAssetStatusAsync(request.AssetId.Value, 
                        AssetStatus.InUse, completedById);
                    break;
                case RequestType.MaintenanceService:
                    // Asset maintenance completed
                    await _assetService.UpdateAssetStatusAsync(request.AssetId.Value, 
                        AssetStatus.InUse, completedById);
                    break;
            }
        }
    }
}
