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
        public int UnderRepairAssets { get; set; }
        public int MaintenanceAssets { get; set; }
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
}
