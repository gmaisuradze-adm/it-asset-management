using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Services;

namespace HospitalAssetTracker.Controllers
{
    [Authorize]
    public class UnifiedBusinessLogicController : Controller
    {
        private readonly IUnifiedBusinessLogicService _unifiedService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UnifiedBusinessLogicController> _logger;

        public UnifiedBusinessLogicController(
            IUnifiedBusinessLogicService unifiedService,
            UserManager<ApplicationUser> userManager,
            ILogger<UnifiedBusinessLogicController> logger)
        {
            _unifiedService = unifiedService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var dashboardData = await _unifiedService.GetDashboardDataAsync(user.Id, userRoles.ToList());

                ViewBag.UserName = user.UserName;
                ViewBag.UserRoles = userRoles;
                
                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading unified dashboard for user {UserId}", User.Identity?.Name);
                TempData["ErrorMessage"] = "An error occurred while loading the dashboard. Please try again.";
                return View(new UnifiedDashboardViewModel());
            }
        }

        public async Task<IActionResult> Actions()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var actionData = await _unifiedService.GetActionItemsAsync(user.Id, userRoles.ToList());

                ViewBag.UserName = user.UserName;
                ViewBag.UserRoles = userRoles;
                
                return View(actionData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading action items for user {UserId}", User.Identity?.Name);
                TempData["ErrorMessage"] = "An error occurred while loading action items. Please try again.";
                return View(new UnifiedActionViewModel());
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetSmartInsights()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var insights = await _unifiedService.GetSmartInsightsAsync(user.Id, userRoles.ToList());

                return Json(new { success = true, data = insights });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting smart insights for user {UserId}", User.Identity?.Name);
                return Json(new { success = false, message = "Error loading insights" });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetRecentActivities(int count = 10)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var activities = await _unifiedService.GetRecentActivitiesAsync(user.Id, userRoles.ToList(), count);

                return Json(new { success = true, data = activities });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent activities for user {UserId}", User.Identity?.Name);
                return Json(new { success = false, message = "Error loading activities" });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetAlerts(bool unreadOnly = false)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var alerts = await _unifiedService.GetAlertsAsync(user.Id, userRoles.ToList(), unreadOnly);

                return Json(new { success = true, data = alerts });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts for user {UserId}", User.Identity?.Name);
                return Json(new { success = false, message = "Error loading alerts" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> MarkAlertAsRead(int alertId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var result = await _unifiedService.MarkAlertAsReadAsync(alertId, user.Id);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking alert as read for user {UserId}", User.Identity?.Name);
                return Json(new { success = false, message = "Error updating alert" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> DismissAlert(int alertId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var result = await _unifiedService.DismissAlertAsync(alertId, user.Id);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dismissing alert for user {UserId}", User.Identity?.Name);
                return Json(new { success = false, message = "Error dismissing alert" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ExecuteQuickAction(string actionId, [FromBody] Dictionary<string, object>? parameters = null)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var result = await _unifiedService.ExecuteQuickActionAsync(actionId, user.Id, parameters);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing quick action {ActionId} for user {UserId}", actionId, User.Identity?.Name);
                return Json(new { success = false, message = "Error executing action" });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetPerformanceMetrics()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var metrics = await _unifiedService.GetPerformanceMetricsAsync(user.Id, userRoles.ToList());

                return Json(new { success = true, data = metrics });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics for user {UserId}", User.Identity?.Name);
                return Json(new { success = false, message = "Error loading metrics" });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetWorkflowSummary()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var summary = await _unifiedService.GetWorkflowSummaryAsync(user.Id, userRoles.ToList());

                return Json(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow summary for user {UserId}", User.Identity?.Name);
                return Json(new { success = false, message = "Error loading workflow summary" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<JsonResult> RefreshCache()
        {
            try
            {
                await _unifiedService.RefreshCacheAsync();
                return Json(new { success = true, message = "Cache refreshed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing cache");
                return Json(new { success = false, message = "Error refreshing cache" });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetQuickActions()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var actions = await _unifiedService.GetQuickActionsAsync(user.Id, userRoles.ToList());

                return Json(new { success = true, data = actions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quick actions for user {UserId}", User.Identity?.Name);
                return Json(new { success = false, message = "Error loading quick actions" });
            }
        }
    }
}
