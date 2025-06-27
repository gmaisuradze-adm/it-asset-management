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
        public RequestStatus Status { get; set; } = RequestStatus.Submitted; // Default to Submitted

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

        public DateTime? CompletedDate { get; set; }

        [StringLength(450)]
        public string? CompletedByUserId { get; set; }
        public virtual ApplicationUser? CompletedByUser { get; set; }

        // Business logic fields
        [StringLength(1000)]
        public string? BusinessJustification { get; set; }

        [StringLength(1000)]
        public string? Justification { get; set; } // Added based on build errors

        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }

        [StringLength(2000)]
        public string? AssignmentNotes { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? EstimatedCost { get; set; }

        [StringLength(2000)]
        public string? CompletionNotes { get; set; }

        public DateTime? ResolutionDate { get; set; }
        
        [StringLength(2000)]
        public string? ResolutionDetails { get; set; }

        public int? RequiredInventoryItemId { get; set; }
        public virtual InventoryItem? RequiredInventoryItem { get; set; }

        public int? ProvidedInventoryItemId { get; set; }
        public virtual InventoryItem? ProvidedInventoryItem { get; set; }

        // New fields for asset replacement workflow
        public int? DamagedAssetId { get; set; } // To link to the existing, damaged asset being replaced
        public virtual Asset? DamagedAsset { get; set; }

        [StringLength(1000)]
        public string? DisposalNotesForUnmanagedAsset { get; set; } // Notes if the damaged item isn't a tracked asset

        // --- ALIASES FOR BACKWARDS COMPATIBILITY ---
        [NotMapped]
        public DateTime CreatedDate { get => RequestDate; set => RequestDate = value; }

        [NotMapped]
        public DateTime? DueDate { get => RequiredByDate; set => RequiredByDate = value; }

        [NotMapped]
        public ApplicationUser Requester { get => RequestedByUser; set => RequestedByUser = value; }

        [NotMapped]
        public ApplicationUser? AssignedTo { get => AssignedToUser; set => AssignedToUser = value; }
        
        [NotMapped]
        public string? AssignedToId { get => AssignedToUserId; set => AssignedToUserId = value; }

        [NotMapped]
        public DateTime? LastModifiedDate { get => ModifiedAt; set => ModifiedAt = value; }

        [NotMapped]
        public DateTime? LastUpdatedDate { get => ModifiedAt; set => ModifiedAt = value; }
        
        [NotMapped]
        public string? LastUpdatedByUserId { get => ModifiedBy; set => ModifiedBy = value; }
        
        [NotMapped]
        public ApplicationUser? LastUpdatedByUser { get; set; }


        // Navigation properties for related data
        public virtual ICollection<RequestComment> Comments { get; set; } = new List<RequestComment>();
        public virtual ICollection<RequestAttachment> Attachments { get; set; } = new List<RequestAttachment>();
        public virtual ICollection<ProcurementRequest> ProcurementRequests { get; set; } = new List<ProcurementRequest>();
        public virtual ICollection<RequestActivity> Activities { get; set; } = new List<RequestActivity>();
    }

    // Enum definitions

    public enum RequestType
    {
        NewHardware = 1,
        SoftwareInstallation = 2,
        AccessRequest = 3,
        Maintenance = 4,
        Repair = 5, // Added
        NewEquipment = 6, // Added
        HardwareReplacement = 7, // Added
        HardwareRepair = 8, // Added
        UserAccessRights = 9, // Added
        SoftwareUpgrade = 10, // Added
        MaintenanceService = 11, // Added
        NetworkConnectivity = 12, // Added
        ITConsultation = 13, // Added
        Training = 14, // Added
        NewSoftware = 15, // Added
        Service = 16, // Added for ProcurementService
        Other = 99
    }

    public enum RequestPriority
    {
        Low = 1,
        Medium = 2,
        High = 3, // Added
        Critical = 4
    }

    public enum RequestStatus
    {
        Submitted = 1,
        InProgress = 2,
        OnHold = 3,
        Completed = 4,
        Cancelled = 5
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
