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

    public class RequestDashboardData
    {
        public int TotalRequests { get; set; }
        public int TotalActiveRequests { get; set; } // Alias for controller compatibility
        public int PendingRequests { get; set; }
        public int PendingApprovals { get; set; } // Alias for controller compatibility
        public int InProgressRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int OverdueRequests { get; set; }
        public int CompletedToday { get; set; }
        public int MyOpenRequests { get; set; } // For logged-in user
        
        public List<RequestSummaryViewModel> HighPriorityRequests { get; set; } = new(); // Changed to ViewModel
        public List<RequestSummaryViewModel> RecentRequests { get; set; } = new(); // Changed to ViewModel
        public List<RequestSummaryViewModel> MyAssignments { get; set; } = new(); // Changed to ViewModel
        
        // Chart data for request trends (e.g., new vs. completed over time)
        public List<string> RequestTrendLabels { get; set; } = new(); // Renamed
        public List<int> NewRequestsTrendData { get; set; } = new(); // Renamed and specified
        public List<int> CompletedRequestsTrendData { get; set; } = new(); // Renamed and specified
        
        // Chart data for requests by type
        public List<string> RequestTypeDistributionLabels { get; set; } = new(); // Renamed
        public List<int> RequestTypeDistributionData { get; set; } = new(); // Renamed

        // Chart data for requests by priority
        public List<string> RequestPriorityDistributionLabels { get; set; } = new();
        public List<int> RequestPriorityDistributionData { get; set; } = new();

        // Chart data for requests by status
        public List<string> RequestStatusDistributionLabels { get; set; } = new();
        public List<int> RequestStatusDistributionData { get; set; } = new();

        // Alias properties for view compatibility
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public List<string> TrendLabels 
        { 
            get => RequestTrendLabels; 
            set => RequestTrendLabels = value; 
        }
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public List<int> TrendData 
        { 
            get => NewRequestsTrendData; 
            set => NewRequestsTrendData = value; 
        }
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public List<string> TypeLabels 
        { 
            get => RequestTypeDistributionLabels; 
            set => RequestTypeDistributionLabels = value; 
        }
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public List<int> TypeData 
        { 
            get => RequestTypeDistributionData; 
            set => RequestTypeDistributionData = value; 
        }

        // Legacy properties for compatibility
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int OpenRequests 
        { 
            get => PendingRequests + InProgressRequests; 
            set { /* Read-only computed property */ } 
        }
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public Dictionary<string, int> RequestsByType { get; set; } = new();
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public Dictionary<string, int> RequestsByPriority { get; set; } = new();
    }

    // ViewModel for displaying request summaries in the dashboard
    public class RequestSummaryViewModel
    {
        public int Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public RequestStatus Status { get; set; }
        public RequestPriority Priority { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public string? AssignedTo { get; set; }
        
        // Additional string properties for service compatibility  
        public string? RequestType { get; set; }
        public string? RequesterName { get; set; }
        public string? AssignedToName { get; set; }
        
        // Alias properties for views that expect User objects
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public RequestedByUser RequestedByUser => new RequestedByUser { FirstName = RequesterName?.Split(' ')[0] ?? "", LastName = RequesterName?.Split(' ').Skip(1).FirstOrDefault() ?? "" };
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public AssignedToUser? AssignedToUser => string.IsNullOrEmpty(AssignedToName) ? null : new AssignedToUser { FirstName = AssignedToName?.Split(' ')[0] ?? "", LastName = AssignedToName?.Split(' ').Skip(1).FirstOrDefault() ?? "" };
        
        // Additional properties expected by views
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string Description => Title; // Use Title as Description
    }

    public class ProcurementDashboardData
    {
        public int TotalOpenProcurementRequests { get; set; } // Renamed from TotalProcurements for clarity
        public int PendingApprovalCount { get; set; } // Renamed from PendingApproval
        public int ApprovedAndProcessingCount { get; set; } // Renamed from InProgress
        public int PartiallyReceivedCount { get; set; }
        public int FullyReceivedCount { get; set; }
        public int CancelledCount { get; set; }

        public decimal TotalOpenOrdersValue { get; set; }
        public decimal TotalSpendingThisMonth { get; set; }
        public decimal AverageProcessingTimeDays { get; set; }

        public Dictionary<string, int> ProcurementsByStatus { get; set; } = new(); // Replaces ProcurementsByType if type refers to status
        public Dictionary<string, decimal> SpendingByVendorLast90Days { get; set; } = new(); // Renamed from TopVendors and specified period
        public List<ProcurementSummaryViewModel> RecentProcurementRequests { get; set; } = new(); // Changed to ViewModel
        public List<ProcurementSummaryViewModel> PendingMyApproval { get; set; } = new(); // For approvers

        // Chart data for procurement trends (e.g., new orders over time, spending over time)
        public List<string> ProcurementTrendLabels { get; set; } = new();
        public List<int> NewProcurementOrdersTrend { get; set; } = new();
        public List<decimal> ProcurementSpendingTrend { get; set; } = new();
        
        // Alias properties for backward compatibility
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int TotalProcurements 
        { 
            get => TotalOpenProcurementRequests; 
            set => TotalOpenProcurementRequests = value; 
        }
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int PendingApproval 
        { 
            get => PendingApprovalCount; 
            set => PendingApprovalCount = value; 
        }
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int InProgress 
        { 
            get => ApprovedAndProcessingCount; 
            set => ApprovedAndProcessingCount = value; 
        }
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int CompletedThisMonth 
        { 
            get => FullyReceivedCount; 
            set => FullyReceivedCount = value; 
        }
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public Dictionary<string, int> ProcurementsByType 
        { 
            get => ProcurementsByStatus; 
            set => ProcurementsByStatus = value; 
        }
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public Dictionary<string, decimal> TopVendors 
        { 
            get => SpendingByVendorLast90Days; 
            set => SpendingByVendorLast90Days = value; 
        }
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public List<ProcurementSummaryViewModel> RecentProcurements 
        { 
            get => RecentProcurementRequests; 
            set => RecentProcurementRequests = value; 
        }
    }

    // ViewModel for displaying procurement summaries
    public class ProcurementSummaryViewModel
    {
        public int Id { get; set; }
        public string ProcurementNumber { get; set; } = string.Empty; // Assuming ProcurementRequest has a number
        public string RequestTitle { get; set; } = string.Empty; // Or a summary of items
        public ProcurementStatus Status { get; set; } // Assuming an enum ProcurementStatus exists
        public DateTime RequestDate { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public decimal EstimatedTotalCost { get; set; }
        public string? AssignedVendor { get; set; }
        
        // Additional properties expected by services
        public string RequestNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string VendorName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string RequesterName { get; set; } = string.Empty;
        
        // Alias properties for backward compatibility
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string StatusString 
        { 
            get => Status.ToString(); 
            set => Status = Enum.Parse<ProcurementStatus>(value); 
        }
    }

    // Helper classes for view compatibility
    public class RequestedByUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
    
    public class AssignedToUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
