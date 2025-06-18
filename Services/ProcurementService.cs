using Microsoft.EntityFrameworkCore;
using HospitalAssetTracker.Data;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Services;

namespace HospitalAssetTracker.Services
{
    public class ProcurementService : IProcurementService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly IAssetService _assetService;
        private readonly IInventoryService _inventoryService;

        public ProcurementService(
            ApplicationDbContext context,
            IAuditService auditService,
            IAssetService assetService,
            IInventoryService inventoryService)
        {
            _context = context;
            _auditService = auditService;
            _assetService = assetService;
            _inventoryService = inventoryService;
        }

        public async Task<PagedResult<ProcurementRequest>> GetProcurementRequestsAsync(ProcurementSearchModel searchModel)
        {
            var query = _context.ProcurementRequests
                .Include(p => p.RequestedByUser)
                .Include(p => p.SelectedVendor)
                .Include(p => p.OriginatingRequest)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchModel.SearchTerm))
            {
                query = query.Where(p => p.Title.Contains(searchModel.SearchTerm) ||
                                       p.Description.Contains(searchModel.SearchTerm) ||
                                       p.ProcurementNumber.Contains(searchModel.SearchTerm) ||
                                       p.Vendor.Name.Contains(searchModel.SearchTerm));
            }

            if (searchModel.ProcurementType.HasValue)
            {
                query = query.Where(p => p.ProcurementType == searchModel.ProcurementType.Value);
            }

            if (searchModel.Status.HasValue)
            {
                query = query.Where(p => p.Status == searchModel.Status.Value);
            }

            if (searchModel.Priority.HasValue)
            {
                query = query.Where(p => p.Priority == searchModel.Priority.Value);
            }

            if (searchModel.VendorId.HasValue)
            {
                query = query.Where(p => p.VendorId == searchModel.VendorId.Value);
            }

            if (searchModel.AmountFrom.HasValue)
            {
                query = query.Where(p => p.TotalAmount >= searchModel.AmountFrom.Value);
            }

            if (searchModel.AmountTo.HasValue)
            {
                query = query.Where(p => p.TotalAmount <= searchModel.AmountTo.Value);
            }

            if (searchModel.DateFrom.HasValue)
            {
                query = query.Where(p => p.CreatedDate >= searchModel.DateFrom.Value);
            }

            if (searchModel.DateTo.HasValue)
            {
                query = query.Where(p => p.CreatedDate <= searchModel.DateTo.Value);
            }

            // Apply sorting
            query = searchModel.SortBy?.ToLower() switch
            {
                "title" => searchModel.SortDesc ? query.OrderByDescending(p => p.Title) : query.OrderBy(p => p.Title),
                "priority" => searchModel.SortDesc ? query.OrderByDescending(p => p.Priority) : query.OrderBy(p => p.Priority),
                "status" => searchModel.SortDesc ? query.OrderByDescending(p => p.Status) : query.OrderBy(p => p.Status),
                "totalamount" => searchModel.SortDesc ? query.OrderByDescending(p => p.TotalAmount) : query.OrderBy(p => p.TotalAmount),
                "vendor" => searchModel.SortDesc ? query.OrderByDescending(p => p.Vendor.Name) : query.OrderBy(p => p.Vendor.Name),
                "createdate" => searchModel.SortDesc ? query.OrderByDescending(p => p.CreatedDate) : query.OrderBy(p => p.CreatedDate),
                _ => query.OrderByDescending(p => p.CreatedDate)
            };

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((searchModel.Page - 1) * searchModel.PageSize)
                .Take(searchModel.PageSize)
                .ToListAsync();

            return new PagedResult<ProcurementRequest>
            {
                Items = items,
                TotalCount = totalItems,
                PageNumber = searchModel.Page,
                PageSize = searchModel.PageSize
            };
        }

        public async Task<ProcurementRequest?> GetProcurementRequestByIdAsync(int procurementId)
        {
            return await _context.ProcurementRequests
                .Include(p => p.Requester)
                .Include(p => p.Vendor)
                .Include(p => p.RelatedRequest)
                .Include(p => p.Items)
                .Include(p => p.Activities)
                .Include(p => p.Approvals)
                .FirstOrDefaultAsync(p => p.Id == procurementId);
        }

        public async Task<ProcurementRequest> CreateProcurementRequestAsync(ProcurementRequest procurement, string userId)
        {
            // Generate procurement number
            var datePrefix = DateTime.Now.ToString("yyyyMM");
            var lastProcurement = await _context.ProcurementRequests
                .Where(p => p.RequestNumber.StartsWith($"PROC-{datePrefix}"))
                .OrderByDescending(p => p.RequestNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastProcurement != null)
            {
                var lastSequence = lastProcurement.RequestNumber.Split('-').Last();
                if (int.TryParse(lastSequence, out int lastNum))
                {
                    sequence = lastNum + 1;
                }
            }

            procurement.RequestNumber = $"PROC-{datePrefix}-{sequence:D4}";
            procurement.RequesterId = userId;
            procurement.CreatedDate = DateTime.UtcNow;
            procurement.Status = ProcurementStatus.Draft;

            // Calculate total amount from items
            if (procurement.Items?.Any() == true)
            {
                procurement.TotalAmount = procurement.Items.Sum(i => i.Quantity * i.UnitPrice);
            }

            // Set automatic priority based on urgency and amount
            await SetAutomaticPriorityAsync(procurement);

            _context.ProcurementRequests.Add(procurement);
            await _context.SaveChangesAsync();

            // Create initial activity log
            var initialActivity = new ProcurementActivity
            {
                ProcurementRequestId = procurement.Id,
                ActivityType = ProcurementActivityType.Created,
                Description = "Procurement request created",
                ActivityDate = DateTime.UtcNow,
                UserId = userId
            };
            _context.ProcurementActivities.Add(initialActivity);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Create, "ProcurementRequest", procurement.Id, userId, 
                $"Procurement request {procurement.ProcurementNumber} created");

            return procurement;
        }

        public async Task<ProcurementRequest> CreateProcurementFromRequestAsync(int requestId, string userId)
        {
            var request = await _context.ITRequests
                .Include(r => r.Requester)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                throw new InvalidOperationException("Related request not found");

            var procurement = new ProcurementRequest
            {
                Title = $"Procurement for: {request.Title}",
                Description = request.Description,
                ProcurementType = MapRequestTypeToProcurementType(request.RequestType),
                Priority = MapRequestPriorityToProcurementPriority(request.Priority),
                RequiredByDate = request.RequiredByDate,
                EstimatedAmount = request.EstimatedCost ?? 0,
                RelatedRequestId = requestId,
                BusinessJustification = request.BusinessJustification,
                IsUrgent = request.Priority == RequestPriority.Critical
            };

            // Add procurement items based on request
            if (!string.IsNullOrEmpty(request.RequestedItemCategory))
            {
                var item = new ProcurementItem
                {
                    ItemName = request.RequestedItemCategory,
                    Description = request.RequestedItemSpecifications,
                    Quantity = 1,
                    UnitPrice = request.EstimatedCost ?? 0,
                    TotalPrice = request.EstimatedCost ?? 0
                };
                procurement.Items = new List<ProcurementItem> { item };
            }

            return await CreateProcurementRequestAsync(procurement, userId);
        }

        public async Task<ProcurementRequest> UpdateProcurementRequestAsync(ProcurementRequest procurement, string userId)
        {
            var existing = await GetProcurementRequestByIdAsync(procurement.Id);
            if (existing == null)
                throw new InvalidOperationException("Procurement request not found");

            // Update fields
            existing.Title = procurement.Title;
            existing.Description = procurement.Description;
            existing.Priority = procurement.Priority;
            existing.RequiredByDate = procurement.RequiredByDate;
            existing.BusinessJustification = procurement.BusinessJustification;
            existing.EstimatedAmount = procurement.EstimatedAmount;
            existing.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log activity
            var activity = new ProcurementActivity
            {
                ProcurementRequestId = procurement.Id,
                ActivityType = ProcurementActivityType.Updated,
                Description = "Procurement request updated",
                ActivityDate = DateTime.UtcNow,
                UserId = userId
            };
            _context.ProcurementActivities.Add(activity);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Update, "ProcurementRequest", procurement.Id, userId, 
                $"Procurement request {existing.ProcurementNumber} updated");

            return existing;
        }

        public async Task<bool> SubmitForApprovalAsync(int procurementId, string userId)
        {
            var procurement = await GetProcurementRequestByIdAsync(procurementId);
            if (procurement == null) return false;

            procurement.Status = ProcurementStatus.PendingApproval;
            procurement.SubmittedDate = DateTime.UtcNow;
            procurement.UpdatedDate = DateTime.UtcNow;

            var activity = new ProcurementActivity
            {
                ProcurementRequestId = procurementId,
                ActivityType = ProcurementActivityType.SubmittedForApproval,
                Description = "Submitted for approval",
                ActivityDate = DateTime.UtcNow,
                UserId = userId
            };
            _context.ProcurementActivities.Add(activity);

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Update, "ProcurementRequest", procurementId, userId, 
                $"Procurement request {procurement.ProcurementNumber} submitted for approval");

            return true;
        }

        public async Task<bool> ApproveProcurementAsync(int procurementId, string approverId, string? comments = null)
        {
            var procurement = await GetProcurementRequestByIdAsync(procurementId);
            if (procurement == null) return false;

            // Create approval record
            var approval = new ProcurementApproval
            {
                ProcurementRequestId = procurementId,
                ApproverId = approverId,
                ApprovalDate = DateTime.UtcNow,
                ApprovalStatus = ProcurementApprovalStatus.Approved,
                Comments = comments
            };
            _context.ProcurementApprovals.Add(approval);

            // Update status based on approval workflow
            if (await IsFullyApprovedAsync(procurementId))
            {
                procurement.Status = ProcurementStatus.Approved;
                procurement.ApprovedDate = DateTime.UtcNow;
                
                // Auto-initiate procurement process if applicable
                if (await ShouldAutoInitiateProcurementAsync(procurement))
                {
                    await InitiateProcurementProcessAsync(procurement, approverId);
                }
            }

            procurement.UpdatedDate = DateTime.UtcNow;

            var activity = new ProcurementActivity
            {
                ProcurementRequestId = procurementId,
                ActivityType = ProcurementActivityType.Approved,
                Description = $"Approved{(comments != null ? $": {comments}" : "")}",
                ActivityDate = DateTime.UtcNow,
                UserId = approverId
            };
            _context.ProcurementActivities.Add(activity);

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Update, "ProcurementRequest", procurementId, approverId, 
                $"Procurement request {procurement.ProcurementNumber} approved");

            return true;
        }

        public async Task<bool> ReceiveProcurementAsync(int procurementId, string receivedById, List<ProcurementItemReceived> receivedItems)
        {
            var procurement = await GetProcurementRequestByIdAsync(procurementId);
            if (procurement == null) return false;

            procurement.Status = ProcurementStatus.Received;
            procurement.ReceivedDate = DateTime.UtcNow;
            procurement.UpdatedDate = DateTime.UtcNow;

            // Process received items
            foreach (var receivedItem in receivedItems)
            {
                // Add to inventory
                // Create inventory item first
                var inventoryItem = new InventoryItem
                {
                    Name = receivedItem.ItemName,
                    Description = receivedItem.Description ?? string.Empty,
                    Category = Enum.TryParse<InventoryCategory>(receivedItem.Category, out var cat) ? cat : InventoryCategory.Other,
                    UnitCost = receivedItem.UnitPrice,
                    Quantity = receivedItem.ReceivedQuantity,
                    LocationId = 1, // Default location ID
                    Status = InventoryStatus.Available,
                    CreatedDate = DateTime.UtcNow,
                    CreatedByUserId = receivedById
                };

                var createdItem = await _inventoryService.CreateInventoryItemAsync(inventoryItem, receivedById);

                // If this is for a specific request, try to fulfill it
                if (procurement.RelatedRequestId.HasValue)
                {
                    await ProcessRequestFulfillmentAsync(procurement.RelatedRequestId.Value, receivedItem, receivedById);
                }
            }

            var activity = new ProcurementActivity
            {
                ProcurementRequestId = procurementId,
                ActivityType = ProcurementActivityType.Received,
                Description = $"Items received and added to inventory",
                ActivityDate = DateTime.UtcNow,
                UserId = receivedById
            };
            _context.ProcurementActivities.Add(activity);

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Update, "ProcurementRequest", procurementId, receivedById, 
                $"Procurement {procurement.ProcurementNumber} received");

            return true;
        }

        public async Task<ProcurementDashboardData> GetProcurementDashboardDataAsync()
        {
            var totalProcurements = await _context.ProcurementRequests.CountAsync();
            var pendingApproval = await _context.ProcurementRequests.CountAsync(p => 
                p.Status == ProcurementStatus.PendingApproval);
            var inProgress = await _context.ProcurementRequests.CountAsync(p => 
                p.Status == ProcurementStatus.InProgress || p.Status == ProcurementStatus.Ordered);
            var completedThisMonth = await _context.ProcurementRequests.CountAsync(p => 
                p.Status == ProcurementStatus.Received && 
                p.ReceivedDate.HasValue && 
                p.ReceivedDate.Value.Month == DateTime.UtcNow.Month);

            // Calculate total spending this month
            var totalSpendingThisMonth = await _context.ProcurementRequests
                .Where(p => p.Status == ProcurementStatus.Received && 
                           p.ReceivedDate.HasValue && 
                           p.ReceivedDate.Value.Month == DateTime.UtcNow.Month)
                .SumAsync(p => p.TotalAmount);

            // Get procurement by type counts
            var procurementsByType = await _context.ProcurementRequests
                .GroupBy(p => p.ProcurementType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();

            // Get top vendors by spending
            var topVendors = await _context.ProcurementRequests
                .Where(p => p.Status == ProcurementStatus.Received && p.VendorId.HasValue)
                .Include(p => p.Vendor)
                .GroupBy(p => p.Vendor.Name)
                .Select(g => new { Vendor = g.Key, Total = g.Sum(p => p.TotalAmount) })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToListAsync();

            // Get recent procurements
            var recentProcurementsData = await _context.ProcurementRequests
                .Include(p => p.Requester)
                .Include(p => p.Vendor)
                .OrderByDescending(p => p.RequestDate)
                .Take(10)
                .ToListAsync();

            var recentProcurements = recentProcurementsData.Select(p => new ProcurementSummaryViewModel
            {
                Id = p.Id,
                RequestNumber = p.RequestNumber,
                Title = p.Title,
                VendorName = p.Vendor?.Name ?? "Not Selected",
                TotalAmount = p.TotalAmount ?? 0,
                Status = p.Status,
                RequestDate = p.RequestDate,
                RequesterName = p.Requester?.FirstName + " " + p.Requester?.LastName
            }).ToList();

            return new ProcurementDashboardData
            {
                TotalProcurements = totalProcurements,
                PendingApproval = pendingApproval,
                InProgress = inProgress,
                CompletedThisMonth = completedThisMonth,
                TotalSpendingThisMonth = totalSpendingThisMonth ?? 0,
                ProcurementsByType = procurementsByType.ToDictionary(x => x.Type.ToString(), x => x.Count),
                TopVendors = topVendors.ToDictionary(x => x.Vendor, x => x.Total ?? 0),
                RecentProcurements = recentProcurements
            };
        }

        public async Task<List<ProcurementRequest>> GetOverdueProcurementsAsync()
        {
            return await _context.ProcurementRequests
                .Include(p => p.Requester)
                .Include(p => p.Vendor)
                .Where(p => p.RequiredByDate.HasValue && 
                           p.RequiredByDate.Value < DateTime.UtcNow && 
                           p.Status != ProcurementStatus.Received && 
                           p.Status != ProcurementStatus.Cancelled)
                .OrderBy(p => p.RequiredByDate)
                .ToListAsync();
        }

        public async Task<List<Vendor>> GetVendorsAsync()
        {
            return await _context.Vendors
                .Where(v => v.IsActive)
                .OrderBy(v => v.Name)
                .ToListAsync();
        }

        public async Task<Vendor> CreateVendorAsync(Vendor vendor, string userId)
        {
            vendor.CreatedDate = DateTime.UtcNow;
            vendor.IsActive = true;

            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Create, "Vendor", vendor.Id, userId, 
                $"Vendor {vendor.Name} created");

            return vendor;
        }

        // Private helper methods

        private async Task SetAutomaticPriorityAsync(ProcurementRequest procurement)
        {
            if (procurement.IsUrgent || procurement.TotalAmount > 10000)
            {
                procurement.Priority = ProcurementPriority.High;
            }
            else if (procurement.TotalAmount > 5000)
            {
                procurement.Priority = ProcurementPriority.Medium;
            }
            else
            {
                procurement.Priority = ProcurementPriority.Low;
            }
        }

        private ProcurementType MapRequestTypeToProcurementType(RequestType requestType)
        {
            return requestType switch
            {
                RequestType.NewEquipmentProvisioning => ProcurementType.NewEquipment,
                RequestType.HardwareReplacement => ProcurementType.Replacement,
                RequestType.SoftwareInstallation => ProcurementType.Software,
                RequestType.MaintenanceService => ProcurementType.Services,
                _ => ProcurementType.Other
            };
        }

        private ProcurementPriority MapRequestPriorityToProcurementPriority(RequestPriority requestPriority)
        {
            return requestPriority switch
            {
                RequestPriority.Critical => ProcurementPriority.High,
                RequestPriority.High => ProcurementPriority.High,
                RequestPriority.Medium => ProcurementPriority.Medium,
                RequestPriority.Low => ProcurementPriority.Low,
                _ => ProcurementPriority.Medium
            };
        }

        private async Task<bool> IsFullyApprovedAsync(int procurementId)
        {
            var procurement = await GetProcurementRequestByIdAsync(procurementId);
            if (procurement == null) return false;

            var approvals = await _context.ProcurementApprovals
                .Where(a => a.ProcurementRequestId == procurementId && a.ApprovalStatus == ProcurementApprovalStatus.Approved)
                .CountAsync();

            return approvals >= GetRequiredApprovalCount(procurement);
        }

        private int GetRequiredApprovalCount(ProcurementRequest procurement)
        {
            if (procurement.TotalAmount <= 1000) return 1;
            if (procurement.TotalAmount <= 10000) return 2;
            return 3;
        }

        private async Task<bool> ShouldAutoInitiateProcurementAsync(ProcurementRequest procurement)
        {
            // Auto-initiate for small amounts or urgent requests
            return procurement.TotalAmount <= 500 || procurement.IsUrgent;
        }

        private async Task InitiateProcurementProcessAsync(ProcurementRequest procurement, string userId)
        {
            procurement.Status = ProcurementStatus.InProgress;
            
            var activity = new ProcurementActivity
            {
                ProcurementRequestId = procurement.Id,
                ActivityType = ProcurementActivityType.ProcessInitiated,
                Description = "Procurement process initiated",
                ActivityDate = DateTime.UtcNow,
                UserId = userId
            };
            _context.ProcurementActivities.Add(activity);
        }

        private async Task ProcessRequestFulfillmentAsync(int requestId, ProcurementItemReceived receivedItem, string userId)
        {
            var request = await _context.ITRequests.FindAsync(requestId);
            if (request == null) return;

            // Create asset from received item if it's a hardware procurement
            if (!string.IsNullOrEmpty(receivedItem.Category) && 
                (receivedItem.Category.Contains("Hardware") || receivedItem.Category.Contains("Computer")))
            {
                var asset = new Asset
                {
                    AssetTag = await _assetService.GenerateAssetTagAsync(),
                    Brand = "Unknown", // Default brand
                    Model = receivedItem.ItemName,
                    Description = receivedItem.Description ?? string.Empty,
                    Category = Enum.TryParse<AssetCategory>(receivedItem.Category, out var assetCat) ? assetCat : AssetCategory.Other,
                    SerialNumber = receivedItem.SerialNumber ?? string.Empty,
                    PurchasePrice = receivedItem.UnitPrice,
                    Status = AssetStatus.Available,
                    WarrantyExpiry = DateTime.UtcNow.AddYears(1), // Default 1 year warranty
                    CreatedDate = DateTime.UtcNow,
                    InstallationDate = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };

                await _assetService.CreateAssetAsync(asset, userId);
                
                // Link asset to request
                request.AssetId = asset.Id;
            }

            // Update request status to ready for completion
            request.Status = RequestStatus.ReadyForCompletion;
            request.LastUpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // Dashboard and analytics methods
        public async Task<int> GetActiveRequestsCountAsync()
        {
            return await _context.ProcurementRequests
                .CountAsync(p => p.Status == ProcurementStatus.InProgress || p.Status == ProcurementStatus.Pending);
        }

        public async Task<int> GetPendingApprovalsCountAsync()
        {
            return await _context.ProcurementRequests
                .CountAsync(p => p.Status == ProcurementStatus.Pending);
        }

        public async Task<int> GetActiveVendorsCountAsync()
        {
            return await _context.Vendors
                .CountAsync(v => v.Status == VendorStatus.Active);
        }

        public async Task<decimal> GetMonthlySpendAsync()
        {
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            return await _context.ProcurementRequests
                .Where(p => p.RequestDate >= startOfMonth && p.Status == ProcurementStatus.Completed)
                .SumAsync(p => p.EstimatedBudget);
        }

        public async Task<List<ProcurementRequest>> GetRecentRequestsAsync(int count = 10)
        {
            return await _context.ProcurementRequests
                .Include(p => p.RequestedByUser)
                .Include(p => p.SelectedVendor)
                .OrderByDescending(p => p.RequestDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<ProcurementApproval>> GetRecentApprovalsAsync(int count = 5)
        {
            return await _context.ProcurementApprovals
                .Include(p => p.ProcurementRequest)
                .Include(p => p.ApprovedByUser)
                .OrderByDescending(p => p.ApprovalDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Vendor>> GetActiveVendorsAsync()
        {
            return await _context.Vendors
                .Where(v => v.Status == VendorStatus.Active)
                .OrderBy(v => v.Name)
                .ToListAsync();
        }

        public async Task<List<List<string>>> GetExportDataAsync(string reportType)
        {
            var data = new List<List<string>>();
            
            // Header row
            data.Add(new List<string> { "ID", "Title", "Description", "Status", "Budget", "Date", "Vendor" });
            
            // Data rows
            var requests = await _context.ProcurementRequests
                .Include(p => p.SelectedVendor)
                .OrderByDescending(p => p.RequestDate)
                .Take(1000) // Limit export size
                .ToListAsync();

            foreach (var request in requests)
            {
                data.Add(new List<string>
                {
                    request.Id.ToString(),
                    request.Title,
                    request.Description,
                    request.Status.ToString(),
                    request.EstimatedBudget.ToString("C"),
                    request.RequestDate.ToString("yyyy-MM-dd"),
                    request.SelectedVendor?.Name ?? "N/A"
                });
            }

            return data;
        }
    }
}