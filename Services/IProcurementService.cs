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

        // Approval workflow
        Task<bool> SubmitForApprovalAsync(int procurementId, string userId);
        Task<bool> ApproveProcurementAsync(int procurementId, string approverId, string? comments = null);
        Task<bool> ReceiveProcurementAsync(int procurementId, string receivedById, List<ProcurementItemReceived> receivedItems);

        // Dashboard and reporting
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

        // Vendor management
        Task<List<Vendor>> GetVendorsAsync();
        Task<Vendor> CreateVendorAsync(Vendor vendor, string userId);
    }
}
