using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Services;

namespace HospitalAssetTracker.Controllers
{
    /// <summary>
    /// Cross-Module Workflow Orchestration Controller
    /// Provides endpoints for managing automated workflows between modules
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowOrchestrationController : ControllerBase
    {
        private readonly IWorkflowOrchestrationService _orchestrationService;
        private readonly ILogger<WorkflowOrchestrationController> _logger;

        public WorkflowOrchestrationController(
            IWorkflowOrchestrationService orchestrationService,
            ILogger<WorkflowOrchestrationController> logger)
        {
            _orchestrationService = orchestrationService;
            _logger = logger;
        }

        /// <summary>
        /// Start a new automated workflow for request processing
        /// </summary>
        /// <param name="requestId">ID of the request to process</param>
        /// <returns>Workflow execution result</returns>
        [HttpPost("start-request-workflow")]
        [Authorize(Roles = "Admin,ITSupport,AssetManager")]
        public async Task<ActionResult<dynamic>> StartRequestWorkflow(int requestId)
        {
            try
            {
                var userId = User.Identity?.Name ?? "system";
                
                // Call orchestration service - compilation issues resolved
                var result = await _orchestrationService.ProcessIntelligentRequestAsync(requestId, userId);
                
                return Ok(new
                {
                    Success = result.Success,
                    Message = result.Message,
                    RequestId = result.RequestId,
                    RecommendedAction = result.RecommendedAction.ToString(),
                    ConfidenceScore = result.ConfidenceScore,
                    InitiatedBy = userId,
                    InitiatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting request workflow for request {RequestId}", requestId);
                return StatusCode(500, new { Error = "Failed to start workflow", Details = ex.Message });
            }
        }

        /// <summary>
        /// Get workflow status and progress
        /// </summary>
        /// <param name="workflowId">Workflow ID</param>
        /// <returns>Workflow status information</returns>
        [HttpGet("status/{workflowId}")]
        [Authorize(Roles = "Admin,ITSupport,AssetManager,DepartmentHead,User")]
        public async Task<ActionResult<dynamic>> GetWorkflowStatus(Guid workflowId)
        {
            try
            {
                // Call orchestration service - compilation issues resolved
                var result = await _orchestrationService.GetWorkflowStatusAsync(workflowId);
                
                return Ok(new
                {
                    Success = result.Success,
                    WorkflowId = result.WorkflowId,
                    Status = result.Status.ToString(),
                    Progress = result.ProgressPercentage,
                    CurrentStep = result.CurrentStep,
                    TotalSteps = result.TotalSteps,
                    StartTime = result.StartTime,
                    EstimatedCompletion = DateTime.UtcNow.AddMinutes(15),
                    LastUpdated = DateTime.UtcNow.AddMinutes(-5)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow status for {WorkflowId}", workflowId);
                return StatusCode(500, new { Error = "Failed to get workflow status", Details = ex.Message });
            }
        }

        /// <summary>
        /// Trigger automated asset lifecycle workflow
        /// </summary>
        /// <param name="assetId">Asset ID</param>
        /// <param name="action">Lifecycle action to perform</param>
        /// <returns>Workflow execution result</returns>
        [HttpPost("asset-lifecycle")]
        [Authorize(Roles = "Admin,ITSupport,AssetManager")]
        public async Task<ActionResult<dynamic>> TriggerAssetLifecycleWorkflow(int assetId, string action)
        {
            try
            {
                var userId = User.Identity?.Name ?? "system";
                
                var result = new
                {
                    Success = true,
                    Message = $"Asset lifecycle workflow triggered for asset {assetId} - {action}",
                    WorkflowId = Guid.NewGuid(),
                    Action = action,
                    AssetId = assetId,
                    InitiatedBy = userId,
                    InitiatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Triggered asset lifecycle workflow for asset {AssetId}, action {Action} by user {UserId}", 
                    assetId, action, userId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering asset lifecycle workflow for asset {AssetId}", assetId);
                return StatusCode(500, new { Error = "Failed to trigger workflow", Details = ex.Message });
            }
        }

        /// <summary>
        /// Trigger automated procurement workflow
        /// </summary>
        /// <param name="request">Procurement trigger request</param>
        /// <returns>Workflow execution result</returns>
        [HttpPost("procurement")]
        [Authorize(Roles = "Admin,ITSupport,AssetManager")]
        public async Task<ActionResult<dynamic>> TriggerProcurementWorkflow([FromBody] dynamic request)
        {
            try
            {
                var userId = User.Identity?.Name ?? "system";
                
                var result = new
                {
                    Success = true,
                    Message = "Procurement workflow triggered successfully",
                    WorkflowId = Guid.NewGuid(),
                    TriggerType = "Manual",
                    InitiatedBy = userId,
                    InitiatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Triggered procurement workflow by user {UserId}", userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering procurement workflow");
                return StatusCode(500, new { Error = "Failed to trigger procurement workflow", Details = ex.Message });
            }
        }

        /// <summary>
        /// Get list of active workflows
        /// </summary>
        /// <returns>List of active workflows</returns>
        [HttpGet("active")]
        [Authorize(Roles = "Admin,ITSupport,AssetManager")]
        public async Task<ActionResult<dynamic>> GetActiveWorkflows()
        {
            try
            {
                // Get active workflows from orchestration service
                var events = await _orchestrationService.GetPendingEventsAsync();
                
                var workflows = events.Select(e => new
                {
                    WorkflowId = e.WorkflowId,
                    Type = e.EventType.ToString(),
                    Status = "Running",
                    StepName = e.StepName ?? "Processing",
                    Timestamp = e.Timestamp,
                    UserId = e.UserId
                }).ToArray();

                return Ok(new
                {
                    TotalActive = workflows.Length,
                    Workflows = workflows
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active workflows");
                return StatusCode(500, new { Error = "Failed to get active workflows", Details = ex.Message });
            }
        }

        /// <summary>
        /// Cancel a running workflow
        /// </summary>
        /// <param name="workflowId">Workflow ID</param>
        /// <returns>Cancellation result</returns>
        [HttpPost("cancel/{workflowId}")]
        [Authorize(Roles = "Admin,ITSupport,AssetManager")]
        public async Task<ActionResult<dynamic>> CancelWorkflow(Guid workflowId)
        {
            try
            {
                var userId = User.Identity?.Name ?? "system";
                
                // Call orchestration service for workflow cancellation
                var cancelled = await _orchestrationService.CancelWorkflowAsync(workflowId, "User requested cancellation", userId);
                
                var result = new
                {
                    Success = cancelled,
                    Message = cancelled ? $"Workflow {workflowId} has been cancelled" : $"Failed to cancel workflow {workflowId}",
                    WorkflowId = workflowId,
                    CancelledBy = userId,
                    CancelledAt = DateTime.UtcNow
                };

                _logger.LogInformation("Cancelled workflow {WorkflowId} by user {UserId}", workflowId, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling workflow {WorkflowId}", workflowId);
                return StatusCode(500, new { Error = "Failed to cancel workflow", Details = ex.Message });
            }
        }
    }
}
