using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalAssetTracker.Models
{
    #region Asset Analytics Models

    public class AssetAnalyticsResult
    {
        public DateTime AnalysisDate { get; set; }
        public int AnalysisPeriodMonths { get; set; }
        public List<AssetCategoryMetrics> CategoryMetrics { get; set; } = new();
        public List<AssetLifecycleMetrics> LifecycleMetrics { get; set; } = new();
        public List<AssetRecommendation> Recommendations { get; set; } = new();
        
        public int TotalAssets { get; set; }
        public decimal TotalValue { get; set; }
        public int ActiveAssets { get; set; }
        public int MaintenanceAssets { get; set; }
        public int RetiredAssets { get; set; }
        
        public double UtilizationRate => TotalAssets > 0 ? (double)ActiveAssets / TotalAssets * 100 : 0;
        public decimal AverageAssetValue => TotalAssets > 0 ? TotalValue / TotalAssets : 0;
    }

    public class AssetCategoryMetrics
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalValue { get; set; }
        public double AverageAge { get; set; }
        public int MaintenanceEvents { get; set; }
        public decimal MaintenanceCost { get; set; }
        public double FailureRate { get; set; }
        public int WarrantyExpired { get; set; }
        
        public decimal AverageValue => Count > 0 ? TotalValue / Count : 0;
        public decimal MaintenanceCostPerAsset => Count > 0 ? MaintenanceCost / Count : 0;
    }

    public class AssetLifecycleMetrics
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Value { get; set; }
        public double AverageTimeInStatus { get; set; }
        public List<string> TopLocations { get; set; } = new();
        public List<string> TopBrands { get; set; } = new();
        
        public decimal AverageValue => Count > 0 ? Value / Count : 0;
        public double Percentage { get; set; }
    }

    public class AssetRecommendation
    {
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty; // High, Medium, Low
        public string Type { get; set; } = string.Empty; // Maintenance, Replacement, Procurement
        public string Recommendation { get; set; } = string.Empty;
        public string[] ActionItems { get; set; } = Array.Empty<string>();
        public DateTime? DueDate { get; set; }
        public decimal EstimatedCost { get; set; }
    }

    #endregion

    #region Asset Performance Models

    public class AssetPerformanceAnalysis
    {
        public Asset Asset { get; set; } = null!;
        public double PerformanceScore { get; set; }
        public double ReliabilityScore { get; set; }
        public double UtilizationScore { get; set; }
        public double CostEfficiencyScore { get; set; }
        public List<PerformanceIndicator> Indicators { get; set; } = new();
        public List<MaintenancePattern> MaintenancePatterns { get; set; } = new();
        public PredictiveInsight PredictiveInsight { get; set; } = new();
        
        public double OverallScore => (PerformanceScore + ReliabilityScore + UtilizationScore + CostEfficiencyScore) / 4;
        public string PerformanceGrade => OverallScore >= 90 ? "A" : OverallScore >= 80 ? "B" : OverallScore >= 70 ? "C" : "D";
    }

    public class PerformanceIndicator
    {
        public string Name { get; set; } = string.Empty;
        public double Value { get; set; }
        public double Target { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Good, Warning, Critical
        public string Trend { get; set; } = string.Empty; // Up, Down, Stable
    }

    public class MaintenancePattern
    {
        public string PatternType { get; set; } = string.Empty;
        public int Frequency { get; set; }
        public decimal AverageCost { get; set; }
        public double AverageDowntime { get; set; }
        public List<DateTime> ScheduledDates { get; set; } = new();
        public string Recommendation { get; set; } = string.Empty;
    }

    public class MaintenanceScheduleItem
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public string MaintenanceType { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public int EstimatedDurationHours { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string AssignedTechnician { get; set; } = string.Empty;
    }

    public class PredictiveInsight
    {
        public DateTime? PredictedFailureDate { get; set; }
        public double FailureProbability { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public decimal EstimatedMaintenanceCost { get; set; }
        public int RemainingUsefulLife { get; set; } // in months
        public string RiskLevel { get; set; } = string.Empty; // Low, Medium, High, Critical
        public List<string> Recommendations { get; set; } = new();
    }

    #endregion

    #region Asset Dashboard Models

    public class AssetDashboardModel
    {
        public AssetOverviewMetrics Overview { get; set; } = new();
        public List<AssetStatusSummary> StatusSummary { get; set; } = new();
        public List<AssetCategoryBreakdown> CategoryBreakdown { get; set; } = new();
        public List<AssetLocationSummary> LocationSummary { get; set; } = new();
        public List<AssetAlert> Alerts { get; set; } = new();
        public List<AssetTrend> Trends { get; set; } = new();
        public List<UpcomingMaintenance> UpcomingMaintenance { get; set; } = new();
        public List<WarrantyExpiration> WarrantyExpirations { get; set; } = new();
        public AssetUtilizationData Utilization { get; set; } = new();
        public List<AssetCostAnalysis> CostAnalysis { get; set; } = new();
    }

    public class AssetOverviewMetrics
    {
        public int TotalAssets { get; set; }
        public int ActiveAssets { get; set; }
        public int InMaintenanceAssets { get; set; }
        public int RetiredAssets { get; set; }
        public decimal TotalValue { get; set; }
        public decimal MonthlyDepreciation { get; set; }
        public int NewAssetsThisMonth { get; set; }
        public int DeployedThisMonth { get; set; }
        
        public double ActivePercentage => TotalAssets > 0 ? (double)ActiveAssets / TotalAssets * 100 : 0;
        public decimal AverageAssetValue => TotalAssets > 0 ? TotalValue / TotalAssets : 0;
    }

    public class AssetStatusSummary
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Value { get; set; }
        public double Percentage { get; set; }
        public string Color { get; set; } = string.Empty;
        public int ChangeFromLastMonth { get; set; }
        public string TrendDirection { get; set; } = string.Empty; // Up, Down, Stable
    }

    public class AssetCategoryBreakdown
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Value { get; set; }
        public double Percentage { get; set; }
        public int MaintenanceEvents { get; set; }
        public decimal MaintenanceCost { get; set; }
        public double AverageAge { get; set; }
        public string HealthStatus { get; set; } = string.Empty; // Excellent, Good, Fair, Poor
    }

    public class AssetLocationSummary
    {
        public string LocationName { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
        public int AssetCount { get; set; }
        public decimal TotalValue { get; set; }
        public int ActiveAssets { get; set; }
        public int MaintenanceAssets { get; set; }
        public double UtilizationRate { get; set; }
        public List<string> TopCategories { get; set; } = new();
    }

    public class AssetAlert
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public string AssetTag { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty; // Warranty, Maintenance, Performance, Security
        public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical
        public string Message { get; set; } = string.Empty;
        public DateTime AlertDate { get; set; }
        public DateTime CreatedDate 
        { 
            get => AlertDate; 
            set => AlertDate = value; 
        }
        public DateTime? DueDate { get; set; }
        public bool IsAcknowledged { get; set; }
        public string Location { get; set; } = string.Empty;
        public string ActionRequired { get; set; } = string.Empty;
    }

    public class AssetOptimizationOpportunity
    {
        public int AssetId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string OpportunityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal PotentialSavings { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string ImplementationComplexity { get; set; } = string.Empty;
        public string ImplementationEffort { get; set; } = string.Empty;
        public DateTime IdentifiedDate { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public int EstimatedImplementationDays { get; set; }
    }

    public class AssetTrend
    {
        public string Metric { get; set; } = string.Empty;
        public List<TrendDataPoint> DataPoints { get; set; } = new();
        public string TrendDirection { get; set; } = string.Empty; // Up, Down, Stable
        public double TrendPercentage { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    public class TrendDataPoint
    {
        public DateTime Date { get; set; }
        public double Value { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class UpcomingMaintenance
    {
        public int AssetId { get; set; }
        public string AssetTag { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string MaintenanceType { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public int DaysUntilDue { get; set; }
        public decimal EstimatedCost { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string AssignedTechnician { get; set; } = string.Empty;
    }

    public class WarrantyExpiration
    {
        public int AssetId { get; set; }
        public string AssetTag { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public DateTime? WarrantyEndDate { get; set; }
        public int DaysUntilExpiration { get; set; }
        public decimal PurchasePrice { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Expired, Expiring, Valid
    }

    public class AssetUtilizationData
    {
        // Individual asset utilization properties
        public int AssetId { get; set; }
        public double UtilizationRate { get; set; }
        public DateTime MeasurementDate { get; set; }
        
        // Overall utilization metrics
        public double OverallUtilization { get; set; }
        public List<CategoryUtilization> CategoryUtilization { get; set; } = new();
        public List<LocationUtilization> LocationUtilization { get; set; } = new();
        public int UnderutilizedAssets { get; set; }
        public int OverutilizedAssets { get; set; }
        public List<string> OptimizationOpportunities { get; set; } = new();
    }

    public class CategoryUtilization
    {
        public string Category { get; set; } = string.Empty;
        public double UtilizationRate { get; set; }
        public int TotalAssets { get; set; }
        public int ActiveAssets { get; set; }
        public int IdleAssets { get; set; }
        public string UtilizationTrend { get; set; } = string.Empty;
    }

    public class LocationUtilization
    {
        public string Location { get; set; } = string.Empty;
        public double UtilizationRate { get; set; }
        public int TotalAssets { get; set; }
        public int ActiveAssets { get; set; }
        public decimal TotalValue { get; set; }
        public string OptimizationStatus { get; set; } = string.Empty;
    }

    public class AssetCostAnalysis
    {
        public string Category { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public decimal AcquisitionCost { get; set; }
        public decimal MaintenanceCost { get; set; }
        public decimal OperationalCost { get; set; }
        public decimal DepreciationCost { get; set; }
        public decimal TotalCostOfOwnership { get; set; }
        public decimal CostPerDay { get; set; }
        public double ROI { get; set; }
        public string CostTrend { get; set; } = string.Empty;
    }

    #endregion

    #region Asset Lifecycle Models

    public class AssetLifecycleAnalysis
    {
        public int AssetId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime? AcquisitionDate { get; set; }
        public string LifecycleStage { get; set; } = string.Empty;
        public int RemainingUsefulLife { get; set; }
        public string ReplacementRecommendation { get; set; } = string.Empty;
        public string MaintenanceSchedule { get; set; } = string.Empty;
        
        public Asset Asset { get; set; } = null!;
        public List<LifecycleStage> Stages { get; set; } = new();
        public LifecycleMetrics Metrics { get; set; } = new();
        public List<LifecycleEvent> Events { get; set; } = new();
        public LifecyclePrediction Prediction { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    public class LifecycleStage
    {
        public string Stage { get; set; } = string.Empty; // Procurement, Deployment, Active, Maintenance, Retirement
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int DurationDays { get; set; }
        public decimal CostIncurred { get; set; }
        public double PerformanceScore { get; set; }
        public List<string> KeyEvents { get; set; } = new();
        public string Status { get; set; } = string.Empty; // Completed, Current, Upcoming
    }

    public class LifecycleMetrics
    {
        public int TotalLifespanDays { get; set; }
        public int ActiveDays { get; set; }
        public int MaintenanceDays { get; set; }
        public decimal TotalCostOfOwnership { get; set; }
        public decimal CostPerDay { get; set; }
        public double UtilizationRate { get; set; }
        public int MaintenanceEvents { get; set; }
        public double MeanTimeBetweenFailures { get; set; }
        public double MeanTimeToRepair { get; set; }
        public double OverallEfficiency { get; set; }
    }

    public class LifecycleEvent
    {
        public DateTime Date { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public string Impact { get; set; } = string.Empty; // Low, Medium, High
        public string Category { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
    }

    public class LifecyclePrediction
    {
        public DateTime? PredictedRetirementDate { get; set; }
        public int EstimatedRemainingLife { get; set; }
        public decimal EstimatedFutureCosts { get; set; }
        public DateTime? OptimalReplacementDate { get; set; }
        public decimal ReplacementCost { get; set; }
        public double Confidence { get; set; }
        public string Reasoning { get; set; } = string.Empty;
    }

    #endregion

    #region Asset Optimization Models

    public class AssetOptimizationResult
    {
        public DateTime AnalysisDate { get; set; }
        public List<OptimizationOpportunity> Opportunities { get; set; } = new();
        public decimal PotentialSavings { get; set; }
        public decimal InvestmentRequired { get; set; }
        public double ROI { get; set; }
        public int PaybackPeriodMonths { get; set; }
        public List<OptimizationRecommendation> Recommendations { get; set; } = new();
    }

    public class OptimizationOpportunity
    {
        public string Type { get; set; } = string.Empty; // Consolidation, Replacement, Reallocation, Upgrade
        public string Description { get; set; } = string.Empty;
        public List<int> AssetIds { get; set; } = new();
        public decimal CurrentCost { get; set; }
        public decimal OptimizedCost { get; set; }
        public decimal Savings { get; set; }
        public decimal ImplementationCost { get; set; }
        public int PaybackMonths { get; set; }
        public string Priority { get; set; } = string.Empty;
        public List<string> Benefits { get; set; } = new();
        public List<string> Risks { get; set; } = new();
    }

    public class OptimizationRecommendation
    {
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> ActionSteps { get; set; } = new();
        public DateTime? TargetDate { get; set; }
        public decimal EstimatedCost { get; set; }
        public decimal ExpectedSavings { get; set; }
        public string Owner { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    #endregion

    #region Advanced Asset Analysis Models

    public class AssetLifecycleAnalysisResult
    {
        public int AssetId { get; set; }
        public DateTime AnalysisDate { get; set; }
        public string AnalystUserId { get; set; } = string.Empty;
        public Asset Asset { get; set; } = null!;
        public List<LifecyclePhase> Phases { get; set; } = new();
        public LifecycleMetrics Metrics { get; set; } = new();
        public List<LifecycleRecommendation> Recommendations { get; set; } = new();
        public double OverallEfficiency { get; set; }
        public decimal TotalLifecycleCost { get; set; }
        public int PredictedRemainingLifeDays { get; set; }
        
        // Additional properties from service usage
        public int AssetAgeDays { get; set; }
        public double MaintenanceFrequency { get; set; }
        public double MovementFrequency { get; set; }
        public decimal TotalMaintenanceCost { get; set; }
        public decimal AverageMaintenanceCost { get; set; }
        public double ReturnOnInvestment { get; set; }
        public decimal CostPerDay { get; set; }
        public double MaintenanceRiskScore { get; set; }
        public string ReplacementUrgency { get; set; } = string.Empty;
        public List<string> StrategicRecommendations { get; set; } = new();
        public List<AssetOptimizationOpportunity> OptimizationOpportunities { get; set; } = new();
        public string CurrentLifecycleStage { get; set; } = string.Empty;
        public int EstimatedRemainingLifeMonths { get; set; }
        
        // Additional properties for controller compatibility
        public string CurrentStage => CurrentLifecycleStage;
        public int RemainingUsefulLife => EstimatedRemainingLifeMonths;
        public string ReplacementRecommendation { get; set; } = string.Empty;
        public List<MaintenanceScheduleItem> MaintenanceSchedule { get; set; } = new();
    }

    public class LifecyclePhase
    {
        public string Phase { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int DurationDays { get; set; }
        public decimal CostIncurred { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class LifecycleRecommendation
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime? TargetDate { get; set; }
        public decimal EstimatedCost { get; set; }
    }

    public class AssetReplacementForecastResult
    {
        public DateTime ForecastDate { get; set; }
        public int ForecastPeriodDays { get; set; }
        public string InitiatedByUserId { get; set; } = string.Empty;
        public List<AssetReplacementPrediction> Predictions { get; set; } = new();
        public decimal TotalForecastedCost { get; set; }
        public int AssetsRequiringReplacement { get; set; }
        public DateTime ForecastPeriodStart { get; set; }
        public DateTime ForecastPeriodEnd { get; set; }
        public List<string> CriticalReplacements { get; set; } = new();
        
        // Additional properties from service usage
        public List<AssetReplacementPrediction> AssetReplacementPredictions { get; set; } = new();
        public int TotalAssetsRequiringReplacement { get; set; }
        public decimal EstimatedTotalReplacementCost { get; set; }
        public int HighPriorityReplacements { get; set; }
        public List<string> BudgetingRecommendations { get; set; } = new();
        public double ForecastAccuracy { get; set; }
    }

    public class AssetReplacementPrediction
    {
        public int AssetId { get; set; }
        public string AssetTag { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public Asset Asset { get; set; } = null!;
        public DateTime PredictedReplacementDate { get; set; }
        public decimal EstimatedReplacementCost { get; set; }
        public string ReplacementReason { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public string Priority { get; set; } = string.Empty;
        
        // Additional properties from service usage
        public double ReplacementProbability { get; set; }
        public string BusinessImpactLevel { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
    }

    public class AssetUtilizationOptimizationResult
    {
        public DateTime OptimizationDate { get; set; }
        public string OptimizerUserId { get; set; } = string.Empty;
        public List<UnderutilizedAsset> UnderutilizedAssets { get; set; } = new();
        public List<OverDemandScenario> OverDemandScenarios { get; set; } = new();
        public List<UtilizationOptimizationRecommendation> Recommendations { get; set; } = new();
        public List<UtilizationImprovement> Improvements { get; set; } = new();
        public double CurrentUtilizationRate { get; set; }
        public double OptimizedUtilizationRate { get; set; }
        public decimal PotentialSavings { get; set; }
        
        // Additional properties from service usage
        public Dictionary<string, double> CurrentUtilizationMetrics { get; set; } = new();
        public List<AssetOptimizationOpportunity> OptimizationRecommendations { get; set; } = new();
        public decimal PotentialCostSavings { get; set; }
        public string ImplementationPriority { get; set; } = string.Empty;
        public List<string> ProjectedImprovements { get; set; } = new();
    }

    public class UnderutilizedAsset
    {
        public Asset Asset { get; set; } = null!;
        public double CurrentUtilization { get; set; }
        public double OptimalUtilization { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
        public decimal PotentialSavings { get; set; }
        
        // Additional properties for service compatibility
        public int AssetId => Asset?.Id ?? 0;
        public string AssetName => Asset?.Name ?? string.Empty;
        public string OpportunityType { get; set; } = "Underutilization";
        public string Description => RecommendedAction;
        public string ImplementationEffort { get; set; } = "Medium";
        public string Priority { get; set; } = "Medium";
    }

    public class OverDemandScenario
    {
        public string Location { get; set; } = string.Empty;
        public string AssetCategory { get; set; } = string.Empty;
        public double DemandLevel { get; set; }
        public double SupplyLevel { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
        
        // Additional properties for service compatibility
        public int AssetId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string OpportunityType { get; set; } = "Over-demand";
        public string Description => RecommendedAction;
        public decimal PotentialSavings { get; set; }
        public string ImplementationEffort { get; set; } = "High";
        public string Priority { get; set; } = "High";
    }

    public class UtilizationOptimizationRecommendation
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<int> AffectedAssetIds { get; set; } = new();
        public decimal EstimatedSavings { get; set; }
        public string Priority { get; set; } = string.Empty;
    }

    public class UtilizationImprovement
    {
        public string ImprovementType { get; set; } = string.Empty;
        public double ImprovementPercentage { get; set; }
        public decimal CostSavings { get; set; }
        public string Implementation { get; set; } = string.Empty;
        
        // Additional properties for service compatibility
        public string Description => $"{ImprovementType}: {ImprovementPercentage:F1}% improvement possible";
    }

    public class AssetUtilizationMetrics
    {
        public double OverallUtilizationRate { get; set; }
        public Dictionary<string, double> CategoryUtilization { get; set; } = new();
        public Dictionary<string, double> LocationUtilization { get; set; } = new();
        public List<Asset> UnderutilizedAssets { get; set; } = new();
        public List<Asset> OverutilizedAssets { get; set; } = new();
        public double EfficiencyScore { get; set; }
        public decimal PotentialCostSavings { get; set; }
        
        // Additional properties for service compatibility
        public double OverallUtilization => OverallUtilizationRate;
        public double AverageUptime { get; set; } = 95.0;
    }

    public class IntelligentMaintenanceScheduleResult
    {
        public DateTime ScheduleGenerationDate { get; set; }
        public int PlanningPeriodDays { get; set; }
        public string SchedulerUserId { get; set; } = string.Empty;
        public List<IntelligentMaintenanceScheduleItem> ScheduleItems { get; set; } = new();
        public MaintenanceResourceRequirements ResourceRequirements { get; set; } = new();
        public decimal TotalBudgetRequired { get; set; }
        public int OptimizedScheduleItems { get; set; }
        public double EfficiencyGain { get; set; }
        
        // Additional properties from service usage
        public List<IntelligentMaintenanceScheduleItem> MaintenanceScheduleItems { get; set; } = new();
        public decimal EstimatedTotalCost { get; set; }
        public List<string> SchedulingInsights { get; set; } = new();
        public List<string> CostOptimizationRecommendations { get; set; } = new();
    }

    public class IntelligentMaintenanceScheduleItem
    {
        public int AssetId { get; set; }
        public string AssetTag { get; set; } = string.Empty;
        public Asset Asset { get; set; } = null!;
        public DateTime ScheduledDate { get; set; }
        public string MaintenanceType { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public int EstimatedDuration { get; set; }
        public string Priority { get; set; } = string.Empty;
        public List<string> RequiredSkills { get; set; } = new();
    }

    public class MaintenanceResourceRequirements
    {
        public Dictionary<string, int> TechnicianHours { get; set; } = new();
        public Dictionary<string, decimal> PartsCost { get; set; } = new();
        public Dictionary<string, int> ToolsRequired { get; set; } = new();
        public decimal TotalCost { get; set; }
    }

    public class AssetDeploymentResult
    {
        public DateTime DeploymentDate { get; set; }
        public string DeployedByUserId { get; set; } = string.Empty;
        public int InventoryItemId { get; set; }
        public int TargetLocationId { get; set; }
        public Asset Asset { get; set; } = null!;
        public AssetDeploymentMetrics Metrics { get; set; } = new();
        public List<string> DeploymentSteps { get; set; } = new();
        public DateTime EstimatedCompletionDate { get; set; }
        public List<string> Requirements { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        
        // Additional properties from service usage
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public int CreatedAssetId { get; set; }
        public List<string> DeploymentDocumentation { get; set; } = new();
        public AssetDeploymentMetrics DeploymentMetrics { get; set; } = new();
    }

    public class AssetDeploymentMetrics
    {
        public int TotalSteps { get; set; }
        public int CompletedSteps { get; set; }
        public double ProgressPercentage { get; set; }
        public TimeSpan EstimatedTimeRemaining { get; set; }
        public List<string> Blockers { get; set; } = new();
        
        // Additional properties from service usage
        public double DeploymentTime { get; set; }
        public double CostEfficiency { get; set; }
    }

    public class AssetRetirementResult
    {
        public DateTime RetirementDate { get; set; }
        public string ProcessedByUserId { get; set; } = string.Empty;
        public int AssetId { get; set; }
        public string RetirementReason { get; set; } = string.Empty;
        public Asset Asset { get; set; } = null!;
        public AssetRetirementMetrics Metrics { get; set; } = new();
        public AssetDataArchivalResult DataArchival { get; set; } = new();
        public ReplacementProcessingResult ReplacementProcessing { get; set; } = new();
        public DisposalCoordinationResult DisposalCoordination { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        
        // Additional properties from service usage
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public AssetDataArchivalResult DataArchivalResult { get; set; } = new();
        public ReplacementProcessingResult ReplacementProcessingResult { get; set; } = new();
        public DisposalCoordinationResult DisposalCoordinationResult { get; set; } = new();
        public List<string> RetirementDocumentation { get; set; } = new();
        public AssetRetirementMetrics RetirementMetrics { get; set; } = new();
    }

    public class AssetRetirementMetrics
    {
        public decimal RecoveredValue { get; set; }
        public decimal DisposalCost { get; set; }
        public int DataMigrationHours { get; set; }
        public List<string> ComplianceRequirements { get; set; } = new();
        
        // Additional properties from service usage
        public int TotalLifespanDays { get; set; }
        public decimal TotalCostOfOwnership { get; set; }
    }

    public class AssetDataArchivalResult
    {
        public bool DataArchived { get; set; }
        public DateTime ArchivalDate { get; set; }
        public int RecordsArchived { get; set; }
        public string ArchivalLocation { get; set; } = string.Empty;
        
        // Additional property from service usage
        public bool Success { get; set; }
    }

    public class ReplacementProcessingResult
    {
        public bool ReplacementRequired { get; set; }
        public Asset? ReplacementAsset { get; set; }
        public DateTime? ReplacementDate { get; set; }
        public string ReplacementStatus { get; set; } = string.Empty;
        
        // Additional property from service usage
        public bool Success { get; set; }
    }

    public class DisposalCoordinationResult
    {
        public string DisposalMethod { get; set; } = string.Empty;
        public DateTime? DisposalDate { get; set; }
        public decimal DisposalCost { get; set; }
        public string DisposalVendor { get; set; } = string.Empty;
        
        // Additional property from service usage
        public bool Success { get; set; }
    }

    #endregion

    #region Additional Result Models for Service Implementation

    public class AssetDashboardViewModel
    {
        public int TotalAssets { get; set; }
        public int ActiveAssets { get; set; }
        public int InMaintenanceAssets { get; set; }
        public int RetiredAssets { get; set; }
        public List<AssetCategoryData> CategoryData { get; set; } = new();
        public List<AssetAlert> RecentAlerts { get; set; } = new();
        public List<MaintenanceRecord> UpcomingMaintenance { get; set; } = new();
        public Dictionary<string, int> AssetsByLocation { get; set; } = new();
        public Dictionary<string, decimal> AssetValueByCategory { get; set; } = new();
        
        // Additional properties for controller compatibility
        public Dictionary<string, int> StatusSummary { get; set; } = new();
        public Dictionary<string, int> CategoryBreakdown { get; set; } = new();
        public Dictionary<string, object> Trends { get; set; } = new();
    }

    public class AssetAnalyticsViewModel
    {
        public List<AssetPerformanceData> PerformanceData { get; set; } = new();
        public List<AssetUtilizationData> UtilizationData { get; set; } = new();
        public List<AssetCostData> CostData { get; set; } = new();
        public Dictionary<string, double> EfficiencyMetrics { get; set; } = new();
    }

    public class AssetPerformanceAnalysisResult
    {
        public List<AssetPerformanceMetric> Metrics { get; set; } = new();
        public double OverallPerformance { get; set; }
        public List<string> Recommendations { get; set; } = new();
        public DateTime AnalysisDate { get; set; }
    }

    public class AssetPerformanceReportResult
    {
        public List<AssetPerformanceData> PerformanceData { get; set; } = new();
        public DateTime ReportGenerationDate { get; set; }
        public string GeneratedByUserId { get; set; } = string.Empty;
    }

    public class AssetServiceRequestResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RequestId { get; set; }
    }

    public class AssetDataSynchronizationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime SynchronizationDate { get; set; }
    }

    public class AssetHealthAnalysisResult
    {
        public int AssetId { get; set; }
        public DateTime AnalysisDate { get; set; }
        public double HealthScore { get; set; }
        public List<string> HealthIndicators { get; set; } = new();
    }

    public class AssetCostBenefitAnalysisResult
    {
        public DateTime AnalysisDate { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalBenefit { get; set; }
        public double ROI { get; set; }
    }

    public class AssetPerformanceMetricsResult
    {
        public DateTime ReportPeriodStart { get; set; }
        public DateTime ReportPeriodEnd { get; set; }
        public List<AssetPerformanceMetric> Metrics { get; set; } = new();
    }

    public class AssetFailureRiskAssessmentResult
    {
        public DateTime AssessmentDate { get; set; }
        public List<AssetRiskScore> RiskScores { get; set; } = new();
    }

    public class AssetPortfolioOptimizationResult
    {
        public DateTime OptimizationDate { get; set; }
        public List<string> Recommendations { get; set; } = new();
    }

    public class AssetComplianceAnalysisResult
    {
        public DateTime AnalysisDate { get; set; }
        public bool IsCompliant { get; set; }
        public List<string> ComplianceIssues { get; set; } = new();
    }

    public class AssetBudgetPlanningResult
    {
        public int FiscalYear { get; set; }
        public DateTime PlanningDate { get; set; }
        public decimal BudgetRequired { get; set; }
    }

    public class AssetSecurityRiskResult
    {
        public DateTime AnalysisDate { get; set; }
        public List<string> SecurityRisks { get; set; } = new();
    }

    public class AssetAutomationResult
    {
        public DateTime ExecutionDate { get; set; }
        public List<string> CompletedTasks { get; set; } = new();
    }

    public class AssetWorkflowOrchestrationResult
    {
        public DateTime OrchestrationDate { get; set; }
        public bool Success { get; set; }
    }

    public class AssetAlertingResult
    {
        public DateTime ProcessingDate { get; set; }
        public List<AssetAlert> ProcessedAlerts { get; set; } = new();
    }

    public class AssetLifecycleReportResult
    {
        public DateTime ReportGenerationDate { get; set; }
        public List<Asset> AssetsInReport { get; set; } = new();
    }

    public class AssetInvestmentRequest
    {
        public int AssetId { get; set; }
        public decimal InvestmentAmount { get; set; }
        public string RequestReason { get; set; } = string.Empty;
    }

    public class AssetWorkflowRequest
    {
        public int AssetId { get; set; }
        public string WorkflowType { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
    }

    public class AssetReportingCriteria
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<int> AssetIds { get; set; } = new();
    }

    public class AssetPerformanceData
    {
        public int AssetId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public double PerformanceScore { get; set; }
        public DateTime MeasurementDate { get; set; }
    }

    public class AssetCostData
    {
        public int AssetId { get; set; }
        public decimal Cost { get; set; }
        public string CostType { get; set; } = string.Empty;
        public DateTime CostDate { get; set; }
    }

    public class AssetPerformanceMetric
    {
        public string MetricName { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    public class AssetRiskScore
    {
        public int AssetId { get; set; }
        public double RiskScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
    }

    public class AssetCategoryData
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalValue { get; set; }
    }

    #endregion
}
