using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalAssetTracker.Models
{
    public enum WriteOffReason
    {
        EndOfLife,
        BeyondRepair,
        Obsolete,
        Lost,
        Stolen,
        Damaged,
        Compliance,
        Upgrade,
        Other
    }

    public enum WriteOffMethod
    {
        Donation,
        Recycling,
        Destruction,
        Resale,
        Trade,
        Other
    }

    public enum WriteOffStatus
    {
        Pending,
        UnderReview,
        Approved,
        Rejected,
        Processed,
        Cancelled
    }

    public class WriteOffRecord
    {
        public int Id { get; set; }

        [Required]
        public int AssetId { get; set; }
        public virtual Asset Asset { get; set; } = null!;

        [Required]
        public WriteOffReason Reason { get; set; }

        [Required]
        public WriteOffMethod Method { get; set; }

        [Required]
        public WriteOffStatus Status { get; set; } = WriteOffStatus.Pending;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        // Additional properties required by views and controllers
        [Required]
        [StringLength(2000)]
        public string Justification { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Notes { get; set; }

        [StringLength(500)]
        public string? DisposalMethod { get; set; }

        public DateTime? DisposalDate { get; set; }

        // Auto-generated write-off number
        [Required]
        [StringLength(50)]
        public string WriteOffNumber { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? AdditionalNotes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? EstimatedValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SalvageValue { get; set; }

        [StringLength(100)]
        public string? DisposalVendor { get; set; }

        [StringLength(500)]
        public string? CertificateOfDestruction { get; set; }

        // Request Information
        public string RequestedByUserId { get; set; } = string.Empty;
        public virtual ApplicationUser RequestedByUser { get; set; } = null!;

        public DateTime RequestDate { get; set; }

        // Review Information
        public string? ReviewedByUserId { get; set; }
        public virtual ApplicationUser? ReviewedByUser { get; set; }

        public DateTime? ReviewDate { get; set; }

        [StringLength(1000)]
        public string? ReviewNotes { get; set; }

        // Approval Information
        public string? ApprovedByUserId { get; set; }
        public virtual ApplicationUser? ApprovedByUser { get; set; }

        public DateTime? ApprovalDate { get; set; }

        [StringLength(1000)]
        public string? ApprovalNotes { get; set; }

        // Processing Information
        public string? ProcessedByUserId { get; set; }
        public virtual ApplicationUser? ProcessedByUser { get; set; }

        public DateTime? ProcessingDate { get; set; }

        [StringLength(1000)]
        public string? ProcessingNotes { get; set; }

        // Audit Trail
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdated { get; set; }

        // Computed Properties for approval workflow
        public bool IsApproved => Status == WriteOffStatus.Approved;
        public bool IsRejected => Status == WriteOffStatus.Rejected;
        public bool IsPending => Status == WriteOffStatus.Pending;
        public bool IsProcessed => Status == WriteOffStatus.Processed;

        // Computed Properties
        public string StatusDisplayName => Status switch
        {
            WriteOffStatus.Pending => "Pending Review",
            WriteOffStatus.UnderReview => "Under Review",
            WriteOffStatus.Approved => "Approved",
            WriteOffStatus.Rejected => "Rejected",
            WriteOffStatus.Processed => "Processed",
            WriteOffStatus.Cancelled => "Cancelled",
            _ => Status.ToString()
        };

        public string ReasonDisplayName => Reason switch
        {
            WriteOffReason.EndOfLife => "End of Life",
            WriteOffReason.BeyondRepair => "Beyond Repair",
            WriteOffReason.Obsolete => "Obsolete",
            WriteOffReason.Lost => "Lost",
            WriteOffReason.Stolen => "Stolen",
            WriteOffReason.Damaged => "Damaged",
            WriteOffReason.Compliance => "Compliance Issue",
            WriteOffReason.Upgrade => "Upgrade",
            WriteOffReason.Other => "Other",
            _ => Reason.ToString()
        };
    }

    public class WriteOffSummary
    {
        public int TotalWriteOffs { get; set; }
        public int PendingRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int ProcessedRequests { get; set; }
        public int RejectedRequests { get; set; }
        public decimal TotalEstimatedValue { get; set; }
        public decimal TotalSalvageValue { get; set; }
        public Dictionary<WriteOffReason, int> ReasonBreakdown { get; set; } = new();
        public Dictionary<string, int> MonthlyTrends { get; set; } = new();
    }
}
