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

        // Enhanced Advanced Search Operations
        Task<PagedResult<AdvancedInventorySearchResult>> GetInventoryItemsAdvancedAsync(AdvancedInventorySearchModel searchModel);
        Task<IEnumerable<AdvancedInventorySearchResult>> SearchInventoryItemsAsync(string searchTerm, int maxResults = 50);
        Task<IEnumerable<InventoryQuickFilterModel>> GetQuickFiltersAsync();
        Task<int> GetInventoryCountAsync(AdvancedInventorySearchModel searchModel);

        // Bulk Operations
        Task<BulkOperationResult> ExecuteBulkOperationAsync(BulkInventoryOperationModel operationModel, string userId);
        Task<IEnumerable<AdvancedInventorySearchResult>> GetItemsForBulkOperationAsync(List<int> itemIds);
        Task<bool> ValidateBulkOperationAsync(BulkInventoryOperationModel operationModel);

        // Export Operations
        Task<byte[]> ExportInventoryToExcelAsync(AdvancedInventorySearchModel searchModel, bool includeDetails = true);
        Task<byte[]> ExportInventoryToPdfAsync(AdvancedInventorySearchModel searchModel, bool includeDetails = true);
        Task<byte[]> ExportInventoryToCsvAsync(AdvancedInventorySearchModel searchModel);

        // Stock Management
        Task<bool> AdjustStockAsync(int itemId, int adjustmentQuantity, string reason, string userId);
        Task<bool> ReserveStockAsync(int itemId, int quantity, string reason, string userId);
        Task<bool> ReleaseReservationAsync(int itemId, int quantity, string reason, string userId);
        Task<bool> AllocateStockAsync(int itemId, int quantity, string reason, string userId);
        Task<bool> SetStockLevelsAsync(int itemId, int minStock, int maxStock, int reorderLevel, string userId);

        // Movement Operations
        Task<bool> TransferInventoryAsync(InventoryTransferRequest transferRequest, string userId);
        Task<bool> StockInAsync(StockInRequest stockInRequest, string userId);
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

        // Procurement Integration
        Task<bool> UpdateInventoryFromProcurementAsync(ProcurementItemReceived receivedItem, string userId);

        // Dashboard and Alerts
        Task<InventoryDashboardData> GetInventoryDashboardDataAsync();
        Task<IEnumerable<StockLevelAlert>> GetStockLevelAlertsAsync();
        Task<IEnumerable<InventoryExpiryAlert>> GetExpiryAlertsAsync();

        // Advanced Stock Operations
        Task<bool> CheckAvailabilityAsync(int itemId, int quantity);
        Task<InventoryReservationResult> CheckAvailabilityAndReserveAsync(int itemId, int quantity, string reason, string userId);
        Task<bool> UpdateInventoryQuantityAsync(int itemId, int newQuantity, string reason, string userId);
        Task<bool> ReceiveStockFromProcurementAsync(int inventoryItemId, int quantity, decimal unitCost, string supplier, string purchaseOrderNumber, string userId);

        // Analytics and Reporting
        Task<InventoryAnalyticsSummary> GetInventoryAnalyticsSummaryAsync();
        Task<IEnumerable<InventoryTrendData>> GetInventoryTrendsAsync(int months = 12);
        Task<IEnumerable<InventoryAlertSummary>> GetInventoryAlertSummaryAsync();

        // Advanced bulk operations
        Task<InventorySearchModels.BulkOperationResult> BulkUpdateInventoryAsync(InventorySearchModels.BulkInventoryUpdateRequest request, string userId);
        
        // Export functionality
        Task<byte[]?> ExportInventoryAsync(InventorySearchModels.InventoryExportRequest request);
        
        // Quick filter methods
        Task<IEnumerable<InventorySearchModels.AdvancedInventorySearchResult>> GetLowStockItemsAsync();
        Task<IEnumerable<InventorySearchModels.AdvancedInventorySearchResult>> GetOutOfStockItemsAsync();
        Task<IEnumerable<InventorySearchModels.AdvancedInventorySearchResult>> GetExpiringSoonItemsAsync();
        Task<IEnumerable<InventorySearchModels.AdvancedInventorySearchResult>> GetHighValueItemsAsync();
        Task<IEnumerable<InventorySearchModels.AdvancedInventorySearchResult>> GetRecentlyAddedItemsAsync();
    }

    // Supporting models for enhanced operations
    public class BulkOperationResult
    {
        public bool Success { get; set; }
        public int TotalItems { get; set; }
        public int SuccessfulItems { get; set; }
        public int FailedItems { get; set; }
        public List<string> ErrorMessages { get; set; } = new();
        public List<string> WarningMessages { get; set; } = new();
        public string? Summary { get; set; }
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
        public string ExecutedBy { get; set; } = string.Empty;
    }

    public class InventoryAnalyticsSummary
    {
        public int TotalItems { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
        public int LowStockItems { get; set; }
        public int CriticalStockItems { get; set; }
        public int OverstockedItems { get; set; }
        public int ExpiringWarrantyItems { get; set; }
        public decimal AverageStockTurnover { get; set; }
        public Dictionary<string, int> ItemsByCategory { get; set; } = new();
        public Dictionary<string, decimal> ValueByCategory { get; set; } = new();
        public Dictionary<string, int> ItemsByStatus { get; set; } = new();
        public Dictionary<string, int> ItemsByLocation { get; set; } = new();
    }

    public class InventoryTrendData
    {
        public DateTime Date { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalValue { get; set; }
        public int StockInQuantity { get; set; }
        public int StockOutQuantity { get; set; }
        public decimal StockInValue { get; set; }
        public decimal StockOutValue { get; set; }
        public int LowStockCount { get; set; }
        public int CriticalStockCount { get; set; }
    }

    public class InventoryAlertSummary
    {
        public string AlertType { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }
}
