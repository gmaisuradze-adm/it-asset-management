using HospitalAssetTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HospitalAssetTracker.Services
{
    /// <summary>
    /// Central orchestrator for unified business logic that coordinates existing services
    /// according to Georgian requirements (Manager/IT Support roles)
    /// </summary>
    public interface IUnifiedBusinessLogicService
    {
        // Request processing using existing services
        Task<UnifiedRequestProcessingResult> ProcessRequestAsync(ITRequest request, string userId);
        Task<AssetLifecycleDecisionResult> MakeAssetLifecycleDecisionAsync(int assetId, string userId);
        Task<CrossModuleWorkflowResult> ExecuteCrossModuleWorkflowAsync(string workflowType, object context, string userId);
        
        // Role-based operations (Georgian requirements)
        Task<RoleBasedActionResult> ExecuteManagerActionAsync(string action, object parameters, string userId);
        Task<RoleBasedActionResult> ExecuteITSupportActionAsync(string action, object parameters, string userId);
        Task<PermissionCheckResult> CheckRolePermissionAsync(string userId, string action, object context);
        
        // Asset lifecycle intelligence
        Task<AssetConditionAssessment> AssessAssetConditionAsync(int assetId, string assessorUserId);
        Task<List<AssetLifecycleDecisionResult>> GetAssetRecommendationsAsync(string userId);
        
        // Automation and auto-fulfillment
        Task<AutoFulfillmentResult> AttemptAutoFulfillmentAsync(int requestId, string userId);
        Task<List<string>> GetAutomationSuggestionsAsync(string userId);
        
        // Dashboard and reporting
        Task<UnifiedDashboardData> GetUnifiedDashboardDataAsync(string userId);
        Task<List<PendingApprovalItem>> GetPendingApprovalsAsync(string userId);
        
        // Actions management
        Task<ManagerActionsData> GetManagerActionsAsync(string userId);
        Task<ITSupportActionsData> GetITSupportActionsAsync(string userId);
        Task<ActionResult> ProcessManagerActionAsync(string actionType, int targetId, string userId, string? reason);
        Task<ActionResult> ProcessITSupportActionAsync(string actionType, int targetId, string userId, string? notes);
        
        // Automation rules
        Task<List<AutomationRule>> GetAutomationRulesAsync();
        Task<AutomationRuleResult> CreateAutomationRuleAsync(AutomationRule rule);
        Task<AutomationRuleResult> ToggleAutomationRuleAsync(int ruleId, string userId);
    }
}
