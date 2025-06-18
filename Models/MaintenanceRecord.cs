using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalAssetTracker.Models
{
    public enum MaintenanceType
    {
        PreventiveMaintenance,
        Repair,
        Upgrade,
        Inspection,
        Calibration,
        Cleaning,
        Other
    }

    public enum MaintenanceStatus
    {
        Scheduled,
        InProgress,
        Completed,
        Cancelled,
        Failed
    }

    public class MaintenanceRecord
    {
        public int Id { get; set; }

        [Required]
        public int AssetId { get; set; }
        public virtual Asset Asset { get; set; } = null!;

        [Required]
        public MaintenanceType MaintenanceType { get; set; }

        [Required]
        public MaintenanceStatus Status { get; set; }

        [Required]
        [StringLength(200)]
        public required string Title { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime ScheduledDate { get; set; }

        // Additional property for compatibility
        public DateTime MaintenanceDate 
        { 
            get => CompletedDate ?? ScheduledDate; 
            set => ScheduledDate = value; 
        }

        public DateTime? StartDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        [StringLength(100)]
        public string? PerformedBy { get; set; }

        [StringLength(100)]
        public string? ServiceProvider { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Cost { get; set; }

        [StringLength(1000)]
        public string? WorkPerformed { get; set; }

        [StringLength(1000)]
        public string? PartsUsed { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime? NextMaintenanceDate { get; set; }

        public string CreatedByUserId { get; set; } = string.Empty;
        public virtual ApplicationUser CreatedByUser { get; set; } = null!;

        public DateTime CreatedDate { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
