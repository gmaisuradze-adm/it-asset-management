using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Services
{
    /// <summary>
    /// Simplified Workflow Orchestration Service Interface
    /// This interface provides basic workflow orchestration without complex dependencies
    /// </summary>
    public interface ISimpleWorkflowOrchestrationService
    {
        // Basic workflow operations
        Task<WorkflowResult> StartRequestWorkflowAsync(int requestId, string userId);
        Task<WorkflowResult> StartAssetMaintenanceWorkflowAsync(int assetId, string action, string userId);
        Task<WorkflowResult> StartProcurementWorkflowAsync(string trigger, string userId);
        
        // Status and monitoring
        Task<WorkflowStatusInfo> GetWorkflowStatusAsync(Guid workflowId);
        Task<List<ActiveWorkflowInfo>> GetActiveWorkflowsAsync();
        Task<WorkflowResult> CancelWorkflowAsync(Guid workflowId, string userId);
        
        // Analytics
        Task<WorkflowAnalytics> GetWorkflowAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }

    /// <summary>
    /// Basic workflow result
    /// </summary>
    public class WorkflowResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? WorkflowId { get; set; }
        public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;
        public string InitiatedBy { get; set; } = string.Empty;
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    /// <summary>
    /// Workflow status information
    /// </summary>
    public class WorkflowStatusInfo
    {
        public Guid WorkflowId { get; set; }
        public string WorkflowType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double Progress { get; set; }
        public string CurrentStep { get; set; } = string.Empty;
        public int StepsCompleted { get; set; }
        public int TotalSteps { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EstimatedCompletion { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<string> CompletedActions { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Active workflow information
    /// </summary>
    public class ActiveWorkflowInfo
    {
        public Guid WorkflowId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double Progress { get; set; }
        public DateTime StartTime { get; set; }
        public string InitiatedBy { get; set; } = string.Empty;
        public int? RequestId { get; set; }
        public int? AssetId { get; set; }
        public string? AdditionalInfo { get; set; }
    }

    /// <summary>
    /// Workflow analytics summary
    /// </summary>
    public class WorkflowAnalytics
    {
        public int TotalWorkflows { get; set; }
        public int ActiveWorkflows { get; set; }
        public int CompletedToday { get; set; }
        public double AverageProcessingTimeHours { get; set; }
        public double AutomationRate { get; set; }
        public Dictionary<string, int> WorkflowTypeDistribution { get; set; } = new();
        public List<WorkflowPerformancePoint> PerformanceData { get; set; } = new();
    }

    /// <summary>
    /// Performance data point for charts
    /// </summary>
    public class WorkflowPerformancePoint
    {
        public DateTime Date { get; set; }
        public int CompletedWorkflows { get; set; }
        public double AverageProcessingTime { get; set; }
    }
}
