using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalAssetTracker.Models
{
    [Table("budget_department_analysis")]
    public class BudgetDepartmentAnalysis
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("department")]
        [MaxLength(200)]
        public string Department { get; set; } = string.Empty;

        [Column("allocated_budget")]
        public decimal AllocatedBudget { get; set; }

        [Column("actual_spend")]
        public decimal ActualSpend { get; set; }

        [Column("remaining_budget")]
        public decimal RemainingBudget { get; set; }

        [Column("spend_rate")]
        public decimal SpendRate { get; set; }

        [Column("budget_utilization_rate")]
        public double BudgetUtilizationRate { get; set; }

        [Column("variance_amount")]
        public decimal VarianceAmount { get; set; }

        [Column("variance_percentage")]
        public double VariancePercentage { get; set; }

        [Column("analysis_date")]
        public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
