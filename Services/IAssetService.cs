using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Services
{
    public interface IAssetService
    {
        Task<IEnumerable<Asset>> GetAllAssetsAsync();
        Task<IEnumerable<Asset>> GetActiveAssetsAsync();
        Task<IEnumerable<Asset>> GetWriteOffAssetsAsync();
        Task<Asset?> GetAssetByIdAsync(int id);
        Task<Asset?> GetAssetByTagAsync(string assetTag);
        Task<IEnumerable<Asset>> SearchAssetsAsync(string searchTerm);
        Task<IEnumerable<Asset>> GetAssetsByLocationAsync(int locationId);
        Task<IEnumerable<Asset>> GetAssetsByUserAsync(string userId);
        Task<IEnumerable<Asset>> GetAssetsByStatusAsync(AssetStatus status);
        Task<IEnumerable<Asset>> GetAssetsByCategoryAsync(AssetCategory category);
        Task<Asset> CreateAssetAsync(Asset asset, string userId);
        Task<Asset> UpdateAssetAsync(Asset asset, string userId);
        Task<bool> DeleteAssetAsync(int id, string userId);
        Task<bool> MoveAssetAsync(int assetId, int? newLocationId, string? newUserId, string reason, string performedByUserId);
        Task<bool> ChangeAssetStatusAsync(int assetId, AssetStatus newStatus, string reason, string userId);
        Task<bool> AssignAssetAsync(int assetId, string userId, string assignedByUserId);
        Task<bool> UnassignAssetAsync(int assetId, string userId);
        Task<IEnumerable<AssetMovement>> GetAssetMovementHistoryAsync(int assetId);
        Task<bool> IsAssetTagUniqueAsync(string assetTag, int? excludeId = null);
        Task<IEnumerable<Asset>> GetAssetsForMaintenanceAsync();
        Task<IEnumerable<Asset>> GetExpiredWarrantyAssetsAsync();
        
        // Pagination methods
        Task<PagedResult<Asset>> GetAssetsPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, AssetCategory? category = null, AssetStatus? status = null, int? locationId = null);
        Task<PagedResult<Asset>> GetActiveAssetsPagedAsync(int pageNumber, int pageSize);
        
        // Advanced Search
        Task<PagedResult<Asset>> AdvancedSearchAsync(AssetSearchCriteria criteria, int pageNumber, int pageSize);
        
        // Asset Health & Statistics
        Task<AssetHealthDashboard> GetAssetHealthDashboardAsync();
        Task<IEnumerable<Asset>> GetAssetsNeedingAttentionAsync();
        
        // New extended functionality
        Task<Asset> CloneAssetAsync(int sourceAssetId, string userId);
        Task<bool> BulkUpdateStatusAsync(List<int> assetIds, AssetStatus newStatus, string reason, string userId);
        Task<bool> BulkUpdateLocationAsync(List<int> assetIds, int? newLocationId, string reason, string userId);
        Task<bool> BulkAssignAsync(List<int> assetIds, string assignedToUserId, string assignedByUserId);
        Task<bool> DecommissionAssetAsync(int assetId, string reason, string userId);
        Task<bool> WriteOffAssetAsync(int assetId, string reason, string userId);
        Task<bool> AttachDocumentAsync(int assetId, string documentPath, string userId);
        Task<bool> AttachImageAsync(int assetId, string imagePath, string userId);
        Task<bool> RemoveDocumentAsync(int assetId, string documentPath, string userId);
        Task<bool> RemoveImageAsync(int assetId, string imagePath, string userId);
        Task<List<string>> GetAssetDocumentsAsync(int assetId);
        Task<List<string>> GetAssetImagesAsync(int assetId);
        Task<bool> GenerateAssetQRCodeAsync(int assetId);
        Task<byte[]> GetAssetQRCodeAsync(int assetId);
        Task<IEnumerable<Location>> GetActiveLocationsAsync();
        Task<IEnumerable<ApplicationUser>> GetActiveUsersAsync();
        
        // Bulk Export
        Task<byte[]> ExportAssetsToExcelAsync(List<int>? assetIds = null);
        Task<byte[]> ExportAssetsToCsvAsync(List<int>? assetIds = null);
        Task<byte[]> ExportAssetsWithFiltersAsync(AssetSearchCriteria criteria, string format = "excel");
        
        // Additional methods for Request Service integration
        Task<PagedResult<Asset>> GetAssetsAsync(AssetSearchModel searchModel);
        Task<bool> UpdateAssetStatusAsync(int assetId, AssetStatus status, string userId);
        Task<string> GenerateAssetTagAsync();
    }
}
