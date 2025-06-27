
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalAssetTracker.Models
{
    public class ProcurementItem
    {
        public int Id { get; set; }

        [Required]
        public int ProcurementRequestId { get; set; }
        public virtual ProcurementRequest ProcurementRequest { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(2000)]
        public string? TechnicalSpecifications { get; set; }

        [Required]
        public int Quantity { get; set; }

        [StringLength(50)]
        public string? Unit { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal EstimatedUnitPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? ActualUnitPrice { get; set; }

        // Compatibility properties for existing code
        [NotMapped]
        public decimal UnitPrice
        {
            get => ActualUnitPrice ?? EstimatedUnitPrice;
            set => ActualUnitPrice = value;
        }

        [NotMapped]
        public decimal TotalPrice
        {
            get => Quantity * UnitPrice;
            set
            {
                if (Quantity > 0)
                    UnitPrice = value / Quantity;
            }
        }

        [NotMapped]
        public decimal EstimatedTotalPrice => Quantity * EstimatedUnitPrice;

        [NotMapped]
        public decimal? ActualTotalPrice => ActualUnitPrice.HasValue ? Quantity * ActualUnitPrice.Value : null;

        // Integration with Inventory Module
        public int? ExpectedInventoryItemId { get; set; }
        public virtual InventoryItem? ExpectedInventoryItem { get; set; }

        public int? ReceivedInventoryItemId { get; set; }
        public virtual InventoryItem? ReceivedInventoryItem { get; set; }

        // Delivery tracking
        public int QuantityReceived { get; set; } = 0;
        public DateTime? FirstDeliveryDate { get; set; }
        public DateTime? LastDeliveryDate { get; set; }

        [NotMapped]
        public bool IsFullyDelivered => QuantityReceived >= Quantity;

        [NotMapped]
        public int PendingQuantity => Math.Max(0, Quantity - QuantityReceived);
    }

    public class ProcurementItemReceived
    {
        public int ProcurementItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrderedQuantity { get; set; }
        public int ReceivedQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Category { get; set; }
        public bool IsAsset { get; set; }
        public int? AssetCategoryId { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public string? SupplierName { get; set; }
        public string? PurchaseOrderNumber { get; set; }
    }

    public class Vendor
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(50)]
        public string? Phone { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? TaxNumber { get; set; }

        [StringLength(50)]
        public string? RegistrationNumber { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsApproved { get; set; } = false;
        public VendorStatus Status { get; set; } = VendorStatus.Active;

        // Performance metrics
        public decimal PerformanceRating { get; set; } = 0;
        public int TotalOrders { get; set; } = 0;
        public int OnTimeDeliveries { get; set; } = 0;
        public int QualityIssues { get; set; } = 0;

        // Additional rating properties for service compatibility
        public decimal ReliabilityRating { get; set; } = 0;
        public decimal DeliveryRating { get; set; } = 0;
        public decimal QualityRating { get; set; } = 0;
        public decimal ComplianceRating { get; set; } = 0;
        public decimal FinancialStability { get; set; } = 0;

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? LastUpdatedDate { get; set; }

        // Navigation properties
        public virtual ICollection<VendorQuote> Quotes { get; set; } = new List<VendorQuote>();
        public virtual ICollection<ProcurementRequest> ProcurementRequests { get; set; } = new List<ProcurementRequest>();

        [NotMapped]
        public decimal OnTimeDeliveryRate => TotalOrders > 0 ? (decimal)OnTimeDeliveries / TotalOrders * 100 : 0;

        [NotMapped]
        public decimal QualityRate => TotalOrders > 0 ? (decimal)(TotalOrders - QualityIssues) / TotalOrders * 100 : 100;
    }

    public class VendorQuote
    {
        public int Id { get; set; }

        [Required]
        public int ProcurementRequestId { get; set; }
        public virtual ProcurementRequest ProcurementRequest { get; set; } = null!;

        [Required]
        public int VendorId { get; set; }
        public virtual Vendor Vendor { get; set; } = null!;

        [StringLength(100)]
        public string? QuoteNumber { get; set; }

        [Required]
        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? TaxAmount { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? DiscountAmount { get; set; }

        [Required]
        public DateTime QuoteDate { get; set; }

        public DateTime? ValidUntilDate { get; set; }

        public int DeliveryDays { get; set; }

        [StringLength(100)]
        public string? PaymentTerms { get; set; }

        [StringLength(100)]
        public string? WarrantyTerms { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }

        public bool IsSelected { get; set; } = false;

        [StringLength(500)]
        public string? DocumentPath { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public virtual ICollection<QuoteItem> Items { get; set; } = new List<QuoteItem>();

        [NotMapped]
        public bool IsValid => !ValidUntilDate.HasValue || ValidUntilDate >= DateTime.UtcNow;

        [NotMapped]
        public decimal FinalAmount => TotalAmount + (TaxAmount ?? 0) - (DiscountAmount ?? 0);
    }

    public class QuoteItem
    {
        public int Id { get; set; }

        [Required]
        public int VendorQuoteId { get; set; }
        public virtual VendorQuote VendorQuote { get; set; } = null!;

        [Required]
        public int ProcurementItemId { get; set; }
        public virtual ProcurementItem ProcurementItem { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        public int Quantity { get; set; }

        [StringLength(500)]
        public string? ItemDescription { get; set; }

        [StringLength(100)]
        public string? Brand { get; set; }

        [StringLength(100)]
        public string? Model { get; set; }

        [StringLength(1000)]
        public string? Specifications { get; set; }

        [NotMapped]
        public decimal TotalPrice => Quantity * UnitPrice;
    }

    public class ProcurementApproval
    {
        public int Id { get; set; }

        [Required]
        public int ProcurementRequestId { get; set; }
        public virtual ProcurementRequest ProcurementRequest { get; set; } = null!;

        [Required]
        public ProcurementApprovalLevel ApprovalLevel { get; set; }

        [Required]
        [StringLength(450)]
        public string ApproverId { get; set; } = string.Empty;
        public virtual ApplicationUser Approver { get; set; } = null!;
        
        // Additional property for compatibility
        public virtual ApplicationUser ApprovedByUser 
        { 
            get => Approver; 
            set => Approver = value; 
        }

        [Required]
        public ProcurementApprovalStatus Status { get; set; } = ProcurementApprovalStatus.Pending;

        [StringLength(1000)]
        public string? Comments { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? DecisionDate { get; set; }

        [Required]
        public int Sequence { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? ApprovedAmount { get; set; }

        // Compatibility properties
        [NotMapped]
        public DateTime? ApprovalDate
        {
            get => DecisionDate;
            set => DecisionDate = value;
        }

        [NotMapped]
        public ProcurementApprovalStatus ApprovalStatus
        {
            get => Status;
            set => Status = value;
        }
    }

    public class ProcurementDocument
    {
        public int Id { get; set; }

        [Required]
        public int ProcurementRequestId { get; set; }
        public virtual ProcurementRequest ProcurementRequest { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string DocumentName { get; set; } = string.Empty;

        [Required]
        public ProcurementDocumentType DocumentType { get; set; }

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ContentType { get; set; }

        public long FileSize { get; set; }

        [Required]
        [StringLength(450)]
        public string UploadedByUserId { get; set; } = string.Empty;
        public virtual ApplicationUser UploadedByUser { get; set; } = null!;

        [Required]
        public DateTime UploadedDate { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class ProcurementActivity
    {
        public int Id { get; set; }

        [Required]
        public int ProcurementRequestId { get; set; }
        public virtual ProcurementRequest ProcurementRequest { get; set; } = null!;

        [Required]
        [StringLength(450)]
        public string ActionByUserId { get; set; } = string.Empty;
        public virtual ApplicationUser ActionByUser { get; set; } = null!;

        [Required]
        public DateTime ActionDate { get; set; }

        [Required]
        public ProcurementActivityType ActivityType { get; set; }

        [StringLength(1000)]
        public string? ActivityDetails { get; set; }

        public ProcurementStatus? FromStatus { get; set; }
        public ProcurementStatus? ToStatus { get; set; }

        // Compatibility properties for existing code
        [NotMapped]
        public string? Description
        {
            get => ActivityDetails;
            set => ActivityDetails = value;
        }

        [NotMapped]
        public DateTime ActivityDate
        {
            get => ActionDate;
            set => ActionDate = value;
        }

        [NotMapped]
        public string UserId
        {
            get => ActionByUserId;
            set => ActionByUserId = value;
        }
    }
}
