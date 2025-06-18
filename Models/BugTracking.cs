using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalAssetTracker.Models
{
    [Table("bug_tracking")]
    public class BugTracking
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("bug_title")]
        [MaxLength(255)]
        public string BugTitle { get; set; } = string.Empty;

        [Column("bug_description")]
        public string BugDescription { get; set; } = string.Empty;

        [Required]
        [Column("status")]
        [MaxLength(50)]
        public string Status { get; set; } = "Open"; // Open, InProgress, Fixed, Closed

        [Required]
        [Column("severity")]
        [MaxLength(20)]
        public string Severity { get; set; } = "Medium"; // Low, Medium, High, Critical

        [Column("module_name")]
        [MaxLength(100)]
        public string ModuleName { get; set; } = string.Empty;

        [Column("error_message")]
        public string ErrorMessage { get; set; } = string.Empty;

        [Column("stack_trace")]
        public string StackTrace { get; set; } = string.Empty;

        [Column("reproduction_steps")]
        public string ReproductionSteps { get; set; } = string.Empty;

        [Column("fix_description")]
        public string FixDescription { get; set; } = string.Empty;

        [Column("reported_by")]
        [MaxLength(100)]
        public string ReportedBy { get; set; } = "System";

        [Column("assigned_to")]
        [MaxLength(100)]
        public string AssignedTo { get; set; } = string.Empty;

        [Column("reported_date")]
        public DateTime ReportedDate { get; set; } = DateTime.UtcNow;

        [Column("fixed_date")]
        public DateTime? FixedDate { get; set; }

        [Column("closed_date")]
        public DateTime? ClosedDate { get; set; }

        [Column("version_found")]
        [MaxLength(20)]
        public string VersionFound { get; set; } = string.Empty;

        [Column("version_fixed")]
        [MaxLength(20)]
        public string VersionFixed { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
