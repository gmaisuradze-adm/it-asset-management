using System.ComponentModel.DataAnnotations;

namespace HospitalAssetTracker.Models
{
    // Note: RequestType, RequestStatus, RequestPriority enums are defined in ITRequest.cs
    
    /// <summary>
    /// Request routing options for intelligent request management
    /// </summary>
    public enum RequestRoute
    {
        AutoApprove,
        ManagerApproval,
        ITApproval,
        ProcurementWorkflow,
        InventoryCheck,
        ExternalVendor,
        EmergencyRoute,
        StandardWorkflow,
        EscalationRequired,
        CrossDepartmental,
        InventoryFulfillment,
        AssetMaintenance,
        ProcurementRequired,
        HybridApproach
    }
    
    // === REQUEST ANALYSIS & ROUTING MODELS ===

    /// <summary>
    /// Comprehensive request analysis result with intelligent insights
    /// </summary>
    public class RequestAnalysisResult
    {
        public int RequestId { get; set; }
        public DateTime AnalysisDate { get; set; }
        public RequestType RequestType { get; set; }
        public RequestPriority Priority { get; set; }
        public string Department { get; set; } = string.Empty;
        public List<FulfillmentOption> FulfillmentOptions { get; set; } = new List<FulfillmentOption>();
        public RequestRoute RecommendedRoute { get; set; }
        public int EstimatedEffort { get; set; }
        public int ComplexityScore { get; set; }
        public List<string> StrategicRecommendations { get; set; } = new List<string>();
        public List<string> RiskFactors { get; set; } = new List<string>();
        public List<string> Dependencies { get; set; } = new List<string>();
        public double SuccessProbability { get; set; }
        public string AnalysisNotes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Fulfillment option analysis
    /// </summary>
    public class FulfillmentOption
    {
        public string OptionType { get; set; } = string.Empty;
        public bool Available { get; set; }
        public string EstimatedTimeframe { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public int Feasibility { get; set; }
        public List<string> Requirements { get; set; } = new List<string>();
        public List<string> Constraints { get; set; } = new List<string>();
    }

