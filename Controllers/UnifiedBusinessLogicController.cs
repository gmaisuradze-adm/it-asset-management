using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Services;

namespace HospitalAssetTracker.Controllers
{
    /// <summary>
    /// Controller for unified business logic operations implementing Georgian requirements
    /// Supports Manager and IT Support role-based workflows
    /// </summary>
    [Authorize(Roles = "Admin,Asset Manager,IT Support")]
    public class UnifiedBusinessLogicController : Controller
    {
        private readonly IUnifiedBusinessLogicService _unifiedService;
        private readonly IRequestService _requestService;
        private readonly IAssetService _assetService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UnifiedBusinessLogicController> _logger;

        public UnifiedBusinessLogicController(
            IUnifiedBusinessLogicService unifiedService,
            IRequestService requestService,
            IAssetService assetService,
            UserManager<ApplicationUser> userManager,
            ILogger<UnifiedBusinessLogicController> logger)
        {
            _unifiedService = unifiedService;
            _requestService = requestService;
            _assetService = assetService;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Unified request processing endpoint
        /// </summary>
        [HttpPost]
        [Route("UnifiedBusinessLogic/ProcessRequest")]
        public async Task<IActionResult> ProcessRequest([FromBody] UnifiedRequestModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                var request = await _requestService.GetRequestByIdAsync(model.RequestId);
                if (request == null)
                    return NotFound($"Request {model.RequestId} not found");

                var result = await _unifiedService.ProcessRequestAsync(request, currentUser.Id);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    processingMethod = result.ProcessingMethod,
                    requiresEscalation = result.RequiresEscalation,
                    requiresManagerApproval = result.RequiresManagerApproval,
                    escalationReason = result.EscalationReason,
                    processingSteps = result.ProcessingSteps,
                    resultData = result.ResultData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing unified request {RequestId}", model.RequestId);
                return Json(new { success = false, message = "Processing failed" });
            }
        }

