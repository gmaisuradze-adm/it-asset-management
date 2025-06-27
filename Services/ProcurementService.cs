using Microsoft.EntityFrameworkCore;
using HospitalAssetTracker.Data;
using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Services
{
    public class ProcurementService : IProcurementService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public ProcurementService(
            ApplicationDbContext context,
            IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // Basic CRUD operations
        public async Task<PagedResult<ProcurementRequest>> GetProcurementRequestsAsync(ProcurementSearchModel searchModel)
        {
            var query = _context.ProcurementRequests
                .Include(p => p.RequestedByUser)
                .Include(p => p.SelectedVendor)
                .Include(p => p.Items)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchModel.SearchTerm))
            {
                query = query.Where(p => p.Title.Contains(searchModel.SearchTerm) ||
                                       p.Description.Contains(searchModel.SearchTerm) ||
                                       p.ProcurementNumber.Contains(searchModel.SearchTerm));
            }

            if (searchModel.Status.HasValue)
            {
                query = query.Where(p => p.Status == searchModel.Status.Value);
            }

            if (searchModel.ProcurementType.HasValue)
            {
                query = query.Where(p => p.ProcurementType == searchModel.ProcurementType.Value);
            }

            if (searchModel.Category.HasValue)
            {
                query = query.Where(p => p.Category == searchModel.Category.Value);
            }

            if (searchModel.VendorId.HasValue)
            {
                query = query.Where(p => p.SelectedVendorId == searchModel.VendorId.Value);
            }

            if (searchModel.AmountFrom.HasValue)
            {
                query = query.Where(p => p.EstimatedBudget >= searchModel.AmountFrom.Value);
            }

            if (searchModel.AmountTo.HasValue)
            {
                query = query.Where(p => p.EstimatedBudget <= searchModel.AmountTo.Value);
            }

            if (searchModel.DateFrom.HasValue)
            {
                query = query.Where(p => p.RequestDate >= searchModel.DateFrom.Value);
            }

            if (searchModel.DateTo.HasValue)
            {
                query = query.Where(p => p.RequestDate <= searchModel.DateTo.Value);
            }

            var totalCount = await query.CountAsync();
            var pageSize = searchModel.PageSize;
            var page = searchModel.Page;

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<ProcurementRequest>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<ProcurementRequest?> GetProcurementRequestByIdAsync(int procurementId)
        {
            return await _context.ProcurementRequests
                .Include(p => p.RequestedByUser)
                .Include(p => p.SelectedVendor)
                .Include(p => p.Items)
                .Include(p => p.OriginatingRequest)
                .FirstOrDefaultAsync(p => p.Id == procurementId);
        }

        public async Task<ProcurementRequest> CreateProcurementRequestAsync(ProcurementRequest procurement, string userId)
        {
            procurement.RequestDate = DateTime.UtcNow;
            procurement.Status = ProcurementStatus.Draft;
            procurement.ProcurementNumber = await GenerateProcurementNumberAsync();
            procurement.RequestedByUserId = userId;

            _context.ProcurementRequests.Add(procurement);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Create, "ProcurementRequest", procurement.Id, userId, 
                $"Procurement {procurement.ProcurementNumber} created");

            return procurement;
        }

        public async Task<ProcurementRequest> CreateProcurementFromRequestAsync(int requestId, string userId)
        {
            var request = await _context.ITRequests
                .Include(r => r.RequestedByUser)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                throw new ArgumentException("Request not found", nameof(requestId));

            var procurement = new ProcurementRequest
            {
                Title = request.Title,
                Description = request.Description ?? string.Empty,
                Department = request.Department ?? string.Empty,
                ProcurementType = ProcurementType.Equipment,
                Category = ProcurementCategory.ITEquipment,
                Method = ProcurementMethod.DirectPurchase,
                RequiredByDate = request.RequiredByDate,
                OriginatingRequestId = requestId,
                RequestedByUserId = userId,
                EstimatedBudget = 0m,
                Items = new List<ProcurementItem>
                {
                    new ProcurementItem
                    {
                        ItemName = request.Title,
                        Description = request.Description,
                        Quantity = 1,
                        EstimatedUnitPrice = 0,
                        Unit = "each"
                    }
                }
            };

            return await CreateProcurementRequestAsync(procurement, userId);
        }

        public async Task<ProcurementRequest> UpdateProcurementRequestAsync(ProcurementRequest procurement, string userId)
        {
            var existing = await _context.ProcurementRequests.FindAsync(procurement.Id);
            if (existing == null)
                throw new ArgumentException("Procurement request not found", nameof(procurement));

            existing.Title = procurement.Title;
            existing.Description = procurement.Description;
            existing.ProcurementType = procurement.ProcurementType;
            existing.Category = procurement.Category;
            existing.RequiredByDate = procurement.RequiredByDate;
            existing.EstimatedBudget = procurement.EstimatedBudget;
            existing.SelectedVendorId = procurement.SelectedVendorId;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Update, "ProcurementRequest", procurement.Id, userId, 
                $"Procurement {existing.ProcurementNumber} updated");

            return existing;
        }

        private async Task<string> GenerateProcurementNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var count = await _context.ProcurementRequests
                .CountAsync(p => p.RequestDate.Year == year);
            return $"PR-{year}-{(count + 1):D6}";
        }

        // Advanced Search and Filtering - Stub implementations
        public async Task<PagedResult<ProcurementSearchModels.AdvancedProcurementSearchResult>> GetProcurementRequestsAdvancedAsync(ProcurementSearchModels.AdvancedProcurementSearchModel searchModel)
        {
            await Task.CompletedTask;
            return new PagedResult<ProcurementSearchModels.AdvancedProcurementSearchResult>
            {
                Items = new List<ProcurementSearchModels.AdvancedProcurementSearchResult>(),
                TotalCount = 0,
                PageNumber = searchModel.PageNumber,
                PageSize = searchModel.PageSize
            };
        }

        public async Task<IEnumerable<ProcurementSearchModels.AdvancedProcurementSearchResult>> SearchProcurementRequestsAsync(string searchTerm, int maxResults = 50)
        {
            await Task.CompletedTask;
            return new List<ProcurementSearchModels.AdvancedProcurementSearchResult>();
        }

        // Approval workflow - Stub implementations
        public async Task<bool> SubmitForApprovalAsync(int procurementId, string userId)
        {
            var procurement = await GetProcurementRequestByIdAsync(procurementId);
            if (procurement == null) return false;

            procurement.Status = ProcurementStatus.PendingApproval;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApproveProcurementAsync(int procurementId, string approverId, string? comments = null)
        {
            var procurement = await GetProcurementRequestByIdAsync(procurementId);
            if (procurement == null) return false;

            procurement.Status = ProcurementStatus.Approved;
            procurement.ApprovedByUserId = approverId;
            procurement.ApprovalDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReceiveProcurementAsync(int procurementId, string receivedById, List<ProcurementItemReceived> receivedItems)
        {
            await Task.CompletedTask;
            return true;
        }

        // All other methods - stub implementations that return empty data or Task.CompletedTask
        public async Task<ProcurementSearchModels.ApprovalChainModel> GetApprovalChainAsync(int procurementId)
        {
            await Task.CompletedTask;
            return new ProcurementSearchModels.ApprovalChainModel();
        }

        public async Task<bool> ProcessApprovalStepAsync(int procurementId, string approverId, bool approve, string? comments = null)
        {
            await Task.CompletedTask;
            return true;
        }

        public async Task<IEnumerable<ProcurementSearchModels.AdvancedProcurementSearchResult>> GetPendingApprovalsForUserAsync(string userId)
        {
            await Task.CompletedTask;
            return new List<ProcurementSearchModels.AdvancedProcurementSearchResult>();
        }

        public async Task<ProcurementSearchModels.BulkOperationResult> BulkApproveProcurementsAsync(ProcurementSearchModels.BulkApprovalRequest request, string userId)
        {
            await Task.CompletedTask;
            return new ProcurementSearchModels.BulkOperationResult();
        }

        public async Task<ProcurementSearchModels.BulkOperationResult> BulkUpdateProcurementsAsync(ProcurementSearchModels.BulkProcurementUpdateRequest request, string userId)
        {
            await Task.CompletedTask;
            return new ProcurementSearchModels.BulkOperationResult();
        }

        public async Task<ProcurementSearchModels.BulkOperationResult> BulkOperationProcurementsAsync(ProcurementSearchModels.BulkProcurementOperationRequest request, string userId)
        {
            await Task.CompletedTask;
            return new ProcurementSearchModels.BulkOperationResult();
        }

        public async Task<IEnumerable<ProcurementSearchModels.AdvancedProcurementSearchResult>> GetPendingApprovalRequestsAsync()
        {
            await Task.CompletedTask;
            return new List<ProcurementSearchModels.AdvancedProcurementSearchResult>();
        }

        public async Task<IEnumerable<ProcurementSearchModels.AdvancedProcurementSearchResult>> GetOverdueProcurementRequestsAsync()
        {
            await Task.CompletedTask;
            return new List<ProcurementSearchModels.AdvancedProcurementSearchResult>();
        }

        public async Task<IEnumerable<ProcurementSearchModels.AdvancedProcurementSearchResult>> GetEmergencyProcurementsAsync()
        {
            await Task.CompletedTask;
            return new List<ProcurementSearchModels.AdvancedProcurementSearchResult>();
        }

        public async Task<IEnumerable<ProcurementSearchModels.AdvancedProcurementSearchResult>> GetHighValueProcurementsAsync()
        {
            await Task.CompletedTask;
            return new List<ProcurementSearchModels.AdvancedProcurementSearchResult>();
        }

        public async Task<IEnumerable<ProcurementSearchModels.AdvancedProcurementSearchResult>> GetRecentProcurementRequestsAsync()
        {
            await Task.CompletedTask;
            return new List<ProcurementSearchModels.AdvancedProcurementSearchResult>();
        }

        public async Task<IEnumerable<ProcurementSearchModels.AdvancedProcurementSearchResult>> GetUserProcurementRequestsAsync(string userId)
        {
            await Task.CompletedTask;
            return new List<ProcurementSearchModels.AdvancedProcurementSearchResult>();
        }

        public async Task<byte[]?> ExportProcurementDataAsync(ProcurementSearchModels.ProcurementExportRequest request)
        {
            await Task.CompletedTask;
            return null;
        }

        public async Task<ProcurementSearchModels.ProcurementAnalyticsModel> GetProcurementAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            await Task.CompletedTask;
            return new ProcurementSearchModels.ProcurementAnalyticsModel();
        }

        public async Task<ProcurementSearchModels.ProcurementReport> GenerateProcurementReportAsync(string reportType, DateTime fromDate, DateTime toDate)
        {
            await Task.CompletedTask;
            return new ProcurementSearchModels.ProcurementReport();
        }

        public async Task<ProcurementDashboardData> GetProcurementDashboardDataAsync()
        {
            await Task.CompletedTask;
            return new ProcurementDashboardData();
        }

        public async Task<List<ProcurementRequest>> GetOverdueProcurementsAsync()
        {
            await Task.CompletedTask;
            return new List<ProcurementRequest>();
        }

        public async Task<int> GetActiveRequestsCountAsync()
        {
            return await _context.ProcurementRequests
                .CountAsync(p => p.Status == ProcurementStatus.Draft || p.Status == ProcurementStatus.PendingApproval);
        }

        public async Task<int> GetPendingApprovalsCountAsync()
        {
            return await _context.ProcurementRequests
                .CountAsync(p => p.Status == ProcurementStatus.PendingApproval);
        }

        public async Task<int> GetActiveVendorsCountAsync()
        {
            return await _context.Vendors.CountAsync();
        }

        public async Task<decimal> GetMonthlySpendAsync()
        {
            var firstDayOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            return await _context.ProcurementRequests
                .Where(p => p.RequestDate >= firstDayOfMonth && p.ActualCost.HasValue)
                .SumAsync(p => p.ActualCost.Value);
        }

        public async Task<List<ProcurementRequest>> GetRecentRequestsAsync(int count = 10)
        {
            return await _context.ProcurementRequests
                .Include(p => p.RequestedByUser)
                .OrderByDescending(p => p.RequestDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<ProcurementApproval>> GetRecentApprovalsAsync(int count = 5)
        {
            await Task.CompletedTask;
            return new List<ProcurementApproval>();
        }

        public async Task<List<Vendor>> GetActiveVendorsAsync()
        {
            return await _context.Vendors.ToListAsync();
        }

        public async Task<List<List<string>>> GetExportDataAsync(string reportType)
        {
            await Task.CompletedTask;
            return new List<List<string>>();
        }

        public async Task<List<Vendor>> GetVendorsAsync()
        {
            return await _context.Vendors.ToListAsync();
        }

        public async Task<Vendor> CreateVendorAsync(Vendor vendor, string userId)
        {
            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();
            return vendor;
        }

        public async Task<ProcurementSearchModels.VendorEvaluationModel> GetVendorEvaluationAsync(int vendorId)
        {
            await Task.CompletedTask;
            return new ProcurementSearchModels.VendorEvaluationModel();
        }

        public async Task<ProcurementSearchModels.VendorPerformanceMetrics> GetVendorPerformanceAsync(int vendorId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            await Task.CompletedTask;
            return new ProcurementSearchModels.VendorPerformanceMetrics();
        }

        public async Task<ProcurementSearchModels.VendorComparisonModel> CompareVendorsAsync(List<int> vendorIds, string criteria)
        {
            await Task.CompletedTask;
            return new ProcurementSearchModels.VendorComparisonModel();
        }

        public async Task<IEnumerable<Vendor>> GetRecommendedVendorsAsync(ProcurementType procurementType, ProcurementCategory category)
        {
            await Task.CompletedTask;
            return new List<Vendor>();
        }

        public async Task<ProcurementSearchModels.BudgetAllocationModel> GetBudgetAllocationAsync(string budgetCode)
        {
            await Task.CompletedTask;
            return new ProcurementSearchModels.BudgetAllocationModel();
        }

        public async Task<bool> ValidateBudgetAvailabilityAsync(string budgetCode, decimal amount)
        {
            await Task.CompletedTask;
            return true;
        }

        public async Task<IEnumerable<ProcurementSearchModels.BudgetAllocationModel>> GetDepartmentBudgetsAsync(string department)
        {
            await Task.CompletedTask;
            return new List<ProcurementSearchModels.BudgetAllocationModel>();
        }

        public async Task<ProcurementSearchModels.CostAnalysisModel> GetCostAnalysisAsync(DateTime fromDate, DateTime toDate)
        {
            await Task.CompletedTask;
            return new ProcurementSearchModels.CostAnalysisModel();
        }

        public async Task<ProcurementRequest> CreateFromInventoryTriggerAsync(int inventoryItemId, string userId)
        {
            var inventoryItem = await _context.InventoryItems.FindAsync(inventoryItemId);
            if (inventoryItem == null)
                throw new ArgumentException("Inventory item not found", nameof(inventoryItemId));

            var procurement = new ProcurementRequest
            {
                Title = $"Restock {inventoryItem.Name}",
                Description = $"Automatic restock trigger for {inventoryItem.Name}",
                Department = "IT",
                ProcurementType = ProcurementType.Consumables,
                Category = ProcurementCategory.ITEquipment,
                Method = ProcurementMethod.DirectPurchase,
                TriggeredByInventoryItemId = inventoryItemId,
                RequestedByUserId = userId,
                EstimatedBudget = 0m
            };

            return await CreateProcurementRequestAsync(procurement, userId);
        }

        public async Task<ProcurementRequest> CreateFromAssetReplacementAsync(int assetId, string userId)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null)
                throw new ArgumentException("Asset not found", nameof(assetId));

            var procurement = new ProcurementRequest
            {
                Title = $"Replace {asset.Name}",
                Description = $"Replacement procurement for asset {asset.AssetTag}",
                Department = "IT",
                ProcurementType = ProcurementType.Equipment,
                Category = ProcurementCategory.ITEquipment,
                Method = ProcurementMethod.DirectPurchase,
                ReplacementForAssetId = assetId,
                RequestedByUserId = userId,
                EstimatedBudget = 0m
            };

            return await CreateProcurementRequestAsync(procurement, userId);
        }

        public async Task<bool> LinkToRequestAsync(int procurementId, int requestId, string userId)
        {
            var procurement = await GetProcurementRequestByIdAsync(procurementId);
            if (procurement == null) return false;

            procurement.OriginatingRequestId = requestId;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
