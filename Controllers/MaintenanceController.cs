using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Services;
using System.Security.Claims;
using Microsoft.Extensions.Logging; // Added for ILogger

namespace HospitalAssetTracker.Controllers
{
    [Authorize(Roles = "Admin,IT Support")]
    public class MaintenanceController : Controller
    {
        private readonly IAssetService _assetService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<MaintenanceController> _logger; // Added ILogger

        public MaintenanceController(IAssetService assetService, UserManager<ApplicationUser> userManager, ILogger<MaintenanceController> logger) // Added logger parameter
        {
            _assetService = assetService;
            _userManager = userManager;
            _logger = logger; // Assign logger
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScheduleMaintenance(int AssetId, MaintenanceType MaintenanceType, string Title, string Description, DateTime ScheduledDate)
        {
            if (AssetId <= 0)
            {
                return Json(new { success = false, message = "Invalid Asset ID." });
            }
            if (string.IsNullOrWhiteSpace(Title))
            {
                return Json(new { success = false, message = "Title is required." });
            }
            if (ScheduledDate < DateTime.UtcNow.Date) // Check if the scheduled date is in the past
            {
                return Json(new { success = false, message = "Scheduled date cannot be in the past." });
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in ScheduleMaintenance.");
                    return Json(new { success = false, message = "User not found. Please log in again." });
                }

                var record = new MaintenanceRecord
                {
                    AssetId = AssetId,
                    MaintenanceType = MaintenanceType,
                    Title = Title,
                    Description = Description,
                    ScheduledDate = ScheduledDate,
                    Status = MaintenanceStatus.Scheduled
                };

                await _assetService.ScheduleMaintenanceAsync(record, userId);

                return Json(new { success = true, message = "Maintenance scheduled successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling maintenance for AssetId {AssetId}", AssetId);
                return Json(new { success = false, message = "An error occurred while scheduling maintenance." });
            }
        }

        /*
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScheduleBulkMaintenance(int[] assetIds, MaintenanceType MaintenanceType, string Title, string Description, DateTime ScheduledDate)
        {
            if (assetIds == null || !assetIds.Any())
            {
                return Json(new { success = false, message = "No assets selected." });
            }
            if (string.IsNullOrWhiteSpace(Title))
            {
                return Json(new { success = false, message = "Title is required." });
            }
            if (ScheduledDate < DateTime.UtcNow.Date) // Check if the scheduled date is in the past
            {
                return Json(new { success = false, message = "Scheduled date cannot be in the past." });
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in ScheduleBulkMaintenance.");
                    return Json(new { success = false, message = "User not found. Please log in again." });
                }

                var record = new MaintenanceRecord
                {
                    MaintenanceType = MaintenanceType,
                    Title = Title,
                    Description = Description,
                    ScheduledDate = ScheduledDate,
                    Status = MaintenanceStatus.Scheduled
                    // AssetId will be set by the service for each asset in the bulk operation
                };

                await _assetService.ScheduleBulkMaintenanceAsync(assetIds, record, userId);

                return Json(new { success = true, message = "Bulk maintenance scheduled successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling bulk maintenance for {AssetCount} assets.", assetIds.Length);
                return Json(new { success = false, message = "An error occurred while scheduling bulk maintenance." });
            }
        }
        */
    }
}
