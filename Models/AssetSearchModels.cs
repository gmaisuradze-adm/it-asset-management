namespace HospitalAssetTracker.Models
{
    public class AssetSearchModel
    {
        public string? SearchTerm { get; set; }
        public AssetCategory? Category { get; set; }
        public AssetStatus? Status { get; set; }
        public int? LocationId { get; set; }
        public string? AssetTagFrom { get; set; }
        public string? AssetTagTo { get; set; }
        public decimal? PriceFrom { get; set; }
        public decimal? PriceTo { get; set; }
        public DateTime? InstallFrom { get; set; }
        public DateTime? InstallTo { get; set; }
        public string? WarrantyStatus { get; set; }
        public string? Department { get; set; }
        public string? Supplier { get; set; }
        public string? ResponsiblePerson { get; set; }
        public string? AssignedToUserId { get; set; }
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1; // Added PageNumber
    }

    public class AssetSearchCriteria
    {
        public string? SearchTerm { get; set; }
        public AssetCategory? Category { get; set; }
        public AssetStatus? Status { get; set; }
        public int? LocationId { get; set; }
        public string? AssetTagFrom { get; set; }
        public string? AssetTagTo { get; set; }
        public decimal? PriceFrom { get; set; }
        public decimal? PriceTo { get; set; }
        public DateTime? InstallFrom { get; set; }
        public DateTime? InstallTo { get; set; }
        public string? WarrantyStatus { get; set; }
        public string? Department { get; set; }
        public string? Supplier { get; set; }
        public string? ResponsiblePerson { get; set; }
        public string? AssignedToUserId { get; set; }
    }

    public class AssetHealthDashboard
    {
        public int TotalAssets { get; set; }
        public int ActiveAssets { get; set; }
        public int InUseAssets { get; set; }
        public int AvailableAssets { get; set; }
        public int UnderMaintenanceAssets { get; set; }
        public int DecommissionedAssets { get; set; }
        
        // Warranty Status
        public int ExpiredWarrantyAssets { get; set; }
        public int ExpiringWarrantyAssets { get; set; } // Next 3 months
        public int NoWarrantyAssets { get; set; }
        
        // Financial
        public decimal TotalValue { get; set; }
        public decimal AverageAssetValue { get; set; }
        
        // Age Analysis
        public int AssetsOlderThan5Years { get; set; }
        public int AssetsOlderThan3Years { get; set; }
        public int AssetsNewerThan1Year { get; set; }
        
        // Categories
        public Dictionary<AssetCategory, int> AssetsByCategory { get; set; } = new();
        public Dictionary<AssetStatus, int> AssetsByStatus { get; set; } = new();
        
        // Location Distribution
        public Dictionary<string, int> AssetsByLocation { get; set; } = new();
        
        // Monthly Trends
        public Dictionary<string, int> AssetsAddedByMonth { get; set; } = new();
        
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public class AssetUtilization
    {
        public int AssetId { get; set; }
        public string AssetTag { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public AssetStatus Status { get; set; }
        public DateTime? LastMovement { get; set; }
        public DateTime? LastMaintenance { get; set; }
        public int MovementCount { get; set; }
        public int MaintenanceCount { get; set; }
        public double UtilizationScore { get; set; }
        public string UtilizationLevel { get; set; } = string.Empty; // High, Medium, Low, Unused
        public string Location { get; set; } = string.Empty;
        public string? AssignedTo { get; set; }
    }

    public class AdvancedAssetSearchModel
    {
        // Text Search
        public string? SearchTerm { get; set; }
        public bool SearchInAssetTag { get; set; } = true;
        public bool SearchInBrand { get; set; } = true;
        public bool SearchInModel { get; set; } = true;
        public bool SearchInSerialNumber { get; set; } = true;
        public bool SearchInDescription { get; set; } = true;

        // Category and Status Filters
        public List<AssetCategory>? Categories { get; set; } = new List<AssetCategory>();
        public List<AssetStatus>? Statuses { get; set; } = new List<AssetStatus>();

        // Location Filters
        public List<int>? LocationIds { get; set; } = new List<int>();
        public string? Building { get; set; }
        public string? Floor { get; set; }
        public string? Department { get; set; }

        // Date Range Filters
        public DateTime? PurchaseDateFrom { get; set; }
        public DateTime? PurchaseDateTo { get; set; }
        public DateTime? InstallationDateFrom { get; set; }
        public DateTime? InstallationDateTo { get; set; }
        public DateTime? WarrantyExpiryFrom { get; set; }
        public DateTime? WarrantyExpiryTo { get; set; }

        // Price Range Filters
        public decimal? PriceFrom { get; set; }
        public decimal? PriceTo { get; set; }

        // Assignment Filters
        public bool? IsAssigned { get; set; }
        public List<string>? AssignedUserIds { get; set; } = new List<string>();
        public string? ResponsiblePerson { get; set; }

        // Warranty Status
        public bool? WarrantyExpired { get; set; }
        public int? WarrantyExpiringInDays { get; set; }

        // Sorting and Pagination
        public string SortBy { get; set; } = "AssetTag";
        public string SortDirection { get; set; } = "asc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;

        // Export Options
        public bool IncludeInactive { get; set; } = false;
        public List<string>? ExportColumns { get; set; } = new List<string>();
    }

    public class AssetSearchResult
    {
        public List<Asset> Assets { get; set; } = new List<Asset>();
        public int TotalCount { get; set; }
        public int FilteredCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)FilteredCount / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
        
        // Search Statistics
        public Dictionary<AssetCategory, int> CategoryCounts { get; set; } = new Dictionary<AssetCategory, int>();
        public Dictionary<AssetStatus, int> StatusCounts { get; set; } = new Dictionary<AssetStatus, int>();
        public Dictionary<string, int> LocationCounts { get; set; } = new Dictionary<string, int>();
        
        public decimal TotalValue { get; set; }
        public int WarrantyExpiringCount { get; set; }
        public int UnassignedCount { get; set; }
    }

    public class BulkOperationModel
    {
        public List<int> AssetIds { get; set; } = new List<int>();
        public string Operation { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
        public string? Reason { get; set; }
    }

    public class BulkOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ProcessedCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<BulkOperationItem> Results { get; set; } = new List<BulkOperationItem>();
    }

    public class BulkOperationItem
    {
        public int AssetId { get; set; }
        public string AssetTag { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? Error { get; set; }
    }

    public class AssetComparisonModel
    {
        public List<Asset> Assets { get; set; } = new List<Asset>();
        public Dictionary<string, List<object?>> ComparisonData { get; set; } = new Dictionary<string, List<object?>>();
        public List<string> DifferentFields { get; set; } = new List<string>();
    }

    public class AssetExportModel
    {
        public string Format { get; set; } = "excel"; // excel, csv, pdf
        public List<string> Columns { get; set; } = new List<string>();
        public AdvancedAssetSearchModel? SearchCriteria { get; set; }
        public bool IncludeImages { get; set; } = false;
        public bool IncludeQRCodes { get; set; } = false;
        public string? Title { get; set; }
        public string? Description { get; set; }
    }
}
