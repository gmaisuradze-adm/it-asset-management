using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Services
{
    /// <summary>
    /// Interface for advanced request business logic operations
    /// Provides sophisticated request management with intelligent workflow automation,
    /// predictive analytics, and seamless integration across all modules
    /// </summary>
    public interface IRequestBusinessLogicService
    {
        // === INTELLIGENT REQUEST ROUTING & ANALYSIS ===
        
        /// <summary>
        /// Comprehensive request analysis with intelligent routing recommendations
        /// </summary>
        Task<RequestAnalysisResult> AnalyzeRequestIntelligentlyAsync(int requestId);

        /// <summary>
        /// Smart request routing with automated workflow initiation
        /// </summary>
        Task<RequestRoutingResult> RouteRequestIntelligentlyAsync(int requestId, string routedByUserId);

        /// <summary>
        /// Automated service level agreement monitoring and compliance tracking
        /// </summary>
        Task<SlaComplianceResult> MonitorSlaComplianceAsync(int? requestId = null, int analysisDays = 30);

        // === PREDICTIVE ANALYTICS & DEMAND FORECASTING ===

        /// <summary>
        /// Advanced demand forecasting based on historical request patterns
        /// </summary>
        Task<RequestDemandForecast> GenerateDemandForecastAsync(int forecastDays = 90);

        /// <summary>
        /// Resource utilization optimization with workload balancing
        /// </summary>
        Task<ResourceOptimizationResult> OptimizeResourceUtilizationAsync(string initiatedByUserId);

        // === AUTOMATED WORKFLOW & PROCESS INTELLIGENCE ===

        /// <summary>
        /// Automated escalation management with intelligent rule application
        /// </summary>
        Task<EscalationManagementResult> ManageEscalationsIntelligentlyAsync();

        /// <summary>
        /// Quality assurance monitoring with automated feedback collection
        /// </summary>
        Task<QualityAssuranceResult> MonitorServiceQualityAsync(int analysisMonths = 3);

        // === WORKLOAD OPTIMIZATION & AUTOMATION ===

        /// <summary>
        /// Auto-rebalance workload among team members
        /// </summary>
        Task<WorkloadRebalanceResult> AutoRebalanceWorkloadAsync();

        /// <summary>
        /// Optimize request assignments based on skills and availability
        /// </summary>
        Task<AssignmentOptimizationResult> OptimizeAssignmentsAsync();

        /// <summary>
        /// Get comprehensive resource optimization data
        /// </summary>
        Task<ResourceOptimizationResult> GetResourceOptimizationAsync();

        // === INTEGRATION & CROSS-MODULE ORCHESTRATION ===

        /// <summary>
        /// Comprehensive cross-module integration orchestration
        /// </summary>
        Task<IntegrationOrchestrationResult> OrchestrateCrossModuleWorkflowAsync(int requestId, string orchestratorUserId);
    }
}
