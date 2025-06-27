using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalAssetTracker.Models
{
    public enum AssetCategory
    {
        Desktop,
        Laptop,
        Printer,
        Scanner,
        Monitor,
        Keyboard,
        Mouse,
        Speaker,
        Television,
        NetworkDevice,
        Server,
        MedicalDevice,
        Other
    }

    public enum AssetStatus
    {
        Available,          // In inventory, ready for assignment
        InUse,              // Assigned to a user or location (canonical status)
        UnderMaintenance,   // In repair or scheduled maintenance (canonical status)
        MaintenancePending, // Maintenance scheduled but not started
        InTransit,          // Being moved between locations
        Reserved,           // Reserved for a future deployment
        Lost,               // Asset is lost
        Stolen,             // Asset has been stolen
        Decommissioned,     // Retired from service (canonical status for write-offs)
        PendingApproval     // Awaiting approval for a status change or new asset
    }

    public class Asset
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string AssetTag { get; set; } = string.Empty;

        [Required]
        public AssetCategory Category { get; set; }

        [Required]
        [StringLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Model { get; set; } = string.Empty;

        [StringLength(100)]
        public string SerialNumber { get; set; } = string.Empty;

        // System-assigned unique internal serial number
        [Required]
        [StringLength(50)]
        public string InternalSerialNumber { get; set; } = string.Empty;

        // QR Code for quick access
        [StringLength(200)]
        public string QRCodeData { get; set; } = string.Empty;

        // Document and image paths
        [StringLength(2000)]
        public string? DocumentPaths { get; set; } // JSON array of document file paths

        [StringLength(2000)]
        public string? ImagePaths { get; set; } // JSON array of image file paths

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        // Compatibility property for Name
        [NotMapped]
        public string Name => $"{Brand} {Model}".Trim();

        // Compatibility properties for legacy code
        [NotMapped]
        public DateTime? WarrantyEndDate
        {
            get => WarrantyExpiry;
            set => WarrantyExpiry = value;
        }

        public DateTime InstallationDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastUpdated { get; set; }

        [Required]
        public AssetStatus Status { get; set; }

        // Add AssetCategoryId to be compatible with ProcurementService
        [NotMapped]
        public int AssetCategoryId { get => (int)Category; set => Category = (AssetCategory)value; }

        // Location Information
        public int? LocationId { get; set; }
        public virtual Location? Location { get; set; }

        // Assignment Information
        public string? AssignedToUserId { get; set; }
        public virtual ApplicationUser? AssignedToUser { get; set; }

        [StringLength(100)]
        public string? ResponsiblePerson { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        // Warranty Information
        public DateTime? WarrantyExpiry { get; set; }

        [StringLength(100)]
        public string? Supplier { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PurchasePrice { get; set; }

        // Maintenance Information
        public DateTime? LastMaintenanceDate { get; set; }

        // Additional Properties
        [StringLength(1000)]
        public string? Notes { get; set; }

        // Asset lifecycle dates
        public DateTime? AcquisitionDate { get; set; }

        // Computed Properties
        [NotMapped]
        public bool IsCritical => Category == AssetCategory.Server || 
                                  Category == AssetCategory.NetworkDevice || 
                                  Category == AssetCategory.MedicalDevice;

        // Concurrency Control
        [Timestamp]
        public byte[]? RowVersion { get; set; }

        // Navigation Properties
        public virtual ICollection<AssetMovement> Movements { get; set; } = new List<AssetMovement>();
        public virtual ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public virtual ICollection<WriteOffRecord> WriteOffRecords { get; set; } = new List<WriteOffRecord>();
        public virtual ICollection<QualityAssessmentRecord> QualityAssessments { get; set; } = new List<QualityAssessmentRecord>();
    }
}
