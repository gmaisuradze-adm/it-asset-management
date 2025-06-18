using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Services
{
    public interface IRequestService
    {
        // Basic CRUD operations
        Task<PagedResult<ITRequest>> GetRequestsAsync(RequestSearchModel searchModel);
        Task<ITRequest?> GetRequestByIdAsync(int requestId);
        Task<ITRequest> CreateRequestAsync(ITRequest request, string userId);
        Task<ITRequest> UpdateRequestAsync(ITRequest request, string userId);

        // Assignment and approval
        Task<bool> AssignRequestAsync(int requestId, string assignedToUserId, string currentUserId);
        Task<bool> ApproveRequestAsync(int requestId, string approverId, string? comments = null);
        Task<bool> CompleteRequestAsync(int requestId, string completedById, string? completionNotes = null);

        // Dashboard and reporting
        Task<RequestDashboardData> GetRequestDashboardDataAsync();
        Task<List<ITRequest>> GetOverdueRequestsAsync();
        Task<List<ITRequest>> GetMyRequestsAsync(string userId);
        Task<List<ITRequest>> GetAssignedRequestsAsync(string userId);
    }
}
