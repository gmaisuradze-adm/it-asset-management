using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalAssetTracker.Models
{
    public class InventoryDashboardData
    {
        public int TotalItems { get; set; }
        // public int AvailableItems { get; set; } // This can be calculated or might be redundant with StatusData
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }
        public decimal TotalValue { get; set; }
        
        public Dictionary<string, int> CategoryDistribution { get; set; } = new(); // Renamed from CategoryData for clarity
        public Dictionary<string, int> StatusDistribution { get; set; } = new(); // Renamed from StatusData for clarity
        
        // Alias properties for view compatibility with private backing fields
        private int _availableItems;
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int AvailableItems 
        { 
            get => _availableItems > 0 ? _availableItems : TotalItems - OutOfStockItems; 
            set => _availableItems = value;
        }
        
        private Dictionary<string, int>? _categoryData;
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public Dictionary<string, int> CategoryData 
        { 
            get => _categoryData ?? CategoryDistribution; 
            set => _categoryData = value;
        }
        
        public List<InventoryMovementViewModel> RecentMovements { get; set; } = new(); // Changed to ViewModel
        public List<InventoryAlertViewModel> LowStockAlerts { get; set; } = new(); // Changed to ViewModel
        
        // Chart data for inventory trends (e.g., total items over time)
        public List<string> InventoryTrendLabels { get; set; } = new(); // Renamed
        public List<int> InventoryTrendData { get; set; } = new(); // Renamed

        // Chart data for item value by category
        public List<string> ValueByCategoryLabels { get; set; } = new();
        public List<decimal> ValueByCategoryData { get; set; } = new();

        // Data for top N most active items (by movement count)
        public List<TopItemViewModel> TopMovingItems { get; set; } = new();
    }

    // ViewModel for displaying recent movements in the dashboard
    public class InventoryMovementViewModel
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public InventoryMovementType MovementType { get; set; }
        public int QuantityChanged { get; set; }
        
        // Alias property for views that expect Quantity
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int Quantity => QuantityChanged;
        
        public DateTime MovementDate { get; set; }
        public string? MovedBy { get; set; }
        public string? Reason { get; set; }
        
        // Location properties for warehouse view compatibility
        public string? ToLocationName { get; set; }
        public string? FromLocationName { get; set; }
        public string? PerformedByUserName { get; set; }
        
        // Alias properties for views that expect different property names
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? ToLocation => ToLocationName;
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? FromLocation => FromLocationName;
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? PerformedByUser => PerformedByUserName ?? MovedBy;
        
        // Alias property for views that expect InventoryItem
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public dynamic InventoryItem => new { Name = ItemName };
    }

    // ViewModel for displaying request summaries
    public class RequestSummaryViewModel
    {
        public int Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty; // Added
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty; // Added
        public string RequestType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string RequesterName { get; set; } = string.Empty;
        public string? AssignedToName { get; set; } // Added
        public DateTime RequestDate { get; set; }
        public DateTime? DueDate { get; set; }
    }

    // ViewModel for displaying recent procurements
    public class RecentProcurementViewModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public string? RequestedBy { get; set; }
        public string? Vendor { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? Date { get; set; }
        public string? SupplierName { get; set; }
        public string? PurchaseOrderNumber { get; set; }
    }

    public class ProcurementDashboardData
    {
        public int TotalProcurements { get; set; }
        public int PendingApprovals { get; set; }
        public int InProgressProcurements { get; set; }
        public int CompletedThisMonth { get; set; }
        public decimal TotalSpendThisMonth { get; set; }

        // Aliases for backward compatibility
        [NotMapped]
        public int PendingApproval => PendingApprovals;
        [NotMapped]
        public int InProgress => InProgressProcurements;
        [NotMapped]
        public decimal TotalSpendingThisMonth => TotalSpendThisMonth;
        [NotMapped]
        public Dictionary<string, int> ProcurementsByType => ProcurementsByCategory;

        public List<RecentProcurementViewModel> RecentProcurements { get; set; } = new();
        public Dictionary<string, int> ProcurementsByCategory { get; set; } = new();
        public Dictionary<string, int> ProcurementsByStatus { get; set; } = new();
        public Dictionary<string, decimal> TopVendorsBySpending { get; set; } = new();
    }

    // ViewModel for displaying low stock alerts
    public class InventoryAlertViewModel
    {
        public int ItemId { get; set; }
        
        // Alias property for views that expect Id
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int Id => ItemId;
        
        public string ItemName { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public int CurrentQuantity { get; set; }
        public int MinimumStock { get; set; }
        public string? Location { get; set; }
        public string? Unit { get; set; }
        
        // Alert specific properties
        public string AlertType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        
        // Additional properties expected by views
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string Name => ItemName;
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int Quantity => CurrentQuantity;
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int MinimumLevel => MinimumStock;
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped] 
        public string Category { get; set; } = string.Empty;
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string Status { get; set; } = string.Empty;
    }

    // ViewModel for Top Moving Items
    public class TopItemViewModel
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public int MovementCount { get; set; }
    }
}
