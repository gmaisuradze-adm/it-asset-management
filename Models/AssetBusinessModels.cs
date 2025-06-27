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

    // Assuming AssetOptimizationResult, AssetUtilizationOptimizationResult, 
    // AssetMetric, UtilizationRecommendation, ProjectedImprovement are defined elsewhere in this file or project.

    // Add AssetOverDemandAnalysis if it's confirmed missing
    public class AssetOverDemandAnalysis
    {
        public List<string> OverDemandedCategories { get; set; } = new List<string>();
        public List<string> SuggestedProcurements { get; set; } = new List<string>();
        public int EstimatedShortfallCount { get; set; }
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

    #endregion

    #region Missing Models for IAssetBusinessLogicService

    public class AssetPerformanceReportResult { }
    public class AssetServiceRequestResult { }
    public class CrossModuleAssetSyncResult { }

    public class AssetHealthAnalysisResult 
    {
        public Asset? AnalyzedAsset { get; set; }
        public DateTime AnalysisDate { get; set; }
        public string? AnalystUserId { get; set; }
        public string HealthStatus { get; set; } = "Unknown"; // e.g., Good, Fair, Poor, Critical
        public double HealthScore { get; set; } // e.g., 0-100
        public List<string> IssuesFound { get; set; } = new List<string>();
        public List<string> Recommendations { get; set; } = new List<string>();
        public int AgeInDays { get; set; }
        public int MaintenanceCountLastYear { get; set; }
        public bool IsWarrantyActive { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
    }

    public class AssetCostBenefitAnalysisResult { }
    public class AssetInvestmentRequest { }
    public class AssetPerformanceMetricsResult { }
    public class AssetFailureRiskAssessmentResult { }
    public class AssetPortfolioOptimizationResult { }
    public class AssetComplianceAnalysisResult { }
    public class AssetBudgetPlanningResult { }
    public class AssetSecurityRiskAnalysisResult { }
    public class AutomatedAssetManagementTaskResult { }
    public class AssetWorkflowOrchestrationResult { }
    public class AssetWorkflowRequest { }
    public class AssetAlertingResult { }
    public class AssetLifecycleReportResult { }
    public class AssetReportingCriteria { }

    #endregion

    #region Asset Business Logic Service Models

    // Ensure these supporting types are defined before AssetUtilizationOptimizationResult
    public class AssetMetric
    {
        public string Name { get; set; } = string.Empty;
        public double Value { get; set; }
        // Add other properties if needed by the service
    }

    public class UtilizationRecommendation
    {
        public string Type { get; set; } = string.Empty; // e.g., Reallocate, Consolidate
        public string Description { get; set; } = string.Empty;
        public decimal EstimatedSavings { get; set; }
        // Add other properties if needed
    }

    public class ProjectedImprovement
    {
        public string Description { get; set; } = string.Empty; // Added Description
        public string MetricName { get; set; } = string.Empty;
        public double CurrentValue { get; set; }
        public double ProjectedValue { get; set; }
        public string Unit { get; set; } = string.Empty;
        public double EstimatedImpact { get; set; } // Kept this from previous version
    }

    public class AssetReplacementForecastResult
    {
        public DateTime ForecastDate { get; set; }
        public int ForecastPeriodDays { get; set; }
        public string InitiatedByUserId { get; set; } = string.Empty;
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
        public double ReplacementProbability { get; set; }
        public DateTime? PredictedReplacementDate { get; set; }
        public decimal EstimatedReplacementCost { get; set; }
        public string BusinessImpactLevel { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
    }

    public class AssetUtilizationOptimizationResult
    {
        public DateTime OptimizationDate { get; set; }
        public string OptimizerUserId { get; set; } = string.Empty;
        public List<AssetMetric> CurrentUtilizationMetrics { get; set; } = new List<AssetMetric>();
        public List<Asset> UnderutilizedAssets { get; set; } = new List<Asset>();
        public AssetOverDemandAnalysis OverDemandScenarios { get; set; } = new AssetOverDemandAnalysis(); // Ensure AssetOverDemandAnalysis is defined
        public List<UtilizationRecommendation> OptimizationRecommendations { get; set; } = new List<UtilizationRecommendation>();
        public decimal PotentialCostSavings { get; set; }
        public string ImplementationPriority { get; set; } = string.Empty;
        public List<ProjectedImprovement> ProjectedImprovements { get; set; } = new List<ProjectedImprovement>();
    }

    public class IntelligentMaintenanceScheduleResult
    {
        public DateTime ScheduleGenerationDate { get; set; }
        public int PlanningPeriodDays { get; set; }
        public string SchedulerUserId { get; set; } = string.Empty;
        public List<MaintenanceScheduleItem> MaintenanceScheduleItems { get; set; } = new();
        public Dictionary<string, int> ResourceRequirements { get; set; } = new();
        public decimal EstimatedTotalCost { get; set; }
        public List<string> SchedulingInsights { get; set; } = new();
        public List<string> CostOptimizationRecommendations { get; set; } = new();
    }

    public class AssetDeploymentResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int CreatedAssetId { get; set; }
        public DateTime DeploymentDate { get; set; }
        public string DeployedByUserId { get; set; } = string.Empty;
        public int InventoryItemId { get; set; }
        public int TargetLocationId { get; set; }
        public string? DeploymentDocumentation { get; set; }
        public Dictionary<string, object> DeploymentMetrics { get; set; } = new Dictionary<string, object>(); // Verified
    }

    public class AssetRetirementResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int AssetId { get; set; }
        public DateTime RetirementDate { get; set; }
        public string ProcessedByUserId { get; set; } = string.Empty;
        public string RetirementReason { get; set; } = string.Empty;
        public string? DataArchivalResult { get; set; }
        public AssetReplacementProcessingResult? ReplacementProcessingResult { get; set; } // Ensure AssetReplacementProcessingResult is defined
        public AssetDisposalCoordinationResult? DisposalCoordinationResult { get; set; } // Ensure AssetDisposalCoordinationResult is defined
        public string? RetirementDocumentation { get; set; }
        public Dictionary<string, object> RetirementMetrics { get; set; } = new Dictionary<string, object>(); // Verified
    }

    // Ensure AssetReplacementProcessingResult is defined
    public class AssetReplacementProcessingResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int? ProcurementRequestId { get; set; }
    }

    // Ensure AssetDisposalCoordinationResult is defined
    public class AssetDisposalCoordinationResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? DisposalMethod { get; set; }
        public DateTime? DisposalDate { get; set; }
    }
    
    // Ensure AssetOverDemandAnalysis is defined (if not already defined earlier in the file)
    // public class AssetOverDemandAnalysis
    // {
    //     public List<string> OverDemandedCategories { get; set; } = new List<string>();
    //     public List<string> SuggestedProcurements { get; set; } = new List<string>();
    //     public int EstimatedShortfallCount { get; set; }
    // }


    #endregion
}
