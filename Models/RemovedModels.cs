namespace HospitalAssetTracker.Models
{
    // Placeholder models for removed functionality
    public class CategoryForecast
    {
        public int Id { get; set; }
        public string? Category { get; set; }
        public decimal ForecastAmount { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal ForecastedValue { get; set; }
        public decimal PredictedSpend { get; set; }
        public double Confidence { get; set; }
        public string? TrendDirection { get; set; }
    }

    public class BudgetCategoryAnalysis
    {
        public int Id { get; set; }
        public string? Category { get; set; }
        public decimal Budget { get; set; }
        public decimal Spent { get; set; }
        public decimal Remaining { get; set; }
        public decimal AllocatedBudget { get; set; }
        public decimal ActualSpend { get; set; }
        public decimal RemainingBudget { get; set; }
        public decimal SpendRate { get; set; }
    }

    public class BudgetDepartmentAnalysis
    {
        public int Id { get; set; }
        public string? Department { get; set; }
        public decimal Budget { get; set; }
        public decimal Spent { get; set; }
        public decimal Remaining { get; set; }
        public decimal AllocatedBudget { get; set; }
        public decimal ActualSpend { get; set; }
        public decimal RemainingBudget { get; set; }
        public decimal SpendRate { get; set; }
    }

    public class SpendTrend
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string? Category { get; set; }
        public string? Period { get; set; }
        public int RequestCount { get; set; }
    }

    public class SpendAnomaly
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public int RequestId { get; set; }
        public string? AnomalyType { get; set; }
        public string? Severity { get; set; }
    }

    public class BugTracking
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime ReportedDate { get; set; }
        public string? Status { get; set; }
    }

    public class SystemVersion
    {
        public int Id { get; set; }
        public string? Version { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string? Notes { get; set; }
    }

    public class BugFixHistory
    {
        public int Id { get; set; }
        public int BugTrackingId { get; set; }
        public string? FixDescription { get; set; }
        public DateTime FixDate { get; set; }
        public string? FixedBy { get; set; }
    }
}
