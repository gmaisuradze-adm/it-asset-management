using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HospitalAssetTracker.Services;
using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Controllers
{
    /// <summary>
    /// Cross-Module Integration Controller
    /// Handles complex business processes that span multiple modules
    /// Supports advanced workflows like asset repair with procurement and inventory integration
    /// </summary>
    [Authorize]
    public class IntegrationController : Controller
    {
        private readonly ICrossModuleIntegrationService _integrationService;
        private readonly IRequestService _requestService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<IntegrationController> _logger;

        public IntegrationController(
            ICrossModuleIntegrationService integrationService,
            IRequestService requestService,
            UserManager<ApplicationUser> userManager,
            ILogger<IntegrationController> logger)
        {
            _integrationService = integrationService;
            _requestService = requestService;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Start asset repair workflow
        /// POST: Integration/StartRepairWorkflow
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,IT Support")]
        public async Task<IActionResult> StartRepairWorkflow(int requestId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var result = await _integrationService.ProcessAssetRepairWorkflowAsync(requestId, userId);

                if (result.Success)
                {
                    return Json(new 
                    { 
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
                _logger.LogError(ex, "Error starting repair workflow for request {RequestId}", requestId);
                return Json(new { success = false, message = "Error starting repair workflow" });
            }
        }

        /// <summary>
        /// Replace asset temporarily
        /// POST: Integration/ReplaceAssetTemporarily
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

                var result = await _integrationService.ReplaceAssetTemporarilyAsync(requestId, replacementAssetId, userId);

                return Json(new 
                { 
                    success = result.Success, 
                    message = result.Message,
                    originalAssetId = result.OriginalAssetId,
                    replacementAssetId = result.ReplacementAssetId,
                    locationId = result.LocationId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error replacing asset temporarily");
                return Json(new { success = false, message = "Error replacing asset" });
            }
        }

        /// <summary>
        /// Generate procurement from repair needs
        /// POST: Integration/GenerateProcurementFromRepair
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

                var result = await _integrationService.GenerateProcurementFromRepairAsync(requestId, partRequests, userId);

                return Json(new 
                { 
                    success = result.Success, 
                    message = result.Message,
                    generatedIds = result.GeneratedProcurementRequestIds,
                    totalCost = result.TotalEstimatedCost,
                    items = result.GeneratedItems
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating procurement from repair");
                return Json(new { success = false, message = "Error generating procurement" });
            }
        }

        /// <summary>
        /// Complete asset repair
        /// POST: Integration/CompleteRepair
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,IT Support")]
        public async Task<IActionResult> CompleteRepair(int requestId, string completionNotes, int? finalLocationId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var result = await _integrationService.CompleteAssetRepairAsync(requestId, completionNotes, finalLocationId, userId);

                return Json(new 
                { 
                    success = result.Success, 
                    message = result.Message,
                    assetId = result.AssetId,
                    newStatus = result.NewAssetStatus.ToString(),
                    finalLocationId = result.FinalLocationId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing asset repair");
                return Json(new { success = false, message = "Error completing repair" });
            }
        }

        /// <summary>
        /// Get workflow status
        /// GET: Integration/GetWorkflowStatus/{requestId}
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetWorkflowStatus(int requestId)
        {
            try
            {
                var result = await _integrationService.GetWorkflowStatusAsync(requestId);

                return Json(new 
                { 
                    success = true,
                    requestId = result.RequestId,
                    status = result.RequestStatus.ToString(),
                    workflowSteps = result.WorkflowSteps,
                    pendingActions = result.PendingActions,
                    relatedProcurementIds = result.RelatedProcurementIds,
                    temporaryAssetId = result.TemporaryAssetId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow status for request {RequestId}", requestId);
                return Json(new { success = false, message = "Error retrieving workflow status" });
            }
        }

        /// <summary>
        /// Get repair workflow view
        /// GET: Integration/RepairWorkflow/{requestId}
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> RepairWorkflow(int requestId)
        {
            try
            {
                var request = await _requestService.GetRequestByIdAsync(requestId);
                if (request == null)
                {
                    return NotFound();
                }

                var workflowStatus = await _integrationService.GetWorkflowStatusAsync(requestId);

                var model = new RepairWorkflowViewModel
                {
                    Request = request,
                    WorkflowStatus = workflowStatus
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading repair workflow view for request {RequestId}", requestId);
                return RedirectToAction("Details", "Requests", new { id = requestId });
            }
        }

        /// <summary>
        /// Update request from procurement completion
        /// This is called automatically when procurement is completed
        /// POST: Integration/UpdateFromProcurementCompletion
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

                var success = await _integrationService.UpdateRequestFromProcurementCompletionAsync(requestId, procurementRequestId, userId);

                return Json(new 
                { 
                    success = success, 
                    message = success ? "Request updated from procurement completion" : "Failed to update request"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating request from procurement completion");
                return Json(new { success = false, message = "Error updating request" });
            }
        }
    }

    /// <summary>
    /// Repair workflow view model
    /// </summary>
    public class RepairWorkflowViewModel
    {
        public ITRequest Request { get; set; } = null!;
        public WorkflowStatusResult WorkflowStatus { get; set; } = null!;
    }
}
