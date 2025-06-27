using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public RequestService(
            ApplicationDbContext context,
            IAuditService auditService,
            IAssetService assetService,
            IInventoryService inventoryService,
            IProcurementService procurementService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _auditService = auditService;
            _assetService = assetService;
            _inventoryService = inventoryService;
            _procurementService = procurementService;
            _userManager = userManager;
        }

        public async Task<PagedResult<ITRequest>> GetRequestsAsync(RequestSearchModel searchModel, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) 
            {
                return new PagedResult<ITRequest>(); // Return empty result if user not found
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var canViewAll = userRoles.Contains("Admin") || userRoles.Contains("IT Support") || userRoles.Contains("Asset Manager");

            var query = _context.ITRequests
                .Include(r => r.RequestedByUser)
                .Include(r => r.RelatedAsset)
                .Include(r => r.AssignedToUser)
                .AsQueryable();

            // If the user is not an admin/support/manager, only show their own requests.
            if (!canViewAll)
            {
                query = query.Where(r => r.RequestedByUserId == userId || r.AssignedToUserId == userId);
            }

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
            request.RequestNumber = await GenerateRequestNumberAsync(); // Use the dedicated method

            request.RequestedByUserId = userId;
            request.RequestDate = DateTime.UtcNow;
            request.LastUpdatedDate = DateTime.UtcNow;
            request.LastUpdatedByUserId = userId;

            if (!string.IsNullOrEmpty(request.AssignedToUserId))
            {
                request.Status = RequestStatus.InProgress;
            }
            else
            {
                request.Status = RequestStatus.Submitted;
            }

            if (request.RequiredInventoryItemId.HasValue && request.RequiredInventoryItemId > 0)
            {
                var inventoryItem = await _context.InventoryItems.FindAsync(request.RequiredInventoryItemId.Value);
                if (inventoryItem == null || inventoryItem.Quantity <= 0)
                {
                    request.RequiredInventoryItemId = null; 
                }
            }
            else
            {
                request.RequiredInventoryItemId = null;
            }

            if (request.RequiredByDate.HasValue && request.RequiredByDate.Value.Kind != DateTimeKind.Utc)
            {
                request.RequiredByDate = DateTime.SpecifyKind(request.RequiredByDate.Value, DateTimeKind.Utc);
            }

            _context.ITRequests.Add(request);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Create, "ITRequest", request.Id, userId,
                $"IT request {request.RequestNumber} created. Status: {request.Status}" +
                (string.IsNullOrEmpty(request.AssignedToUserId) ? "" : $", Assigned to: {request.AssignedToUserId}") +
                (request.RequiredInventoryItemId.HasValue ? $", Linked Inventory ID: {request.RequiredInventoryItemId.Value}" : "")
                );

            return request;
        }

        public async Task<ITRequest> UpdateRequestAsync(ITRequest request, string userId)
        {
            var existingRequest = await _context.ITRequests
                                        .Include(r => r.RequestedByUser) 
                                        .FirstOrDefaultAsync(r => r.Id == request.Id);

            if (existingRequest == null)
                throw new InvalidOperationException("Request not found");

            existingRequest.Title = request.Title;
            existingRequest.Description = request.Description;
            existingRequest.RequestType = request.RequestType;
            existingRequest.Priority = request.Priority;
            existingRequest.RequiredByDate = request.RequiredByDate;
            existingRequest.BusinessJustification = request.BusinessJustification;
            existingRequest.RequestedItemCategory = request.RequestedItemCategory;
            existingRequest.RequestedItemSpecifications = request.RequestedItemSpecifications;
            existingRequest.LocationId = request.LocationId;
            existingRequest.RelatedAssetId = request.RelatedAssetId;

            if (existingRequest.AssignedToUserId != request.AssignedToUserId)
            {
                existingRequest.AssignedToUserId = request.AssignedToUserId;
                if (!string.IsNullOrEmpty(request.AssignedToUserId) && existingRequest.Status == RequestStatus.Submitted)
                {
                    existingRequest.Status = RequestStatus.InProgress;
                }
                else if (string.IsNullOrEmpty(request.AssignedToUserId) && existingRequest.Status == RequestStatus.InProgress)
                {
                     existingRequest.Status = RequestStatus.Submitted;
                }
            }
            
            existingRequest.RequiredInventoryItemId = request.RequiredInventoryItemId;
            if (request.RequiredInventoryItemId.HasValue && request.RequiredInventoryItemId > 0)
            {
                var inventoryItem = await _context.InventoryItems.FindAsync(request.RequiredInventoryItemId.Value);
                if (inventoryItem == null || inventoryItem.Quantity <= 0)
                {
                }
            } else {
                 existingRequest.RequiredInventoryItemId = null;
            }

            existingRequest.DamagedAssetId = request.DamagedAssetId;
            existingRequest.DisposalNotesForUnmanagedAsset = request.DisposalNotesForUnmanagedAsset;

            existingRequest.LastUpdatedDate = DateTime.UtcNow;
            existingRequest.LastUpdatedByUserId = userId;
            
            if (existingRequest.RequiredByDate.HasValue && existingRequest.RequiredByDate.Value.Kind != DateTimeKind.Utc)
            {
                existingRequest.RequiredByDate = DateTime.SpecifyKind(existingRequest.RequiredByDate.Value, DateTimeKind.Utc);
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Update, "ITRequest", request.Id, userId,
                $"IT request {existingRequest.RequestNumber} updated. Status: {existingRequest.Status}" +
                (string.IsNullOrEmpty(existingRequest.AssignedToUserId) ? "" : $", Assigned to: {existingRequest.AssignedToUserId}") +
                (existingRequest.RequiredInventoryItemId.HasValue ? $", Linked Inventory ID: {existingRequest.RequiredInventoryItemId.Value}" : "")
                );

            return existingRequest;
        }

        public async Task<bool> AssignRequestAsync(int requestId, string assignedToUserId, string currentUserId)
        {
            var request = await GetRequestByIdAsync(requestId);
            if (request == null) return false;

            request.AssignedToUserId = assignedToUserId;
            if(request.Status == RequestStatus.Submitted)
            {
                request.Status = RequestStatus.InProgress;
            }
            request.LastUpdatedDate = DateTime.UtcNow;
            request.LastUpdatedByUserId = currentUserId;

            await AddActivityAsync(requestId, currentUserId, $"Request assigned to user {assignedToUserId}");

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Update, "ITRequest", requestId, currentUserId, 
                $"Request {request.RequestNumber} assigned");

            return true;
        }

        public async Task<bool> CancelRequestAsync(int requestId, string userId, string? reason = null)
        {
            var request = await GetRequestByIdAsync(requestId);
            if (request == null || request.Status == RequestStatus.Completed || request.Status == RequestStatus.Cancelled) return false;

            request.Status = RequestStatus.Cancelled;
            request.LastUpdatedDate = DateTime.UtcNow;
            request.LastUpdatedByUserId = userId;

            var activityDescription = string.IsNullOrEmpty(reason) ? "Request cancelled." : $"Request cancelled. Reason: {reason}";
            await AddActivityAsync(requestId, userId, activityDescription);

            await _context.SaveChangesAsync();
            await _auditService.LogAsync(AuditAction.Update, "ITRequest", requestId, userId, $"Request {request.RequestNumber} cancelled");

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

            await AddActivityAsync(requestId, completedById, $"Request completed. Notes: {completionNotes}");

            if (request.RelatedAssetId.HasValue)
            {
                await UpdateAssetStatusAfterCompletion(request, completedById);
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Update, "ITRequest", requestId, completedById, 
                $"Request {request.RequestNumber} completed");

            return true;
        }

        public async Task<bool> PlaceRequestOnHoldAsync(int requestId, string userId, string reason)
        {
            var request = await GetRequestByIdAsync(requestId);
            if (request == null || request.Status != RequestStatus.InProgress) return false;

            request.Status = RequestStatus.OnHold;
            request.LastUpdatedDate = DateTime.UtcNow;
            request.LastUpdatedByUserId = userId;
            
            await AddActivityAsync(requestId, userId, $"Request placed on hold. Reason: {reason}");

            await _context.SaveChangesAsync();
            await _auditService.LogAsync(AuditAction.Update, "ITRequest", requestId, userId, $"Request {request.RequestNumber} placed on hold");

            return true;
        }

        public async Task<bool> ResumeRequestAsync(int requestId, string userId, string? comments = null) // Added comments parameter
        {
            var request = await GetRequestByIdAsync(requestId);
            if (request == null || request.Status != RequestStatus.OnHold) return false;

            // Ensure the user taking action is the one assigned or an Admin/IT Support
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;
            var userRoles = await _userManager.GetRolesAsync(user);
            bool isAdminOrSupport = userRoles.Contains("Admin") || userRoles.Contains("IT Support");

            if (request.AssignedToUserId != userId && !isAdminOrSupport)
            {
                // If not assigned to current user and user is not Admin/Support, deny action
                return false;
            }

            request.Status = RequestStatus.InProgress;
            request.LastUpdatedDate = DateTime.UtcNow;
            request.LastUpdatedByUserId = userId;

            var activityDescription = "Request resumed.";
            if (!string.IsNullOrWhiteSpace(comments))
            {
                activityDescription += $" Comments: {comments}";
            }
            await AddActivityAsync(requestId, userId, activityDescription);

            await _context.SaveChangesAsync();
            await _auditService.LogAsync(AuditAction.Update, "ITRequest", requestId, userId, $"Request {request.RequestNumber} resumed");

            return true;
        }

        public async Task<bool> UpdateRequestStatusAsync(int requestId, RequestStatus newStatus, string userId, string? notes = null)
        {
            var request = await GetRequestByIdAsync(requestId);
            if (request == null)
            {
                // Consider logging this or throwing a specific exception
                return false;
            }

            // Optional: Add more sophisticated logic here to check if the status transition is valid
            // For example, can't go from Completed to InProgress without specific permissions or logic.
            // Can't change status if user is not authorized (e.g., not assigned and not Admin/Support)

            var oldStatus = request.Status;
            request.Status = newStatus;
            request.LastUpdatedDate = DateTime.UtcNow;
            request.LastUpdatedByUserId = userId;

            // Add activity log
            var activityDescription = $"Status changed from {oldStatus} to {newStatus}.";
            if (!string.IsNullOrWhiteSpace(notes))
            {
                activityDescription += $" Notes: {notes}";
            }
            await AddActivityAsync(requestId, userId, activityDescription);

            // Specific logic for certain status changes
            if (newStatus == RequestStatus.Completed)
            {
                request.CompletedDate = DateTime.UtcNow;
                request.CompletedByUserId = userId;
                if (!string.IsNullOrWhiteSpace(notes))
                {
                    request.CompletionNotes = notes;
                }
            }

            await _context.SaveChangesAsync();
            await _auditService.LogAsync(AuditAction.Update, "ITRequest", requestId, userId, $"Request {request.RequestNumber} status updated to {newStatus}. Notes: {notes}");

            return true;
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
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();
        }

        public async Task<List<ITRequest>> GetAssignedRequestsAsync(string userId)
        {
            return await _context.ITRequests
                .Where(r => r.AssignedToUserId == userId)
                .Include(r => r.RequestedByUser)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();
        }

        public async Task<int> GetPendingRequestsCountAsync()
        {
            return await _context.ITRequests
                .CountAsync(r => r.Status == RequestStatus.Submitted);
        }

        public async Task<int> GetMyActiveRequestsCountAsync(string userId)
        {
            return await _context.ITRequests
                .CountAsync(r => r.AssignedToUserId == userId &&
                               (r.Status != RequestStatus.Completed && r.Status != RequestStatus.Cancelled));
        }

        public async Task<RequestDashboardData> GetRequestDashboardDataAsync()
        {
            var totalRequests = await _context.ITRequests.CountAsync();
            var submittedRequests = await _context.ITRequests.CountAsync(r => r.Status == RequestStatus.Submitted);
            var inProgressRequests = await _context.ITRequests.CountAsync(r => r.Status == RequestStatus.InProgress);
            var onHoldRequests = await _context.ITRequests.CountAsync(r => r.Status == RequestStatus.OnHold);
            var completedRequests = await _context.ITRequests.CountAsync(r => r.Status == RequestStatus.Completed);
            var cancelledRequests = await _context.ITRequests.CountAsync(r => r.Status == RequestStatus.Cancelled);

            var requestsByType = await _context.ITRequests
                .GroupBy(r => r.RequestType)
                .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count());

            var requestsByPriority = await _context.ITRequests
                .GroupBy(r => r.Priority)
                .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count());

            var recentRequests = await _context.ITRequests
                .Include(r => r.RequestedByUser)
                .Include(r => r.AssignedToUser)
                .OrderByDescending(r => r.RequestDate)
                .Take(5)
                .Select(r => new RequestSummaryViewModel
                {
                    Id = r.Id,
                    RequestNumber = r.RequestNumber,
                    Title = r.Title,
                    RequestType = r.RequestType.ToString(),
                    Status = r.Status.ToString(),
                    Priority = r.Priority.ToString(),
                    RequesterName = r.RequestedByUser.FullName,
                    AssignedToName = r.AssignedToUser != null ? r.AssignedToUser.FullName : "Unassigned",
                    RequestDate = r.RequestDate,
                    DueDate = r.RequiredByDate
                })
                .ToListAsync();

            var overdueRequests = await GetOverdueRequestsAsync();
            var completedTodayCount = await _context.ITRequests.CountAsync(r => r.Status == RequestStatus.Completed && r.CompletedDate.HasValue && r.CompletedDate.Value.Date == DateTime.UtcNow.Date);

            var highPriorityActiveRequests = await _context.ITRequests
                .Include(r => r.RequestedByUser)
                .Where(r => (r.Priority == RequestPriority.High || r.Priority == RequestPriority.Critical) &&
                             r.Status != RequestStatus.Completed && r.Status != RequestStatus.Cancelled)
                .OrderByDescending(r => r.Priority) // Show critical first, then high
                .ThenBy(r => r.RequiredByDate)    // Then by due date
                .Take(10) // Limit to 10 for the dashboard view
                .Select(r => new RequestSummaryViewModel
                {
                    Id = r.Id,
                    RequestNumber = r.RequestNumber,
                    Title = r.Title,
                    Description = r.Description, // Added description for display
                    RequestType = r.RequestType.ToString(),
                    Status = r.Status.ToString(),
                    Priority = r.Priority.ToString(),
                    RequesterName = r.RequestedByUser.FullName,
                    AssignedToName = r.AssignedToUser != null ? r.AssignedToUser.FullName : "Unassigned",
                    RequestDate = r.RequestDate,
                    DueDate = r.RequiredByDate
                })
                .ToListAsync();

            return new RequestDashboardData
            {
                TotalRequests = totalRequests,
                SubmittedRequests = submittedRequests,
                InProgressRequests = inProgressRequests,
                OnHoldRequests = onHoldRequests,
                CompletedRequests = completedRequests,
                CancelledRequests = cancelledRequests,
                RequestsByType = requestsByType,
                RequestsByPriority = requestsByPriority,
                RecentRequests = recentRequests,
                OverdueRequests = overdueRequests.Count,
                CompletedToday = completedTodayCount,
                HighPriorityRequests = highPriorityActiveRequests // Populate the new property
            };
        }

        public async Task<string> GenerateRequestNumberAsync()
        {
            // Generate a unique request number based on current year and sequence
            var currentYear = DateTime.UtcNow.Year;
            var lastRequest = await _context.ITRequests
                .Where(r => r.RequestNumber.StartsWith($"REQ-{currentYear}-"))
                .OrderByDescending(r => r.RequestNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastRequest != null)
            {
                var lastSequence = lastRequest.RequestNumber.Split('-').LastOrDefault();
                if (int.TryParse(lastSequence, out int lastNum))
                {
                    sequence = lastNum + 1;
                }
            }

            return $"REQ-{currentYear}-{sequence:D4}";
        }

        public async Task<IEnumerable<ApplicationUser>> GetAssignableITStaffAsync()
        {
            var itRoles = new[] { "Admin", "IT Support" }; // Define roles that can be assigned requests
            var itStaff = new List<ApplicationUser>();

            foreach (var roleName in itRoles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
                itStaff.AddRange(usersInRole);
            }
            
            // Return distinct users, ordered by name, who are active
            return itStaff.Where(u => u.IsActive).DistinctBy(u => u.Id).OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToList();
        }

        public async Task<IEnumerable<InventoryItem>> GetRelevantInventoryItemsAsync(string? category = null, string? searchTerm = null)
        {
            var query = _context.InventoryItems
                .Where(i => i.Status == InventoryStatus.InStock && i.Quantity > 0); // Only available items

            if (!string.IsNullOrEmpty(category) && Enum.TryParse<InventoryCategory>(category, true, out var invCategory))
            {
                query = query.Where(i => i.Category == invCategory);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(i => 
                    (i.Name != null && i.Name.ToLower().Contains(term)) ||
                    (i.ItemCode != null && i.ItemCode.ToLower().Contains(term)) ||
                    (i.Description != null && i.Description.ToLower().Contains(term)) ||
                    (i.Brand != null && i.Brand.ToLower().Contains(term)) ||
                    (i.Model != null && i.Model.ToLower().Contains(term))
                );
            }

            return await query.OrderBy(i => i.Name).Take(100).ToListAsync(); // Limit results for performance
        }

        public async Task ProcessAssetReplacementFromInventoryAsync(int requestId, int replacementInventoryItemId, int? damagedAssetIdToUpdate, string? disposalNotesForUnmanagedAsset, string userId)
        {
            var request = await _context.ITRequests
                .Include(r => r.RequestedByUser) // Needed for new asset assignment
                .Include(r => r.Location) // Needed for new asset assignment
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                throw new KeyNotFoundException("Request not found.");

            var replacementInventoryItem = await _inventoryService.GetInventoryItemByIdAsync(replacementInventoryItemId);
            if (replacementInventoryItem == null || replacementInventoryItem.Quantity <= 0)
                throw new InvalidOperationException("Selected replacement inventory item is not available or not found.");

            // 1. Create a new Asset from the replacement inventory item
            var newAsset = new Asset
            {
                AssetTag = await _assetService.GenerateAssetTagAsync(), 
                Category = (AssetCategory)replacementInventoryItem.Category, // May need mapping
                Brand = replacementInventoryItem.Brand,
                Model = replacementInventoryItem.Model,
                SerialNumber = replacementInventoryItem.SerialNumber ?? string.Empty, // Handle possible null from inventory item
                Description = replacementInventoryItem.Description ?? $"Deployed from inventory item {replacementInventoryItem.ItemCode}",
                Status = AssetStatus.InUse, // New asset is immediately in use
                LocationId = request.LocationId, // Assign to request's location; if null, asset's location is null
                AssignedToUserId = request.RequestedByUserId, // Assign to the original requester
                Department = request.Department,
                PurchasePrice = replacementInventoryItem.UnitCost,
                WarrantyExpiry = replacementInventoryItem.WarrantyExpiry,
                InstallationDate = DateTime.UtcNow, // Or a specific date if provided
            };
            var createdAsset = await _assetService.CreateAssetAsync(newAsset, userId);
            request.RelatedAssetId = createdAsset.Id; // Link the new asset that fulfilled the request
            request.ProvidedInventoryItemId = replacementInventoryItemId;

            // 2. Update the inventory: reduce stock for the used item
            await _inventoryService.StockOutAsync(replacementInventoryItemId, 1, $"Deployed to fulfill request {request.RequestNumber}", userId);

            // 3. Handle the damaged asset
            if (damagedAssetIdToUpdate.HasValue)
            {
                var damagedAsset = await _assetService.GetAssetByIdAsync(damagedAssetIdToUpdate.Value);
                if (damagedAsset != null)
                {
                    await _assetService.ChangeAssetStatusAsync(damagedAsset.Id, AssetStatus.Decommissioned, $"Replaced by request {request.RequestNumber}", userId);
                }
            }
            else if (!string.IsNullOrWhiteSpace(disposalNotesForUnmanagedAsset))
            {
                request.DisposalNotesForUnmanagedAsset = disposalNotesForUnmanagedAsset;
                await _auditService.LogAsync(AuditAction.Update, "ITRequest", request.Id, userId, $"Disposal notes for unmanaged item: {disposalNotesForUnmanagedAsset}");
            }
            
            if(request.DamagedAssetId == null && damagedAssetIdToUpdate.HasValue)
            {
                request.DamagedAssetId = damagedAssetIdToUpdate;
            }


            // 4. Update the request status
            request.Status = RequestStatus.Completed;
            request.CompletedDate = DateTime.UtcNow;
            request.CompletedByUserId = userId;
            request.CompletionNotes = $"Fulfilled from inventory item {replacementInventoryItem.ItemCode}. New asset {createdAsset.AssetTag} deployed. " +
                                    (damagedAssetIdToUpdate.HasValue ? $"Old asset ID {damagedAssetIdToUpdate.Value} marked for disposal." : "") +
                                    (!string.IsNullOrWhiteSpace(disposalNotesForUnmanagedAsset) ? $" Disposal notes: {disposalNotesForUnmanagedAsset}" : "");
            request.LastUpdatedDate = DateTime.UtcNow;
            request.LastUpdatedByUserId = userId;

            await _context.SaveChangesAsync();
            await _auditService.LogAsync(AuditAction.Update, "ITRequest", request.Id, userId, $"Request completed. Fulfilled from inventory. New Asset: {createdAsset.AssetTag}.");
        }

        public async Task ProcessAssetReplacementViaProcurementAsync(int requestId, int? damagedAssetIdToUpdate, string? disposalNotesForUnmanagedAsset, string userId)
        {
            var request = await _context.ITRequests.FindAsync(requestId);
            if (request == null)
                throw new KeyNotFoundException("Request not found.");

            // 1. Handle the damaged asset
            if (damagedAssetIdToUpdate.HasValue)
            {
                var damagedAsset = await _assetService.GetAssetByIdAsync(damagedAssetIdToUpdate.Value);
                if (damagedAsset != null)
                {
                    await _assetService.ChangeAssetStatusAsync(damagedAsset.Id, AssetStatus.Decommissioned, $"Awaiting replacement via procurement for request {request.RequestNumber}", userId);
                }
            }
            else if (!string.IsNullOrWhiteSpace(disposalNotesForUnmanagedAsset))
            {
                request.DisposalNotesForUnmanagedAsset = disposalNotesForUnmanagedAsset;
                await _auditService.LogAsync(AuditAction.Update, "ITRequest", request.Id, userId, $"Disposal notes for unmanaged item (procurement path): {disposalNotesForUnmanagedAsset}");
            }
            
            if(request.DamagedAssetId == null && damagedAssetIdToUpdate.HasValue)
            {
                request.DamagedAssetId = damagedAssetIdToUpdate;
            }

            // 2. Trigger Procurement
            await _procurementService.CreateProcurementFromRequestAsync(requestId, userId);

            request.Status = RequestStatus.InProgress; // Set to InProgress as procurement is underway
            request.LastUpdatedDate = DateTime.UtcNow;
            request.LastUpdatedByUserId = userId;

            await _context.SaveChangesAsync();
            await _auditService.LogAsync(AuditAction.Update, "ITRequest", request.Id, userId, $"Procurement initiated for request {request.RequestNumber}.");
        }

        public async Task AddActivityAsync(int requestId, string userId, string description)
        {
            var activity = new RequestActivity
            {
                ITRequestId = requestId,
                UserId = userId,
                Description = description,
                ActivityDate = DateTime.UtcNow
            };
            _context.RequestActivities.Add(activity);
            await _context.SaveChangesAsync();
        }


        // Private methods for business logic

        private async Task UpdateAssetStatusAfterCompletion(ITRequest request, string completedById)
        {
            if (!request.RelatedAssetId.HasValue) return;

            switch (request.RequestType)
            {
                case RequestType.HardwareReplacement:
                    await _assetService.ChangeAssetStatusAsync(request.RelatedAssetId.Value, 
                        AssetStatus.InUse, $"Asset back in service after request {request.RequestNumber}", completedById);
                    break;
                case RequestType.MaintenanceService:
                    await _assetService.ChangeAssetStatusAsync(request.RelatedAssetId.Value, 
                        AssetStatus.InUse, $"Maintenance completed via request {request.RequestNumber}", completedById);
                    break;
            }
        }
    }
}
