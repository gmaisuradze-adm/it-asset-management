using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HospitalAssetTracker.Services;
using HospitalAssetTracker.Models;
using Microsoft.AspNetCore.Identity;

namespace HospitalAssetTracker.Controllers
{
    /// <summary>
    /// Advanced Procurement Dashboard Controller
    /// Provides comprehensive procurement analytics, vendor intelligence, and business insights
    /// </summary>
    [Authorize]
    public class ProcurementDashboardController : Controller
    {
        private readonly IProcurementService _procurementService;
        private readonly IProcurementBusinessLogicService _procurementBusinessLogicService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ProcurementDashboardController> _logger;

        public ProcurementDashboardController(
            IProcurementService procurementService,
            IProcurementBusinessLogicService procurementBusinessLogicService,
            UserManager<ApplicationUser> userManager,
            ILogger<ProcurementDashboardController> logger)
        {
            _procurementService = procurementService;
            _procurementBusinessLogicService = procurementBusinessLogicService;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Main procurement dashboard with key metrics and insights
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                
                var model = new ProcurementDashboardViewModel
                {
                    // Basic metrics
                    TotalActiveRequests = await _procurementService.GetActiveRequestsCountAsync(),
                    PendingApprovals = await _procurementService.GetPendingApprovalsCountAsync(),
                    TotalVendors = await _procurementService.GetActiveVendorsCountAsync(),
                    
                    // Advanced analytics
                    VendorPerformanceAnalysis = await _procurementBusinessLogicService.AnalyzeVendorPerformanceAsync(),
                    ProcurementForecast = await _procurementBusinessLogicService.GenerateProcurementForecastAsync(6),
                    BudgetAnalysis = await _procurementBusinessLogicService.AnalyzeBudgetPerformanceAsync(),
                    
                    // Recent activities
                    RecentRequests = await _procurementService.GetRecentRequestsAsync(10),
                    RecentApprovals = await _procurementService.GetRecentApprovalsAsync(5),
                    
                    // Current user context
                    CurrentUserId = userId,
                    LastRefreshed = DateTime.UtcNow
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading procurement dashboard");
                TempData["Error"] = "Failed to load dashboard data. Please try again.";
                return View(new ProcurementDashboardViewModel());
            }
        }

        /// <summary>
        /// Vendor intelligence and performance analytics
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> VendorIntelligence()
        {
            try
            {
                var analysis = await _procurementBusinessLogicService.AnalyzeVendorPerformanceAsync();
                
                var model = new VendorIntelligenceViewModel
                {
                    PerformanceAnalysis = analysis,
                    TopPerformers = analysis.PreferredVendors.Take(5).ToList(),
                    UnderPerformers = analysis.UnderperformingVendors.Take(3).ToList(),
                    AnalysisDate = analysis.AnalysisDate,
                    TotalVendorsAnalyzed = analysis.VendorMetrics.Count
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vendor intelligence");
                TempData["Error"] = "Failed to load vendor intelligence data.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Cost optimization and spend analysis
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CostOptimization()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var optimizationResult = await _procurementBusinessLogicService.ExecuteCostOptimizationAsync(userId);
                
                var spendAnalysisParams = new SpendAnalysisParameters
                {
                    StartDate = DateTime.UtcNow.AddMonths(-12),
                    EndDate = DateTime.UtcNow,
                    IncludeTrends = true,
                    IncludeOptimization = true
                };
                
                var spendAnalysis = await _procurementBusinessLogicService.PerformSpendAnalysisAsync(spendAnalysisParams);

                var model = new CostOptimizationViewModel
                {
                    OptimizationResult = optimizationResult,
                    SpendAnalysis = spendAnalysis,
                    TotalSavingsIdentified = optimizationResult.Analysis.IdentifiedSavings,
                    SavingsPercentage = optimizationResult.Analysis.SavingsPercentage,
                    OptimizationOpportunities = optimizationResult.Analysis.Opportunities.Take(10).ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cost optimization data");
                TempData["Error"] = "Failed to load cost optimization data.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Procurement forecasting and planning
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Forecasting()
        {
            try
            {
                var forecast = await _procurementBusinessLogicService.GenerateProcurementForecastAsync(12);
                var budgetAnalysis = await _procurementBusinessLogicService.AnalyzeBudgetPerformanceAsync();

                var model = new ProcurementForecastingViewModel
                {
                    Forecast = forecast,
                    BudgetAnalysis = budgetAnalysis,
                    ForecastAccuracy = forecast.ConfidenceLevel,
                    TotalForecastedValue = forecast.TotalForecastedValue,
                    KeyInsights = forecast.StrategicRecommendations.Take(5).ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading forecasting data");
                TempData["Error"] = "Failed to load forecasting data.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Emergency procurement processing
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public IActionResult EmergencyProcurement()
        {
            var model = new EmergencyProcurementViewModel
            {
                Request = new EmergencyProcurementRequest
                {
                    RequiredByDate = DateTime.UtcNow.AddDays(1),
                    Priority = "High"
                }
            };

            return View(model);
        }

        /// <summary>
        /// Process emergency procurement request
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> ProcessEmergencyProcurement(EmergencyProcurementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("EmergencyProcurement", model);
            }

            try
            {
                var userId = _userManager.GetUserId(User);
                var result = await _procurementBusinessLogicService.ProcessEmergencyProcurementAsync(model.Request, userId);

                if (result.Success)
                {
                    TempData["Success"] = $"Emergency procurement processed successfully. Tracking: {result.TrackingNumber}";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = $"Emergency procurement failed: {result.Status}";
                    return View("EmergencyProcurement", model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing emergency procurement");
                TempData["Error"] = "Failed to process emergency procurement request.";
                return View("EmergencyProcurement", model);
            }
        }

        /// <summary>
        /// Vendor risk assessment dashboard
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> VendorRiskAssessment(int? vendorId)
        {
            try
            {
                if (!vendorId.HasValue)
                {
                    // Show list of vendors for risk assessment selection
                    var vendors = await _procurementService.GetActiveVendorsAsync();
                    return View("SelectVendorForRisk", vendors);
                }

                var riskAssessment = await _procurementBusinessLogicService.AssessVendorRiskAsync(vendorId.Value);
                
                var model = new VendorRiskAssessmentViewModel
                {
                    Assessment = riskAssessment,
                    RiskLevel = riskAssessment.RiskLevel.ToString(),
                    TotalRiskScore = riskAssessment.OverallRiskScore,
                    MitigationActions = riskAssessment.MitigationRecommendations
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vendor risk assessment");
                TempData["Error"] = "Failed to load vendor risk assessment.";
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
                var metrics = new
                {
                    activeRequests = await _procurementService.GetActiveRequestsCountAsync(),
                    pendingApprovals = await _procurementService.GetPendingApprovalsCountAsync(),
                    totalVendors = await _procurementService.GetActiveVendorsCountAsync(),
                    monthlySpend = await _procurementService.GetMonthlySpendAsync(),
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
        /// Export procurement data to Excel
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportData(string reportType = "summary")
        {
            try
            {
                // Implementation would depend on export library (e.g., EPPlus)
                // For now, return a simple CSV response
                var data = await _procurementService.GetExportDataAsync(reportType);
                
                var csv = string.Join("\n", data.Select(row => string.Join(",", row)));
                var bytes = System.Text.Encoding.UTF8.GetBytes(csv);

                return File(bytes, "text/csv", $"procurement_report_{DateTime.UtcNow:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting procurement data");
                TempData["Error"] = "Failed to export data.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
