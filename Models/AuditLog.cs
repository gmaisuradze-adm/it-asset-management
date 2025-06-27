using System.ComponentModel.DataAnnotations;

namespace HospitalAssetTracker.Models
{
    public enum AuditAction
    {
        Create,
        Update,
        Delete,
        Move,
        StatusChange,
        Assignment,
        Maintenance,
        Login,
        Logout,
        Error
    }

    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        public AuditAction Action { get; set; }

        [Required]
        [StringLength(100)]
        public string EntityType { get; set; } = string.Empty;

        public int? EntityId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        public DateTime Timestamp { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public string? OldValues { get; set; }

        public string? NewValues { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        // For asset-related audits
        public int? AssetId { get; set; }
        public virtual Asset? Asset { get; set; }
    }
}
