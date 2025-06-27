using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalAssetTracker.Models
{
    /// <summary>
    /// Priority levels for requests and actions
    /// </summary>
    public enum Priority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4,
        Emergency = 5
    }
    /// <summary>
    /// Unified request processing result with intelligent analysis
    /// </summary>
    public class UnifiedRequestProcessingResult
    {
        public int RequestId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime ProcessingTime { get; set; }
        public string ProcessedByUserId { get; set; } = string.Empty;
        
        // Integration with existing workflow models
        public WorkflowExecutionResult? WorkflowResult { get; set; }
        public List<string> ProcessingSteps { get; set; } = new();
        public Dictionary<string, object> ResultData { get; set; } = new();
        
        // Georgian requirements support
        public bool RequiresEscalation { get; set; }
        public string? EscalationReason { get; set; }
        public bool RequiresManagerApproval { get; set; }
        public string ProcessingMethod { get; set; } = string.Empty; // "Automated" | "Manual" | "Escalated"
    }

    /// <summary>
    /// Asset lifecycle decision result with intelligent recommendations
    /// </summary>
    public class AssetLifecycleDecisionResult
    {
        public int AssetId { get; set; }
        public AssetLifecycleAction RecommendedAction { get; set; }
        public string Reasoning { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public double ConfidenceScore { get; set; }
        public bool RequiresManagerApproval { get; set; }
        public List<string> NextSteps { get; set; } = new();
        public DateTime AssessmentDate { get; set; }
        public string AssessedByUserId { get; set; } = string.Empty;
        
        // Detailed analysis
        public int OverallConditionScore { get; set; } // 0-100
        public List<string> IdentifiedIssues { get; set; } = new();
        public Dictionary<string, object> AnalysisData { get; set; } = new();
    }

    /// <summary>
    /// <summary>
    /// Role-based action result for Georgian requirements
    /// </summary>
    public class RoleBasedActionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool RequiresEscalation { get; set; }
        public string? EscalationReason { get; set; }
        public Dictionary<string, object> ActionResults { get; set; } = new();
        public string ActionType { get; set; } = string.Empty;
        public DateTime ExecutedAt { get; set; }
        public string ExecutedByUserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Permission check result for role-based operations
    /// </summary>
    public class PermissionCheckResult
    {
        public bool HasPermission { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public List<string> RequiredRoles { get; set; } = new();
        public List<string> UserRoles { get; set; } = new();
        public string Action { get; set; } = string.Empty;
        public bool RequiresEscalation { get; set; }
    }

    /// <summary>
    /// Asset condition assessment with intelligent scoring
    /// </summary>
    public class AssetConditionAssessment
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public virtual Asset Asset { get; set; } = null!;
        
        public DateTime AssessmentDate { get; set; }
        public string AssessedByUserId { get; set; } = string.Empty;
        public virtual ApplicationUser AssessedByUser { get; set; } = null!;
        
        // Condition scoring (0-100)
        public int PhysicalConditionScore { get; set; }
        public int FunctionalConditionScore { get; set; }
        public int CosmeticConditionScore { get; set; }
        public int OverallConditionScore { get; set; }
        
        // Assessment details
        public List<string> IssuesFound { get; set; } = new();
        public List<string> RepairRequirements { get; set; } = new();
        public decimal EstimatedRepairCost { get; set; }
        public decimal CurrentMarketValue { get; set; }
        public decimal ReplacementCost { get; set; }
        
        // Intelligent recommendations
        public AssetRecommendationType PrimaryRecommendation { get; set; }
        public string RecommendationReasoning { get; set; } = string.Empty;
        public double ConfidenceScore { get; set; }
        
        // Risk assessment
        public int SecurityRiskScore { get; set; } // 0-100
        public int OperationalRiskScore { get; set; } // 0-100
        public List<string> RiskFactors { get; set; } = new();
    }

    /// <summary>
    /// Asset recommendation types
    /// </summary>
    public enum AssetRecommendationType
    {
        Repair = 1,
        Replace = 2,
        WriteOff = 3,
        Upgrade = 4,
        Maintain = 5,
        Monitor = 6
    }

    /// <summary>
    /// Cross-module workflow result
    /// </summary>
    public class CrossModuleWorkflowResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string WorkflowType { get; set; } = string.Empty;
        public int? AssetId { get; set; }
        public int? RequestId { get; set; }
        public string InitiatedByUserId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<string> Steps { get; set; } = new();
        public Dictionary<string, object> Results { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Extended auto-fulfillment result for unified business logic
    /// </summary>
    public class ExtendedAutoFulfillmentResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RequestId { get; set; }
        public bool CanAutoFulfill { get; set; }
        public AutoFulfillmentMethod Method { get; set; }
        public double FulfillmentConfidence { get; set; }
        public List<string> FulfillmentSteps { get; set; } = new();
        public List<int> AllocatedAssetIds { get; set; } = new();
        public List<int> AllocatedInventoryIds { get; set; } = new();
        public decimal EstimatedCost { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public string? BlockingReason { get; set; }
        public DateTime AttemptedAt { get; set; }
        public string AttemptedByUserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Unified dashboard data
    /// </summary>
    public class UnifiedDashboardData
    {
        public int TotalRequests { get; set; }
        public int PendingApprovals { get; set; }
        public int CompletedToday { get; set; }
        public int AssetsNeedingAttention { get; set; }
        public int LowStockItems { get; set; }
        public int PendingDecisions { get; set; }
        public int AutomationSuggestions { get; set; }
        public int AutoFulfilledToday { get; set; }
        public int CrossModuleActions { get; set; }
        public int ManagerActions { get; set; }
        public int ITSupportActions { get; set; }
        public int RecentRecommendations { get; set; }
        public List<AssetLifecycleDecisionResult> RecentRecommendationsList { get; set; } = new();
        public int SystemAlerts { get; set; }
        public double AutomationEfficiency { get; set; }
        public TimeSpan AverageProcessingTime { get; set; }
        public int SuccessfulWorkflows { get; set; }
        public int FailedWorkflows { get; set; }
        public Dictionary<string, object> AdditionalMetrics { get; set; } = new();
    }

    /// <summary>
    /// Pending approval item
    /// </summary>
    public class PendingApprovalItem
    {
        public int Id { get; set; }
        public string ItemType { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; }
        public DateTime RequestedDate { get; set; }
        public string SubmittedByUserId { get; set; } = string.Empty;
        public string SubmittedByUserName { get; set; } = string.Empty;
        public string RequestedByUserId { get; set; } = string.Empty;
        public string RequestedByUserName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public RequestPriority Priority { get; set; }
        public Dictionary<string, object> ApprovalData { get; set; } = new();
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    /// <summary>
    /// Request models for unified processing
    /// </summary>
    public class UnifiedRequestModel
    {
        public int RequestId { get; set; }
        public string Action { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
    }

    public class AssetLifecycleModel
    {
        public int AssetId { get; set; }
        public string? PreferredAction { get; set; }
        public string? Notes { get; set; }
    }

    public class ManagerApprovalModel
    {
        public int ItemId { get; set; }
        public string ItemType { get; set; } = string.Empty;
        public bool Approved { get; set; }
        public string? ApprovalNotes { get; set; }
    }

    // Action request models
    public class ManagerActionRequest
    {
        public string ActionType { get; set; } = string.Empty;
        public int TargetId { get; set; }
        public string? Reason { get; set; }
    }

    public class ITSupportActionRequest
    {
        public string ActionType { get; set; } = string.Empty;
        public int TargetId { get; set; }
        public string? Notes { get; set; }
    }

    public class AutomationRuleRequest
    {
        public string RuleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TriggerType { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool RequiresApproval { get; set; } = false;
    }

    public class ToggleRuleRequest
    {
        public int RuleId { get; set; }
    }

    public class UnifiedActionModel
    {
        public int TotalPendingActions { get; set; }
        public int ManagerActions { get; set; }
        public int ITSupportActions { get; set; }
        public int AutomationRules { get; set; }
    }

    // Manager action data models
    public class ManagerActionsData
    {
        public List<PendingApproval> PendingApprovals { get; set; } = new();
        public List<StrategicDecision> StrategicDecisions { get; set; } = new();
    }

    public class PendingApproval
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Priority Priority { get; set; }
        public DateTime RequestDate { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    public class StrategicDecision
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Priority Priority { get; set; }
        public string Recommendation { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty;
    }

    // IT Support action data models
    public class ITSupportActionsData
    {
        public List<AssetAssignment> AssetAssignments { get; set; } = new();
        public List<MaintenanceTask> MaintenanceTasks { get; set; } = new();
        public List<UrgentIssue> UrgentIssues { get; set; } = new();
    }

    public class AssetAssignment
    {
        public int Id { get; set; }
        public string AssetTag { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public Priority Priority { get; set; }
    }

    public class MaintenanceTask
    {
        public int Id { get; set; }
        public string AssetTag { get; set; } = string.Empty;
        public string TaskType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Priority Priority { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class UrgentIssue
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ReporterName { get; set; } = string.Empty;
        public DateTime ReportedTime { get; set; }
        public string Severity { get; set; } = string.Empty;
    }

    // Result models
    public class ActionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ActionTaken { get; set; } = string.Empty;
        public string NextSteps { get; set; } = string.Empty;
        public string UpdatedStatus { get; set; } = string.Empty;
    }

    public class AutomationRuleResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RuleId { get; set; }
        public bool NewStatus { get; set; }
    }

    /// <summary>
    /// Automation rule for intelligent workflow processing
    /// </summary>
    public class AutomationRule
    {
        // Database columns (exact match with DB schema)
        public int Id { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string Trigger { get; set; } = string.Empty;
        public string ConditionsJson { get; set; } = string.Empty;
        public string ActionsJson { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string CreatedByUserId { get; set; } = string.Empty;
        public DateTime? LastModifiedDate { get; set; }
        public int TriggerCount { get; set; } = 0;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        public string Name { get; set; } = string.Empty;
        public int Priority { get; set; } = 0;
        public int ExecutionCount { get; set; } = 0;
        public int Category { get; set; } = 0;
        public bool HasExecutionErrors { get; set; } = false;
        public DateTime? LastExecutedDate { get; set; }
        public string? LastExecutionError { get; set; }
        public string TriggerType { get; set; } = string.Empty;
        
        // Compatibility properties for unified business logic layer
        [NotMapped]
        public DateTime? LastTriggered 
        { 
            get => LastExecutedDate; 
            set => LastExecutedDate = value; 
        }
        
        [NotMapped]
        public DateTime? LastExecuted 
        { 
            get => LastExecutedDate; 
            set => LastExecutedDate = value; 
        }
        
        [NotMapped]
        public DateTime CreatedAt
        {
            get => CreatedDate;
            set => CreatedDate = value;
        }
        
        [NotMapped]
        public string CreatedBy
        {
            get => CreatedByUserId;
            set => CreatedByUserId = value;
        }
        
        [NotMapped]
        public string? RuleConfiguration 
        { 
            get => ConditionsJson; 
            set => ConditionsJson = value ?? string.Empty; 
        }
        
        [NotMapped]
        public string ActionType 
        { 
            get => TriggerType; 
            set => TriggerType = value; 
        }
        
        [NotMapped]
        public bool RequiresApproval { get; set; } = false;
        
        [NotMapped]
        public float SuccessRate { get; set; } = 1.0f;
        
        [NotMapped]
        public AutomationTrigger RuleType
        {
            get 
            {
                if (Enum.TryParse<AutomationTrigger>(Trigger, out var triggerType))
                    return triggerType;
                return AutomationTrigger.ManualTrigger;
            }
            set => Trigger = value.ToString();
        }
        
        // Navigation properties
        public virtual ApplicationUser? CreatedByUser { get; set; }
        public virtual ICollection<AutomationLog> Logs { get; set; } = new List<AutomationLog>();
    }

    /// <summary>
    /// Automation trigger types
    /// </summary>
    public enum AutomationTrigger
    {
        StockLevelReached = 1,
        ItemReceived = 2,
        QualityAssessmentCompleted = 3,
        RequestCreated = 4,
        ScheduledInterval = 5,
        ManualTrigger = 6
    }
}
