using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Services
{
    /// <summary>
    /// Interface for advanced procurement business logic operations
    /// Provides sophisticated procurement management with vendor intelligence, 
    /// cost optimization, and seamless integration across all modules
    /// </summary>
    public interface IProcurementBusinessLogicService
    {
        // === VENDOR INTELLIGENCE & PERFORMANCE ===
        
        /// <summary>
        /// Comprehensive vendor performance analysis with scoring and ranking
        /// </summary>
        Task<VendorPerformanceAnalysis> AnalyzeVendorPerformanceAsync(int? vendorId = null, int analysisMonths = 12);

        /// <summary>
        /// Intelligent vendor selection based on multi-criteria decision analysis
        /// </summary>
        Task<VendorSelectionResult> SelectOptimalVendorAsync(VendorSelectionCriteria criteria);

        /// <summary>
        /// Automated vendor risk assessment using financial and operational metrics
        /// </summary>
        Task<VendorRiskAssessment> AssessVendorRiskAsync(int vendorId);

        // === STRATEGIC PROCUREMENT PLANNING ===

        /// <summary>
        /// Generate comprehensive procurement forecast based on historical patterns and business intelligence
        /// </summary>
        Task<ProcurementForecast> GenerateProcurementForecastAsync(int forecastMonths = 12);

        /// <summary>
        /// Execute intelligent cost optimization across all procurement activities
        /// </summary>
        Task<CostOptimizationResult> ExecuteCostOptimizationAsync(string initiatedByUserId);

        /// <summary>
        /// Optimize contract portfolio with intelligent renewal and renegotiation recommendations
        /// </summary>
        Task<ContractOptimizationResult> OptimizeContractPortfolioAsync();

        // === PURCHASE ORDER AUTOMATION ===

        /// <summary>
        /// Optimize purchase order processing with intelligent automation
        /// </summary>
        Task<PurchaseOrderOptimizationResult> OptimizePurchaseOrderAsync(int procurementRequestId);

        /// <summary>
        /// Process emergency procurement requests with expedited workflows
        /// </summary>
        Task<EmergencyProcurementResult> ProcessEmergencyProcurementAsync(EmergencyProcurementRequest request, string processorUserId);

        // === BUDGET & SPEND ANALYTICS ===

        /// <summary>
        /// Analyze budget performance with variance analysis and forecasting
        /// </summary>
        Task<BudgetAnalysisResult> AnalyzeBudgetPerformanceAsync(string fiscalYear = null);

        /// <summary>
        /// Comprehensive spend analysis with trend identification and optimization opportunities
        /// </summary>
        Task<SpendAnalysisResult> PerformSpendAnalysisAsync(SpendAnalysisParameters parameters);

        // === INTEGRATED PROCESSING ===

        /// <summary>
        /// Process procurement requests with intelligent routing and optimization
        /// </summary>
        Task<ProcurementProcessingResult> ProcessProcurementRequestIntelligentlyAsync(int requestId, string processorUserId);
    }
}
