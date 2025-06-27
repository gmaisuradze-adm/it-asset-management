using System.ComponentModel.DataAnnotations;

namespace HospitalAssetTracker.Models
{
    public class InventorySearchModels
    {
        // Enhanced Advanced Search Model for comprehensive inventory filtering
        public class AdvancedInventorySearchModel
        {
            // Basic Search
            [Display(Name = "Search Term")]
            public string? SearchTerm { get; set; }

            [Display(Name = "Item Code")]
            public string? ItemCode { get; set; }

            [Display(Name = "Category")]
            public InventoryCategory? Category { get; set; }

            [Display(Name = "Item Type")]
            public InventoryItemType? ItemType { get; set; }

            [Display(Name = "Status")]
            public InventoryStatus? Status { get; set; }

            [Display(Name = "Condition")]
            public InventoryCondition? Condition { get; set; }

            [Display(Name = "Location")]
            public int? LocationId { get; set; }

            [Display(Name = "Storage Zone")]
            public string? StorageZone { get; set; }

            [Display(Name = "Storage Shelf")]
            public string? StorageShelf { get; set; }

            [Display(Name = "Brand")]
            public string? Brand { get; set; }

            [Display(Name = "Model")]
            public string? Model { get; set; }

            [Display(Name = "Supplier")]
            public string? Supplier { get; set; }

            // Stock Level Filters
            [Display(Name = "Stock Level Filter")]
            public StockLevelFilter? StockLevelFilter { get; set; }

            [Display(Name = "ABC Classification")]
            public string? AbcClassification { get; set; }

            [Display(Name = "Minimum Quantity")]
            [Range(0, int.MaxValue, ErrorMessage = "Minimum quantity must be non-negative")]
            public int? MinQuantity { get; set; }

            [Display(Name = "Maximum Quantity")]
            [Range(0, int.MaxValue, ErrorMessage = "Maximum quantity must be non-negative")]
            public int? MaxQuantity { get; set; }

            // Value Filters
            [Display(Name = "Minimum Unit Cost")]
            [Range(0, double.MaxValue, ErrorMessage = "Minimum cost must be non-negative")]
            public decimal? MinUnitCost { get; set; }

            [Display(Name = "Maximum Unit Cost")]
            [Range(0, double.MaxValue, ErrorMessage = "Maximum cost must be non-negative")]
            public decimal? MaxUnitCost { get; set; }

            [Display(Name = "Minimum Total Value")]
            [Range(0, double.MaxValue, ErrorMessage = "Minimum value must be non-negative")]
            public decimal? MinTotalValue { get; set; }

            [Display(Name = "Maximum Total Value")]
            [Range(0, double.MaxValue, ErrorMessage = "Maximum value must be non-negative")]
            public decimal? MaxTotalValue { get; set; }

            // Date Filters
            [Display(Name = "Created From")]
            [DataType(DataType.Date)]
            public DateTime? CreatedFrom { get; set; }

            [Display(Name = "Created To")]
            [DataType(DataType.Date)]
            public DateTime? CreatedTo { get; set; }

            [Display(Name = "Purchase Date From")]
            [DataType(DataType.Date)]
            public DateTime? PurchaseDateFrom { get; set; }

            [Display(Name = "Purchase Date To")]
            [DataType(DataType.Date)]
            public DateTime? PurchaseDateTo { get; set; }

            [Display(Name = "Warranty Expiry From")]
            [DataType(DataType.Date)]
            public DateTime? WarrantyExpiryFrom { get; set; }

            [Display(Name = "Warranty Expiry To")]
            [DataType(DataType.Date)]
            public DateTime? WarrantyExpiryTo { get; set; }

            // Boolean Filters
            [Display(Name = "Show Consumables Only")]
            public bool? IsConsumable { get; set; }

            [Display(Name = "Requires Calibration")]
            public bool? RequiresCalibration { get; set; }

            [Display(Name = "Has Serial Number")]
            public bool? HasSerialNumber { get; set; }

