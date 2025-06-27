using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Services;
using System.Security.Claims;

namespace HospitalAssetTracker.Controllers
{
    [Authorize]
    [Route("AssetDashboard")]
    public class AssetDashboardController : Controller
    {
        private readonly IAssetBusinessLogicService _assetBusinessLogicService;
        private readonly IAssetService _assetService;
        private readonly ILogger<AssetDashboardController> _logger;

        public AssetDashboardController(
            IAssetBusinessLogicService assetBusinessLogicService,
            IAssetService assetService,
            ILogger<AssetDashboardController> logger)
        {
            _assetBusinessLogicService = assetBusinessLogicService;
            _assetService = assetService;
            _logger = logger;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "test-user";
                var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "Admin";
                
                _logger.LogInformation("Loading Asset Dashboard for user {UserId} with role {UserRole}", userId, userRole);

                // Get dashboard data
                var dashboardModel = await _assetBusinessLogicService.GetAssetDashboardAsync(userId);
                
                _logger.LogInformation("Asset Dashboard loaded successfully. Total assets: {TotalAssets}", dashboardModel?.Overview?.TotalAssets ?? 0);
                
                return View(dashboardModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Asset Dashboard: {Message}", ex.Message);
                TempData["ErrorMessage"] = $"Unable to load Asset Dashboard: {ex.Message}";
                
                // Create a simple error view instead of redirecting
                ViewBag.ErrorDetails = ex.ToString();
                return View("Error");
            }
        }

        [HttpGet("Analytics")]
        public async Task<IActionResult> Analytics()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                
                _logger.LogInformation("Loading Asset Analytics for user {UserId}", userId);

                var analyticsResult = await _assetBusinessLogicService.GetAssetAnalyticsAsync(userId, 12); // 12 months
                
