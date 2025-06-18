using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalAssetTracker.Models
{
    [Table("system_versions")]
    public class SystemVersion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("version_number")]
        [MaxLength(20)]
        public string VersionNumber { get; set; } = string.Empty;

        [Column("release_notes")]
        public string ReleaseNotes { get; set; } = string.Empty;

        [Column("bugs_fixed")]
        public int BugsFixed { get; set; } = 0;

        [Column("features_added")]
        public int FeaturesAdded { get; set; } = 0;

        [Column("release_date")]
        public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;

        [Column("is_current")]
        public bool IsCurrent { get; set; } = false;

        [Column("created_by")]
        [MaxLength(100)]
        public string CreatedBy { get; set; } = "System";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<BugFixHistory> BugFixHistories { get; set; } = new List<BugFixHistory>();
    }
}
