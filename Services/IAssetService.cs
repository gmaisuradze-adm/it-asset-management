using HospitalAssetTracker.Models;
using Microsoft.AspNetCore.Mvc;

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
        Task<bool> UpdateAssetStatusAsync(int assetId, AssetStatus status, string userId);
        Task<bool> AssignAssetAsync(int assetId, string userId, string assignedByUserId);
        Task<bool> UnassignAssetAsync(int assetId, string userId);
        Task<IEnumerable<AssetMovement>> GetAssetMovementHistoryAsync(int assetId);
        Task<bool> IsAssetTagUniqueAsync(string assetTag, int? excludeId = null);
        Task<IEnumerable<Asset>> GetAssetsForMaintenanceAsync();
        
        // Pagination methods
        Task<PagedResult<Asset>> GetAssetsPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, AssetCategory? category = null, AssetStatus? status = null, int? locationId = null);
        Task<PagedResult<Asset>> GetActiveAssetsPagedAsync(int pageNumber, int pageSize);
        
        // Advanced Search
        Task<AssetSearchResult> AdvancedSearchAsync(AdvancedAssetSearchModel searchModel);
        Task<List<object>> GetSearchSuggestionsAsync(string term, string type = "all");
        
        // Asset Health & Statistics
        Task<AssetHealthDashboard> GetAssetHealthDashboardAsync();
        Task<IEnumerable<Asset>> GetAssetsNeedingAttentionAsync();
        
        // New extended functionality
        Task<Asset> CloneAssetAsync(int sourceAssetId, string userId);
        Task<bool> DecommissionAssetAsync(int assetId, string reason, string userId);
        Task<bool> WriteOffAssetAsync(int assetId, string reason, string userId);
        Task<bool> AttachDocumentAsync(int assetId, string documentPath, string userId);
        Task<bool> AttachImageAsync(int assetId, string imagePath, string userId);
        Task<bool> RemoveDocumentAsync(int assetId, string documentPath, string userId);
        Task<bool> RemoveImageAsync(int assetId, string imagePath, string userId);

        // Maintenance
        Task ScheduleMaintenanceAsync(MaintenanceRecord record, string userId);

        // Methods called by AssetsController but missing from interface
        Task<IEnumerable<Location>> GetActiveLocationsAsync();
        Task<PagedResult<Asset>> GetAssetsAsync(AssetSearchModel searchModel); // Overload for AssetSearchModel
        Task<byte[]> GetAssetQRCodeAsync(int assetId);
        Task<bool> GenerateAssetQRCodeAsync(int assetId);
        Task<IEnumerable<ApplicationUser>> GetActiveUsersAsync(); // Added based on AssetService implementation

        Task<string> GenerateAssetTagAsync(); // Added for generating unique asset tags

        // Asset tag management
        Task<Asset?> GetLatestAssetByPrefixAsync(string prefix);
        Task<bool> AssetTagExistsAsync(string assetTag);
        
        // Location management
        Task<Location> CreateLocationAsync(Location location, string userId);
        Task<Location> CreateLocationAsync(string name, string? description = null, 
            string? building = null, string? floor = null, string? room = null);
        
        // Advanced Search and Management
        Task<HospitalAssetTracker.Models.BulkOperationResult> ProcessBulkOperationAsync(BulkOperationModel operationModel);
        Task<FileResult?> ExportAssetsAsync(AssetExportModel exportModel);
        Task<AssetComparisonModel> CompareAssetsAsync(List<int> assetIds);
        Task<List<string>> GetDepartmentsAsync();
        Task<List<string>> GetSuppliersAsync();
    }
}