                return View(analyticsResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Asset Analytics");
                TempData["ErrorMessage"] = "Unable to load Asset Analytics. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet("Performance")]
        public async Task<IActionResult> Performance()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                
                _logger.LogInformation("Loading Asset Performance Analysis for user {UserId}", userId);

                var assets = await _assetService.GetAllAssetsAsync();
                var performanceAnalyses = new List<AssetPerformanceAnalysisResult>();

                foreach (var asset in assets.Take(50)) // Limit for performance
                {
                    var analysis = _assetBusinessLogicService.AnalyzeAssetPerformance(asset.Id, userId);
                    performanceAnalyses.Add(analysis);
                }
                
                return View(performanceAnalyses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Asset Performance Analysis");
                TempData["ErrorMessage"] = "Unable to load Asset Performance Analysis. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet("Lifecycle")]
        public async Task<IActionResult> Lifecycle()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                
                _logger.LogInformation("Loading Asset Lifecycle Analysis for user {UserId}", userId);

                var assets = await _assetService.GetAllAssetsAsync();
                var lifecycleAnalyses = new List<AssetLifecycleAnalysis>();

                foreach (var asset in assets.Take(50)) // Limit for performance
                {
                    var analysisResult = await _assetBusinessLogicService.AnalyzeAssetLifecycleAsync(asset.Id, userId);
                    var analysis = new AssetLifecycleAnalysis
                    {
                        AssetId = asset.Id,
                        AssetName = asset.Name,
                        Category = asset.Category.ToString(),
                        AcquisitionDate = asset.AcquisitionDate,
                        LifecycleStage = analysisResult.CurrentStage,
                        RemainingUsefulLife = analysisResult.RemainingUsefulLife,
                        ReplacementRecommendation = analysisResult.ReplacementRecommendation,
                        MaintenanceSchedule = string.Join(", ", analysisResult.MaintenanceSchedule.Select(m => $"{m.MaintenanceType} ({m.ScheduledDate:MM/dd/yyyy})"))
                    };
                    lifecycleAnalyses.Add(analysis);
                }
                
                return View(lifecycleAnalyses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Asset Lifecycle Analysis");
                TempData["ErrorMessage"] = "Unable to load Asset Lifecycle Analysis. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet("Optimization")]
        public async Task<IActionResult> Optimization()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                
                _logger.LogInformation("Loading Asset Optimization Analysis for user {UserId}", userId);

                var optimizationResult = await _assetBusinessLogicService.GetAssetOptimizationOpportunitiesAsync(userId);
                
                return View(optimizationResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Asset Optimization Analysis");
                TempData["ErrorMessage"] = "Unable to load Asset Optimization Analysis. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet("Alerts")]
        public async Task<IActionResult> Alerts()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                
                _logger.LogInformation("Loading Asset Alerts for user {UserId}", userId);

                var alerts = await _assetBusinessLogicService.GetAssetAlertsAsync(userId);
                
                return View(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Asset Alerts");
                TempData["ErrorMessage"] = "Unable to load Asset Alerts. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("AcknowledgeAlert")]
        public IActionResult AcknowledgeAlert(int assetId, string alertType)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                
                // For now, use assetId as alertId - this should be improved with proper alert ID mapping
                _assetBusinessLogicService.AcknowledgeAlert(assetId, userId);
                
                return Json(new { success = true, message = "Alert acknowledged successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acknowledging alert for asset {AssetId}", assetId);
                return Json(new { success = false, message = "An error occurred while acknowledging the alert." });
            }
        }

        [HttpGet("Reports")]
        public async Task<IActionResult> Reports()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                
                _logger.LogInformation("Loading Asset Reports for user {UserId}", userId);

                var dashboardModel = await _assetBusinessLogicService.GetAssetDashboardAsync(userId);
                
                return View(dashboardModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Asset Reports");
                TempData["ErrorMessage"] = "Unable to load Asset Reports. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet("Export/{type}")]
        public async Task<IActionResult> Export(string type)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                
                _logger.LogInformation("Exporting Asset data of type {Type} for user {UserId}", type, userId);

                byte[] data;
                string fileName;
                string contentType;

                switch (type.ToLower())
                {
                    case "dashboard":
                        var dashboardData = await _assetBusinessLogicService.GetAssetDashboardAsync(userId);
                        data = _assetBusinessLogicService.ExportDashboardData("excel", userId);
                        fileName = $"Asset_Dashboard_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        break;
                    
                    case "analytics":
                        var analyticsData = await _assetBusinessLogicService.GetAssetAnalyticsAsync(userId);
                        data = _assetBusinessLogicService.ExportAnalyticsData("excel", userId);
                        fileName = $"Asset_Analytics_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        break;
                    
                    case "performance":
                        var performanceData = _assetBusinessLogicService.GetAssetPerformanceReport(userId);
                        data = _assetBusinessLogicService.ExportPerformanceData("excel", userId);
                        fileName = $"Asset_Performance_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        break;
                    
                    default:
                        return BadRequest("Invalid export type");
                }

                return File(data, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting Asset data of type {Type}", type);
                TempData["ErrorMessage"] = "Unable to export data. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet("Api/StatusSummary")]
        public async Task<IActionResult> GetStatusSummary()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                var dashboardModel = await _assetBusinessLogicService.GetAssetDashboardAsync(userId);
                
                return Json(dashboardModel.StatusSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting asset status summary");
                return Json(new { error = "Unable to load status summary" });
            }
        }

        [HttpGet("Api/CategoryBreakdown")]
        public async Task<IActionResult> GetCategoryBreakdown()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                var dashboardModel = await _assetBusinessLogicService.GetAssetDashboardAsync(userId);
                
                return Json(dashboardModel.CategoryBreakdown);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting asset category breakdown");
                return Json(new { error = "Unable to load category breakdown" });
            }
        }

        [HttpGet("Api/Trends")]
        public async Task<IActionResult> GetTrends()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                var dashboardModel = await _assetBusinessLogicService.GetAssetDashboardAsync(userId);
                
                return Json(dashboardModel.Trends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting asset trends");
                return Json(new { error = "Unable to load trends" });
            }
        }

        [HttpGet("Api/UpcomingMaintenance")]
        public async Task<IActionResult> GetUpcomingMaintenance()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                var dashboardModel = await _assetBusinessLogicService.GetAssetDashboardAsync(userId);
                
                return Json(dashboardModel.UpcomingMaintenance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming maintenance");
                return Json(new { error = "Unable to load upcoming maintenance" });
            }
        }
    }
}
