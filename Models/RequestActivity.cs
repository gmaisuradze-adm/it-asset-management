using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalAssetTracker.Models
{
    public class RequestActivity
    {
        public int Id { get; set; }

        [Required]
        public int ITRequestId { get; set; }
        [ForeignKey("ITRequestId")]
        public virtual ITRequest ITRequest { get; set; } = null!;

        [Required]
        public DateTime ActivityDate { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        public RequestActivityType ActivityType { get; set; }
    }

    public enum RequestActivityType
    {
        Comment,
        StatusChange,
        Assignment,
        Attachment,
        Note,
        Approval,
        System
    }
}
