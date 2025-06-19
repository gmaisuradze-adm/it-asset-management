using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Services
{
    /// <summary>
    /// Cross-Module Integration Service Interface
    /// Handles complex business processes that span multiple modules (Assets, Requests, Procurement, Inventory)
    /// </summary>
    public interface ICrossModuleIntegrationService
    {
        /// <summary>
        /// Complete asset repair workflow with procurement and inventory integration
        /// </summary>
        Task<AssetRepairWorkflowResult> ProcessAssetRepairWorkflowAsync(int requestId, string userId);

        /// <summary>
        /// Replace asset temporarily from inventory during repair
        /// </summary>
        Task<AssetReplacementResult> ReplaceAssetTemporarilyAsync(int requestId, int replacementAssetId, string userId);

        /// <summary>
        /// Handle procurement request generation from repair needs
        /// </summary>
        Task<ProcurementGenerationResult> GenerateProcurementFromRepairAsync(int requestId, List<RepairPartRequest> partRequests, string userId);

        /// <summary>
        /// Update request status when procurement is completed
        /// </summary>
        Task<bool> UpdateRequestFromProcurementCompletionAsync(int requestId, int procurementRequestId, string userId);

        /// <summary>
        /// Complete repair and update asset status and location
        /// </summary>
        Task<RepairCompletionResult> CompleteAssetRepairAsync(int requestId, string completionNotes, int? finalLocationId, string userId);

        /// <summary>
        /// Get integrated workflow status for a request
        /// </summary>
        Task<WorkflowStatusResult> GetWorkflowStatusAsync(int requestId);
    }

    /// <summary>
    /// Asset repair workflow result
    /// </summary>
    public class AssetRepairWorkflowResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> WorkflowSteps { get; set; } = new();
        public Dictionary<string, object> StatusUpdates { get; set; } = new();
        public bool RequiresProcurement { get; set; }
        public List<int> GeneratedProcurementIds { get; set; } = new();
        public int? TemporaryReplacementAssetId { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Asset replacement result
    /// </summary>
    public class AssetReplacementResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int OriginalAssetId { get; set; }
        public int ReplacementAssetId { get; set; }
        public int LocationId { get; set; }
        public DateTime ReplacementDate { get; set; } = DateTime.UtcNow;
        public string ReplacementReason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Procurement generation result
    /// </summary>
    public class ProcurementGenerationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<int> GeneratedProcurementRequestIds { get; set; } = new();
        public decimal TotalEstimatedCost { get; set; }
        public List<string> GeneratedItems { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Repair completion result
    /// </summary>
    public class RepairCompletionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int AssetId { get; set; }
        public AssetStatus NewAssetStatus { get; set; }
        public int? FinalLocationId { get; set; }
        public DateTime CompletionDate { get; set; } = DateTime.UtcNow;
        public string CompletionNotes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Workflow status result
    /// </summary>
    public class WorkflowStatusResult
    {
        public int RequestId { get; set; }
        public RequestStatus RequestStatus { get; set; }
        public List<WorkflowStep> WorkflowSteps { get; set; } = new();
        public List<string> PendingActions { get; set; } = new();
        public List<int> RelatedProcurementIds { get; set; } = new();
        public int? TemporaryAssetId { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

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
}
