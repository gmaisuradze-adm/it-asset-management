using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalAssetTracker.Models
{
    #region ABC Analysis Models

    public class AbcAnalysisResult
    {
        public DateTime AnalysisDate { get; set; }
        public int AnalysisPeriodMonths { get; set; }
        public List<ItemAnalysisData> CategoryA { get; set; } = new();
        public List<ItemAnalysisData> CategoryB { get; set; } = new();
        public List<ItemAnalysisData> CategoryC { get; set; } = new();
        public int TotalItems { get; set; }
        public decimal TotalValue { get; set; }
        public List<AbcRecommendation> Recommendations { get; set; } = new();
        
        public decimal CategoryAValue => CategoryA.Sum(i => i.TotalValue);
        public decimal CategoryBValue => CategoryB.Sum(i => i.TotalValue);
        public decimal CategoryCValue => CategoryC.Sum(i => i.TotalValue);
        
        public double CategoryAPercentage => TotalItems > 0 ? (double)CategoryA.Count / TotalItems * 100 : 0;
        public double CategoryBPercentage => TotalItems > 0 ? (double)CategoryB.Count / TotalItems * 100 : 0;
        public double CategoryCPercentage => TotalItems > 0 ? (double)CategoryC.Count / TotalItems * 100 : 0;
    }

    public class ItemAnalysisData
    {
        public InventoryItem InventoryItem { get; set; } = null!;
        public decimal TotalValue { get; set; }
        public int UsageFrequency { get; set; }
        public double VelocityScore { get; set; }
        public double CriticalityScore { get; set; }
        public decimal CarryingCost { get; set; }
        public string AbcClassification { get; set; } = string.Empty;
        
        public double CompositeScore => TotalValue > 0 ? (double)TotalValue * VelocityScore * CriticalityScore : 0;
    }

    public class AbcRecommendation
    {
        public string Category { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public string[] ActionItems { get; set; } = Array.Empty<string>();
    }

    #endregion

    #region Demand Forecasting Models

    public class DemandForecast
    {
        public int InventoryItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public DateTime ForecastDate { get; set; }
        public int ForecastPeriodDays { get; set; }
        public int HistoricalPeriodDays { get; set; }
        public double AverageDailyDemand { get; set; }
        public double PeakDailyDemand { get; set; }
        public double MinDailyDemand { get; set; }
        public bool SeasonalityDetected { get; set; }
        public double ConfidenceLevel { get; set; }
        public int RecommendedReorderPoint { get; set; }
        public int RecommendedOrderQuantity { get; set; }
        public List<DailyDemandForecast> DailyForecast { get; set; } = new();
        
        public double TotalForecastedDemand => DailyForecast.Sum(d => d.ForecastedDemand);
        public DateTime StockoutRiskDate => DailyForecast.FirstOrDefault(d => d.ProjectedStock <= 0)?.Date ?? DateTime.MaxValue;
    }

    public class DailyDemandForecast
    {
        public DateTime Date { get; set; }
        public double ForecastedDemand { get; set; }
        public double ConfidenceInterval { get; set; }
        public int ProjectedStock { get; set; }
        public double SeasonalFactor { get; set; }
        public string RiskLevel { get; set; } = "Low";
    }

    #endregion

    #region Smart Replenishment Models

    public class SmartReplenishmentResult
    {
        public DateTime ExecutionDate { get; set; }
        public string InitiatedByUserId { get; set; } = string.Empty;
        public int TotalItemsAnalyzed { get; set; }
        public int ItemsNeedingReplenishment { get; set; }
        public int AutoProcurementsCreated { get; set; }
        public List<ReplenishmentAction> ReplenishmentActions { get; set; } = new();
        
        public decimal TotalReplenishmentValue => ReplenishmentActions.Sum(a => a.EstimatedCost);
        public int CriticalItems => ReplenishmentActions.Count(a => a.Priority == ReplenishmentPriority.Critical);
    }

    public class ReplenishmentAction
    {
        public int InventoryItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int RecommendedOrderQuantity { get; set; }
        public ReplenishmentPriority Priority { get; set; }
        public string Reasoning { get; set; } = string.Empty;
        public int EstimatedDeliveryDays { get; set; }
        public decimal EstimatedCost { get; set; }
        public bool ActionTaken { get; set; }
        public bool ProcurementRequestCreated { get; set; }
        public int? ProcurementRequestId { get; set; }
    }

    public class ReplenishmentDecision
    {
        public bool ShouldReplenish { get; set; }
        public int OrderQuantity { get; set; }
        public ReplenishmentPriority Priority { get; set; }
        public string Reasoning { get; set; } = string.Empty;
        public int EstimatedDeliveryDays { get; set; }
        public decimal EstimatedCost { get; set; }
        public SupplierInfo? RecommendedSupplier { get; set; }
    }

    public class SupplierInfo
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal LastPrice { get; set; }
        public double LeadTimeDays { get; set; }
        public double ReliabilityScore { get; set; }
    }

    public enum ReplenishmentPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    #endregion

    #region Space Optimization Models

    public class SpaceOptimizationResult
    {
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public DateTime OptimizationDate { get; set; }
        public int CurrentItems { get; set; }
        public int TotalRecommendations { get; set; }
        public double EstimatedEfficiencyGain { get; set; }
        public List<SpaceOptimizationRecommendation> Recommendations { get; set; } = new();
    }

    public class SpaceOptimizationRecommendation
    {
        public int InventoryItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? CurrentZone { get; set; }
        public string RecommendedZone { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public double EstimatedTimeSaving { get; set; }
    }

    #endregion

    #region Quality Management Models

    public class QualityAssessmentResult
    {
        public int InventoryItemId { get; set; }
        public string InspectorUserId { get; set; } = string.Empty;
        public DateTime AssessmentDate { get; set; }
        public QualityChecklistData ChecklistData { get; set; } = new();
        public InventoryCondition OverallCondition { get; set; }
        public double QualityScore { get; set; }
        public string ActionRequired { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class QualityChecklistData
    {
        public bool PhysicalConditionGood { get; set; }
        public bool AllComponentsPresent { get; set; }
        public bool FunctionalityTested { get; set; }
        public bool CosmeticConditionAcceptable { get; set; }
        public bool PackagingIntact { get; set; }
        public bool DocumentationIncluded { get; set; }
        public bool SerialNumberVerified { get; set; }
        public bool WarrantyValid { get; set; }
        public List<string> IssuesFound { get; set; } = new();
        public List<string> Notes { get; set; } = new();
        public int OverallRating { get; set; } // 1-10 scale
    }

    public class QualityAssessmentRecord
    {
        public int Id { get; set; }
        public int InventoryItemId { get; set; }
        public virtual InventoryItem InventoryItem { get; set; } = null!;
        
        // Asset-related properties for compatibility
        public int? AssetId { get; set; }
        public virtual Asset? Asset { get; set; }
        
        public DateTime AssessmentDate { get; set; }
        public string InspectorUserId { get; set; } = string.Empty;
        public virtual ApplicationUser Inspector { get; set; } = null!;
        
        // Additional user property for compatibility
        public string PerformedByUserId 
        { 
            get => InspectorUserId; 
            set => InspectorUserId = value; 
        }
        public virtual ApplicationUser PerformedByUser 
        { 
            get => Inspector; 
            set => Inspector = value; 
        }
        
        public InventoryCondition OverallCondition { get; set; }
        public double QualityScore { get; set; }
        public string ChecklistJson { get; set; } = string.Empty;
        public string ActionRequired { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    #endregion

    #region Request Fulfillment Models

    public class RequestFulfillmentResult
    {
        public int RequestId { get; set; }
        public string FulfillerUserId { get; set; } = string.Empty;
        public DateTime FulfillmentDate { get; set; }
        public bool Success { get; set; }
        public List<FulfillmentAction> Actions { get; set; } = new();
        public string? FailureReason { get; set; }
        
        public int TotalItemsFulfilled => Actions.Count(a => a.Success);
        public decimal TotalValue => Actions.Where(a => a.Success).Sum(a => a.EstimatedValue);
    }

    public class FulfillmentAction
    {
        public int InventoryItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int QuantityRequired { get; set; }
        public int QuantityAvailable { get; set; }
        public double MatchScore { get; set; }
        public bool Success { get; set; }
        public int? AssetId { get; set; }
        public decimal EstimatedValue { get; set; }
        public string? FailureReason { get; set; }
    }

    public class InventoryMatch
    {
        public int InventoryItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int RequiredQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public double MatchScore { get; set; }
        public string MatchReasoning { get; set; } = string.Empty;
    }

    #endregion

    #region Performance Metrics Models

    public class WarehousePerformanceMetrics
    {
        public DateTime ReportDate { get; set; }
        public int ReportPeriodDays { get; set; }
        
        // Inventory Metrics
        public decimal TotalInventoryValue { get; set; }
        public double InventoryTurnoverRate { get; set; }
        public double StockAccuracy { get; set; }
        public int TotalItems { get; set; }
        public int LowStockItems { get; set; }
        public int OverstockItems { get; set; }
        
        // Operational Metrics
        public int TotalMovements { get; set; }
        public double AverageProcessingTime { get; set; }
        public double FulfillmentRate { get; set; }
        public int StockoutIncidents { get; set; }
        
        // Financial Metrics
        public decimal TotalCarryingCost { get; set; }
        public decimal ObsolescenceCost { get; set; }
        public decimal ProcurementSavings { get; set; }
        
        // Quality Metrics
        public double QualityScore { get; set; }
        public int QualityAssessments { get; set; }
        public int QualityIssues { get; set; }
    }

    public class WarehouseKpi
    {
        public string MetricName { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal TargetValue { get; set; }
        public decimal PreviousValue { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Trend { get; set; } = string.Empty; // "Up", "Down", "Stable"
        public string Status { get; set; } = string.Empty; // "Good", "Warning", "Critical"
    }

    #endregion

    #region Automation Models

    // AutomationRule and AutomationTrigger moved to UnifiedBusinessLogicModels.cs to avoid conflicts

    public class AutomationLog
    {
        public int Id { get; set; }
        public int AutomationRuleId { get; set; }
        public DateTime ExecutedAt { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ExecutedByUserId { get; set; }
        public virtual ApplicationUser? ExecutedByUser { get; set; }
        public string Action { get; set; } = string.Empty;
        public string ExecutionDetailsJson { get; set; } = string.Empty;
        
        // Compatibility properties
        public DateTime Timestamp
        {
            get => ExecutedAt;
            set => ExecutedAt = value;
        }
        
        public int RuleId
        {
            get => AutomationRuleId;
            set => AutomationRuleId = value;
        }
        
        public virtual AutomationRule? Rule { get; set; }
    }

    #endregion

    #region Dashboard View Models

    /// <summary>
    /// View model for warehouse dashboard
    /// </summary>
    public class WarehouseDashboardViewModel
    {
        public int TotalItems { get; set; }
        public int LowStockItems { get; set; }
        public int OverstockedItems { get; set; }
        public decimal TotalValue { get; set; }
        public List<StockLevelAlert> Alerts { get; set; } = new();
        public List<InventoryMovementViewModel> RecentMovements { get; set; } = new();
        public List<TopItemViewModel> TopMovingItems { get; set; } = new();
        public Dictionary<string, int> CategoryDistribution { get; set; } = new();
        public Dictionary<string, int> StatusDistribution { get; set; } = new();
        
        // Additional properties for compatibility with DashboardViewModels.cs
        public int TotalInventoryItems 
        { 
            get => TotalItems; 
            set => TotalItems = value; 
        }
        public int CriticalStockItems { get; set; }
        public decimal TotalInventoryValue 
        { 
            get => TotalValue; 
            set => TotalValue = value; 
        }
        public double OverallTurnoverRate { get; set; }
        public List<InventoryItem> TopMovers { get; set; } = new();
        public AbcAnalysisResult AbcAnalysis { get; set; } = new AbcAnalysisResult();
        public SmartReplenishmentResult ReplenishmentAnalysis { get; set; } = new SmartReplenishmentResult();
        public DateTime LastRefreshed { get; set; }
        public string CurrentUserId { get; set; } = string.Empty;
        public List<QuickAction> QuickActions { get; set; } = new List<QuickAction>();
        
        // Additional alias for Alerts compatibility
        public List<StockLevelAlert> StockAlerts 
        { 
            get => Alerts; 
            set => Alerts = value; 
        }
    }

    /// <summary>
    /// View model for quality management dashboard
    /// </summary>
    public class QualityManagementViewModel
    {
        public List<QualityAssessmentResult> PendingQualityChecks { get; set; } = new();
        public List<QualityAssessmentResult> RecentAssessments { get; set; } = new();
        public List<QualityMetric> QualityMetrics { get; set; } = new();
        public double OverallQualityScore { get; set; }
        public int TotalAssessments { get; set; }
        public int PassingAssessments { get; set; }
        public int FailingAssessments { get; set; }
    }

    #endregion

    #region Missing Model Classes for Compatibility

    /// <summary>
    /// Stock level alert model
    /// </summary>
    public class StockLevelAlert
    {
        public int Id { get; set; }
        public int InventoryItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinimumLevel { get; set; }
        public int ReorderLevel { get; set; }
        public string AlertType { get; set; } = string.Empty; // "LowStock", "CriticalStock", "OutOfStock"
        public DateTime CreatedDate { get; set; }
        public string? Notes { get; set; }
        
        // Additional properties for InventoryService compatibility
        public InventoryCategory Category { get; set; }
        public int MinimumStock { get; set; } // Alias for MinimumLevel
        public string LocationName { get; set; } = string.Empty;
        public DateTime? LastMovementDate { get; set; }
        public int DaysSinceLastMovement { get; set; }
    }

    /// <summary>
    /// Quality metric model
    /// </summary>
    public class QualityMetric
    {
        public string MetricName { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal TargetValue { get; set; }
        public decimal PreviousValue { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Trend { get; set; } = string.Empty; // "Up", "Down", "Stable"
        public string Status { get; set; } = string.Empty; // "Good", "Warning", "Critical"
        
        // Alias properties for compatibility
        public string Name 
        { 
            get => MetricName; 
            set => MetricName = value; 
        }
        public decimal Value 
        { 
            get => CurrentValue; 
            set => CurrentValue = value; 
        }
        public decimal Target 
        { 
            get => TargetValue; 
            set => TargetValue = value; 
        }
    }

    #endregion

    #region Warehouse Metrics and Recommendations

    public class WarehouseMetrics
    {
        public decimal TotalSpaceUsed { get; set; }
        public decimal TotalSpaceAvailable { get; set; }
        public decimal SpaceUtilizationPercentage { get; set; }
        public int TotalMovements { get; set; }
        public int PendingOrders { get; set; }
        public int CriticalItems { get; set; }
        public Dictionary<string, decimal> CategoryUtilization { get; set; } = new();
    }

    public class InventoryRecommendation
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string RecommendationType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    #endregion
}
