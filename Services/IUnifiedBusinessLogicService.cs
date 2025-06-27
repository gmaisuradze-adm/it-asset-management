using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Services
{
    public interface IUnifiedBusinessLogicService
    {
        Task<UnifiedDashboardViewModel> GetDashboardDataAsync(string userId, List<string> userRoles);
        Task<UnifiedActionViewModel> GetActionItemsAsync(string userId, List<string> userRoles);
        Task<List<SmartInsight>> GetSmartInsightsAsync(string userId, List<string> userRoles);
        Task<List<RecentActivity>> GetRecentActivitiesAsync(string userId, List<string> userRoles, int count = 10);
        Task<List<Alert>> GetAlertsAsync(string userId, List<string> userRoles, bool unreadOnly = false);
        Task<List<QuickAction>> GetQuickActionsAsync(string userId, List<string> userRoles);
        Task<bool> MarkAlertAsReadAsync(int alertId, string userId);
        Task<bool> DismissAlertAsync(int alertId, string userId);
        Task<PerformanceMetrics> GetPerformanceMetricsAsync(string userId, List<string> userRoles);
        Task<WorkflowSummary> GetWorkflowSummaryAsync(string userId, List<string> userRoles);
        Task<bool> ExecuteQuickActionAsync(string actionId, string userId, Dictionary<string, object>? parameters = null);
        Task RefreshCacheAsync();
    }
}
