using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Models
{
    // === ASSET DASHBOARD VIEW MODELS ===

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

    public class AssetCategoryData
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalValue { get; set; }
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


    // === PROCUREMENT DASHBOARD VIEW MODELS ===

    public class ProcurementDashboardViewModel
    {
        public int TotalActiveRequests { get; set; }
        public int PendingApprovals { get; set; }
        public int TotalVendors { get; set; }
        public VendorPerformanceAnalysis VendorPerformanceAnalysis { get; set; } = new VendorPerformanceAnalysis();
        public ProcurementForecast ProcurementForecast { get; set; } = new ProcurementForecast();
        public BudgetAnalysisResult BudgetAnalysis { get; set; } = new BudgetAnalysisResult();
        public List<ProcurementRequest> RecentRequests { get; set; } = new List<ProcurementRequest>();
        public List<ProcurementApproval> RecentApprovals { get; set; } = new List<ProcurementApproval>();
        public string CurrentUserId { get; set; } = string.Empty;
        public DateTime LastRefreshed { get; set; }
    }

    public class VendorIntelligenceViewModel
    {
        public VendorPerformanceAnalysis PerformanceAnalysis { get; set; } = new VendorPerformanceAnalysis();
        public List<VendorMetrics> TopPerformers { get; set; } = new List<VendorMetrics>();
        public List<VendorMetrics> UnderPerformers { get; set; } = new List<VendorMetrics>();
        public DateTime AnalysisDate { get; set; }
        public int TotalVendorsAnalyzed { get; set; }
    }

    public class CostOptimizationViewModel
    {
        public CostOptimizationResult OptimizationResult { get; set; } = new CostOptimizationResult();
        public SpendAnalysisResult SpendAnalysis { get; set; } = new SpendAnalysisResult();
        public decimal TotalSavingsIdentified { get; set; }
        public double SavingsPercentage { get; set; }
        public List<CostOptimizationOpportunity> OptimizationOpportunities { get; set; } = new List<CostOptimizationOpportunity>();
    }

    public class ProcurementForecastingViewModel
    {
        public ProcurementForecast Forecast { get; set; } = new ProcurementForecast();
        public BudgetAnalysisResult BudgetAnalysis { get; set; } = new BudgetAnalysisResult();
        public double ForecastAccuracy { get; set; }
        public decimal TotalForecastedValue { get; set; }
        public List<string> KeyInsights { get; set; } = new List<string>();
    }

    public class EmergencyProcurementViewModel
    {
        public EmergencyProcurementRequest Request { get; set; } = new EmergencyProcurementRequest();
        public EmergencyProcurementResult? Result { get; set; }
    }

    public class VendorRiskAssessmentViewModel
    {
        public VendorRiskAssessment Assessment { get; set; } = new VendorRiskAssessment();
        public string RiskLevel { get; set; } = string.Empty;
        public double TotalRiskScore { get; set; }
        public List<string> MitigationActions { get; set; } = new List<string>();
    }

    // === WAREHOUSE DASHBOARD VIEW MODELS ===

    // WarehouseDashboardViewModel moved to WarehouseModels.cs to avoid duplication

    public class QuickAction
    {
        public string Title { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class QualityMetrics
    {
        public int TotalAssessments { get; set; }
        public double AverageQualityScore { get; set; }
        public int ItemsNeedingImprovement { get; set; }
        public string QualityTrend { get; set; } = string.Empty;
    }

    public class InventoryAnalyticsViewModel
    {
        public AbcAnalysisResult AbcAnalysis { get; set; } = new AbcAnalysisResult();
        public List<DemandForecast> DemandForecasts { get; set; } = new List<DemandForecast>();
        public SpaceOptimizationResult SpaceOptimization { get; set; } = new SpaceOptimizationResult();
        public DateTime AnalysisDate { get; set; }
    }

    public class ReplenishmentViewModel
    {
        public SmartReplenishmentResult ReplenishmentResult { get; set; } = new SmartReplenishmentResult();
        public List<InventoryItem> ItemsToReorder { get; set; } = new List<InventoryItem>();
        public decimal EstimatedReplenishmentCost { get; set; }
        public List<string> RecommendedActions { get; set; } = new List<string>();
    }

    public class CostOptimizationOpportunity
    {
        public string OpportunityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal PotentialSavings { get; set; }
        public string ImplementationDifficulty { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty; // Added Priority property
    }
}
