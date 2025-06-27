using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Services
{
    public interface IRequestService
    {
        // Basic CRUD operations
        Task<PagedResult<ITRequest>> GetRequestsAsync(RequestSearchModel searchModel, string userId);
        Task<ITRequest?> GetRequestByIdAsync(int requestId);
        Task<ITRequest> CreateRequestAsync(ITRequest request, string userId);
        Task<ITRequest> UpdateRequestAsync(ITRequest request, string userId);
        Task<string> GenerateRequestNumberAsync();

        // Assignment and status changes
        Task<bool> AssignRequestAsync(int requestId, string assignedToUserId, string currentUserId);
        Task<bool> CompleteRequestAsync(int requestId, string completedById, string? completionNotes = null);
        Task<bool> CancelRequestAsync(int requestId, string userId, string? reason = null);
        Task<bool> PlaceRequestOnHoldAsync(int requestId, string userId, string reason);
        Task<bool> ResumeRequestAsync(int requestId, string userId, string? comments);
        Task<bool> UpdateRequestStatusAsync(int requestId, RequestStatus newStatus, string userId, string? notes = null);
        Task AddActivityAsync(int requestId, string userId, string description);

        // Dashboard and reporting
        Task<RequestDashboardData> GetRequestDashboardDataAsync();
        Task<List<ITRequest>> GetOverdueRequestsAsync();
        Task<List<ITRequest>> GetMyRequestsAsync(string userId);
        Task<List<ITRequest>> GetAssignedRequestsAsync(string userId);

        // Notification counts
        Task<int> GetPendingRequestsCountAsync();
        Task<int> GetMyActiveRequestsCountAsync(string userId);

        // New methods for UI support
        Task<IEnumerable<ApplicationUser>> GetAssignableITStaffAsync();
        Task<IEnumerable<InventoryItem>> GetRelevantInventoryItemsAsync(string? category = null, string? searchTerm = null);

        // New methods for asset replacement workflow
        Task ProcessAssetReplacementFromInventoryAsync(int requestId, int replacementInventoryItemId, int? damagedAssetIdToUpdate, string? disposalNotesForUnmanagedAsset, string userId);
        Task ProcessAssetReplacementViaProcurementAsync(int requestId, int? damagedAssetIdToUpdate, string? disposalNotesForUnmanagedAsset, string userId);
    }
}
