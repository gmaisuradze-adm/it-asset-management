public interface IIntegratedBusinessLogicService
{
    // Cross-module workflow operations
    // Task<bool> ProcessRequestApprovalWorkflowAsync(int requestId, string approverId, bool approved, string? comments = null); // Obsolete
    Task<bool> ExecuteRequestFulfillmentAsync(int requestId, string fulfillmentUserId);
}