    /// <summary>
    /// Request routing result with automated workflow initiation
    /// </summary>
    public class RequestRoutingResult
    {
        public int RequestId { get; set; }
        public string RoutedByUserId { get; set; } = string.Empty;
        public DateTime RoutingDate { get; set; }
        public RequestRoute SelectedRoute { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string RoutedTo { get; set; } = string.Empty;
        public TimeSpan EstimatedCompletionTime { get; set; }
        public List<string> RoutingActions { get; set; } = new List<string>();
        public List<string> AutomatedTriggers { get; set; } = new List<string>();
        public string NextSteps { get; set; } = string.Empty;
        public DateTime EstimatedCompletion { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Service Level Agreement compliance monitoring result
    /// </summary>
    public class SlaComplianceResult
    {
        public DateTime AnalysisDate { get; set; }
        public int AnalysisPeriodDays { get; set; }
        public int TotalRequestsAnalyzed { get; set; }
        public List<RequestSlaCompliance> RequestCompliances { get; set; } = new List<RequestSlaCompliance>();
        public double OverallComplianceRate { get; set; }
        public int CriticalViolations { get; set; }
        public int HighViolations { get; set; }
        public List<string> ImprovementRecommendations { get; set; } = new List<string>();
        public Dictionary<RequestPriority, double> ComplianceByPriority { get; set; } = new Dictionary<RequestPriority, double>();
        public Dictionary<string, double> ComplianceByDepartment { get; set; } = new Dictionary<string, double>();
        
        // Additional property for compatibility
        public double AverageResolutionTime => RequestCompliances.Any() 
            ? RequestCompliances.Average(rc => rc.ActualResolutionHours) 
            : 0.0;
            
        // Properties for view compatibility
        public double AverageResponseTimeHours { get; set; }
        public int AtRiskCount { get; set; }
        public int BreachedCount { get; set; }
        public Dictionary<string, double> PriorityBreakdown { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Individual request SLA compliance details
    /// </summary>
    public class RequestSlaCompliance
    {
        public int RequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public RequestPriority Priority { get; set; }
        public string Department { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int SlaTargetHours { get; set; }
        public int ActualResolutionHours { get; set; }
        public double ComplianceScore { get; set; }
        public string SlaStatus { get; set; } = string.Empty;
        public string ComplianceNotes { get; set; } = string.Empty;
    }

    // === DEMAND FORECASTING & ANALYTICS MODELS ===

    /// <summary>
    /// Comprehensive demand forecasting with predictive analytics
    /// </summary>
    public class RequestDemandForecast
    {
        public DateTime ForecastDate { get; set; }
        public int ForecastPeriodDays { get; set; }
        public int HistoricalDataPoints { get; set; }
        public List<CategoryDemandForecast> CategoryForecasts { get; set; } = new List<CategoryDemandForecast>();
        public List<ResourceRequirement> ResourceRequirements { get; set; } = new List<ResourceRequirement>();
        public List<string> StrategicInsights { get; set; } = new List<string>();
        public double ForecastAccuracy { get; set; }
        public Dictionary<string, int> DepartmentTrends { get; set; } = new Dictionary<string, int>();
        public List<SeasonalPattern> SeasonalPatterns { get; set; } = new List<SeasonalPattern>();
        public List<string> RecommendedActions { get; set; } = new List<string>();
    }

    /// <summary>
    /// Category-specific demand forecast
    /// </summary>
    public class CategoryDemandForecast
    {
        public RequestType RequestType { get; set; }
        public string Category { get; set; } = string.Empty;
        public int ForecastedRequests { get; set; }
        public int HistoricalAverage { get; set; }
        public double GrowthRate { get; set; }
        public double ConfidenceLevel { get; set; }
        public List<MonthlyDemandForecast> MonthlyBreakdown { get; set; } = new List<MonthlyDemandForecast>();
        public List<string> DemandDrivers { get; set; } = new List<string>();
        
        // Alias properties for compatibility
        public string CategoryName => Category;
        public int CurrentDemand => HistoricalAverage;
        public int PredictedDemand => ForecastedRequests;
        public int PredictedVolume => ForecastedRequests;
        public string TrendDirection => GrowthRate > 0 ? "Up" : GrowthRate < 0 ? "Down" : "Stable";
    }

    /// <summary>
    /// Monthly demand forecast breakdown
    /// </summary>
    public class MonthlyDemandForecast
    {
        public DateTime Month { get; set; }
        public int ForecastedRequests { get; set; }
        public double ConfidenceLevel { get; set; }
        public List<string> Assumptions { get; set; } = new List<string>();
    }

    /// <summary>
    /// Resource requirement analysis
    /// </summary>
    public class ResourceRequirement
    {
        public string ResourceType { get; set; } = string.Empty;
        public int RequiredQuantity { get; set; }
        public int CurrentAvailable { get; set; }
        public int Gap { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
    }

    /// <summary>
    /// Seasonal demand pattern analysis
    /// </summary>
    public class SeasonalPattern
    {
        public string PatternName { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public double Multiplier { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<string> AffectedCategories { get; set; } = new List<string>();
    }

    // === RESOURCE OPTIMIZATION MODELS ===

    /// <summary>
    /// Resource utilization optimization result
    /// </summary>
    public class ResourceOptimizationResult
    {
        public DateTime OptimizationDate { get; set; }
        public string InitiatedByUserId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public WorkloadAnalysis WorkloadAnalysis { get; set; } = new WorkloadAnalysis();
        public List<RequestOptimizationOpportunity> OptimizationOpportunities { get; set; } = new List<RequestOptimizationOpportunity>();
        public WorkloadRebalancingPlan RebalancingPlan { get; set; } = new WorkloadRebalancingPlan();
        public List<PerformanceImprovement> ProjectedImprovements { get; set; } = new List<PerformanceImprovement>();
        public List<string> ProcessingMessages { get; set; } = new List<string>();
        
        // Properties for view compatibility
        public int SuggestedTeamSize { get; set; }
        public double CurrentUtilizationRate { get; set; }
        public List<string> Recommendations { get; set; } = new();
        public int CurrentTeamSize { get; set; }
        public double OptimalUtilizationRate { get; set; }
        public Dictionary<string, int> WorkloadDistribution { get; set; } = new();
        public double AverageWorkload { get; set; }
        public double PeakCapacity { get; set; }
        public double CurrentWorkload { get; set; }
    }

    /// <summary>
    /// Current workload analysis
    /// </summary>
    public class WorkloadAnalysis
    {
        public int TotalActiveRequests { get; set; }
        public int TotalTechnicians { get; set; }
        public double AverageWorkloadPerTechnician { get; set; }
        public List<TechnicianWorkload> TechnicianWorkloads { get; set; } = new List<TechnicianWorkload>();
        public Dictionary<RequestPriority, int> RequestsByPriority { get; set; } = new Dictionary<RequestPriority, int>();
        public Dictionary<string, int> RequestsByDepartment { get; set; } = new Dictionary<string, int>();
        public double WorkloadBalance { get; set; }
        
        // Additional properties for controller compatibility
        public List<DepartmentWorkload> DepartmentWorkloads { get; set; } = new();
        public List<CriticalBottleneck> CriticalBottlenecks { get; set; } = new();
    }

    /// <summary>
    /// Individual technician workload details
    /// </summary>
    public class TechnicianWorkload
    {
        public string TechnicianId { get; set; } = string.Empty;
        public string TechnicianName { get; set; } = string.Empty;
        public int AssignedRequests { get; set; }
        public int OverdueRequests { get; set; }
        public double UtilizationRate { get; set; }
        public List<string> Specializations { get; set; } = new List<string>();
        public string WorkloadStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Optimization opportunity identification
    /// </summary>
    public class RequestOptimizationOpportunity
    {
        public string OpportunityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty;
        public string Effort { get; set; } = string.Empty;
        public int Priority { get; set; }
        public List<string> RequiredActions { get; set; } = new List<string>();
        public string ExpectedBenefit { get; set; } = string.Empty;
        
        // Additional properties for compatibility
        public double PotentialImpact => Impact switch
        {
            "High" => 90.0,
            "Medium" => 60.0,
            "Low" => 30.0,
            _ => 50.0
        };
        
        public int EstimatedEffort => Effort switch
        {
            "High" => 8,
            "Medium" => 5,
            "Low" => 2,
            _ => 5
        };
    }

    /// <summary>
    /// Request reassignment recommendation
    /// </summary>
    public class RequestReassignment
    {
        public int RequestId { get; set; }
        public string FromTechnicianId { get; set; } = string.Empty;
        public string ToTechnicianId { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
    }

    /// <summary>
    /// Performance improvement projection
    /// </summary>
    public class PerformanceImprovement
    {
        public string MetricName { get; set; } = string.Empty;
        public double CurrentValue { get; set; }
        public double ProjectedValue { get; set; }
        public double ImprovementPercentage { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    // === ESCALATION & QUALITY MANAGEMENT MODELS ===

    /// <summary>
    /// Escalation management result
    /// </summary>
    public class EscalationManagementResult
    {
        public DateTime ProcessingDate { get; set; }
        public bool Success { get; set; }
        public int RequestsEvaluated { get; set; }
        public int EscalationCandidates { get; set; }
        public List<EscalationAction> EscalationActions { get; set; } = new List<EscalationAction>();
        public List<string> ProcessingMessages { get; set; } = new List<string>();
    }

    /// <summary>
    /// Escalation action details
    /// </summary>
    public class EscalationAction
    {
        public int RequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string EscalationType { get; set; } = string.Empty;
        public string EscalatedTo { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime EscalationDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> NotificationsSent { get; set; } = new List<string>();

        // Additional properties for service compatibility
        public string ActionType { get; set; } = string.Empty;
        public DateTime ActionDate { get; set; }
        public bool Success { get; set; }
    }

    /// <summary>
    /// Quality assurance monitoring result
    /// </summary>
    public class QualityAssuranceResult
    {
        public DateTime AnalysisDate { get; set; }
        public int AnalysisPeriodMonths { get; set; }
        public int TotalRequestsAnalyzed { get; set; }
        public List<RequestQualityMetrics> QualityMetrics { get; set; } = new List<RequestQualityMetrics>();
        public double OverallSatisfactionScore { get; set; }
        public double AverageResolutionTime { get; set; }
        public double FirstCallResolutionRate { get; set; }
        public List<string> ImprovementRecommendations { get; set; } = new List<string>();
        public Dictionary<string, double> SatisfactionByCategory { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> ResolutionTimeByCategory { get; set; } = new Dictionary<string, double>();
        
        // Properties for view compatibility
        public double OverallQualityScore { get; set; }
        public double CustomerSatisfactionRate { get; set; }
        public double ReworkRate { get; set; }
        public Dictionary<string, double> QualityByCategory { get; set; } = new();
        public Dictionary<string, int> CommonIssues { get; set; } = new();
        public List<string> ImprovementAreas { get; set; } = new();
    }

    /// <summary>
    /// Individual request quality metrics
    /// </summary>
    public class RequestQualityMetrics
    {
        public int RequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public RequestType RequestType { get; set; }
        public double SatisfactionScore { get; set; }
        public double ResolutionTimeHours { get; set; }
        public bool MetSlaTargets { get; set; }
        public int ReworkRequired { get; set; }
        public string QualityNotes { get; set; } = string.Empty;
        public List<string> IssuesIdentified { get; set; } = new List<string>();
    }

    // === INTEGRATION & ORCHESTRATION MODELS ===

    /// <summary>
    /// Cross-module integration orchestration result
    /// </summary>
    public class IntegrationOrchestrationResult
    {
        public int RequestId { get; set; }
        public string OrchestratorUserId { get; set; } = string.Empty;
        public DateTime OrchestrationDate { get; set; }
        public bool Success { get; set; }
        public Dictionary<string, ModuleIntegrationResult> IntegrationResults { get; set; } = new Dictionary<string, ModuleIntegrationResult>();
        public List<string> OrchestrationSteps { get; set; } = new List<string>();
        public string ErrorMessage { get; set; } = string.Empty;
        public Dictionary<string, object> IntegrationMetadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Individual module integration result
    /// </summary>
    public class ModuleIntegrationResult
    {
        public string ModuleName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public DateTime IntegrationTime { get; set; }
        public List<string> ActionsPerformed { get; set; } = new List<string>();
        public bool RequiresProcurement { get; set; }
        public string Status { get; set; } = string.Empty;
        public Dictionary<string, object> ResultData { get; set; } = new Dictionary<string, object>();
    }

    // === VIEW MODELS FOR REQUEST DASHBOARD ===

    /// <summary>
    /// Main request dashboard view model
    /// </summary>
    public class RequestDashboardViewModel
    {
        public int TotalActiveRequests { get; set; }
        public int PendingApprovals { get; set; }
        public int OverdueRequests { get; set; }
        public int CompletedToday { get; set; }
        public double AverageResponseTime { get; set; }
        public double SlaComplianceRate { get; set; }
        public List<RequestTypeMetrics> RequestsByType { get; set; } = new List<RequestTypeMetrics>();
        public List<DepartmentMetrics> RequestsByDepartment { get; set; } = new List<DepartmentMetrics>();
        public List<PriorityMetrics> RequestsByPriority { get; set; } = new List<PriorityMetrics>();
        public List<ITRequest> RecentRequests { get; set; } = new List<ITRequest>();
        public List<ITRequest> HighPriorityRequests { get; set; } = new List<ITRequest>();
        public RequestTrendData TrendData { get; set; } = new RequestTrendData();
        
        // Additional properties for advanced dashboard
        public RequestDemandForecast DemandForecast { get; set; } = new RequestDemandForecast();
        public SlaComplianceResult SlaCompliance { get; set; } = new SlaComplianceResult();
        public QualityAssuranceResult QualityMetrics { get; set; } = new QualityAssuranceResult();
        public List<ITRequest> OverdueRequestsList { get; set; } = new List<ITRequest>();
        public string CurrentUserId { get; set; } = string.Empty;
        public DateTime LastRefreshed { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Request analysis view model
    /// </summary>
    public class RequestAnalysisViewModel
    {
        public RequestAnalysisResult AnalysisResult { get; set; } = new RequestAnalysisResult();
        public List<string> RecommendedActions { get; set; } = new List<string>();
        public List<ResourceAvailability> ResourceStatus { get; set; } = new List<ResourceAvailability>();
        public EstimatedTimeline Timeline { get; set; } = new EstimatedTimeline();
        
        // Additional properties for controller compatibility
        public RequestAnalysisResult Analysis { get; set; } = new RequestAnalysisResult();
        public int RequestId { get; set; }
        public RequestRoute RecommendedRoute { get; set; }
        public string ComplexityLevel { get; set; } = string.Empty;
        public int EstimatedEffort { get; set; }
        public List<string> RiskFactors { get; set; } = new List<string>();
    }

    /// <summary>
    /// SLA compliance view model
    /// </summary>
    public class SlaComplianceViewModel
    {
        public double OverallComplianceRate { get; set; }
        public List<SlaMetric> ComplianceByPriority { get; set; } = new List<SlaMetric>();
        public List<SlaMetric> ComplianceByType { get; set; } = new List<SlaMetric>();
        public List<SlaMetric> ComplianceByDepartment { get; set; } = new List<SlaMetric>();
        public List<ITRequest> SlaViolations { get; set; } = new List<ITRequest>();
        public SlaAnalysisData TrendAnalysis { get; set; } = new SlaAnalysisData();
        
        // Additional properties for controller compatibility
        public SlaComplianceResult Compliance { get; set; } = new SlaComplianceResult();
        public string AnalysisPeriod { get; set; } = string.Empty;
        public int TotalViolations { get; set; }
        public List<string> ImprovementActions { get; set; } = new List<string>();
    }

    /// <summary>
    /// Demand forecasting view model
    /// </summary>
    public class DemandForecastingViewModel
    {
        public DemandForecastResult ForecastData { get; set; } = new DemandForecastResult();
        public List<ResourceDemand> PredictedDemands { get; set; } = new List<ResourceDemand>();
        public List<SeasonalPattern> SeasonalTrends { get; set; } = new List<SeasonalPattern>();
        public List<string> Recommendations { get; set; } = new List<string>();
        
        // Additional properties for controller compatibility
        public DemandForecastResult Forecast { get; set; } = new DemandForecastResult();
        public ResourceOptimizationResult ResourceOptimization { get; set; } = new ResourceOptimizationResult();
        public string ForecastPeriod { get; set; } = string.Empty;
        public double ForecastAccuracy { get; set; }
        public List<string> KeyInsights { get; set; } = new List<string>();
    }

    /// <summary>
    /// Resource optimization view model
    /// </summary>
    public class ResourceOptimizationViewModel
    {
        public ResourceOptimizationResult OptimizationResult { get; set; } = new ResourceOptimizationResult();
        public List<ResourceAllocation> CurrentAllocations { get; set; } = new List<ResourceAllocation>();
        public List<ResourceAllocation> OptimizedAllocations { get; set; } = new List<ResourceAllocation>();
        public List<string> OptimizationRecommendations { get; set; } = new List<string>();
        
        // Additional properties for controller compatibility
        public ResourceOptimizationResult Optimization { get; set; } = new ResourceOptimizationResult();
        public WorkloadBalanceResult WorkloadBalance { get; set; } = new WorkloadBalanceResult();
        public List<string> OptimizationOpportunities { get; set; } = new List<string>();
        public List<string> ProjectedImprovements { get; set; } = new List<string>();
    }

    /// <summary>
    /// Quality assurance view model
    /// </summary>
    public class QualityAssuranceViewModel
    {
        public QualityAssuranceResult QualityMetrics { get; set; } = new QualityAssuranceResult();
        
        // Additional properties for controller compatibility
        public QualityAssuranceResult QualityResult { get; set; } = new QualityAssuranceResult();
        public string AnalysisPeriod { get; set; } = string.Empty;
        public double OverallSatisfaction { get; set; }
        public double AverageResolutionTime { get; set; }
        public List<string> ImprovementActions { get; set; } = new List<string>();
        public List<QualityIssue> IdentifiedIssues { get; set; } = new List<QualityIssue>();
        public List<QualityImprovement> ImprovementSuggestions { get; set; } = new List<QualityImprovement>();
        public double OverallQualityScore { get; set; }
    }

    // === SUPPORTING CLASSES FOR VIEW MODELS ===

    /// <summary>
    /// Request type metrics
    /// </summary>
    public class RequestTypeMetrics
    {
        public RequestType Type { get; set; }
        public int Count { get; set; }
        public double AverageResolutionTime { get; set; }
        public double SlaComplianceRate { get; set; }
    }

    /// <summary>
    /// Department metrics
    /// </summary>
    public class DepartmentMetrics
    {
        public string Department { get; set; } = string.Empty;
        public int RequestCount { get; set; }
        public double AverageResolutionTime { get; set; }
        public double SatisfactionScore { get; set; }
    }

    /// <summary>
    /// Priority metrics
    /// </summary>
    public class PriorityMetrics
    {
        public RequestPriority Priority { get; set; }
        public int Count { get; set; }
        public double AverageResponseTime { get; set; }
        public int OverdueCount { get; set; }
    }

    /// <summary>
    /// Request trend data
    /// </summary>
    public class RequestTrendData
    {
        public List<int> LastWeekData { get; set; } = new List<int>();
        public List<int> LastMonthData { get; set; } = new List<int>();
        public List<string> TrendLabels { get; set; } = new List<string>();
        public double GrowthRate { get; set; }
    }

    /// <summary>
    /// Resource availability
    /// </summary>
    public class ResourceAvailability
    {
        public string ResourceType { get; set; } = string.Empty;
        public bool Available { get; set; }
        public int Quantity { get; set; }
        public string EstimatedAvailabilityDate { get; set; } = string.Empty;
    }

    /// <summary>
    /// Estimated timeline
    /// </summary>
    public class EstimatedTimeline
    {
        public DateTime StartDate { get; set; }
        public DateTime EstimatedCompletionDate { get; set; }
        public List<TimelinePhase> Phases { get; set; } = new List<TimelinePhase>();
        public int TotalEstimatedHours { get; set; }
    }

    /// <summary>
    /// Timeline phase
    /// </summary>
    public class TimelinePhase
    {
        public string PhaseName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// SLA metric
    /// </summary>
    public class SlaMetric
    {
        public string Category { get; set; } = string.Empty;
        public double ComplianceRate { get; set; }
        public int TotalRequests { get; set; }
        public int CompliantRequests { get; set; }
        public double AverageResponseTime { get; set; }
    }

    /// <summary>
    /// SLA analysis data
    /// </summary>
    public class SlaAnalysisData
    {
        public List<double> ComplianceTrend { get; set; } = new List<double>();
        public List<string> TrendLabels { get; set; } = new List<string>();
        public double TargetComplianceRate { get; set; }
        public string TrendDirection { get; set; } = string.Empty;
    }

    /// <summary>
    /// Resource demand
    /// </summary>
    public class ResourceDemand
    {
        public string ResourceType { get; set; } = string.Empty;
        public int PredictedDemand { get; set; }
        public DateTime ForecastDate { get; set; }
        public double Confidence { get; set; }
    }

    /// <summary>
    /// Resource allocation
    /// </summary>
    public class ResourceAllocation
    {
        public string ResourceId { get; set; } = string.Empty;
        public string ResourceName { get; set; } = string.Empty;
        public string CurrentAssignment { get; set; } = string.Empty;
        public double UtilizationRate { get; set; }
        public string OptimizedAssignment { get; set; } = string.Empty;
    }

    /// <summary>
    /// Quality issue
    /// </summary>
    public class QualityIssue
    {
        public string IssueType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public List<string> AffectedRequests { get; set; } = new List<string>();
    }

    /// <summary>
    /// Quality improvement
    /// </summary>
    public class QualityImprovement
    {
        public string Area { get; set; } = string.Empty;
        public string Suggestion { get; set; } = string.Empty;
        public double EstimatedImpact { get; set; }
        public string Implementation { get; set; } = string.Empty;
    }

    // === MISSING SUPPORTING CLASSES ===

    /// <summary>
    /// Demand forecast result
    /// </summary>
    public class DemandForecastResult
    {
        public DateTime ForecastDate { get; set; }
        public int ForecastPeriodDays { get; set; }
        public List<ResourceDemand> PredictedDemands { get; set; } = new List<ResourceDemand>();
        public List<SeasonalPattern> SeasonalPatterns { get; set; } = new List<SeasonalPattern>();
        public double ConfidenceLevel { get; set; }
        public List<string> Assumptions { get; set; } = new List<string>();
        public List<string> RiskFactors { get; set; } = new List<string>();
        public string ModelUsed { get; set; } = string.Empty;
        
        // Properties for controller compatibility
        public double ForecastAccuracy { get; set; } = 85.5;
        public List<string> StrategicInsights { get; set; } = new();
        public List<RequestCategoryForecast> CategoryForecasts { get; set; } = new();
        public List<string> RecommendedActions { get; set; } = new();
        
        // Properties for view compatibility
        public int TotalProjectedRequests { get; set; }
        public string PeakPeriod { get; set; } = string.Empty;
        public Dictionary<string, int> CategoryBreakdown { get; set; } = new();
    }

    /// <summary>
    /// Workload balance result
    /// </summary>
    public class WorkloadBalanceResult
    {
        public double OverallBalance { get; set; }
        public List<ResourceWorkload> Workloads { get; set; } = new List<ResourceWorkload>();
        public List<string> Recommendations { get; set; } = new List<string>();
        public DateTime AnalysisDate { get; set; }
        
        // Properties for controller compatibility
        public List<DepartmentWorkload> DepartmentWorkloads { get; set; } = new();
        public List<CriticalBottleneck> CriticalBottlenecks { get; set; } = new();
    }

    /// <summary>
    /// Resource workload details
    /// </summary>
    public class ResourceWorkload
    {
        public string ResourceId { get; set; } = string.Empty;
        public string ResourceName { get; set; } = string.Empty;
        public double CurrentLoad { get; set; }
        public double OptimalLoad { get; set; }
        public double UtilizationRate { get; set; }
        public string Status { get; set; } = string.Empty; // "Underutilized", "Optimal", "Overloaded"
    }

    /// <summary>
    /// Department workload details
    /// </summary>
    public class DepartmentWorkload
    {
        public string DepartmentName { get; set; } = string.Empty;
        public int RequestCount { get; set; }
        public double AverageResolutionTime { get; set; }
        public double WorkloadPercentage { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Critical bottleneck information
    /// </summary>
    public class CriticalBottleneck
    {
        public string BottleneckType { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double ImpactScore { get; set; }
        public List<string> RecommendedActions { get; set; } = new();
    }

    // === ESCALATION MODELS ===

    /// <summary>
    /// Request escalation criteria
    /// </summary>
    public class EscalationCriteria
    {
        public string Reason { get; set; } = string.Empty;
        public bool IsAutomatic { get; set; }
        public TimeSpan MaxWaitTime { get; set; }
        public RequestPriority MinimumPriority { get; set; }
        
        // Additional properties for service compatibility
        public int MaxResolutionTimeHours { get; set; } = 24;
        public int CriticalPriorityThresholdHours { get; set; } = 2;
        public int HighPriorityThresholdHours { get; set; } = 8;
        public int MediumPriorityThresholdHours { get; set; } = 24;
        public int LowPriorityThresholdHours { get; set; } = 72;
    }

    /// <summary>
    /// Escalation result information
    /// </summary>
    public class EscalationResult
    {
        public bool Success { get; set; }
        public int EscalationLevel { get; set; }
        public string EscalatedTo { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime NextReviewDate { get; set; }
    }

    /// <summary>
    /// Escalation path configuration
    /// </summary>
    public class EscalationPath
    {
        public string NextApprover { get; set; } = string.Empty;
        public int ReviewTimeHours { get; set; }
    }

    /// <summary>
    /// Request escalation entity
    /// </summary>
    public class RequestEscalation
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public int EscalationLevel { get; set; }
        public DateTime EscalatedDate { get; set; }
        public string EscalatedTo { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public bool AutoEscalated { get; set; }
        
        // Navigation properties
        public ITRequest Request { get; set; } = null!;
    }

    /// <summary>
    /// Historical data for demand analysis
    /// </summary>
    public class RequestDemandHistoricalData
    {
        public RequestType RequestType { get; set; }
        public string Department { get; set; } = string.Empty;
        public int Month { get; set; }
        public int RequestCount { get; set; }
        public double AverageResolutionTime { get; set; }
    }

    /// <summary>
    /// Request category forecast
    /// </summary>
    public class RequestCategoryForecast
    {
        public RequestType RequestType { get; set; }
        public int PredictedVolume { get; set; }
        public double ConfidenceLevel { get; set; }
        public string TrendDirection { get; set; } = string.Empty;
        
        // Additional properties for compatibility
        public string CategoryName => RequestType.ToString();
        public int CurrentDemand { get; set; }
        public int PredictedDemand => PredictedVolume;
    }

    /// <summary>
    /// Workload analysis result
    /// </summary>
    public class WorkloadAnalysisResult
    {
        public int TotalActiveRequests { get; set; }
        public double WorkloadBalance { get; set; }
        public Dictionary<string, int> DepartmentWorkloads { get; set; } = new Dictionary<string, int>();
        public List<string> CriticalBottlenecks { get; set; } = new List<string>();
    }

    /// <summary>
    /// Workload rebalancing plan
    /// </summary>
    public class WorkloadRebalancingPlan
    {
        public string PlanName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public TimeSpan EstimatedImplementationTime { get; set; }
        public List<string> ExpectedImprovements { get; set; } = new List<string>();
        public List<string> RecommendedActions { get; set; } = new List<string>();
    }

    /// <summary>
    /// Result of workload rebalancing operation
    /// </summary>
    public class WorkloadRebalanceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RequestsRebalanced { get; set; }
        public List<string> Changes { get; set; } = new();
        public Dictionary<string, int> NewWorkloadDistribution { get; set; } = new();
        public double ImprovementPercentage { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Result of assignment optimization operation
    /// </summary>
    public class AssignmentOptimizationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int OptimizedAssignments { get; set; }
        public List<string> OptimizationActions { get; set; } = new();
        public Dictionary<string, List<string>> SkillBasedMatches { get; set; } = new();
        public double EfficiencyImprovement { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Integration result with external modules
    /// </summary>
    public class IntegrationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string[] IntegratedModules { get; set; } = new string[0];
        
        // Additional properties for service compatibility
        public bool RequiresProcurement { get; set; }
    }

    /// <summary>
    /// Service quality analysis result
    /// </summary>
    public class ServiceQualityResult
    {
        public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
        public int AnalysisPeriodDays { get; set; }
        public double OverallSatisfactionScore { get; set; }
        public TimeSpan AverageResolutionTime { get; set; }
        public double FirstCallResolutionRate { get; set; }
        public int TotalRequestsAnalyzed { get; set; }
        public int CompletedRequests { get; set; }
        public int EscalatedRequests { get; set; }
        public double QualityScore { get; set; }
        public List<string> ImprovementRecommendations { get; set; } = new List<string>();
        public Dictionary<string, double> QualityMetrics { get; set; } = new Dictionary<string, double>();
        public List<string> CommonIssues { get; set; } = new List<string>();
        public List<string> BestPractices { get; set; } = new List<string>();
    }
}
