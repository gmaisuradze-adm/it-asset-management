using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalAssetTracker.Models
{
    [Table("spend_trends")]
    public class SpendTrend
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("period")]
        [MaxLength(50)]
        public string Period { get; set; } = string.Empty;

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("request_count")]
        public int RequestCount { get; set; }

        [Column("category")]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [Column("trend")]
        [MaxLength(50)]
        public string Trend { get; set; } = string.Empty;

        [Column("change_percentage")]
        public double ChangePercentage { get; set; }

        [Column("analysis_date")]
        public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
