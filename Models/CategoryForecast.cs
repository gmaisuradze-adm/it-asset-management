using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalAssetTracker.Models
{
    [Table("category_forecasts")]
    public class CategoryForecast
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("category")]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [Column("predicted_spend")]
        public decimal PredictedSpend { get; set; }

        [Column("confidence")]
        public double Confidence { get; set; }

        [Column("trend_direction")]
        [MaxLength(50)]
        public string TrendDirection { get; set; } = string.Empty;

        [Column("forecasted_value")]
        public decimal ForecastedValue { get; set; }

        [Column("historical_average")]
        public decimal HistoricalAverage { get; set; }

        [Column("growth_rate")]
        public double GrowthRate { get; set; }

        [Column("analysis_date")]
        public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
