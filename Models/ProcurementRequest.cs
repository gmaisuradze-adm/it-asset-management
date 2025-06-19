using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalAssetTracker.Models
{
    public class ProcurementRequest
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ProcurementNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public ProcurementType ProcurementType { get; set; }

        [Required]
        public ProcurementCategory Category { get; set; }

        [Required]
        public ProcurementStatus Status { get; set; } = ProcurementStatus.Draft;

        [Required]
        public ProcurementMethod Method { get; set; }

        // Source tracking - Integration with Request Module
        public ProcurementSource Source { get; set; } = ProcurementSource.Manual;

        public int? OriginatingRequestId { get; set; }
        public virtual ITRequest? OriginatingRequest { get; set; }

        // Integration with Inventory Module for automatic triggers
        public int? TriggeredByInventoryItemId { get; set; }
        public virtual InventoryItem? TriggeredByInventoryItem { get; set; }

        // Integration with Asset Module for lifecycle-based procurement
        public int? ReplacementForAssetId { get; set; }
        public virtual Asset? ReplacementForAsset { get; set; }

        // Requester information
        [Required]
        [StringLength(450)]
        public string RequestedByUserId { get; set; } = string.Empty;
        public virtual ApplicationUser RequestedByUser { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required]
        public DateTime RequestDate { get; set; }

        public DateTime? RequiredByDate { get; set; }

        // Financial information
        [Required]
        [Column(TypeName = "decimal(12,2)")]
        public decimal EstimatedBudget { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? ApprovedBudget { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? ActualCost { get; set; }

        [StringLength(50)]
        public string? BudgetCode { get; set; }

        [StringLength(20)]
        public string? FiscalYear { get; set; }

        // Approval workflow
        [StringLength(450)]
        public string? ApprovedByUserId { get; set; }
        public virtual ApplicationUser? ApprovedByUser { get; set; }

        public DateTime? ApprovalDate { get; set; }

        // Procurement execution
        [StringLength(450)]
        public string? AssignedToProcurementOfficerId { get; set; }
        public virtual ApplicationUser? AssignedToProcurementOfficer { get; set; }

        public DateTime? ProcurementStartDate { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        public DateTime? ActualDeliveryDate { get; set; }

        // Vendor and contract information
        public int? SelectedVendorId { get; set; }
        public virtual Vendor? SelectedVendor { get; set; }

        [StringLength(100)]
        public string? PurchaseOrderNumber { get; set; }

        [StringLength(100)]
        public string? ContractNumber { get; set; }

        // Quality and receipt
        [StringLength(450)]
        public string? ReceivedByUserId { get; set; }
        public virtual ApplicationUser? ReceivedByUser { get; set; }

        public DateTime? ReceivedDate { get; set; }

        public bool QualityApproved { get; set; } = false;

        [StringLength(450)]
        public string? QualityApprovedByUserId { get; set; }
        public virtual ApplicationUser? QualityApprovedByUser { get; set; }

        public DateTime? QualityApprovalDate { get; set; }

        // Integration completion - Updates to other modules
        public bool AssetRegistered { get; set; } = false; // Updated Asset Module
        public bool InventoryUpdated { get; set; } = false; // Updated Warehouse Module
        public bool RequestFulfilled { get; set; } = false; // Updated Request Module

        // Warranty and support
        public DateTime? WarrantyStartDate { get; set; }
        public DateTime? WarrantyEndDate { get; set; }

        [StringLength(100)]
        public string? WarrantyReference { get; set; }

        [StringLength(1000)]
        public string? SupportDetails { get; set; }

        // Notes and documentation
        [StringLength(2000)]
        public string? SpecificationNotes { get; set; }

        [StringLength(2000)]
        public string? ProcurementNotes { get; set; }

        [StringLength(2000)]
        public string? DeliveryNotes { get; set; }

        // Audit fields
        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? LastUpdatedDate { get; set; }

        [StringLength(450)]
        public string? LastUpdatedByUserId { get; set; }
        public virtual ApplicationUser? LastUpdatedByUser { get; set; }

        // Navigation properties
        public virtual ICollection<ProcurementItem> Items { get; set; } = new List<ProcurementItem>();
        public virtual ICollection<ProcurementApproval> Approvals { get; set; } = new List<ProcurementApproval>();
        public virtual ICollection<VendorQuote> Quotes { get; set; } = new List<VendorQuote>();
        public virtual ICollection<ProcurementDocument> Documents { get; set; } = new List<ProcurementDocument>();
        public virtual ICollection<ProcurementActivity> Activities { get; set; } = new List<ProcurementActivity>();

        // Computed properties
        [NotMapped]
        public string StatusDescription => Status switch
        {
            ProcurementStatus.Draft => "მომზადება",
            ProcurementStatus.PendingApproval => "დამტკიცების მომლოდინე",
            ProcurementStatus.Approved => "დამტკიცებული",
            ProcurementStatus.Rejected => "უარყოფილი",
            ProcurementStatus.InProcurement => "შესყიდვის პროცესში",
            ProcurementStatus.OrderPlaced => "შეკვეთა განთავსებული",
            ProcurementStatus.PartiallyDelivered => "ნაწილობრივ მიღებული",
            ProcurementStatus.Delivered => "მიღებული",
            ProcurementStatus.Completed => "დასრულებული",
            ProcurementStatus.Cancelled => "გაუქმებული",
            _ => "უცნობი"
        };

        [NotMapped]
        public bool RequiresTender => EstimatedBudget > 50000;

        [NotMapped]
        public bool RequiresExecutiveApproval => EstimatedBudget > 5000;

        [NotMapped]
        public int DaysOverdue => ActualDeliveryDate.HasValue ? 0 : 
                                RequiredByDate.HasValue && RequiredByDate < DateTime.UtcNow ? 
                                (DateTime.UtcNow - RequiredByDate.Value).Days : 0;

        // Additional properties for compatibility with existing code
        [NotMapped]
        public string RequestNumber
        {
            get => ProcurementNumber;
            set => ProcurementNumber = value;
        }

        [NotMapped]
        public ApplicationUser? Requester => RequestedByUser;

        [NotMapped]
        public string? RequesterId
        {
            get => RequestedByUserId;
            set => RequestedByUserId = value ?? string.Empty;
        }

        [NotMapped]
        public decimal? EstimatedAmount
        {
            get => EstimatedBudget;
            set => EstimatedBudget = value ?? 0;
        }

        [NotMapped]
        public int? RelatedRequestId
        {
            get => OriginatingRequestId;
            set => OriginatingRequestId = value;
        }

        [StringLength(2000)]
        public string? BusinessJustification { get; set; }

        public bool IsUrgent { get; set; } = false;

        [NotMapped]
        public decimal? TotalAmount
        {
            get => Items?.Sum(i => i.Quantity * i.UnitPrice) ?? EstimatedBudget;
            set => EstimatedBudget = value ?? 0;
        }

        public ProcurementPriority Priority { get; set; } = ProcurementPriority.Medium;

        [NotMapped]
        public DateTime? UpdatedDate
        {
            get => LastUpdatedDate;
            set => LastUpdatedDate = value ?? DateTime.UtcNow;
        }

        // Additional compatibility properties
        [NotMapped]
        public int? VendorId => SelectedVendorId;

        [NotMapped]
        public Vendor? Vendor => SelectedVendor;

        [NotMapped]
        public DateTime? SubmittedDate { get; set; }

        [NotMapped]
        public DateTime? ApprovedDate
        {
            get => ApprovalDate;
            set => ApprovalDate = value;
        }

        // Additional navigation properties
        [NotMapped]
        public ITRequest? RelatedRequest => OriginatingRequest;
    }

    public enum ProcurementType
    {
        Hardware = 1,
        Software = 2,
        Services = 3,
        Consumables = 4,
        Infrastructure = 5,
        Maintenance = 6,
        Training = 7,
        NewEquipment = 8,
        Replacement = 9,
        Equipment = 10,
        Other = 99
    }

    public enum ProcurementCategory
    {
        Desktop = 1,
        Laptop = 2,
        Server = 3,
        NetworkEquipment = 4,
        Printer = 5,
        Monitor = 6,
        SoftwareLicense = 7,
        OperatingSystem = 8,
        SecuritySoftware = 9,
        ProductivitySoftware = 10,
        MaintenanceContract = 11,
        Maintenance = 11, // Alias for MaintenanceContract
        SupportServices = 12,
        ConsultingServices = 13,
        TrainingServices = 14,
        Cables = 15,
        Accessories = 16,
        Consumables = 17,
        ITEquipment = 18,
        Other = 99
    }

    public enum ProcurementStatus
    {
        Draft = 1,
        Pending = 2, // Added for compatibility
        PendingApproval = 2, // Alias for Pending
        Approved = 3,
        Rejected = 4,
        InProcurement = 5,
        InProgress = 5, // Alias for InProcurement
        OrderPlaced = 6,
        Ordered = 6, // Alias for OrderPlaced
        PartiallyDelivered = 7,
        Delivered = 8,
        Received = 9,
        Completed = 10,
        Cancelled = 11
    }

    public enum ProcurementMethod
    {
        DirectPurchase = 1,
        FrameworkAgreement = 2,
        CompetitiveBidding = 3,
        FormalTender = 4,
        EmergencyProcurement = 5
    }

    public enum ProcurementSource
    {
        Manual = 1,
        RequestModule = 2,
        InventoryThreshold = 3,
        AssetLifecycle = 4,
        AutoGenerated = 5
    }

    public enum ProcurementPriority
    {
        Low = 1,
        Medium = 2,
        High = 3
    }
}
