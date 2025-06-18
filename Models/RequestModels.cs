using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalAssetTracker.Models
{
    public class RequestApproval
    {
        public int Id { get; set; }

        [Required]
        public int ITRequestId { get; set; }
        public virtual ITRequest ITRequest { get; set; } = null!;

        [Required]
        public ApprovalLevel ApprovalLevel { get; set; }

        [Required]
        [StringLength(450)]
        public string ApproverId { get; set; } = string.Empty;
        public virtual ApplicationUser Approver { get; set; } = null!;

        [Required]
        public ApprovalStatus Status { get; set; }

        [StringLength(1000)]
        public string? Comments { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? DecisionDate { get; set; }

        [Required]
        public int Sequence { get; set; } // Order of approval

        // Compatibility properties
        [NotMapped]
        public int RequestId 
        { 
            get => ITRequestId; 
            set => ITRequestId = value; 
        }

        [NotMapped]
        public DateTime? ApprovalDate 
        { 
            get => DecisionDate; 
            set => DecisionDate = value; 
        }

        [NotMapped]
        public ApprovalStatus ApprovalStatus 
        { 
            get => Status; 
            set => Status = value; 
        }

        // Computed properties
        [NotMapped]
        public string StatusDescription => Status switch
        {
            ApprovalStatus.Pending => "მომლოდინე",
            ApprovalStatus.Approved => "დამტკიცებული",
            ApprovalStatus.Rejected => "უარყოფილი",
            ApprovalStatus.Delegated => "დელეგირებული",
            _ => "უცნობი"
        };
    }

    public class RequestComment
    {
        public int Id { get; set; }

        [Required]
        public int ITRequestId { get; set; }
        public virtual ITRequest ITRequest { get; set; } = null!;

        [Required]
        [StringLength(450)]
        public string CommentedByUserId { get; set; } = string.Empty;
        public virtual ApplicationUser CommentedByUser { get; set; } = null!;

        [Required]
        [StringLength(2000)]
        public string Comment { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; }

        public bool IsInternal { get; set; } = false;

        public CommentType CommentType { get; set; } = CommentType.General;
    }

    public class RequestAttachment
    {
        public int Id { get; set; }

        [Required]
        public int ITRequestId { get; set; }
        public virtual ITRequest ITRequest { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

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

    public enum ApprovalLevel
    {
        Supervisor = 1,
        Department = 2,
        DepartmentHead = 3,
        ITDepartment = 4,
        Finance = 5,
        Executive = 6
    }

    public enum ApprovalStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Delegated = 4,
        Escalated = 5
    }

    public enum CommentType
    {
        General = 1,
        StatusUpdate = 2,
        TechnicalNote = 3,
        ApprovalNote = 4,
        CompletionNote = 5
    }
}
