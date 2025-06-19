using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HospitalAssetTracker.Services;
using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Controllers
{
    /// <summary>
    /// Cross-Module Integration Controller
    /// Handles complex business workflows that span multiple modules
    /// </summary>
    [Authorize]
    public class CrossModuleController : Controller
    {
        private readonly ICrossModuleIntegrationService _crossModuleService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CrossModuleController> _logger;

        public CrossModuleController(
            ICrossModuleIntegrationService crossModuleService,
            UserManager<ApplicationUser> userManager,
            ILogger<CrossModuleController> logger)
        {
            _crossModuleService = crossModuleService;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Start asset repair workflow with full integration
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,IT Support")]
        public async Task<IActionResult> StartAssetRepairWorkflow(int requestId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var result = await _crossModuleService.ProcessAssetRepairWorkflowAsync(requestId, userId);

                if (result.Success)
                {
                    return Json(new { 
                        success = true, 
                        message = result.Message,
                        workflowSteps = result.WorkflowSteps,
                        requiresProcurement = result.RequiresProcurement,
                        procurementIds = result.GeneratedProcurementIds,
                        temporaryAssetId = result.TemporaryReplacementAssetId
                    });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting asset repair workflow for request {RequestId}", requestId);
                return Json(new { success = false, message = "An error occurred while starting the workflow" });
            }
        }

        /// <summary>
        /// Replace asset temporarily from inventory
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,IT Support")]
        public async Task<IActionResult> ReplaceAssetTemporarily(int requestId, int replacementAssetId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var result = await _crossModuleService.ReplaceAssetTemporarilyAsync(requestId, replacementAssetId, userId);

                return Json(new { 
                    success = result.Success, 
                    message = result.Message,
                    originalAssetId = result.OriginalAssetId,
                    replacementAssetId = result.ReplacementAssetId,
                    locationId = result.LocationId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error replacing asset temporarily for request {RequestId}", requestId);
                return Json(new { success = false, message = "An error occurred during asset replacement" });
            }
        }

        /// <summary>
        /// Generate procurement requests from repair needs
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> GenerateProcurementFromRepair(int requestId, [FromBody] List<RepairPartRequest> partRequests)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var result = await _crossModuleService.GenerateProcurementFromRepairAsync(requestId, partRequests, userId);

                return Json(new { 
                    success = result.Success, 
                    message = result.Message,
                    procurementIds = result.GeneratedProcurementRequestIds,
                    totalCost = result.TotalEstimatedCost,
                    items = result.GeneratedItems
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating procurement from repair for request {RequestId}", requestId);
                return Json(new { success = false, message = "An error occurred while generating procurement requests" });
            }
        }

        /// <summary>
        /// Complete asset repair with full workflow closure
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,IT Support")]
        public async Task<IActionResult> CompleteAssetRepair(int requestId, string completionNotes, int? finalLocationId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var result = await _crossModuleService.CompleteAssetRepairAsync(requestId, completionNotes, finalLocationId, userId);

                return Json(new { 
                    success = result.Success, 
                    message = result.Message,
                    assetId = result.AssetId,
                    newStatus = result.NewAssetStatus.ToString(),
                    finalLocationId = result.FinalLocationId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing asset repair for request {RequestId}", requestId);
                return Json(new { success = false, message = "An error occurred while completing the repair" });
            }
        }

        /// <summary>
        /// Get integrated workflow status
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetWorkflowStatus(int requestId)
        {
            try
            {
                var result = await _crossModuleService.GetWorkflowStatusAsync(requestId);

                return Json(new { 
                    success = true,
                    requestId = result.RequestId,
                    status = result.RequestStatus.ToString(),
                    workflowSteps = result.WorkflowSteps,
                    pendingActions = result.PendingActions,
                    relatedProcurements = result.RelatedProcurementIds,
                    temporaryAssetId = result.TemporaryAssetId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow status for request {RequestId}", requestId);
                return Json(new { success = false, message = "An error occurred while retrieving workflow status" });
            }
        }

        /// <summary>
        /// Update request from procurement completion
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,IT Support,Asset Manager")]
        public async Task<IActionResult> UpdateFromProcurementCompletion(int requestId, int procurementRequestId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var success = await _crossModuleService.UpdateRequestFromProcurementCompletionAsync(requestId, procurementRequestId, userId);

                return Json(new { 
                    success = success, 
                    message = success ? "Request updated successfully after procurement completion" : "No updates needed or procurement not completed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating request from procurement completion for request {RequestId}", requestId);
                return Json(new { success = false, message = "An error occurred while updating the request" });
            }
        }
    }
}
