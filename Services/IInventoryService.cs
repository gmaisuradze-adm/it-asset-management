using HospitalAssetTracker.Models;
using static HospitalAssetTracker.Models.InventorySearchModels;

namespace HospitalAssetTracker.Services
{
    public interface IInventoryService
    {
        // Basic CRUD Operations
        Task<InventoryItem?> GetInventoryItemByIdAsync(int id);
        Task<InventoryItem?> GetInventoryItemByItemCodeAsync(string itemCode);
        Task<IEnumerable<InventoryItem>> GetAllInventoryItemsAsync();
        Task<PagedResult<InventoryItem>> GetInventoryItemsPagedAsync(InventorySearchCriteria criteria);
        Task<InventoryItem> CreateInventoryItemAsync(InventoryItem item, string userId);
        Task<InventoryItem> UpdateInventoryItemAsync(InventoryItem item, string userId);
        Task<bool> DeleteInventoryItemAsync(int id, string userId);
        Task<bool> IsItemCodeUniqueAsync(string itemCode, int? excludeId = null);

        // Stock Management
        Task<bool> AdjustStockAsync(int itemId, int adjustmentQuantity, string reason, string userId);
        Task<bool> ReserveStockAsync(int itemId, int quantity, string reason, string userId);
        Task<bool> ReleaseReservationAsync(int itemId, int quantity, string reason, string userId);
        Task<bool> AllocateStockAsync(int itemId, int quantity, string reason, string userId);
        Task<bool> SetStockLevelsAsync(int itemId, int minStock, int maxStock, int reorderLevel, string userId);

        // Movement Operations
        Task<bool> TransferInventoryAsync(int itemId, int quantity, int fromLocationId, int toLocationId, 
            string reason, string userId, string? fromZone = null, string? toZone = null,
            string? fromShelf = null, string? toShelf = null, string? fromBin = null, string? toBin = null);
        Task<bool> StockInAsync(int itemId, int quantity, decimal? unitCost, string supplier, 
            string reason, string userId, string? purchaseOrderNumber = null, string? invoiceNumber = null);
        Task<bool> StockOutAsync(int itemId, int quantity, string reason, string userId);
        Task<IEnumerable<InventoryMovement>> GetInventoryMovementHistoryAsync(int itemId);
        Task<IEnumerable<InventoryMovement>> GetRecentMovementsAsync(int days = 30);

        // Asset Integration
        Task<bool> DeployToAssetAsync(int assetId, int inventoryItemId, int quantity, string reason, string userId);
        Task<bool> ReturnFromAssetAsync(int assetId, int inventoryItemId, int quantity, string reason, string userId);
        Task<IEnumerable<AssetInventoryMapping>> GetAssetInventoryMappingsAsync(int assetId);
        Task<IEnumerable<AssetInventoryMapping>> GetInventoryAssetMappingsAsync(int inventoryItemId);
        Task<bool> ReplaceAssetComponentAsync(int assetId, int oldInventoryItemId, int newInventoryItemId, string reason, string userId);

        // Transactions
        Task<InventoryTransaction> CreateTransactionAsync(InventoryTransaction transaction, string userId);
        Task<IEnumerable<InventoryTransaction>> GetTransactionHistoryAsync(int itemId);
        Task<IEnumerable<InventoryTransaction>> GetTransactionsByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<InventoryTransaction>> GetPurchaseTransactionsAsync(DateTime? fromDate = null, DateTime? toDate = null);

        // Bulk Operations
        Task<bool> BulkUpdateStatusAsync(List<int> itemIds, InventoryStatus newStatus, string reason, string userId);
        Task<bool> BulkUpdateLocationAsync(List<int> itemIds, int newLocationId, string reason, string userId);
        Task<bool> BulkUpdateConditionAsync(List<int> itemIds, InventoryCondition newCondition, string reason, string userId);
        Task<bool> BulkAdjustStockAsync(List<(int ItemId, int AdjustmentQuantity)> adjustments, string reason, string userId);

        // Reporting and Analytics
        Task<InventoryStockReport> GetStockReportAsync();
        Task<InventoryMovementReport> GetMovementReportAsync(DateTime fromDate, DateTime toDate);
        Task<InventoryValuationReport> GetValuationReportAsync();
        Task<IEnumerable<StockLevelAlert>> GetStockLevelAlertsAsync();
        Task<IEnumerable<ExpiryAlert>> GetExpiryAlertsAsync(int daysAhead = 30);
        Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync();
        Task<IEnumerable<InventoryItem>> GetCriticalStockItemsAsync();
        Task<IEnumerable<InventoryItem>> GetOverstockedItemsAsync();
        Task<IEnumerable<InventoryItem>> GetExpiredItemsAsync();
        Task<IEnumerable<InventoryItem>> GetItemsNearingExpiryAsync(int daysAhead = 30);

