using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HospitalAssetTracker.Services;
using HospitalAssetTracker.Models;
using Microsoft.AspNetCore.Identity;

namespace HospitalAssetTracker.Controllers
{
    /// <summary>
    /// Advanced Request Dashboard Controller
    /// Provides comprehensive request analytics, workflow intelligence, and performance insights
    /// </summary>
    [Authorize]
    public class RequestDashboardController : Controller
    {
        private readonly IRequestService _requestService;
        private readonly IRequestBusinessLogicService _requestBusinessLogicService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RequestDashboardController> _logger;

        public RequestDashboardController(
            IRequestService requestService,
            IRequestBusinessLogicService requestBusinessLogicService,
            UserManager<ApplicationUser> userManager,
            ILogger<RequestDashboardController> logger)
        {
            _requestService = requestService;
            _requestBusinessLogicService = requestBusinessLogicService;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Main request dashboard with key metrics and workflow insights
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var dashboardData = await _requestService.GetRequestDashboardDataAsync();
                
                var model = new RequestDashboardViewModel
                {
                    // Basic metrics
                    TotalActiveRequests = dashboardData.TotalActiveRequests,
                    PendingApprovals = dashboardData.PendingApprovals,
                    OverdueRequests = dashboardData.OverdueRequests,
                    CompletedToday = dashboardData.CompletedToday,
                    
                    // Advanced analytics
                    DemandForecast = await _requestBusinessLogicService.GenerateDemandForecastAsync(30),
                    SlaCompliance = await _requestBusinessLogicService.MonitorSlaComplianceAsync(null, 30),
                    QualityMetrics = await _requestBusinessLogicService.MonitorServiceQualityAsync(1),
                    
                    // Recent activities
                    RecentRequests = await _requestService.GetMyRequestsAsync(userId),
                    OverdueRequestsList = await _requestService.GetOverdueRequestsAsync(),
                    
                    // Current user context
                    CurrentUserId = userId,
                    LastRefreshed = DateTime.UtcNow
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading request dashboard");
                TempData["Error"] = "Failed to load dashboard data. Please try again.";
                return View(new RequestDashboardViewModel());
            }
        }