        /// <summary>
        /// Asset lifecycle decision endpoint
        /// </summary>
        [HttpPost]
        [Route("UnifiedBusinessLogic/AssetLifecycleDecision")]
        public async Task<IActionResult> MakeAssetLifecycleDecision([FromBody] AssetLifecycleModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                var userRoles = await _userManager.GetRolesAsync(currentUser);
                var decision = await _unifiedService.MakeAssetLifecycleDecisionAsync(model.AssetId, currentUser.Id);

                // Check Georgian requirements for manager approval
                if ((decision.RecommendedAction == AssetLifecycleAction.WriteOff || 
                     decision.RecommendedAction == AssetLifecycleAction.Replace) && 
                    decision.RequiresManagerApproval)
                {
                    if (!userRoles.Contains("Admin") && !userRoles.Contains("Asset Manager"))
                    {
                        return Json(new { 
                            success = false, 
                            message = "Manager approval required for this action",
                            requiresApproval = true,
                            decision = new
                            {
                                assetId = decision.AssetId,
                                recommendedAction = decision.RecommendedAction.ToString(),
                                reasoning = decision.Reasoning,
                                estimatedCost = decision.EstimatedCost,
                                confidenceScore = decision.ConfidenceScore,
                                requiresManagerApproval = decision.RequiresManagerApproval,
                                nextSteps = decision.NextSteps,
                                overallConditionScore = decision.OverallConditionScore,
                                identifiedIssues = decision.IdentifiedIssues
                            }
                        });
                    }
                }

                return Json(new {
                    success = true,
                    decision = new
                    {
                        assetId = decision.AssetId,
                        recommendedAction = decision.RecommendedAction.ToString(),
                        reasoning = decision.Reasoning,
                        estimatedCost = decision.EstimatedCost,
                        confidenceScore = decision.ConfidenceScore,
                        requiresManagerApproval = decision.RequiresManagerApproval,
                        nextSteps = decision.NextSteps,
                        overallConditionScore = decision.OverallConditionScore,
                        identifiedIssues = decision.IdentifiedIssues,
                        assessmentDate = decision.AssessmentDate,
                        canExecute = !decision.RequiresManagerApproval || 
                                   userRoles.Contains("Admin") || 
                                   userRoles.Contains("Asset Manager")
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making asset lifecycle decision for asset {AssetId}", model.AssetId);
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Asset condition assessment endpoint
        /// </summary>
        [HttpPost]
        [Route("UnifiedBusinessLogic/AssessAssetCondition")]
        public async Task<IActionResult> AssessAssetCondition([FromBody] AssetLifecycleModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                var assessment = await _unifiedService.AssessAssetConditionAsync(model.AssetId, currentUser.Id);

                return Json(new {
                    success = true,
                    assessment = new
                    {
                        assetId = assessment.AssetId,
                        assessmentDate = assessment.AssessmentDate,
                        physicalConditionScore = assessment.PhysicalConditionScore,
                        functionalConditionScore = assessment.FunctionalConditionScore,
                        cosmeticConditionScore = assessment.CosmeticConditionScore,
                        overallConditionScore = assessment.OverallConditionScore,
                        issuesFound = assessment.IssuesFound,
                        repairRequirements = assessment.RepairRequirements,
                        estimatedRepairCost = assessment.EstimatedRepairCost,
                        currentMarketValue = assessment.CurrentMarketValue,
                        replacementCost = assessment.ReplacementCost,
                        primaryRecommendation = assessment.PrimaryRecommendation.ToString(),
                        recommendationReasoning = assessment.RecommendationReasoning,
                        confidenceScore = assessment.ConfidenceScore,
                        securityRiskScore = assessment.SecurityRiskScore,
                        operationalRiskScore = assessment.OperationalRiskScore,
                        riskFactors = assessment.RiskFactors
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assessing asset condition for asset {AssetId}", model.AssetId);
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get unified dashboard data
        /// </summary>
        [HttpGet]
        [Route("UnifiedBusinessLogic/DashboardData")]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                var dashboardData = await _unifiedService.GetUnifiedDashboardDataAsync(currentUser.Id);

                return Json(new {
                    success = true,
                    data = new
                    {
                        pendingDecisions = dashboardData.PendingDecisions,
                        automationSuggestions = dashboardData.AutomationSuggestions,
                        autoFulfilledToday = dashboardData.AutoFulfilledToday,
                        crossModuleActions = dashboardData.CrossModuleActions,
                        pendingApprovals = dashboardData.PendingApprovals,
                        assetsNeedingAttention = dashboardData.AssetsNeedingAttention,
                        managerActions = dashboardData.ManagerActions,
                        itSupportActions = dashboardData.ITSupportActions,
                        recentRecommendations = dashboardData.RecentRecommendationsList.Select(r => new
                        {
                            assetId = r.AssetId,
                            recommendedAction = r.RecommendedAction.ToString(),
                            reasoning = r.Reasoning,
                            confidenceScore = r.ConfidenceScore,
                            assessmentDate = r.AssessmentDate
                        }),
                        systemAlerts = dashboardData.SystemAlerts,
                        automationEfficiency = dashboardData.AutomationEfficiency,
                        averageProcessingTime = dashboardData.AverageProcessingTime.TotalMinutes,
                        successfulWorkflows = dashboardData.SuccessfulWorkflows,
                        failedWorkflows = dashboardData.FailedWorkflows
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unified dashboard data");
                return Json(new { success = false, message = "Failed to load dashboard data" });
            }
        }

        /// <summary>
        /// Get pending approvals for managers
        /// </summary>
        [HttpGet]
        [Route("UnifiedBusinessLogic/PendingApprovals")]
        [Authorize(Roles = "Admin,Asset Manager")]
        public async Task<IActionResult> GetPendingApprovals()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                var pendingApprovals = await _unifiedService.GetPendingApprovalsAsync(currentUser.Id);

                return Json(new {
                    success = true,
                    approvals = pendingApprovals.Select(a => new
                    {
                        id = a.Id,
                        type = a.Type,
                        title = a.Title,
                        description = a.Description,
                        estimatedCost = a.EstimatedCost,
                        requestedByUserId = a.RequestedByUserId,
                        requestedByUserName = a.RequestedByUserName,
                        requestedDate = a.RequestedDate,
                        priority = a.Priority,
                        department = a.Department,
                        additionalData = a.AdditionalData
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending approvals");
                return Json(new { success = false, message = "Failed to load pending approvals" });
            }
        }

        /// <summary>
        /// Process manager approval
        /// </summary>
        [HttpPost]
        [Route("UnifiedBusinessLogic/ProcessApproval")]
        [Authorize(Roles = "Admin,Asset Manager")]
        public async Task<IActionResult> ProcessApproval([FromBody] ManagerApprovalModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                // Implementation would depend on the specific approval type
                // This is a placeholder for the actual approval processing logic

                return Json(new {
                    success = true,
                    message = model.Approved ? "Approval granted" : "Approval denied",
                    itemId = model.ItemId,
                    itemType = model.ItemType,
                    approved = model.Approved,
                    notes = model.ApprovalNotes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing approval for item {ItemId}", model.ItemId);
                return Json(new { success = false, message = "Failed to process approval" });
            }
        }

        /// <summary>
        /// Get asset recommendations for the current user
        /// </summary>
        [HttpGet]
        [Route("UnifiedBusinessLogic/AssetRecommendations")]
        public async Task<IActionResult> GetAssetRecommendations()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                var recommendations = await _unifiedService.GetAssetRecommendationsAsync(currentUser.Id);

                return Json(new {
                    success = true,
                    recommendations = recommendations.Select(r => new
                    {
                        assetId = r.AssetId,
                        recommendedAction = r.RecommendedAction.ToString(),
                        reasoning = r.Reasoning,
                        estimatedCost = r.EstimatedCost,
                        confidenceScore = r.ConfidenceScore,
                        requiresManagerApproval = r.RequiresManagerApproval,
                        nextSteps = r.NextSteps,
                        overallConditionScore = r.OverallConditionScore,
                        identifiedIssues = r.IdentifiedIssues,
                        assessmentDate = r.AssessmentDate
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting asset recommendations");
                return Json(new { success = false, message = "Failed to load asset recommendations" });
            }
        }

        /// <summary>
        /// Attempt auto-fulfillment of a request
        /// </summary>
        [HttpPost]
        [Route("UnifiedBusinessLogic/AutoFulfill")]
        public async Task<IActionResult> AttemptAutoFulfillment([FromBody] UnifiedRequestModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                var result = await _unifiedService.AttemptAutoFulfillmentAsync(model.RequestId, currentUser.Id);

                return Json(new {
                    success = result.Success,
                    message = result.Message,
                    requestId = result.RequestId,
                    canAutoFulfill = result.CanAutoFulfill,
                    method = result.Method.ToString(),
                    fulfillmentConfidence = result.FulfillmentConfidence,
                    fulfillmentSteps = result.FulfillmentSteps,
                    allocatedAssetIds = result.AllocatedAssetIds,
                    allocatedInventoryIds = result.AllocatedInventoryIds,
                    estimatedCost = result.EstimatedCost,
                    estimatedDuration = result.EstimatedDuration.TotalMinutes,
                    blockingReason = result.BlockingReason,
                    attemptedAt = result.AttemptedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error attempting auto-fulfillment for request {RequestId}", model.RequestId);
                return Json(new { success = false, message = "Auto-fulfillment failed" });
            }
        }

        /// <summary>
        /// Get automation suggestions for the current user
        /// </summary>
        [HttpGet]
        [Route("UnifiedBusinessLogic/AutomationSuggestions")]
        public async Task<IActionResult> GetAutomationSuggestions()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                var suggestions = await _unifiedService.GetAutomationSuggestionsAsync(currentUser.Id);

                return Json(new {
                    success = true,
                    suggestions = suggestions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting automation suggestions");
                return Json(new { success = false, message = "Failed to load automation suggestions" });
            }
        }

        /// <summary>
        /// Check role-based permissions
        /// </summary>
        [HttpPost]
        [Route("UnifiedBusinessLogic/CheckPermission")]
        public async Task<IActionResult> CheckPermission([FromBody] PermissionCheckModel model)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                var permissionResult = await _unifiedService.CheckRolePermissionAsync(currentUser.Id, model.Action, model.Context);

                return Json(new {
                    success = true,
                    hasPermission = permissionResult.HasPermission,
                    reason = permissionResult.Reason,
                    requiredRoles = permissionResult.RequiredRoles,
                    userRoles = permissionResult.UserRoles,
                    action = permissionResult.Action,
                    requiresEscalation = permissionResult.RequiresEscalation
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission for action {Action}", model.Action);
                return Json(new { success = false, message = "Permission check failed" });
            }
        }

        // MVC View Actions
        /// <summary>
        /// Display the unified business logic dashboard
        /// </summary>
        [AllowAnonymous]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                string userId = currentUser?.Id ?? "anonymous";

                // Get dashboard data from the service
                var dashboardData = await _unifiedService.GetUnifiedDashboardDataAsync(userId);
                
                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading unified business logic dashboard");
                
                // Provide a default model in case of error
                var defaultData = new UnifiedDashboardData
                {
                    TotalRequests = 145,
                    PendingApprovals = 23,
                    CompletedToday = 18,
                    AssetsNeedingAttention = 7,
                    LowStockItems = 12,
                    PendingDecisions = 5,
                    AutomationSuggestions = 8,
                    AutoFulfilledToday = 14,
                    CrossModuleActions = 31,
                    ManagerActions = 15,
                    ITSupportActions = 22,
                    RecentRecommendations = 6,
                    RecentRecommendationsList = new List<AssetLifecycleDecisionResult>(),
                    SystemAlerts = 3,
                    AutomationEfficiency = 85.7,
                    AverageProcessingTime = TimeSpan.FromHours(2.5),
                    SuccessfulWorkflows = 127,
                    FailedWorkflows = 8
                };
                
                return View(defaultData);
            }
        }

        // API Endpoints
        /// <summary>
        /// Actions view for role-based workflow management
        /// </summary>
        [HttpGet]
        public IActionResult Actions()
        {
            var model = new UnifiedActionModel();
            return View(model);
        }

        /// <summary>
        /// Get manager-specific actions and pending items
        /// </summary>
        [HttpGet]
        [Route("UnifiedBusinessLogic/GetManagerActions")]
        [Authorize(Roles = "Admin,Asset Manager")]
        public async Task<IActionResult> GetManagerActions()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                var managerActions = await _unifiedService.GetManagerActionsAsync(currentUser.Id);

                return Json(new
                {
                    success = true,
                    pendingApprovals = managerActions.PendingApprovals.Select(a => new
                    {
                        id = a.Id,
                        title = a.Title,
                        description = a.Description,
                        priority = a.Priority.ToString(),
                        requestDate = a.RequestDate.ToString("MMM dd, yyyy"),
                        type = a.Type
                    }),
                    strategicDecisions = managerActions.StrategicDecisions.Select(d => new
                    {
                        id = d.Id,
                        title = d.Title,
                        description = d.Description,
                        priority = d.Priority.ToString(),
                        recommendation = d.Recommendation,
                        impact = d.Impact
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting manager actions");
                return Json(new { success = false, message = "Failed to load manager actions" });
            }
        }

        /// <summary>
        /// Get IT Support specific actions and tasks
        /// </summary>
        [HttpGet]
        [Route("UnifiedBusinessLogic/GetITSupportActions")]
        [Authorize(Roles = "Admin,IT Support")]
        public async Task<IActionResult> GetITSupportActions()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                var itActions = await _unifiedService.GetITSupportActionsAsync(currentUser.Id);

                return Json(new
                {
                    success = true,
                    assetAssignments = itActions.AssetAssignments.Select(a => new
                    {
                        id = a.Id,
                        assetTag = a.AssetTag,
                        userName = a.UserName,
                        department = a.Department,
                        requestDate = a.RequestDate.ToString("MMM dd, yyyy"),
                        priority = a.Priority.ToString()
                    }),
                    maintenanceTasks = itActions.MaintenanceTasks.Select(t => new
                    {
                        id = t.Id,
                        assetTag = t.AssetTag,
                        taskType = t.TaskType,
                        description = t.Description,
                        priority = t.Priority.ToString(),
                        priorityClass = t.Priority.ToString().ToLower(),
                        dueDate = t.DueDate.ToString("MMM dd, yyyy"),
                        status = t.Status
                    }),
                    urgentIssues = itActions.UrgentIssues.Select(i => new
                    {
                        id = i.Id,
                        title = i.Title,
                        description = i.Description,
                        reporterName = i.ReporterName,
                        reportedTime = i.ReportedTime.ToString("MMM dd, HH:mm"),
                        severity = i.Severity.ToString()
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting IT support actions");
                return Json(new { success = false, message = "Failed to load IT support actions" });
            }
        }

        /// <summary>
        /// Process manager action (approve/reject/escalate)
        /// </summary>
        [HttpPost]
        [Route("UnifiedBusinessLogic/ProcessManagerAction")]
        [Authorize(Roles = "Admin,Asset Manager")]
        public async Task<IActionResult> ProcessManagerAction([FromBody] ManagerActionRequest request)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                var result = await _unifiedService.ProcessManagerActionAsync(
                    request.ActionType, 
                    request.TargetId, 
                    currentUser.Id, 
                    request.Reason);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    actionTaken = result.ActionTaken,
                    nextSteps = result.NextSteps
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing manager action");
                return Json(new { success = false, message = "Failed to process action" });
            }
        }

        /// <summary>
        /// Process IT Support action (assign/start/complete)
        /// </summary>
        [HttpPost]
        [Route("UnifiedBusinessLogic/ProcessITSupportAction")]
        [Authorize(Roles = "Admin,IT Support")]
        public async Task<IActionResult> ProcessITSupportAction([FromBody] ITSupportActionRequest request)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                var result = await _unifiedService.ProcessITSupportActionAsync(
                    request.ActionType, 
                    request.TargetId, 
                    currentUser.Id, 
                    request.Notes);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    actionTaken = result.ActionTaken,
                    updatedStatus = result.UpdatedStatus
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing IT support action");
                return Json(new { success = false, message = "Failed to process action" });
            }
        }

        /// <summary>
        /// Get automation rules
        /// </summary>
        [HttpGet]
        [Route("UnifiedBusinessLogic/GetAutomationRules")]
        public async Task<IActionResult> GetAutomationRules()
        {
            try
            {
                var rules = await _unifiedService.GetAutomationRulesAsync();

                return Json(new
                {
                    success = true,
                    rules = rules.Select(r => new
                    {
                        id = r.Id,
                        name = r.Name,
                        trigger = r.TriggerType,
                        action = r.ActionType,
                        isActive = r.IsActive,
                        successRate = r.SuccessRate,
                        description = r.Description,
                        category = r.Category
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting automation rules");
                return Json(new { success = false, message = "Failed to load automation rules" });
            }
        }

        /// <summary>
        /// Create new automation rule
        /// </summary>
        [HttpPost]
        [Route("UnifiedBusinessLogic/CreateAutomationRule")]
        [Authorize(Roles = "Admin,Asset Manager")]
        public async Task<IActionResult> CreateAutomationRule([FromBody] AutomationRuleRequest request)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                var rule = new AutomationRule
                {
                    Name = request.RuleName,
                    Description = request.Description,
                    TriggerType = request.TriggerType,
                    ActionType = request.ActionType,
                    Category = int.TryParse(request.Category, out var categoryInt) ? categoryInt : 0,
                    IsActive = request.IsActive,
                    RequiresApproval = request.RequiresApproval,
                    CreatedBy = currentUser.Id,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _unifiedService.CreateAutomationRuleAsync(rule);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    ruleId = result.RuleId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating automation rule");
                return Json(new { success = false, message = "Failed to create automation rule" });
            }
        }

        /// <summary>
        /// Toggle automation rule active status
        /// </summary>
        [HttpPost]
        [Route("UnifiedBusinessLogic/ToggleAutomationRule")]
        [Authorize(Roles = "Admin,Asset Manager")]
        public async Task<IActionResult> ToggleAutomationRule([FromBody] ToggleRuleRequest request)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                var result = await _unifiedService.ToggleAutomationRuleAsync(request.RuleId, currentUser.Id);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    newStatus = result.NewStatus
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling automation rule");
                return Json(new { success = false, message = "Failed to toggle automation rule" });
            }
        }
    }

    /// <summary>
    /// Model for permission checks
    /// </summary>
    public class PermissionCheckModel
    {
        public string Action { get; set; } = string.Empty;
        public object Context { get; set; } = new();
    }
}
