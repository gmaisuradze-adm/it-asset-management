using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalAssetTracker.Models
{
    [Table("spend_anomalies")]
    public class SpendAnomaly
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("request_id")]
        public int RequestId { get; set; }

        [Column("amount")]
        public decimal Amount { get; set; }

        [Required]
        [Column("anomaly_type")]
        [MaxLength(100)]
        public string AnomalyType { get; set; } = string.Empty;

        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Column("severity")]
        [MaxLength(20)]
        public string Severity { get; set; } = string.Empty;

        [Column("detected_date")]
        public DateTime DetectedDate { get; set; } = DateTime.UtcNow;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