        /// <summary>
        /// Intelligent request analysis and routing recommendations
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> IntelligentAnalysis(int? requestId)
        {
            try
            {
                if (!requestId.HasValue)
                {
                    // Show list of requests for analysis selection
                    var recentRequests = await _requestService.GetOverdueRequestsAsync();
                    return View("SelectRequestForAnalysis", recentRequests);
                }

                var analysis = await _requestBusinessLogicService.AnalyzeRequestIntelligentlyAsync(requestId.Value);
                
                var model = new RequestAnalysisViewModel
                {
                    Analysis = analysis,
                    RequestId = requestId.Value,
                    RecommendedRoute = Enum.TryParse<RequestRoute>(analysis.RecommendedRoute.ToString(), out var route) ? route : RequestRoute.StandardWorkflow,
                    ComplexityLevel = analysis.ComplexityScore > 70 ? "High" : analysis.ComplexityScore > 40 ? "Medium" : "Low",
                    EstimatedEffort = analysis.EstimatedEffort,
                    RiskFactors = analysis.RiskFactors
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading request analysis");
                TempData["Error"] = "Failed to load request analysis.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Execute intelligent request routing
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> ExecuteIntelligentRouting(int requestId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var result = await _requestBusinessLogicService.RouteRequestIntelligentlyAsync(requestId, userId);

                if (result.Success)
                {
                    TempData["Success"] = $"Request routed successfully via {result.SelectedRoute}";
                    return RedirectToAction("Details", "Requests", new { id = requestId });
                }
                else
                {
                    TempData["Error"] = $"Request routing failed: {result.ErrorMessage}";
                    return RedirectToAction(nameof(IntelligentAnalysis), new { requestId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing intelligent routing for request {RequestId}", requestId);
                TempData["Error"] = "Failed to execute intelligent routing.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// SLA compliance monitoring and reporting
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> SlaCompliance(int analysisDays = 30)
        {
            try
            {
                var compliance = await _requestBusinessLogicService.MonitorSlaComplianceAsync(null, analysisDays);
                
                var model = new SlaComplianceViewModel
                {
                    Compliance = compliance,
                    AnalysisPeriod = analysisDays.ToString(),
                    OverallComplianceRate = compliance.OverallComplianceRate,
                    TotalViolations = compliance.CriticalViolations + compliance.HighViolations,
                    ImprovementActions = compliance.ImprovementRecommendations
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading SLA compliance data");
                TempData["Error"] = "Failed to load SLA compliance data.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Demand forecasting and capacity planning
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> DemandForecasting(int forecastDays = 90)
        {
            try
            {
                var forecast = await _requestBusinessLogicService.GenerateDemandForecastAsync(forecastDays);
                var resourceOptimization = await _requestBusinessLogicService.OptimizeResourceUtilizationAsync(_userManager.GetUserId(User));

                var model = new DemandForecastingViewModel
                {
                    Forecast = new DemandForecastResult
                    {
                        ForecastAccuracy = forecast.ForecastAccuracy,
                        StrategicInsights = forecast.StrategicInsights,
                        CategoryForecasts = forecast.CategoryForecasts.Select(cf => new RequestCategoryForecast
                        {
                            RequestType = cf.RequestType,
                            CurrentDemand = cf.CurrentDemand,
                            PredictedVolume = cf.PredictedVolume,
                            TrendDirection = cf.TrendDirection,
                            ConfidenceLevel = cf.ConfidenceLevel
                        }).ToList(),
                        RecommendedActions = forecast.RecommendedActions
                    },
                    ResourceOptimization = resourceOptimization,
                    ForecastPeriod = forecastDays.ToString(),
                    ForecastAccuracy = forecast.ForecastAccuracy,
                    KeyInsights = forecast.StrategicInsights
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading demand forecasting data");
                TempData["Error"] = "Failed to load demand forecasting data.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Resource optimization and workload balancing
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> ResourceOptimization()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var optimization = await _requestBusinessLogicService.OptimizeResourceUtilizationAsync(userId);

                var model = new ResourceOptimizationViewModel
                {
                    Optimization = optimization,
                    WorkloadBalance = new WorkloadBalanceResult
                    {
                        OverallBalance = optimization.WorkloadAnalysis.WorkloadBalance,
                        DepartmentWorkloads = optimization.WorkloadAnalysis.DepartmentWorkloads,
                        CriticalBottlenecks = optimization.WorkloadAnalysis.CriticalBottlenecks
                    },
                    OptimizationOpportunities = optimization.OptimizationOpportunities.Select(o => o.Description).ToList(),
                    ProjectedImprovements = optimization.ProjectedImprovements.Select(p => p.Description).ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading resource optimization data");
                TempData["Error"] = "Failed to load resource optimization data.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Quality assurance monitoring
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> QualityAssurance(int analysisMonths = 3)
        {
            try
            {
                var quality = await _requestBusinessLogicService.MonitorServiceQualityAsync(analysisMonths);
                
                var model = new QualityAssuranceViewModel
                {
                    QualityResult = quality,
                    AnalysisPeriod = analysisMonths.ToString(),
                    OverallSatisfaction = quality.OverallSatisfactionScore,
                    AverageResolutionTime = quality.AverageResolutionTime,
                    ImprovementActions = quality.ImprovementRecommendations
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading quality assurance data");
                TempData["Error"] = "Failed to load quality assurance data.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Escalation management dashboard
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> ExecuteEscalationManagement()
        {
            try
            {
                var result = await _requestBusinessLogicService.ManageEscalationsIntelligentlyAsync();

                if (result.Success)
                {
                    TempData["Success"] = $"Escalation management completed: {result.EscalationCandidates} escalations processed";
                }
                else
                {
                    TempData["Warning"] = "Escalation management completed with some issues";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing escalation management");
                TempData["Error"] = "Failed to execute escalation management.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Cross-module integration orchestration
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> OrchestrateCrossModuleWorkflow(int requestId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var result = await _requestBusinessLogicService.OrchestrateCrossModuleWorkflowAsync(requestId, userId);

                if (result.Success)
                {
                    TempData["Success"] = "Cross-module workflow orchestration completed successfully";
                    return RedirectToAction("Details", "Requests", new { id = requestId });
                }
                else
                {
                    TempData["Error"] = $"Orchestration failed: {result.ErrorMessage}";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error orchestrating cross-module workflow for request {RequestId}", requestId);
                TempData["Error"] = "Failed to orchestrate cross-module workflow.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// API endpoint for dashboard metrics (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDashboardMetrics()
        {
            try
            {
                var dashboardData = await _requestService.GetRequestDashboardDataAsync();
                var slaCompliance = await _requestBusinessLogicService.MonitorSlaComplianceAsync(null, 7); // Last 7 days
                
                var metrics = new
                {
                    activeRequests = dashboardData.TotalActiveRequests,
                    pendingApprovals = dashboardData.PendingApprovals,
                    overdueRequests = dashboardData.OverdueRequests,
                    completedToday = dashboardData.CompletedToday,
                    slaCompliance = slaCompliance.OverallComplianceRate,
                    lastUpdated = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                };

                return Json(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard metrics");
                return Json(new { error = "Failed to load metrics" });
            }
        }

        /// <summary>
        /// Export request data to various formats
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> ExportData(string reportType = "summary", string format = "csv")
        {
            try
            {
                // Implementation would depend on the specific export requirements
                // For now, return a simple CSV response
                var data = new List<List<string>>
                {
                    new List<string> { "Request ID", "Title", "Status", "Priority", "Department", "Created Date" }
                };
                
                // Add sample data (in real implementation, this would come from the service)
                var recentRequests = await _requestService.GetOverdueRequestsAsync();
                foreach (var request in recentRequests.Take(100))
                {
                    data.Add(new List<string>
                    {
                        request.Id.ToString(),
                        request.Title,
                        request.Status.ToString(),
                        request.Priority.ToString(),
                        request.Department,
                        request.CreatedDate.ToString("yyyy-MM-dd")
                    });
                }

                var csv = string.Join("\n", data.Select(row => string.Join(",", row)));
                var bytes = System.Text.Encoding.UTF8.GetBytes(csv);

                return File(bytes, "text/csv", $"request_report_{DateTime.UtcNow:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting request data");
                TempData["Error"] = "Failed to export data.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
