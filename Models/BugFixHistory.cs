using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalAssetTracker.Models
{
    [Table("bug_fix_histories")]
    public class BugFixHistory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("bug_id")]
        public int BugId { get; set; }

        [Required]
        [Column("version_id")]
        public int VersionId { get; set; }

        [Column("fix_details")]
        public string FixDetails { get; set; } = string.Empty;

        [Column("files_changed")]
        public string FilesChanged { get; set; } = string.Empty;

        [Column("test_status")]
        [MaxLength(50)]
        public string TestStatus { get; set; } = "Pending";

        [Column("fixed_date")]
        public DateTime FixedDate { get; set; } = DateTime.UtcNow;

        [Column("fixed_by")]
        [MaxLength(100)]
        public string FixedBy { get; set; } = "System";

        [Column("verification_date")]
        public DateTime? VerificationDate { get; set; }

        [Column("verified_by")]
        [MaxLength(100)]
        public string? VerifiedBy { get; set; }

        [Column("rollback_reason")]
        public string? RollbackReason { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("BugId")]
        public virtual BugTracking Bug { get; set; } = null!;

        [ForeignKey("VersionId")]
        public virtual SystemVersion SystemVersion { get; set; } = null!;
    }
}
