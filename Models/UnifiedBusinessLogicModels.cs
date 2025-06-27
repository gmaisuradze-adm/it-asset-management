using System.ComponentModel.DataAnnotations;

namespace HospitalAssetTracker.Models
{
    // Use RequestPriority as the main Priority enum
    using Priority = RequestPriority;
    
    public class UnifiedDashboardViewModel
    {
        public AssetSummary AssetSummary { get; set; } = new();
        public InventorySummary InventorySummary { get; set; } = new();
        public ProcurementSummary ProcurementSummary { get; set; } = new();
        public RequestSummary RequestSummary { get; set; } = new();
        public List<RecentActivity> RecentActivities { get; set; } = new();
        public List<Alert> PendingAlerts { get; set; } = new();
        public WorkflowSummary WorkflowSummary { get; set; } = new();
        public List<QuickAction> AvailableActions { get; set; } = new();
        public PerformanceMetrics PerformanceMetrics { get; set; } = new();
    }

    public class AssetSummary
    {
        public int TotalAssets { get; set; }
        public int ActiveAssets { get; set; }
        public int AvailableAssets { get; set; }
        public int InUseAssets { get; set; }
        public int InRepairAssets { get; set; }
        public int DecommissionedAssets { get; set; }
        public Dictionary<AssetCategory, int> AssetsByCategory { get; set; } = new();
        public Dictionary<AssetStatus, int> AssetsByStatus { get; set; } = new();
        public List<Asset> RecentlyAddedAssets { get; set; } = new();
        public List<Asset> AssetsNeedingAttention { get; set; } = new();
    }

    public class InventorySummary
    {
        public int TotalItems { get; set; }
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }
        public int PendingReceiptsItems { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public List<InventoryItem> LowStockAlerts { get; set; } = new();
        public List<InventoryItem> RecentMovements { get; set; } = new();
        public Dictionary<string, int> InventoryByLocation { get; set; } = new();
    }

    public class ProcurementSummary
    {
        public int TotalPurchaseOrders { get; set; }
        public int PendingPOs { get; set; }
        public int ApprovedPOs { get; set; }
        public int ReceivedPOs { get; set; }
        public decimal TotalPOValue { get; set; }
        public decimal PendingPOValue { get; set; }
        public List<ProcurementRequest> RecentPOs { get; set; } = new();
        public List<ProcurementRequest> PendingApprovalPOs { get; set; } = new();
        public Dictionary<string, decimal> SpendingByCategory { get; set; } = new();
    }

    public class RequestSummary
    {
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int RejectedRequests { get; set; }
        public List<ITRequest> RecentRequests { get; set; } = new();
        public List<ITRequest> HighPriorityRequests { get; set; } = new();
        public Dictionary<RequestType, int> RequestsByType { get; set; } = new();
        public Dictionary<Priority, int> RequestsByPriority { get; set; } = new();
    }

    public class RecentActivity
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // Asset, Request, Procurement, Inventory
        public string Action { get; set; } = string.Empty; // Created, Updated, Approved, etc.
        public string Description { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string ColorClass { get; set; } = string.Empty;
    }

    public class Alert
    {
        public int Id { get; set; }
        public AlertType Type { get; set; }
        public AlertSeverity Severity { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;
        public string ActionText { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string ColorClass { get; set; } = string.Empty;
    }

    public enum AlertType
    {
        AssetMaintenance,
        WarrantyExpiration,
        LowStock,
        PendingApproval,
        SystemUpdate,
        SecurityAlert,
        RequestOverdue,
        ProcurementDelay
    }

    public enum AlertSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    public class WorkflowSummary
    {
        public int PendingApprovals { get; set; }
        public int ActiveWorkflows { get; set; }
        public int CompletedToday { get; set; }
        public int OverdueItems { get; set; }
        public List<WorkflowItem> PendingItems { get; set; } = new();
        public List<WorkflowItem> RecentCompletions { get; set; } = new();
        public Dictionary<string, int> WorkflowsByType { get; set; } = new();
    }

    public class WorkflowItem
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // Request, Procurement, Asset, etc.
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public Priority Priority { get; set; }
        public string ActionUrl { get; set; } = string.Empty;
        public bool IsOverdue { get; set; }
        public int DaysOverdue { get; set; }
    }

    public class QuickAction
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;
        public string ColorClass { get; set; } = string.Empty;
        public List<string> RequiredRoles { get; set; } = new();
        public int Priority { get; set; }
        public bool IsEnabled { get; set; } = true;
    }

    public class PerformanceMetrics
    {
        public double AverageRequestProcessingTime { get; set; } // in hours
        public double AssetUtilizationRate { get; set; } // percentage
        public double MaintenanceComplianceRate { get; set; } // percentage
        public double ProcurementEfficiency { get; set; } // percentage
        public double UserSatisfactionScore { get; set; } // 1-10 scale
        public Dictionary<string, double> KPIs { get; set; } = new();
        public List<TrendData> TrendData { get; set; } = new();
    }

    public class TrendData
    {
        public DateTime Date { get; set; }
        public string Metric { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    public class UnifiedActionViewModel
    {
        public List<PendingApproval> PendingApprovals { get; set; } = new();
        public List<ScheduledTask> ScheduledTasks { get; set; } = new();
        public List<OverdueItem> OverdueItems { get; set; } = new();
        public List<Assignment> PendingAssignments { get; set; } = new();
        public List<Notification> UnreadNotifications { get; set; } = new();
        public WorkflowStatistics WorkflowStats { get; set; } = new();
    }

    public class PendingApproval
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // Request, Procurement, Asset Transfer, etc.
        public string Title { get; set; } = string.Empty;
        public string Requestor { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; }
        public Priority Priority { get; set; }
        public decimal? Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;
        public bool IsOverdue { get; set; }
        public int DaysWaiting { get; set; }
    }

    public class ScheduledTask
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // Maintenance, Audit, Review, etc.
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
        public TaskStatus Status { get; set; }
        public Priority Priority { get; set; }
        public string ActionUrl { get; set; } = string.Empty;
        public bool IsOverdue { get; set; }
    }

    public enum TaskStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled,
        Overdue
    }

    public class OverdueItem
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
        public Priority Priority { get; set; }
        public string ActionUrl { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class Assignment
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public string AssignedBy { get; set; } = string.Empty;
        public DateTime AssignedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public Priority Priority { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class WorkflowStatistics
    {
        public int TotalWorkflows { get; set; }
        public int ActiveWorkflows { get; set; }
        public int CompletedThisWeek { get; set; }
        public int CompletedThisMonth { get; set; }
        public double AverageCompletionTime { get; set; } // in hours
        public double WorkflowEfficiency { get; set; } // percentage
        public Dictionary<string, int> WorkflowsByStatus { get; set; } = new();
        public Dictionary<string, double> PerformanceByType { get; set; } = new();
    }

    public class SmartInsight
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public InsightType Type { get; set; }
        public InsightPriority Priority { get; set; }
        public string ActionRecommendation { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;
        public double Confidence { get; set; } // 0-1 scale
        public DateTime GeneratedDate { get; set; }
        public List<string> SupportingData { get; set; } = new();
        public string Icon { get; set; } = string.Empty;
        public string ColorClass { get; set; } = string.Empty;
    }

    public enum InsightType
    {
        CostOptimization,
        EfficiencyImprovement,
        MaintenancePrediction,
        InventoryOptimization,
        SecurityRecommendation,
        ComplianceAlert,
        PerformanceOptimization
    }

    public enum InsightPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    // Simple notification model for unified dashboard
    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;
    }
}
