using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Services
{
    /// <summary>
    /// Advanced business logic service for Asset module integration and intelligent operations
    /// </summary>
    public interface IAssetBusinessLogicService
    {
        // === DASHBOARD AND ANALYTICS METHODS ===
        
        /// <summary>
        /// Gets comprehensive asset dashboard data
        /// </summary>
        Task<AssetDashboardModel> GetAssetDashboardAsync(string userId);
        Task<AssetDashboardViewModel> GetAssetDashboardViewModelAsync(string userId);
        
        /// <summary>
        /// Gets asset analytics data
        /// </summary>
        Task<AssetAnalyticsViewModel> GetAssetAnalyticsAsync(string userId, int months = 12);
        
        /// <summary>
        /// Analyzes asset performance metrics
        /// </summary>
        AssetPerformanceAnalysisResult AnalyzeAssetPerformance(int assetId, string analystUserId);
        
        /// <summary>
        /// Gets asset optimization opportunities
        /// </summary>
        Task<List<AssetOptimizationOpportunity>> GetAssetOptimizationOpportunitiesAsync(string userId);
        
        /// <summary>
        /// Gets asset alerts for user
        /// </summary>
        Task<List<AssetAlert>> GetAssetAlertsAsync(string userId);
        
        /// <summary>
        /// Acknowledges an asset alert
        /// </summary>
        bool AcknowledgeAlert(int alertId, string userId);
        
        /// <summary>
        /// Exports dashboard data
        /// </summary>
        byte[] ExportDashboardData(string format, string userId);
        
        /// <summary>
        /// Exports analytics data
        /// </summary>
        byte[] ExportAnalyticsData(string format, string userId);
        
        /// <summary>
        /// Gets asset performance report
        /// </summary>
        AssetPerformanceReportResult GetAssetPerformanceReport(string userId);
        
        /// <summary>
        /// Exports performance data
        /// </summary>
        byte[] ExportPerformanceData(string format, string userId);
        
        // === INTELLIGENT ASSET LIFECYCLE MANAGEMENT ===
        
        /// <summary>
        /// Analyzes asset lifecycle and provides comprehensive insights
        /// </summary>
        Task<AssetLifecycleAnalysisResult> AnalyzeAssetLifecycleAsync(int assetId, string analystUserId);
        
        /// <summary>
        /// Predicts asset replacement needs using ML algorithms
        /// </summary>
        Task<AssetReplacementForecastResult> PredictAssetReplacementNeedsAsync(int forecastPeriodDays, string initiatedByUserId);
        
        /// <summary>
        /// Optimizes asset utilization across departments
        /// </summary>
        Task<AssetUtilizationOptimizationResult> OptimizeAssetUtilizationAsync(string optimizerUserId);
        
        /// <summary>
        /// Provides intelligent maintenance scheduling recommendations
        /// </summary>
        Task<IntelligentMaintenanceScheduleResult> GenerateIntelligentMaintenanceScheduleAsync(int planningPeriodDays, string schedulerUserId);
        
        // === CROSS-MODULE INTEGRATION WORKFLOWS ===
        
        /// <summary>
        /// Orchestrates asset deployment from inventory to active use
        /// </summary>
        Task<AssetDeploymentResult> OrchestateAssetDeploymentFromInventoryAsync(int inventoryItemId, int targetLocationId, string deployedByUserId);
        
        /// <summary>
        /// Processes asset retirement and integration with procurement for replacements
        /// </summary>
        Task<AssetRetirementResult> ProcessAssetRetirementWithReplacementAsync(int assetId, string retirementReason, bool triggerReplacement, string processedByUserId);
        
        /// <summary>
        /// Handles asset service requests and routing to appropriate modules
        /// </summary>
        AssetServiceRequestResult ProcessAssetServiceRequest(int requestId, string processorUserId);
        
        /// <summary>
        /// Synchronizes asset data across all integrated modules
        /// </summary>
        CrossModuleAssetSyncResult SynchronizeAssetDataAcrossModules(int assetId, string synchronizedByUserId);
        
        // === PREDICTIVE ANALYTICS & INSIGHTS ===
        
        /// <summary>
        /// Analyzes asset health using IoT data and usage patterns
        /// </summary>
        AssetHealthAnalysisResult AnalyzeAssetHealth(int assetId, string analystUserId);
        
        /// <summary>
        /// Provides cost-benefit analysis for asset investments
        /// </summary>
        AssetCostBenefitAnalysisResult PerformAssetCostBenefitAnalysis(AssetInvestmentRequest request, string analystUserId);
        
        /// <summary>
        /// Generates asset ROI and performance metrics
        /// </summary>
        AssetPerformanceMetricsResult GenerateAssetPerformanceMetrics(DateTime fromDate, DateTime toDate, string reportGeneratorUserId);
        
        /// <summary>
        /// Predicts asset failure risks using advanced algorithms
        /// </summary>
        AssetFailureRiskAssessmentResult AssessAssetFailureRisks(List<int> assetIds, string assessorUserId);
        
        // === STRATEGIC ASSET MANAGEMENT ===
        
        /// <summary>
        /// Provides strategic asset portfolio optimization recommendations
        /// </summary>
        AssetPortfolioOptimizationResult OptimizeAssetPortfolio(string optimizerUserId);
        
        /// <summary>
        /// Analyzes asset compliance and regulatory requirements
        /// </summary>
        AssetComplianceAnalysisResult AnalyzeAssetCompliance(string complianceOfficerUserId);
        
        /// <summary>
        /// Generates intelligent asset budgeting and planning recommendations
        /// </summary>
        AssetBudgetPlanningResult GenerateAssetBudgetPlanning(int fiscalYear, string plannerUserId);
        
        /// <summary>
        /// Provides asset security and risk management analysis
        /// </summary>
        AssetSecurityRiskAnalysisResult AnalyzeAssetSecurityRisks(string securityOfficerUserId);
        
        // === AUTOMATION & WORKFLOW ORCHESTRATION ===
        
        /// <summary>
        /// Automates routine asset management tasks
        /// </summary>
        AutomatedAssetManagementTaskResult ExecuteAutomatedAssetManagementTasks(string taskExecutorUserId);
        
        /// <summary>
        /// Orchestrates complex multi-module asset workflows
        /// </summary>
        Task<AssetWorkflowOrchestrationResult> OrchestateComplexAssetWorkflow(AssetWorkflowRequest request, string orchestratorUserId);
        
        /// <summary>
        /// Provides intelligent asset alerting and notification system
        /// </summary>
        AssetAlertingResult ProcessIntelligentAssetAlerting(string alertProcessorUserId);
        
        /// <summary>
        /// Generates comprehensive asset lifecycle reports with cross-module data
        /// </summary>
        AssetLifecycleReportResult GenerateComprehensiveAssetLifecycleReport(AssetReportingCriteria criteria, string reportGeneratorUserId);
    }
}