        // Search and Filtering
        Task<IEnumerable<InventoryItem>> SearchInventoryAsync(string searchTerm);
        Task<IEnumerable<InventoryItem>> GetInventoryByLocationAsync(int locationId);
        Task<IEnumerable<InventoryItem>> GetInventoryByCategoryAsync(InventoryCategory category);
        Task<IEnumerable<InventoryItem>> GetInventoryByStatusAsync(InventoryStatus status);
        Task<IEnumerable<InventoryItem>> GetInventoryBySupplierAsync(string supplier);
        Task<IEnumerable<InventoryItem>> GetConsumableItemsAsync();
        Task<IEnumerable<InventoryItem>> GetItemsRequiringCalibrationAsync();

        // Stock Level Management
        Task<bool> UpdateStockLevelsAsync(int itemId, int quantity, string userId);
        Task<bool> ValidateStockAvailabilityAsync(int itemId, int requiredQuantity);
        Task<int> GetAvailableStockAsync(int itemId);
        Task<int> GetReservedStockAsync(int itemId);
        Task<int> GetAllocatedStockAsync(int itemId);

        // Quality Control
        Task<bool> MarkQualityCheckedAsync(int transactionId, bool passed, string notes, string userId);
        Task<IEnumerable<InventoryTransaction>> GetPendingQualityChecksAsync();

        // Calibration Management
        Task<bool> ScheduleCalibrationAsync(int itemId, DateTime calibrationDate, string userId);
        Task<bool> CompleteCalibrationAsync(int itemId, DateTime calibrationDate, string certificateNumber, string userId);
        Task<IEnumerable<InventoryItem>> GetItemsDueForCalibrationAsync(int daysAhead = 30);

        // Location and Storage
        Task<bool> UpdateStorageLocationAsync(int itemId, string? zone, string? shelf, string? bin, string userId);
        Task<IEnumerable<InventoryItem>> GetItemsByStorageLocationAsync(int locationId, string? zone = null, 
            string? shelf = null, string? bin = null);
        Task<IEnumerable<Location>> GetAllInventoryLocationsAsync(); // New method

        // Data Export and Import
        Task<byte[]> ExportInventoryToExcelAsync(InventorySearchCriteria? criteria = null);
        Task<byte[]> ExportMovementHistoryToExcelAsync(int itemId);
        Task<byte[]> ExportStockReportToExcelAsync();
        Task<bool> ImportInventoryFromExcelAsync(byte[] fileData, string userId);

        // Maintenance and Cleanup
        Task<int> CleanupOldMovementsAsync(int daysToKeep = 365);
        Task<int> CleanupOldTransactionsAsync(int daysToKeep = 365);
        Task<bool> RecalculateInventoryValuesAsync();
        // Task<bool> UpdateAllTotalValuesAsync(); // This might be redundant with RecalculateInventoryValuesAsync or part of it.

        // Dashboard and Statistics
        Task<InventoryDashboardData> GetInventoryDashboardDataAsync(); // Return type already updated in a previous step if DashboardModels.cs was processed first.
        // The following can be part of GetInventoryDashboardDataAsync or separate if needed for other specific use cases.
        // Task<Dictionary<InventoryCategory, int>> GetInventoryCountByCategoryAsync();
        // Task<Dictionary<InventoryStatus, int>> GetInventoryCountByStatusAsync();
        // Task<decimal> GetTotalInventoryValueAsync();
        // Task<int> GetTotalInventoryItemsAsync();
        // Task<int> GetUniqueInventoryItemsAsync();
        
        // Additional methods for Request Service integration
        Task<bool> CheckAvailabilityAndReserveAsync(string itemCode, int quantity, int requestId, string userId); // Modified for reservation
        Task<bool> ReleaseReservedStockForRequestAsync(int requestId, string userId); // New method
        Task<InventoryItem?> GetItemBySKUAsync(string sku); // New method for SKU lookup

        // Methods for Procurement Integration
        Task<bool> ReceiveStockFromProcurementAsync(int inventoryItemId, int procurementActivityId, int quantityReceived, decimal unitCost, DateTime receivedDate, string userId, string? batchNumber = null, DateTime? expiryDate = null);

        // Methods for Asset Module Integration (some might be duplicates or need refinement from existing ones)
        Task<bool> AssignComponentToAssetAsync(int assetId, int inventoryItemId, int quantity, string? serialNumber, DateTime installationDate, string userId);
        Task<bool> RemoveComponentFromAssetAsync(int assetId, int inventoryItemId, int quantity, DateTime removalDate, string reason, string userId);
        Task<IEnumerable<InventoryItem>> GetCompatibleComponentsAsync(int assetId); // Or based on asset type/model

        // Stock availability checking
        Task<bool> CheckAvailabilityAsync(int inventoryItemId, int requiredQuantity);
        Task<bool> CheckAvailabilityAsync(string itemName, int requiredQuantity);

        // Additional stock operations
        Task<bool> UpdateInventoryQuantityAsync(int itemId, int newQuantity, string userId);
    }
}