            [Display(Name = "Has Warranty")]
            public bool? HasWarranty { get; set; }

            // Pagination and Sorting
            public int PageNumber { get; set; } = 1;
            public int PageSize { get; set; } = 50;
            public string SortBy { get; set; } = "Name";
            public string SortOrder { get; set; } = "asc";

            // Quick Filter Presets
            public bool ShowLowStockOnly { get; set; }
            public bool ShowCriticalStockOnly { get; set; }
            public bool ShowOverstockedOnly { get; set; }
            public bool ShowExpiringWarrantyOnly { get; set; }
            public bool ShowExpiredWarrantyOnly { get; set; }
            public bool ShowHighValueOnly { get; set; }

            // UI State
            public bool AdvancedFiltersExpanded { get; set; }
            public string? SavedSearchName { get; set; }
        }

        // Enhanced Search Result with additional computed properties
        public class AdvancedInventorySearchResult
        {
            public int Id { get; set; }
            public string ItemCode { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public InventoryCategory Category { get; set; }
            public InventoryItemType ItemType { get; set; }
            public InventoryStatus Status { get; set; }
            public InventoryCondition Condition { get; set; }
            public string Brand { get; set; } = string.Empty;
            public string Model { get; set; } = string.Empty;
            public string? SerialNumber { get; set; }
            public int Quantity { get; set; }
            public int ReservedQuantity { get; set; }
            public int AvailableQuantity => Quantity - ReservedQuantity;
            public int MinimumStock { get; set; }
            public int MaximumStock { get; set; }
            public int ReorderLevel { get; set; }
            public decimal? UnitCost { get; set; }
            public decimal? TotalValue { get; set; }
            public string? Supplier { get; set; }
            public DateTime? PurchaseDate { get; set; }
            public DateTime? WarrantyExpiry { get; set; }
            public string LocationName { get; set; } = string.Empty;
            public string? StorageLocation { get; set; }
            public string? AbcClassification { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? LastUpdatedDate { get; set; }
            public string CreatedByUserName { get; set; } = string.Empty;
            public string? LastUpdatedByUserName { get; set; }

            // Computed Properties
            public string StockLevel
            {
                get
                {
                    if (Quantity == 0) return "Out of Stock";
                    if (Quantity <= MinimumStock) return "Critical";
                    if (Quantity <= ReorderLevel) return "Low";
                    if (Quantity >= MaximumStock) return "Overstocked";
                    return "Normal";
                }
            }

            public string StockLevelClass
            {
                get
                {
                    return StockLevel switch
                    {
                        "Out of Stock" => "badge-danger",
                        "Critical" => "badge-danger",
                        "Low" => "badge-warning",
                        "Overstocked" => "badge-info",
                        _ => "badge-success"
                    };
                }
            }

            public bool IsExpired => WarrantyExpiry.HasValue && WarrantyExpiry.Value < DateTime.UtcNow;
            public bool IsExpiringWarranty => WarrantyExpiry.HasValue &&
                WarrantyExpiry.Value >= DateTime.UtcNow &&
                WarrantyExpiry.Value <= DateTime.UtcNow.AddDays(30);

            public int DaysUntilWarrantyExpiry => WarrantyExpiry.HasValue
                ? (int)(WarrantyExpiry.Value - DateTime.UtcNow).TotalDays
                : int.MaxValue;

            public string ValueDisplayString => TotalValue?.ToString("C") ?? "N/A";
            public string UnitCostDisplayString => UnitCost?.ToString("C") ?? "N/A";
        }

        // Bulk Operations Model
        public class BulkInventoryOperationModel
        {
            [Required]
            public List<int> SelectedItemIds { get; set; } = new();

            [Required]
            [Display(Name = "Operation Type")]
            public BulkOperationType OperationType { get; set; }

            // For bulk updates
            [Display(Name = "New Status")]
            public InventoryStatus? NewStatus { get; set; }

            [Display(Name = "New Condition")]
            public InventoryCondition? NewCondition { get; set; }

            [Display(Name = "New Location")]
            public int? NewLocationId { get; set; }

            [Display(Name = "New Storage Zone")]
            public string? NewStorageZone { get; set; }

            [Display(Name = "New Storage Shelf")]
            public string? NewStorageShelf { get; set; }

            [Display(Name = "New ABC Classification")]
            public string? NewAbcClassification { get; set; }

            // For stock adjustments
            [Display(Name = "Quantity Adjustment")]
            public int? QuantityAdjustment { get; set; }

            [Display(Name = "Adjustment Type")]
            public StockAdjustmentType? AdjustmentType { get; set; }

            [Display(Name = "Reason")]
            [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
            public string? Reason { get; set; }

            // For transfers
            [Display(Name = "Transfer to Location")]
            public int? TransferToLocationId { get; set; }

            [Display(Name = "Transfer Reason")]
            [StringLength(500, ErrorMessage = "Transfer reason cannot exceed 500 characters")]
            public string? TransferReason { get; set; }

            // For export operations
            [Display(Name = "Export Format")]
            public ExportFormat ExportFormat { get; set; } = ExportFormat.Excel;

            [Display(Name = "Include Details")]
            public bool IncludeDetailedInfo { get; set; } = true;

            [Display(Name = "Include Movement History")]
            public bool IncludeMovementHistory { get; set; }

            [Display(Name = "Include Transaction History")]
            public bool IncludeTransactionHistory { get; set; }
        }

        // Enhanced filter models
        public class InventoryQuickFilterModel
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public AdvancedInventorySearchModel FilterCriteria { get; set; } = new();
            public int ItemCount { get; set; }
            public bool IsSystemFilter { get; set; }
        }

        // Supporting Enums
        public enum StockLevelFilter
        {
            All = 0,
            InStock = 1,
            LowStock = 2,
            CriticalStock = 3,
            OutOfStock = 4,
            Overstocked = 5,
            Normal = 6
        }

        public enum BulkOperationType
        {
            UpdateStatus = 1,
            UpdateCondition = 2,
            UpdateLocation = 3,
            AdjustStock = 4,
            Transfer = 5,
            Export = 6,
            Delete = 7,
            UpdateAbcClassification = 8,
            UpdateStorageLocation = 9
        }

        public enum StockAdjustmentType
        {
            Increase = 1,
            Decrease = 2,
            SetAbsolute = 3
        }

        public enum ExportFormat
        {
            Excel = 1,
            PDF = 2,
            CSV = 3
        }

        // Original models preserved for backward compatibility
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

        // Supporting models for enhanced operations
        public class InventoryTransferRequest
        {
            public int ItemId { get; set; }
            public int Quantity { get; set; }
            public int FromLocationId { get; set; }
            public int ToLocationId { get; set; }
            public string Reason { get; set; } = string.Empty;
            public string? FromZone { get; set; }
            public string? ToZone { get; set; }
            public string? FromShelf { get; set; }
            public string? ToShelf { get; set; }
            public string? FromBin { get; set; }
            public string? ToBin { get; set; }
        }

        public class StockInRequest
        {
            public int ItemId { get; set; }
            public int Quantity { get; set; }
            public decimal? UnitCost { get; set; }
            public string Supplier { get; set; } = string.Empty;
            public string Reason { get; set; } = string.Empty;
            public string? PurchaseOrderNumber { get; set; }
            public string? InvoiceNumber { get; set; }
        }

        public class InventoryReservationResult
        {
            public bool Success { get; set; }
            public int QuantityReserved { get; set; }
            public string Message { get; set; } = string.Empty;
            public DateTime? ReservationExpiry { get; set; }
        }

        public class InventoryExpiryAlert
        {
            public int InventoryItemId { get; set; }
            public string ItemName { get; set; } = string.Empty;
            public string ItemCode { get; set; } = string.Empty;
            public DateTime? WarrantyExpiry { get; set; }
            public int DaysUntilExpiry { get; set; }
            public string AlertType { get; set; } = string.Empty; // "Expiring", "Expired"
            public string LocationName { get; set; } = string.Empty;
            public DateTime CreatedDate { get; set; }
        }

        public class InventoryStockReport
        {
            public DateTime GeneratedDate { get; set; }
            public string GeneratedBy { get; set; } = string.Empty;
            public int TotalItems { get; set; }
            public int TotalQuantity { get; set; }
            public decimal TotalValue { get; set; }
            public List<InventoryStockSummary> StockSummaries { get; set; } = new();
            public List<InventoryStockAlert> StockAlerts { get; set; } = new();
        }

        public class InventoryStockSummary
        {
            public string Category { get; set; } = string.Empty;
            public int ItemCount { get; set; }
            public int TotalQuantity { get; set; }
            public decimal TotalValue { get; set; }
            public int LowStockCount { get; set; }
            public int OutOfStockCount { get; set; }
        }

        public class InventoryStockAlert
        {
            public int InventoryItemId { get; set; }
            public string ItemCode { get; set; } = string.Empty;
            public string ItemName { get; set; } = string.Empty;
            public int CurrentQuantity { get; set; }
            public int ReorderPoint { get; set; }
            public string AlertType { get; set; } = string.Empty; // "Low Stock", "Out of Stock"
            public string Category { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
        }

        public class InventoryMovementReport
        {
            public DateTime GeneratedDate { get; set; }
            public string GeneratedBy { get; set; } = string.Empty;
            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }
            public int TotalMovements { get; set; }
            public int StockInCount { get; set; }
            public int StockOutCount { get; set; }
            public List<InventoryMovementSummary> MovementSummaries { get; set; } = new();
        }

        public class InventoryMovementSummary
        {
            public DateTime MovementDate { get; set; }
            public string MovementType { get; set; } = string.Empty;
            public string ItemCode { get; set; } = string.Empty;
            public string ItemName { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public string Reason { get; set; } = string.Empty;
            public string PerformedBy { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
        }

        public class InventoryValuationReport
        {
            public DateTime GeneratedDate { get; set; }
            public string GeneratedBy { get; set; } = string.Empty;
            public decimal TotalInventoryValue { get; set; }
            public int TotalItemTypes { get; set; }
            public int TotalQuantity { get; set; }
            public List<InventoryValuationSummary> ValuationSummaries { get; set; } = new();
            public List<InventoryTopValueItems> TopValueItems { get; set; } = new();
        }

        public class InventoryValuationSummary
        {
            public string Category { get; set; } = string.Empty;
            public int ItemCount { get; set; }
            public int TotalQuantity { get; set; }
            public decimal TotalValue { get; set; }
            public decimal AverageUnitCost { get; set; }
            public decimal PercentageOfTotalValue { get; set; }
        }

        public class InventoryTopValueItems
        {
            public string ItemCode { get; set; } = string.Empty;
            public string ItemName { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal UnitCost { get; set; }
            public decimal TotalValue { get; set; }
            public string Category { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
        }

        public class BulkInventoryUpdateRequest
        {
            public List<int> ItemIds { get; set; } = new();
            public string UpdateType { get; set; } = string.Empty; // "Status", "Location", "Category", etc.
            public object? UpdateValue { get; set; }
            public string Reason { get; set; } = string.Empty;
        }

        public class InventoryExportRequest
        {
            public List<int>? ItemIds { get; set; }
            public AdvancedInventorySearchModel? SearchCriteria { get; set; }
            public string Format { get; set; } = "Excel"; // "Excel", "PDF", "CSV"
            public bool IncludeMovementHistory { get; set; }
            public bool IncludeTransactionHistory { get; set; }
        }

        public class BulkOperationResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public int AffectedItems { get; set; }
            public List<string> Errors { get; set; } = new();
        }

    }
}
