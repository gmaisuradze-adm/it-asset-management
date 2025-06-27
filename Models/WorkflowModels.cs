using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace HospitalAssetTracker.Models
{
    #region Core Workflow Models

    /// <summary>
    /// Workflow instance entity
    /// </summary>
    public class WorkflowInstance
    {
        public Guid Id { get; set; }
        public string WorkflowType { get; set; } = string.Empty;
        public WorkflowStatus Status { get; set; }
        public string InitiatedByUserId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Configuration { get; set; } = string.Empty; // JSON
        public int CurrentStep { get; set; }
        public int TotalSteps { get; set; }
        public string? ErrorMessage { get; set; }
        public string? CompensationData { get; set; } // JSON for rollback
        
        // Navigation properties
        public ApplicationUser InitiatedByUser { get; set; } = null!;
        public List<WorkflowStepInstance> WorkflowSteps { get; set; } = new();
        public List<WorkflowEvent> WorkflowEvents { get; set; } = new();
    }

    /// <summary>
    /// Individual workflow step instance
    /// </summary>
    public class WorkflowStepInstance
    {
        public int Id { get; set; }
        public Guid WorkflowInstanceId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string StepType { get; set; } = string.Empty;
        public int StepOrder { get; set; }
        public WorkflowStepStatus Status { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Input { get; set; } // JSON
        public string? Output { get; set; } // JSON
        public string? ErrorMessage { get; set; }
        public string? CompensationAction { get; set; } // JSON
        public string ExecutedByUserId { get; set; } = string.Empty;
        
        // Navigation properties
        public WorkflowInstance WorkflowInstance { get; set; } = null!;
        public ApplicationUser ExecutedByUser { get; set; } = null!;
    }

    /// <summary>
    /// Workflow event for event-driven processing
    /// </summary>
    public class WorkflowEvent
    {
        public int Id { get; set; }
        public Guid WorkflowId { get; set; }
        public WorkflowEventType EventType { get; set; }
        public string EventData { get; set; } = string.Empty; // JSON
        public string Data { get; set; } = string.Empty; // Alias for EventData for compatibility
        public DateTime Timestamp { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? StepName { get; set; }
        public bool IsProcessed { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? ProcessingResult { get; set; }
        
        // Navigation properties
        public WorkflowInstance? Workflow { get; set; }
        public ApplicationUser User { get; set; } = null!;
    }



    #endregion

    #region Workflow Request Models

    /// <summary>
    /// Workflow execution request
    /// </summary>
    public class WorkflowRequest
    {
        [Required]
        public string WorkflowType { get; set; } = string.Empty;
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public Dictionary<string, object> Configuration { get; set; } = new();
        public WorkflowPriority Priority { get; set; } = WorkflowPriority.Medium;
        public DateTime? ScheduledStartTime { get; set; }
        public string? Description { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    /// <summary>
    /// Workflow execution result
    /// </summary>
    public class WorkflowExecutionResult
    {
        public Guid WorkflowId { get; set; }
        public string WorkflowType { get; set; } = string.Empty;
        public WorkflowStatus Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan? Duration { get; set; }
        public string InitiatedByUserId { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
        
        public List<WorkflowExecutionStep> ExecutionSteps { get; set; } = new();
        public List<WorkflowStepResult> StepResults { get; set; } = new();
        public Dictionary<string, object> Results { get; set; } = new();
    }

    /// <summary>
    /// Workflow orchestration status query result
    /// </summary>
    public class WorkflowOrchestrationStatusResult
    {
        public Guid WorkflowId { get; set; }
        public string WorkflowType { get; set; } = string.Empty;
        public WorkflowStatus Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int CurrentStep { get; set; }
        public int TotalSteps { get; set; }
        public double ProgressPercentage { get; set; }
        public string InitiatedByUserId { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        
        public List<WorkflowStepStatus> StepStatuses { get; set; } = new();
        public List<string> PendingActions { get; set; } = new();
        public Dictionary<string, object> RuntimeData { get; set; } = new();
    }

    #endregion

    #region Processing Result Models

    /// <summary>
    /// Request processing result with intelligent analysis
    /// </summary>
    public class RequestProcessingResult
    {
        public int RequestId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public RequestProcessingAction RecommendedAction { get; set; }
        public double ConfidenceScore { get; set; }
        public WorkflowExecutionResult? WorkflowExecutionResult { get; set; }
        
        public List<string> AutomationSuggestions { get; set; } = new();
        public List<ResourceRecommendation> ResourceRecommendations { get; set; } = new();
        public List<WorkflowStep> ProcessingSteps { get; set; } = new();
        public Dictionary<string, object> AnalysisData { get; set; } = new();
    }

    /// <summary>
    /// Auto-fulfillment attempt result
    /// </summary>
    public class AutoFulfillmentResult
    {
        // Properties for orchestration services
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AutoFulfillmentMethod FulfillmentMethod { get; set; }
        public DateTime AttemptedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        
        // Original properties
        public int RequestId { get; set; }
        public bool CanAutoFulfill { get; set; }
        public AutoFulfillmentMethod Method { get; set; }
        public double FulfillmentConfidence { get; set; }
        public List<AutoFulfillmentStep> Steps { get; set; } = new();
        public List<int> AllocatedAssetIds { get; set; } = new();
        public List<int> AllocatedInventoryIds { get; set; } = new();
        public decimal EstimatedCost { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public string? BlockingReason { get; set; }
        
        // Compatibility properties - for now just use empty lists until proper implementation
        public List<string> FulfillmentSteps { get; set; } = new();
    }

    /// <summary>
    /// Resource allocation optimization result
    /// </summary>
    public class ResourceAllocationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RequestId { get; set; }
        public bool OptimizationSuccessful { get; set; }
        public ResourceAllocationStrategy Strategy { get; set; }
        public List<ResourceAllocation> Allocations { get; set; } = new();
        public decimal OptimizedCost { get; set; }
        public decimal OriginalCost { get; set; }
        public decimal CostSavings => OriginalCost - OptimizedCost;
        public TimeSpan OptimizedDuration { get; set; }
        public List<string> OptimizationNotes { get; set; } = new();
    }

    #endregion

    #region Asset Lifecycle Models

    /// <summary>
    /// Asset lifecycle workflow result
    /// </summary>
    public class AssetLifecycleResult
    {
        public int AssetId { get; set; }
        public AssetLifecycleAction Action { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan? Duration { get; set; }
        public string UserId { get; set; } = string.Empty;
        public AssetStatus NewStatus { get; set; }
        public AssetStatus PreviousStatus { get; set; }
        public List<int> TriggeredWorkflowIds { get; set; } = new();
        public List<string> AutomatedActions { get; set; } = new();
        public Dictionary<string, object> LifecycleData { get; set; } = new();
        public Dictionary<string, object> ActionData { get; set; } = new();
    }

    /// <summary>
    /// Maintenance orchestration result
    /// </summary>
    public class MaintenanceOrchestrationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int AssetId { get; set; }
        public int MaintenanceRecordId { get; set; }
        public MaintenanceType MaintenanceType { get; set; }
        public DateTime ScheduledDate { get; set; }
        public bool AutoScheduled { get; set; }
        public List<int> RequiredPartIds { get; set; } = new();
        public List<int> TriggeredProcurementIds { get; set; } = new();
        public decimal EstimatedCost { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public List<string> PrerequisiteSteps { get; set; } = new();
        public List<MaintenanceRecord> ScheduledMaintenance { get; set; } = new();
        public Dictionary<string, object> OrchestrationData { get; set; } = new();
    }

    /// <summary>
    /// Asset replacement workflow result
    /// </summary>
    public class AssetReplacementResult
    {
        public int OldAssetId { get; set; }
        public int? NewAssetId { get; set; }
        public bool ReplacementCompleted { get; set; }
        public AssetReplacementReason Reason { get; set; }
        public List<int> DataMigrationSteps { get; set; } = new();
        public List<int> UserNotificationIds { get; set; } = new();
        public string? TemporaryAssetId { get; set; }
        public Dictionary<string, object> ReplacementData { get; set; } = new();
    }

    #endregion

    #region Procurement Automation Models

    /// <summary>
    /// Procurement orchestration result
    /// </summary>
    public class ProcurementOrchestrationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<int> CreatedRequestIds { get; set; } = new();
        public ProcurementTrigger Trigger { get; set; }
        public bool AutoApprovalApplied { get; set; }
        public List<VendorRecommendation> VendorRecommendations { get; set; } = new();
        public decimal TotalEstimatedValue { get; set; }
        public List<BudgetAllocation> BudgetAllocations { get; set; } = new();
        public List<string> ComplianceChecks { get; set; } = new();
        public List<ProcurementRequest> GeneratedRequests { get; set; } = new();
        public Dictionary<string, object> OrchestrationData { get; set; } = new();
    }

    /// <summary>
    /// Inventory replenishment result
    /// </summary>
    public class InventoryReplenishmentResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<int> ProcessedInventoryIds { get; set; } = new();
        public List<int> CreatedProcurementRequests { get; set; } = new();
        public int TotalItemsReplenished { get; set; }
        public decimal TotalReplenishmentValue { get; set; }
        public List<ReplenishmentRecommendation> Recommendations { get; set; } = new();
        public Dictionary<int, int> ReorderQuantities { get; set; } = new();
        public List<InventoryItem> ReplenishedItems { get; set; } = new();
        public Dictionary<string, object> ReplenishmentData { get; set; } = new();
    }

    #endregion

    #region Event Processing Models

    /// <summary>
    /// Event processing result
    /// </summary>
    public class EventProcessingResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalEvents { get; set; }
        public int ProcessedEvents { get; set; }
        public int FailedEvents { get; set; }
        public List<EventProcessingError> Errors { get; set; } = new();
        public List<int> TriggeredWorkflowIds { get; set; } = new();
        public TimeSpan ProcessingDuration { get; set; }
        public int ProcessedEventCount { get; set; }
        public List<WorkflowEvent> ProcessedEventsDetails { get; set; } = new();
        public Dictionary<string, object> ProcessingData { get; set; } = new();
    }

    /// <summary>
    /// Event processing error
    /// </summary>
    public class EventProcessingError
    {
        public int EventId { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime ErrorTime { get; set; }
        public string? StackTrace { get; set; }
    }

    #endregion

    #region Analytics Models

    /// <summary>
    /// Workflow analytics model
    /// </summary>
    public class WorkflowAnalyticsModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalWorkflows { get; set; }
        public int CompletedWorkflows { get; set; }
        public int FailedWorkflows { get; set; }
        public double SuccessRate => TotalWorkflows > 0 ? (double)CompletedWorkflows / TotalWorkflows * 100 : 0;
        public TimeSpan AverageExecutionTime { get; set; }
        
        public List<WorkflowTypeMetrics> TypeMetrics { get; set; } = new();
        public List<WorkflowPerformanceMetrics> PerformanceMetrics { get; set; } = new();
        public Dictionary<string, int> WorkflowsByStatus { get; set; } = new();
    }

    /// <summary>
    /// Workflow performance metrics
    /// </summary>
    public class WorkflowPerformanceMetrics
    {
        public string WorkflowType { get; set; } = string.Empty;
        public int ExecutionCount { get; set; }
        public double SuccessRate { get; set; }
        public TimeSpan AverageExecutionTime { get; set; }
        public TimeSpan MinExecutionTime { get; set; }
        public TimeSpan MaxExecutionTime { get; set; }
        public List<string> CommonFailureReasons { get; set; } = new();
    }

    /// <summary>
    /// Workflow optimization suggestions
    /// </summary>
    public class WorkflowOptimizationSuggestions
    {
        public List<OptimizationSuggestion> Suggestions { get; set; } = new();
        public List<PerformanceBottleneck> Bottlenecks { get; set; } = new();
        public List<AutomationOpportunity> AutomationOpportunities { get; set; } = new();
    }

    #endregion

    #region Supporting Models

    /// <summary>
    /// Workflow execution step definition
    /// </summary>
    public class WorkflowExecutionStep
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Order { get; set; }
        public Dictionary<string, object> Configuration { get; set; } = new();
        public List<string> Dependencies { get; set; } = new();
        public bool IsOptional { get; set; }
        public TimeSpan? EstimatedDuration { get; set; }
    }

    /// <summary>
    /// Workflow step execution result
    /// </summary>
    public class WorkflowStepResult
    {
        public string StepName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public Dictionary<string, object> Output { get; set; } = new();
    }

    /// <summary>
    /// Resource recommendation
    /// </summary>
    public class ResourceRecommendation
    {
        public ResourceType Type { get; set; }
        public int ResourceId { get; set; }
        public string ResourceName { get; set; } = string.Empty;
        public double MatchScore { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public decimal Cost { get; set; }
    }

    /// <summary>
    /// Auto-fulfillment step
    /// </summary>
    public class AutoFulfillmentStep
    {
        public int Order { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool RequiresApproval { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
    }



    /// <summary>
    /// Vendor recommendation
    /// </summary>
    public class VendorRecommendation
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public double RecommendationScore { get; set; }
        public decimal QuotedPrice { get; set; }
        public TimeSpan EstimatedDelivery { get; set; }
        public List<string> RecommendationReasons { get; set; } = new();
    }

    /// <summary>
    /// Budget allocation
    /// </summary>
    public class BudgetAllocation
    {
        public string BudgetCode { get; set; } = string.Empty;
        public decimal AllocatedAmount { get; set; }
        public string Department { get; set; } = string.Empty;
        public string ApprovalLevel { get; set; } = string.Empty;
    }

    /// <summary>
    /// Replenishment recommendation
    /// </summary>
    public class ReplenishmentRecommendation
    {
        public int InventoryItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int RecommendedOrderQuantity { get; set; }
        public ReplenishmentUrgency Urgency { get; set; }
        public decimal EstimatedCost { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Workflow type metrics
    /// </summary>
    public class WorkflowTypeMetrics
    {
        public string WorkflowType { get; set; } = string.Empty;
        public int Count { get; set; }
        public double SuccessRate { get; set; }
        public TimeSpan AverageExecutionTime { get; set; }
    }

    /// <summary>
    /// Optimization suggestion
    /// </summary>
    public class OptimizationSuggestion
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public OptimizationImpact Impact { get; set; }
        public OptimizationEffort ImplementationEffort { get; set; }
        public List<string> Steps { get; set; } = new();
    }

    /// <summary>
    /// Performance bottleneck
    /// </summary>
    public class PerformanceBottleneck
    {
        public string WorkflowType { get; set; } = string.Empty;
        public string StepName { get; set; } = string.Empty;
        public TimeSpan AverageExecutionTime { get; set; }
        public string BottleneckReason { get; set; } = string.Empty;
        public List<string> OptimizationSuggestions { get; set; } = new();
    }

    /// <summary>
    /// Automation opportunity
    /// </summary>
    public class AutomationOpportunity
    {
        public string ProcessName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double AutomationPotential { get; set; }
        public TimeSpan PotentialTimeSavings { get; set; }
        public decimal PotentialCostSavings { get; set; }
        public List<string> RequiredSteps { get; set; } = new();
    }

    #endregion

    #region Event and Notification Models

    /// <summary>
    /// Event subscription for automated notifications
    /// </summary>
    public class EventSubscription
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Filters { get; set; } = string.Empty; // JSON
        public string NotificationConfig { get; set; } = string.Empty; // JSON
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModified { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;
        
        // Navigation property
        public ApplicationUser CreatedByUser { get; set; } = null!;
    }

    /// <summary>
    /// System notification
    /// </summary>
    public class Notification
    {
        public Guid Id { get; set; }
        public string RecipientId { get; set; } = string.Empty;
        public string RecipientUserId { get; set; } = string.Empty; // Alias for compatibility
        public NotificationType NotificationType { get; set; }
        public NotificationType Type { get; set; } // Alias for compatibility
        public string Subject { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty; // Alias for Subject
        public string Message { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty; // JSON
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public string? ActionUrl { get; set; }
        public string? Metadata { get; set; }
        public NotificationPriority Priority { get; set; }
        public NotificationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ReadAt { get; set; }
        
        // Navigation property
        public ApplicationUser Recipient { get; set; } = null!;
    }

    /// <summary>
    /// Notification request for creating notifications
    /// </summary>
    public class NotificationRequest
    {
        public string RecipientUserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public NotificationPriority Priority { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public string? ActionUrl { get; set; }
        public string? Metadata { get; set; }
    }

    /// <summary>
    /// Notification processing result
    /// </summary>
    public class NotificationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? NotificationId { get; set; }
        public NotificationDeliveryStatus DeliveryStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string? ErrorMessage { get; set; }
    }

    #endregion

    #region Enums

    public enum WorkflowStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
        Cancelled,
        Suspended
    }

    public enum WorkflowStepStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
        Skipped,
        Cancelled
    }

    public enum WorkflowEventType
    {
        WorkflowStarted,
        WorkflowCompleted,
        WorkflowFailed,
        WorkflowCancelled,
        StepStarted,
        StepCompleted,
        StepFailed,
        EventTriggered,
        RuleApplied,
        CompensationExecuted
    }

    public enum WorkflowPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    public enum RequestProcessingAction
    {
        AutoFulfill,
        RequiresApproval,
        RequiresManualReview,
        Escalate,
        Reject,
        RouteToSpecialist
    }

    public enum AutoFulfillmentMethod
    {
        DirectAllocation,
        ProcurementRequest,
        InventoryReservation,
        AssetReassignment,
        ServiceRequest,
        Hybrid
    }

    public enum ResourceAllocationStrategy
    {
        CostOptimized,
        TimeOptimized,
        QualityOptimized,
        AvailabilityOptimized,
        Balanced
    }

    public enum AssetLifecycleAction
    {
        Deploy,
        Maintain,
        Repair,
        Upgrade,
        Replace,
        Retire,
        Dispose,
        Commission,
        WriteOff,
        Monitor
    }

    public enum AssetReplacementReason
    {
        EndOfLife,
        Failure,
        Upgrade,
        UserRequest,
        PolicyChange,
        Maintenance
    }

    public enum ProcurementTrigger
    {
        LowInventory,
        AssetFailure,
        UserRequest,
        MaintenanceSchedule,
        PolicyUpdate,
        BudgetAllocation
    }

    public enum ResourceType
    {
        Asset,
        InventoryItem,
        Service,
        Personnel,
        Budget,
        Facility
    }

    public enum ReplenishmentUrgency
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum OptimizationImpact
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum OptimizationEffort
    {
        Minimal,
        Low,
        Medium,
        High,
        Extensive
    }

    public enum NotificationType
    {
        Email,
        SMS,
        PushNotification,
        Push,
        InApp
    }

    public enum NotificationPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum NotificationStatus
    {
        Pending,
        Created,
        Sent,
        Delivered,
        Failed,
        Read,
        NotFound
    }

    /// <summary>
    /// Notification delivery status for tracking delivery progress
    /// </summary>
    public enum NotificationDeliveryStatus
    {
        Pending,
        Delivered,
        Failed,
        Retrying
    }

    #endregion

    #region Workflow Step Types and Related Enums

    public enum WorkflowStepType
    {
        AssetRequest,
        InventoryCheck,
        AssetAllocation,
        UserNotification,
        AssetDeployment,
        Validation,
        Approval,
        Escalation,
        Completion,
        DataValidation,
        ResourceAllocation,
        ServiceCall,
        Notification
    }

    #endregion

    #region Additional Workflow Classes

    /// <summary>
    /// Workflow step
    /// </summary>
    public class WorkflowStep
    {
        public string StepName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? CompletedAt { get; set; }
        public string CompletedBy { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Repair part request
    /// </summary>
    public class RepairPartRequest
    {
        public string PartName { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal EstimatedPrice { get; set; }
        public string Vendor { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public RequestPriority Priority { get; set; } = RequestPriority.Medium;
    }

    #endregion

    #region Workflow Status Results

    /// <summary>
    /// Workflow status result
    /// </summary>
    public class WorkflowStatusResult
    {
        // Properties for workflow orchestration
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid WorkflowId { get; set; }
        public WorkflowStatus Status { get; set; }
        public int CurrentStep { get; set; }
        public int TotalSteps { get; set; }
        public double ProgressPercentage { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<WorkflowStepInstance> CompletedSteps { get; set; } = new();
        public List<WorkflowEvent> RecentEvents { get; set; } = new();
        public string? ErrorMessage { get; set; }
        
        // Properties for integration controller
        public int RequestId { get; set; }
        public RequestStatus RequestStatus { get; set; }
        public List<WorkflowStep> WorkflowSteps { get; set; } = new();
        public List<string> PendingActions { get; set; } = new();
        public List<int> RelatedProcurementIds { get; set; } = new();
        public int? TemporaryAssetId { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    #endregion


}
