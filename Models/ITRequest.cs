using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic; // Required for ICollection

namespace HospitalAssetTracker.Models
{
    public class ITRequest
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string RequestNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public RequestType RequestType { get; set; }

        [Required]
        public RequestPriority Priority { get; set; }

        [Required]
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

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

        // Asset relationship - Direct integration with Asset Module
        public int? RelatedAssetId { get; set; }
        public virtual Asset? RelatedAsset { get; set; }
        
        // Alias property for views that expect AssetId
        [NotMapped]
        public int? AssetId 
        { 
            get => RelatedAssetId; 
            set => RelatedAssetId = value; 
        }
        
        // Alias property for views that expect Asset  
        [NotMapped]
        public virtual Asset? Asset 
        { 
            get => RelatedAsset; 
            set => RelatedAsset = value; 
        }
        
        // Properties for requested item details
        [StringLength(200)]
        public string? RequestedItemCategory { get; set; }
        
        [StringLength(2000)]
        public string? RequestedItemSpecifications { get; set; }

        // Location information
        public int? LocationId { get; set; }
        public virtual Location? Location { get; set; }

        // Assignment and processing
        [StringLength(450)]
        public string? AssignedToUserId { get; set; }
        public virtual ApplicationUser? AssignedToUser { get; set; }

        [StringLength(450)]
        public string? ApprovedByUserId { get; set; }
        public virtual ApplicationUser? ApprovedByUser { get; set; }

        public DateTime? ApprovalDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        [StringLength(450)]
        public string? CompletedByUserId { get; set; }
        public virtual ApplicationUser? CompletedByUser { get; set; }

        // Business logic fields
        [StringLength(1000)]
        public string? BusinessJustification { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? EstimatedCost { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? ActualCost { get; set; }

        [StringLength(100)]
        public string? PurchaseOrderNumber { get; set; }

        // Integration with Warehouse Module
        public int? RequiredInventoryItemId { get; set; }
        public virtual InventoryItem? RequiredInventoryItem { get; set; }

        public int? ProvidedInventoryItemId { get; set; }
        public virtual InventoryItem? ProvidedInventoryItem { get; set; }

        // Workflow tracking
        [StringLength(2000)]
        public string? InternalNotes { get; set; }

        [StringLength(2000)]
        public string? CompletionNotes { get; set; }

        [StringLength(500)]
        public string? AttachmentPath { get; set; }

        // Added based on previous analysis and common requirements
        public DateTime? DueDate { get; set; } // Often requested for IT support tickets

        // Lifecycle dates
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        
        // Alias property for services that expect LastUpdatedDate
        [NotMapped]
        public DateTime? LastUpdatedDate 
        { 
            get => LastModifiedDate; 
            set => LastModifiedDate = value; 
        }
        
        // Alias property for services that expect UpdatedDate
        [NotMapped]
        public DateTime? UpdatedDate 
        { 
            get => LastModifiedDate; 
            set => LastModifiedDate = value; 
        }
        
        // CreatedByUserId is already present as RequestedByUserId
        // public string? CreatedByUserId { get; set; } 
        // public virtual ApplicationUser? CreatedByUser { get; set; }
        public string? LastModifiedByUserId { get; set; }
        public virtual ApplicationUser? LastModifiedByUser { get; set; }
        
        // Alias property for services that expect LastUpdatedByUserId
        [NotMapped]
        public string? LastUpdatedByUserId 
        { 
            get => LastModifiedByUserId; 
            set => LastModifiedByUserId = value; 
        }
        
        // Alias property for services that expect Requester
        [NotMapped]
        public virtual ApplicationUser Requester 
        { 
            get => RequestedByUser!; 
            set => RequestedByUser = value; 
        }

        // Alias properties for backward compatibility with services
        [NotMapped]
        public virtual ApplicationUser? AssignedTo
        {
            get => AssignedToUser;
            set => AssignedToUser = value;
        }

        [NotMapped]
        public string? AssignedToId
        {
            get => AssignedToUserId;
            set => AssignedToUserId = value;
        }

        [NotMapped]
        public virtual ApplicationUser? LastUpdatedByUser
        {
            get => LastModifiedByUser;
            set => LastModifiedByUser = value;
        }

        [NotMapped]
        public string? RequesterId
        {
            get => RequestedByUserId;
            set => RequestedByUserId = value!;
        }

        // Additional tracking properties
        [StringLength(2000)]
        public string? AssignmentNotes { get; set; }
        
        public DateTime? ModifiedAt { get; set; }
        
        [StringLength(450)]
        public string? ModifiedBy { get; set; }
        
        // Navigation properties
        public virtual ICollection<RequestApproval> Approvals { get; set; } = new List<RequestApproval>();
        public virtual ICollection<RequestComment> Comments { get; set; } = new List<RequestComment>();
        public virtual ICollection<RequestAttachment> Attachments { get; set; } = new List<RequestAttachment>();
        public virtual ICollection<ProcurementActivity> ProcurementActivities { get; set; } = new List<ProcurementActivity>();
        public virtual ICollection<ProcurementRequest> ProcurementRequests { get; set; } = new List<ProcurementRequest>();
        public virtual ICollection<RequestAction> RequestActions { get; set; } = new List<RequestAction>();

        // Additional fields that might be useful
        // public RequestCategory? Category { get; set; } // e.g., Hardware, Software, Access, Repair
        // public string? ResolutionDetails { get; set; }
        // public TimeSpan? TimeToResolution { get; set; } // Calculated field
        // public int? RelatedTicketId { get; set; } // For linking related tickets
        // public virtual ITRequest? RelatedTicket { get; set; }
        // public bool IsEscalated { get; set; } = false;
        // public DateTime? EscalationDate { get; set; }
        // public string? FeedbackRating { get; set; } // e.g., 1-5 stars
        // public string? FeedbackComments { get; set; }
    }

    public enum RequestType
    {
        HardwareReplacement = 1,
        HardwareRepair = 2,
        NewEquipment = 3,
        SoftwareInstallation = 4,
        SoftwareUpgrade = 5,
        NetworkConnectivity = 6,
        UserAccessRights = 7,
        ITConsultation = 8,
        MaintenanceService = 9,
        Training = 10,
        Other = 99
    }

    public enum RequestPriority
    {
        Critical = 1,
        High = 2,
        Medium = 3,
        Low = 4
    }

    public enum RequestStatus
    {
        Pending = 1,
        Open = 1, // Alias for Pending
        Submitted = 2,
        UnderReview = 3,
        PendingApproval = 4,
        Approved = 5,
        Rejected = 6,
        InProgress = 7,
        ReadyForCompletion = 8,
        OnHold = 9,
        Completed = 10,
        Cancelled = 11
    }

    public class RequestAction
    {
        public int Id { get; set; }
        
        [Required]
        public int RequestId { get; set; }
        public virtual ITRequest Request { get; set; } = null!;
        
        [Required]
        public RequestActionType ActionType { get; set; }
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        public DateTime ActionDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;
        
        [StringLength(2000)]
        public string? Notes { get; set; }
    }

    public enum RequestActionType
    {
        Created = 1,
        Updated = 2,
        Submitted = 3,
        Assigned = 4,
        Approved = 5,
        Rejected = 6,
        Started = 7,
        Completed = 8,
        Cancelled = 9,
        CommentAdded = 10
    }

    // Potentially new enum if RequestCategory is added
    // public enum RequestCategory 
    // {
    //     Hardware,
    //     Software,
    //     Network,
    //     Access,
    //     Repair,
    //     Support,
    //     Other
    // }
}
