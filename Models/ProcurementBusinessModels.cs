using System.ComponentModel.DataAnnotations;

namespace HospitalAssetTracker.Models
{
    // Note: ProcurementCategory enum is defined in ProcurementRequest.cs
    
    // === VENDOR INTELLIGENCE & PERFORMANCE MODELS ===

    /// <summary>
    /// Comprehensive vendor performance analysis result
    /// </summary>
    public class VendorPerformanceAnalysis
    {
        public DateTime AnalysisDate { get; set; }
        public int AnalysisPeriodMonths { get; set; }
        public List<VendorMetrics> VendorMetrics { get; set; } = new List<VendorMetrics>();
        public List<VendorMetrics> PreferredVendors { get; set; } = new List<VendorMetrics>();
        public List<VendorMetrics> UnderperformingVendors { get; set; } = new List<VendorMetrics>();
        public List<string> StrategicRecommendations { get; set; } = new List<string>();
        public decimal TotalProcurementValue { get; set; }
        public int TotalOrders { get; set; }
        public double AverageDeliveryTime { get; set; }
        public double OverallQualityScore { get; set; }
    }

    /// <summary>
    /// Individual vendor performance metrics
    /// </summary>
    public class VendorMetrics
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public decimal TotalValue { get; set; }
        public double AverageOrderValue { get; set; }
        public double OnTimeDeliveryRate { get; set; }
        public double QualityScore { get; set; }
        public double CostCompetitiveness { get; set; }
        public double PriceCompetitiveness { get; set; }
        public double ResponseTime { get; set; }
        public double ComplianceScore { get; set; }
        public double CompositePerformanceScore { get; set; }
        public string PerformanceGrade { get; set; } = string.Empty;
        public List<string> StrengthAreas { get; set; } = new List<string>();
        public List<string> ImprovementAreas { get; set; } = new List<string>();
        public DateTime LastOrderDate { get; set; }
        public int DaysActiveInPeriod { get; set; }
    }

    /// <summary>
    /// Vendor selection criteria for intelligent decision making
    /// </summary>
    public class VendorSelectionCriteria
    {
        public string Category { get; set; } = string.Empty;
        public decimal MaxBudget { get; set; }
        public DateTime RequiredDeliveryDate { get; set; }
        public double MinQualityScore { get; set; } = 70.0;
        public double MinimumScore { get; set; } = 60.0;
        public bool RequireWarranty { get; set; }
        public bool RequireCertification { get; set; }
        public string GeographicRestrictions { get; set; } = string.Empty;
        public Dictionary<string, double> CriteriaWeights { get; set; } = new Dictionary<string, double>
        {
            { "Cost", 0.3 },
            { "Quality", 0.25 },
            { "Delivery", 0.2 },
            { "Service", 0.15 },
            { "Compliance", 0.1 }
        };
    }

    /// <summary>
    /// Result of vendor selection process
    /// </summary>
    public class VendorSelectionResult
    {
        public DateTime SelectionDate { get; set; }
        public VendorSelectionCriteria Criteria { get; set; } = new VendorSelectionCriteria();
        public List<VendorSelectionScore> VendorScores { get; set; } = new List<VendorSelectionScore>();
        public VendorSelectionScore? RecommendedVendor { get; set; }
        public List<string> SelectionReasoning { get; set; } = new List<string>();
        public double ConfidenceLevel { get; set; }
        public List<string> RiskFactors { get; set; } = new List<string>();
        public List<string> AlternativeRecommendations { get; set; } = new List<string>();
    }

    /// <summary>
    /// Vendor selection scoring details
    /// </summary>
    public class VendorSelectionScore
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public double CostScore { get; set; }
        public double QualityScore { get; set; }
        public double DeliveryScore { get; set; }
        public double ServiceScore { get; set; }
        public double ComplianceScore { get; set; }
        public double TotalScore { get; set; }
        public string Rank { get; set; } = string.Empty;
        public List<string> Strengths { get; set; } = new List<string>();
        public List<string> Weaknesses { get; set; } = new List<string>();
        public Dictionary<string, double> DetailedScores { get; set; } = new Dictionary<string, double>();
    }

    /// <summary>
    /// Comprehensive vendor risk assessment
    /// </summary>
    public class VendorRiskAssessment
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public DateTime AssessmentDate { get; set; }
        public double FinancialRisk { get; set; }
        public double OperationalRisk { get; set; }
        public double ComplianceRisk { get; set; }
        public double GeographicalRisk { get; set; }
        public double MarketRisk { get; set; }
        public double OverallRiskScore { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public List<string> MitigationRecommendations { get; set; } = new List<string>();
        public List<RiskFactor> RiskFactors { get; set; } = new List<RiskFactor>();
        public DateTime NextAssessmentDate { get; set; }
    }

    /// <summary>
    /// Individual risk factor detail
    /// </summary>
    public class RiskFactor
    {
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Impact { get; set; }
        public double Probability { get; set; }
        public double RiskScore { get; set; }
        public string Mitigation { get; set; } = string.Empty;
    }

    // === PROCUREMENT FORECASTING & PLANNING MODELS ===

    /// <summary>
    /// Comprehensive procurement forecast with business intelligence
    /// </summary>
    public class ProcurementForecast
    {
        public DateTime ForecastDate { get; set; }
        public int ForecastPeriodMonths { get; set; }
        public List<CategoryForecast> CategoryForecasts { get; set; } = new List<CategoryForecast>();
        public List<BudgetRequirement> BudgetRequirements { get; set; } = new List<BudgetRequirement>();
        public Dictionary<string, double> SeasonalFactors { get; set; } = new Dictionary<string, double>();
        public decimal TotalForecastedValue { get; set; }
        public double ConfidenceLevel { get; set; }
        public List<string> StrategicRecommendations { get; set; } = new List<string>();
        public List<ForecastRisk> ForecastRisks { get; set; } = new List<ForecastRisk>();
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Monthly forecast breakdown
    /// </summary>
    public class MonthlyForecast
    {
        public DateTime Month { get; set; }
        public decimal ForecastedValue { get; set; }
        public int ForecastedQuantity { get; set; }
        public double ConfidenceLevel { get; set; }
        public List<string> KeyAssumptions { get; set; } = new List<string>();
    }

    /// <summary>
    /// Budget requirement analysis
    /// </summary>
    public class BudgetRequirement
    {
        public string Department { get; set; } = string.Empty;
        public ProcurementCategory Category { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal RequiredBudget { get; set; }
        public decimal CurrentBudget { get; set; }
        public decimal BudgetGap { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Justification { get; set; } = string.Empty;
        public DateTime RequiredDate { get; set; }
        public List<string> AlternativeOptions { get; set; } = new List<string>();
    }

    /// <summary>
    /// Forecast risk assessment
    /// </summary>
    public class ForecastRisk
    {
        public string RiskType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Impact { get; set; }
        public double Probability { get; set; }
        public string Mitigation { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
    }

    // === COST OPTIMIZATION & ANALYTICS MODELS ===

    /// <summary>
    /// Comprehensive cost optimization analysis
    /// </summary>
    public class CostOptimizationAnalysis
    {
        public DateTime AnalysisDate { get; set; }
        public int AnalysisPeriodMonths { get; set; }
        public decimal TotalSpend { get; set; }
        public decimal IdentifiedSavings { get; set; }
        public double SavingsPercentage { get; set; }
        public List<CostOptimizationOpportunity> Opportunities { get; set; } = new List<CostOptimizationOpportunity>();
        public List<SpendAnalysis> SpendAnalyses { get; set; } = new List<SpendAnalysis>();
        public List<string> StrategicRecommendations { get; set; } = new List<string>();
        public Dictionary<string, decimal> CategorySavings { get; set; } = new Dictionary<string, decimal>();
        public List<CostDriver> CostDrivers { get; set; } = new List<CostDriver>();
    }

    public class SpendAnalysis // Added definition
    {
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    public class CostDriver // Added definition
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal ImpactAmount { get; set; }
    }

    /// <summary>
    /// Cost optimization execution result
    /// </summary>
    public class CostOptimizationResult
    {
        public bool Success { get; set; }
        public DateTime ExecutionDate { get; set; }
        public string InitiatedByUserId { get; set; } = string.Empty;
        public CostOptimizationAnalysis Analysis { get; set; } = new CostOptimizationAnalysis();
        public List<string> ImplementedActions { get; set; } = new List<string>();
        public decimal TotalSavingsRealized { get; set; }
        public List<string> ProcessingMessages { get; set; } = new List<string>();
        public List<string> NextSteps { get; set; } = new List<string>();
    }

    // === CONTRACT & SUPPLIER MANAGEMENT MODELS ===

    /// <summary>
    /// Contract performance analysis
    /// </summary>
    public class ContractPerformanceAnalysis
    {
        public int ContractId { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string VendorName { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public double ComplianceScore { get; set; }
        public double PerformanceScore { get; set; }
        public decimal ContractValue { get; set; }
        public decimal SpentToDate { get; set; }
        public double UtilizationRate { get; set; }
        public List<ContractKPI> KPIs { get; set; } = new List<ContractKPI>();
        public List<string> ComplianceIssues { get; set; } = new List<string>();
        public List<string> PerformanceIssues { get; set; } = new List<string>();
        public List<string> Recommendations { get; set; } = new List<string>();
    }

    /// <summary>
    /// Contract Key Performance Indicator
    /// </summary>
    public class ContractKPI
    {
        public string KPIName { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public string Actual { get; set; } = string.Empty;
        public double PerformanceScore { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Trend { get; set; } = string.Empty;
    }

    // === PURCHASE ORDER AUTOMATION MODELS ===

    /// <summary>
    /// Purchase order automation configuration
    /// </summary>
    public class PurchaseOrderAutomationConfig
    {
        public string Category { get; set; } = string.Empty;
        public decimal AutoApprovalThreshold { get; set; }
        public List<string> PreferredVendors { get; set; } = new List<string>();
        public List<string> RequiredApprovers { get; set; } = new List<string>();
        public Dictionary<string, string> AutomationRules { get; set; } = new Dictionary<string, string>();
        public bool EnableAutoGeneration { get; set; }
        public bool EnableAutoApproval { get; set; }
        public bool EnableAutoSending { get; set; }
    }

    /// <summary>
    /// Purchase order generation result
    /// </summary>
    public class PurchaseOrderGenerationResult
    {
        public bool Success { get; set; }
        public int? PurchaseOrderId { get; set; }
        public string PurchaseOrderNumber { get; set; } = string.Empty;
        public List<string> ValidationErrors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> ProcessingSteps { get; set; } = new List<string>();
        public DateTime GeneratedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string NextAction { get; set; } = string.Empty;
    }

    // === BUDGET & SPEND ANALYTICS MODELS ===

    /// <summary>
    /// Budget vs spend analysis
    /// </summary>
    public class BudgetSpendAnalysis
    {
        public string Department { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal BudgetAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal RemainingBudget { get; set; }
        public double UtilizationRate { get; set; }
        public double BurnRate { get; set; }
        public DateTime ProjectedExhaustionDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> Alerts { get; set; } = new List<string>();
        public List<string> Recommendations { get; set; } = new List<string>();
    }

    /// <summary>
    /// Spend trend analysis
    /// </summary>
    // === ENUMS ===

    public enum RiskLevel
    {
        VeryLow,
        Low,
        Medium,
        High,
        VeryHigh
    }

    // === ADDITIONAL RESULT MODELS FOR INTERFACE METHODS ===

    /// <summary>
    /// Contract optimization result
    /// </summary>
    public class ContractOptimizationResult
    {
        public bool Success { get; set; }
        public DateTime OptimizationDate { get; set; }
        public List<ContractPerformanceAnalysis> ContractAnalyses { get; set; } = new List<ContractPerformanceAnalysis>();
        public List<string> RenewalRecommendations { get; set; } = new List<string>();
        public List<string> RenegotiationOpportunities { get; set; } = new List<string>();
        public decimal EstimatedSavings { get; set; }
        public List<string> RiskMitigations { get; set; } = new List<string>();
    }

    /// <summary>
    /// Purchase order optimization result
    /// </summary>
    public class PurchaseOrderOptimizationResult
    {
        public bool Success { get; set; }
        public int ProcurementRequestId { get; set; }
        public PurchaseOrderGenerationResult? GenerationResult { get; set; }
        public List<string> OptimizationActions { get; set; } = new List<string>();
        public decimal CostSavings { get; set; }
        public string RecommendedVendor { get; set; } = string.Empty;
        public List<string> ProcessingNotes { get; set; } = new List<string>();
    }

    /// <summary>
    /// Emergency procurement processing request
    /// </summary>
    public class EmergencyProcurementRequest
    {
        public string RequestDescription { get; set; } = string.Empty;
        public string Justification { get; set; } = string.Empty;
        public decimal MaxBudget { get; set; }
        public DateTime RequiredByDate { get; set; }
        public string Department { get; set; } = string.Empty;
        public string Priority { get; set; } = "High";
        public List<string> PreferredVendors { get; set; } = new List<string>();
        public bool SkipStandardApprovals { get; set; }
        public string EmergencyContact { get; set; } = string.Empty;
    }

    /// <summary>
    /// Emergency procurement processing result
    /// </summary>
    public class EmergencyProcurementResult
    {
        public bool Success { get; set; }
        public string ProcessorUserId { get; set; } = string.Empty;
        public DateTime ProcessingDate { get; set; }
        public EmergencyProcurementRequest Request { get; set; } = new EmergencyProcurementRequest();
        public PurchaseOrderGenerationResult? PurchaseOrderResult { get; set; }
        public List<string> ExpediterActions { get; set; } = new List<string>();
        public string Status { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
    }

    /// <summary>
    /// Budget analysis result
    /// </summary>
    public class BudgetAnalysisResult
    {
        public string FiscalYear { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public List<BudgetSpendAnalysis> DepartmentAnalyses { get; set; } = new List<BudgetSpendAnalysis>();
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalRemaining { get; set; }
        public double OverallUtilizationRate { get; set; }
        public List<string> BudgetAlerts { get; set; } = new List<string>();
        public List<string> OptimizationRecommendations { get; set; } = new List<string>();
        public List<SpendTrend> SpendTrends { get; set; } = new List<SpendTrend>();
        
        // Additional properties for business logic compatibility
        public List<BudgetCategoryAnalysis> CategoryAnalysis { get; set; } = new List<BudgetCategoryAnalysis>();
        public List<BudgetDepartmentAnalysis> DepartmentAnalysis { get; set; } = new List<BudgetDepartmentAnalysis>();
        public decimal TotalBudgetAllocated { get; set; }
        public decimal TotalBudgetUtilized { get; set; }
        public List<string> BudgetRecommendations { get; set; } = new List<string>();
    }

    /// <summary>
    /// Spend analysis parameters
    /// </summary>
    public class SpendAnalysisParameters
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> Categories { get; set; } = new List<string>();
        public List<string> Departments { get; set; } = new List<string>();
        public List<string> Vendors { get; set; } = new List<string>();
        public bool IncludeTrends { get; set; } = true;
        public bool IncludeOptimization { get; set; } = true;
        public string GroupBy { get; set; } = "Category"; // Category, Department, Vendor, Month
    }

    /// <summary>
    /// Comprehensive spend analysis result
    /// </summary>
    public class SpendAnalysisResult
    {
        public DateTime AnalysisDate { get; set; }
        public SpendAnalysisParameters Parameters { get; set; } = new SpendAnalysisParameters();
        public List<SpendAnalysis> SpendAnalyses { get; set; } = new List<SpendAnalysis>();
        public List<SpendTrend> Trends { get; set; } = new List<SpendTrend>();
        public CostOptimizationAnalysis? OptimizationAnalysis { get; set; }
        public decimal TotalAnalyzedSpend { get; set; }
        public Dictionary<string, decimal> CategoryBreakdown { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> VendorBreakdown { get; set; } = new Dictionary<string, decimal>();
        public List<string> KeyInsights { get; set; } = new List<string>();
        
        // Additional properties for business logic compatibility
        public string AnalysisPeriod { get; set; } = string.Empty;
        public Dictionary<string, decimal> SpendByCategory { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> SpendByVendor { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> SpendByDepartment { get; set; } = new Dictionary<string, decimal>();
        public List<SpendTrend> SpendTrends { get; set; } = new List<SpendTrend>();
        public List<SpendAnomaly> Anomalies { get; set; } = new List<SpendAnomaly>();
        public decimal TotalSpend { get; set; }
        public List<string> TopSpendingCategories { get; set; } = new List<string>();
        public List<string> TopVendors { get; set; } = new List<string>();
        public List<string> CostSavingOpportunities { get; set; } = new List<string>();
    }

    /// <summary>
    /// Intelligent procurement processing result
    /// </summary>
    public class ProcurementProcessingResult
    {
        public bool Success { get; set; }
        public int RequestId { get; set; }
        public string ProcessorUserId { get; set; } = string.Empty;
        public DateTime ProcessingDate { get; set; }
        public VendorSelectionResult? VendorSelection { get; set; }
        public PurchaseOrderOptimizationResult? PurchaseOrderOptimization { get; set; }
        public List<string> IntelligentRecommendations { get; set; } = new List<string>();
        public List<string> ProcessingSteps { get; set; } = new List<string>();
        public string FinalStatus { get; set; } = string.Empty;
        public List<string> NextActions { get; set; } = new List<string>();
        public Dictionary<string, object> ProcessingMetadata { get; set; } = new Dictionary<string, object>();
        
        // Additional properties for business logic compatibility
        public bool AutoApproved { get; set; }
        public int ProcurementRequestId { get; set; }
        public decimal EstimatedCost { get; set; }
        public List<string> RecommendedVendors { get; set; } = new List<string>();
        public string ProcessingStrategy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Vendor strategic recommendation
    /// </summary>
    public class VendorStrategicRecommendation
    {
        public string Category { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public List<int> VendorIds { get; set; } = new List<int>();
        public string Priority { get; set; } = string.Empty;
        public string ExpectedBenefit { get; set; } = string.Empty;
        public string Timeline { get; set; } = string.Empty;
        public List<string> RequiredActions { get; set; } = new List<string>();
    }

    // === ADDITIONAL SUPPORTING MODELS ===

    /// <summary>
    /// Cost optimization action details
    /// </summary>
    public class CostOptimizationAction
    {
        public string ActionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal EstimatedSavings { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
        public string Timeline { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string Status { get; set; } = "Identified";
        public DateTime IdentifiedDate { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Contract optimization action
    /// </summary>
    public class ContractAction
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;
        public decimal EstimatedImpact { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "Pending";
        public List<string> RequiredSteps { get; set; } = new List<string>();
    }

    /// <summary>
    /// Contract optimization analysis result
    /// </summary>
    public class ContractAnalysisResult
    {
        public bool RequiresAction { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;
        public decimal EstimatedImpact { get; set; }
        public DateTime NextReviewDate { get; set; }
        public List<string> KeyFindings { get; set; } = new List<string>();
    }

    /// <summary>
    /// Consolidation opportunity identification
    /// </summary>
    public class ConsolidationOpportunity
    {
        public string Description { get; set; } = string.Empty;
        public decimal EstimatedSavings { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
        public List<int> AffectedVendors { get; set; } = new List<int>();
        public string Category { get; set; } = string.Empty;
        public double ConfidenceLevel { get; set; }
    }

    /// <summary>
    /// Budget category analysis model
    /// </summary>
    /// <summary>
    /// <summary>
    /// Spend anomaly detection model
    /// <summary>
    /// Vendor contract performance analysis
    /// </summary>
    public class VendorContractPerformance
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public double ContractComplianceRate { get; set; }
        public double DeliveryPerformance { get; set; }
        public List<string> QualityMetrics { get; set; } = new List<string>();
        
        // Additional properties for service compatibility
        public double PerformanceScore { get; set; }
        public double UtilizationRate { get; set; }
    }

    /// <summary>
    /// Procurement requirements for request analysis
    /// </summary>
    public class ProcurementRequirements
    {
        public int RequestId { get; set; }
        public List<string> RequiredItems { get; set; } = new List<string>();
        public decimal EstimatedBudget { get; set; }
        public string Urgency { get; set; } = string.Empty;
        public string TechnicalSpecs { get; set; } = string.Empty;
    }

    /// <summary>
    /// Procurement strategy recommendation
    /// </summary>
    public class ProcurementStrategy
    {
        public string Strategy { get; set; } = string.Empty;
        public string RecommendedApproach { get; set; } = string.Empty;
        public int TimelineWeeks { get; set; }
    }
}
