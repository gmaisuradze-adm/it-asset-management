using System.ComponentModel.DataAnnotations;

namespace HospitalAssetTracker.Models
{
    public class InventorySearchModels
    {
        public class InventorySearchCriteria
        {
            public string? SearchTerm { get; set; }
            public InventoryCategory? Category { get; set; }
            public InventoryItemType? ItemType { get; set; }
            public InventoryStatus? Status { get; set; }
            public InventoryCondition? Condition { get; set; }
            public int? LocationId { get; set; }
            public string? Brand { get; set; }
            public string? Model { get; set; }
            public bool? IsLowStock { get; set; }
            public bool? IsCriticalStock { get; set; }
            public bool? IsConsumable { get; set; }
            public bool? RequiresCalibration { get; set; }
            public DateTime? CreatedFrom { get; set; }
            public DateTime? CreatedTo { get; set; }
            public DateTime? PurchaseDateFrom { get; set; }
            public DateTime? PurchaseDateTo { get; set; }
            public DateTime? WarrantyExpiryFrom { get; set; }
            public DateTime? WarrantyExpiryTo { get; set; }
            public decimal? MinUnitCost { get; set; }
            public decimal? MaxUnitCost { get; set; }
            public int? MinQuantity { get; set; }
            public int? MaxQuantity { get; set; }
            public string? Supplier { get; set; }
            public string? StorageZone { get; set; }
            public int PageNumber { get; set; } = 1;
            public int PageSize { get; set; } = 50;
            public string SortBy { get; set; } = "Name";
            public string SortOrder { get; set; } = "asc";
        }

        public class InventoryStockReport
        {
            public int TotalItems { get; set; }
            public int TotalQuantity { get; set; }
            public decimal TotalValue { get; set; }
            public int LowStockItems { get; set; }
            public int CriticalStockItems { get; set; }
            public int OverstockedItems { get; set; }
            public int ItemsNearingExpiry { get; set; }
            public int ExpiredItems { get; set; }
            public Dictionary<InventoryCategory, int> ItemsByCategory { get; set; } = new();
            public Dictionary<InventoryStatus, int> ItemsByStatus { get; set; } = new();
            public Dictionary<InventoryCondition, int> ItemsByCondition { get; set; } = new();
        }

        public class InventoryMovementReport
        {
            public DateTime ReportDate { get; set; }
            public int TotalMovements { get; set; }
            public int StockInMovements { get; set; }
            public int StockOutMovements { get; set; }
            public int TransferMovements { get; set; }
            public int AssetDeployments { get; set; }
            public int AssetReturns { get; set; }
            public decimal TotalValueIn { get; set; }
            public decimal TotalValueOut { get; set; }
            public List<InventoryMovementSummary> MovementsByCategory { get; set; } = new();
            public List<InventoryMovementSummary> MovementsByLocation { get; set; } = new();
        }

        public class InventoryMovementSummary
        {
            public string Category { get; set; } = string.Empty;
            public int MovementCount { get; set; }
            public int TotalQuantity { get; set; }
            public decimal TotalValue { get; set; }
        }

        public class InventoryValuationReport
        {
            public DateTime ReportDate { get; set; }
            public decimal TotalInventoryValue { get; set; }
            public decimal NewItemsValue { get; set; }
            public decimal RefurbishedItemsValue { get; set; }
            public decimal UsedItemsValue { get; set; }
            public decimal ConsumablesValue { get; set; }
            public List<InventoryValuationByCategory> ValueByCategory { get; set; } = new();
            public List<InventoryValuationByLocation> ValueByLocation { get; set; } = new();
            public List<InventoryValuationBySupplier> ValueBySupplier { get; set; } = new();
        }

        public class InventoryValuationByCategory
        {
            public InventoryCategory Category { get; set; }
            public int ItemCount { get; set; }
            public int TotalQuantity { get; set; }
            public decimal TotalValue { get; set; }
            public decimal AverageUnitCost { get; set; }
        }

        public class InventoryValuationByLocation
        {
            public string LocationName { get; set; } = string.Empty;
            public int ItemCount { get; set; }
            public int TotalQuantity { get; set; }
            public decimal TotalValue { get; set; }
        }

        public class InventoryValuationBySupplier
        {
            public string SupplierName { get; set; } = string.Empty;
            public int ItemCount { get; set; }
            public int TotalQuantity { get; set; }
            public decimal TotalValue { get; set; }
            public DateTime? LastPurchaseDate { get; set; }
        }

        // StockLevelAlert class moved to WarehouseModels.cs to avoid ambiguity

        public class ExpiryAlert
        {
            public int InventoryItemId { get; set; }
            public string ItemCode { get; set; } = string.Empty;
            public string ItemName { get; set; } = string.Empty;
            public InventoryCategory Category { get; set; }
            public DateTime? WarrantyExpiry { get; set; }
            public DateTime? CalibrationDue { get; set; }
            public int DaysUntilExpiry { get; set; }
            public string AlertType { get; set; } = string.Empty; // "Expired", "ExpiringSoon", "CalibrationDue"
            public string LocationName { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal? TotalValue { get; set; }
        }

        public class InventoryTransferRequest
        {
            public int Id { get; set; }

            [Required]
            public int InventoryItemId { get; set; }

            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
            public int Quantity { get; set; }

            [Required]
            public int FromLocationId { get; set; }

            [Required]
            public int ToLocationId { get; set; }

            public string? FromZone { get; set; }
            public string? ToZone { get; set; }
            public string? FromShelf { get; set; }
            public string? ToShelf { get; set; }
            public string? FromBin { get; set; }
            public string? ToBin { get; set; }

            [Required]
            [StringLength(500)]
            public string Reason { get; set; } = string.Empty;

            public string? Notes { get; set; }
            public DateTime? RequestedDate { get; set; }
            public string? ReferenceNumber { get; set; }
        }

        public class AssetDeploymentRequest
        {
            public int Id { get; set; }

            [Required]
            public int AssetId { get; set; }

            [Required]
            public int InventoryItemId { get; set; }

            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
            public int Quantity { get; set; }

            [Required]
            [StringLength(500)]
            public string DeploymentReason { get; set; } = string.Empty;

            public string? Notes { get; set; }
            public DateTime? ScheduledDate { get; set; }
            public string? ReferenceNumber { get; set; }
        }

        public class StockAdjustmentRequest
        {
            public int Id { get; set; }

            [Required]
            public int InventoryItemId { get; set; }

            [Required]
            public int AdjustmentQuantity { get; set; } // Can be positive or negative

            [Required]
            [StringLength(500)]
            public string Reason { get; set; } = string.Empty;

            public string? Notes { get; set; }
            public string? ReferenceNumber { get; set; }
            public bool RequiresApproval { get; set; }
        }

        public class BulkInventoryOperation
        {
            [Required]
            public List<int> InventoryItemIds { get; set; } = new();

            [Required]
            public string OperationType { get; set; } = string.Empty; // "Transfer", "StatusChange", "Disposal", etc.

            public int? NewLocationId { get; set; }
            public InventoryStatus? NewStatus { get; set; }
            public InventoryCondition? NewCondition { get; set; }

            [Required]
            [StringLength(500)]
            public string Reason { get; set; } = string.Empty;

            public string? Notes { get; set; }
            public string? ReferenceNumber { get; set; }
        }
    }
}
