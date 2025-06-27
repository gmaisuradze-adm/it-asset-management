using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Services
{
    public interface IProcurementService
    {
        // Basic CRUD operations
        Task<PagedResult<ProcurementRequest>> GetProcurementRequestsAsync(ProcurementSearchModel searchModel);
        Task<ProcurementRequest?> GetProcurementRequestByIdAsync(int procurementId);
        Task<ProcurementRequest> CreateProcurementRequestAsync(ProcurementRequest procurement, string userId);
        Task<ProcurementRequest> CreateProcurementFromRequestAsync(int requestId, string userId);
        Task<ProcurementRequest> UpdateProcurementRequestAsync(ProcurementRequest procurement, string userId);

        // Advanced Search and Filtering
        Task<PagedResult<ProcurementSearchModels.AdvancedProcurementSearchResult>> GetProcurementRequestsAdvancedAsync(ProcurementSearchModels.AdvancedProcurementSearchModel searchModel);
        Task<IEnumerable<ProcurementSearchModels.AdvancedProcurementSearchResult>> SearchProcurementRequestsAsync(string searchTerm, int maxResults = 50);

        // Approval workflow
        Task<bool> SubmitForApprovalAsync(int procurementId, string userId);
        Task<bool> ApproveProcurementAsync(int procurementId, string approverId, string? comments = null);
        Task<bool> ReceiveProcurementAsync(int procurementId, string receivedById, List<ProcurementItemReceived> receivedItems);

        // Enhanced Approval Methods
        Task<ProcurementSearchModels.ApprovalChainModel> GetApprovalChainAsync(int procurementId);
        Task<bool> ProcessApprovalStepAsync(int procurementId, string approverId, bool approve, string? comments = null);
        Task<IEnumerable<ProcurementSearchModels.AdvancedProcurementSearchResult>> GetPendingApprovalsForUserAsync(string userId);

        // Bulk Operations
        Task<ProcurementSearchModels.BulkOperationResult> BulkApproveProcurementsAsync(ProcurementSearchModels.BulkApprovalRequest request, string userId);
        Task<ProcurementSearchModels.BulkOperationResult> BulkUpdateProcurementsAsync(ProcurementSearchModels.BulkProcurementUpdateRequest request, string userId);
        Task<ProcurementSearchModels.BulkOperationResult> BulkOperationProcurementsAsync(ProcurementSearchModels.BulkProcurementOperationRequest request, string userId);

        // Quick Filter Methods
        Task<IEnumerable<ProcurementSearchModels.AdvancedProcurementSearchResult>> GetPendingApprovalRequestsAsync();
        Task<IEnumerable<ProcurementSearchModels.AdvancedProcurementSearchResult>> GetOverdueProcurementRequestsAsync();
        Task<IEnumerable<ProcurementSearchModels.AdvancedProcurementSearchResult>> GetEmergencyProcurementsAsync();
        Task<IEnumerable<ProcurementSearchModels.AdvancedProcurementSearchResult>> GetHighValueProcurementsAsync();
        Task<IEnumerable<ProcurementSearchModels.AdvancedProcurementSearchResult>> GetRecentProcurementRequestsAsync();
        Task<IEnumerable<ProcurementSearchModels.AdvancedProcurementSearchResult>> GetUserProcurementRequestsAsync(string userId);

        // Export functionality
        Task<byte[]?> ExportProcurementDataAsync(ProcurementSearchModels.ProcurementExportRequest request);

        // Analytics and Reporting
        Task<ProcurementSearchModels.ProcurementAnalyticsModel> GetProcurementAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<ProcurementSearchModels.ProcurementReport> GenerateProcurementReportAsync(string reportType, DateTime fromDate, DateTime toDate);

        // Dashboard and reporting (existing methods)
        Task<ProcurementDashboardData> GetProcurementDashboardDataAsync();
        Task<List<ProcurementRequest>> GetOverdueProcurementsAsync();
        Task<int> GetActiveRequestsCountAsync();
        Task<int> GetPendingApprovalsCountAsync();
        Task<int> GetActiveVendorsCountAsync();
        Task<decimal> GetMonthlySpendAsync();
        Task<List<ProcurementRequest>> GetRecentRequestsAsync(int count = 10);
        Task<List<ProcurementApproval>> GetRecentApprovalsAsync(int count = 5);
        Task<List<Vendor>> GetActiveVendorsAsync();
        Task<List<List<string>>> GetExportDataAsync(string reportType);

        // Enhanced Vendor Management
        Task<List<Vendor>> GetVendorsAsync();
        Task<Vendor> CreateVendorAsync(Vendor vendor, string userId);
        Task<ProcurementSearchModels.VendorEvaluationModel> GetVendorEvaluationAsync(int vendorId);
        Task<ProcurementSearchModels.VendorPerformanceMetrics> GetVendorPerformanceAsync(int vendorId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<ProcurementSearchModels.VendorComparisonModel> CompareVendorsAsync(List<int> vendorIds, string criteria);
        Task<IEnumerable<Vendor>> GetRecommendedVendorsAsync(ProcurementType procurementType, ProcurementCategory category);

        // Budget Management
        Task<ProcurementSearchModels.BudgetAllocationModel> GetBudgetAllocationAsync(string budgetCode);
        Task<bool> ValidateBudgetAvailabilityAsync(string budgetCode, decimal amount);
        Task<IEnumerable<ProcurementSearchModels.BudgetAllocationModel>> GetDepartmentBudgetsAsync(string department);
        Task<ProcurementSearchModels.CostAnalysisModel> GetCostAnalysisAsync(DateTime fromDate, DateTime toDate);

        // Integration Methods
        Task<ProcurementRequest> CreateFromInventoryTriggerAsync(int inventoryItemId, string userId);
        Task<ProcurementRequest> CreateFromAssetReplacementAsync(int assetId, string userId);
        Task<bool> LinkToRequestAsync(int procurementId, int requestId, string userId);
    }
}
