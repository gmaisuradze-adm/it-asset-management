using Microsoft.EntityFrameworkCore;
using HospitalAssetTracker.Data;
using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Services
{
    /// <summary>
    /// Cross-Module Integration Service Implementation
    /// Handles complex business processes that span multiple modules
    /// </summary>
    public class CrossModuleIntegrationService : ICrossModuleIntegrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRequestService _requestService;
        private readonly IAssetService _assetService;
        private readonly IInventoryService _inventoryService;
        private readonly IProcurementService _procurementService;
        private readonly IAuditService _auditService;
        private readonly ILogger<CrossModuleIntegrationService> _logger;

        public CrossModuleIntegrationService(
            ApplicationDbContext context,
            IRequestService requestService,
            IAssetService assetService,
            IInventoryService inventoryService,
            IProcurementService procurementService,
            IAuditService auditService,
            ILogger<CrossModuleIntegrationService> logger)
        {
            _context = context;
            _requestService = requestService;
            _assetService = assetService;
            _inventoryService = inventoryService;
            _procurementService = procurementService;
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// Complete asset repair workflow with procurement and inventory integration
        /// Example: Printer repair request → check parts needed → generate procurement → handle replacement → complete repair
        /// </summary>
        public async Task<AssetRepairWorkflowResult> ProcessAssetRepairWorkflowAsync(int requestId, string userId)
        {
            _logger.LogInformation("Processing asset repair workflow for request {RequestId} by user {UserId}", requestId, userId);

            try
            {
                var request = await _context.ITRequests
                    .Include(r => r.RelatedAsset)
                    .ThenInclude(a => a!.Location)
                    .Include(r => r.AssignedTo)
                    .FirstOrDefaultAsync(r => r.Id == requestId);

                if (request == null)
                {
                    return new AssetRepairWorkflowResult
                    {
                        Success = false,
                        Message = "Request not found"
                    };
                }

                if (request.RelatedAsset == null)
                {
                    return new AssetRepairWorkflowResult
                    {
                        Success = false,
                        Message = "No asset associated with this request"
                    };
                }

                var workflowSteps = new List<string>();
                var statusUpdates = new Dictionary<string, object>();
                var result = new AssetRepairWorkflowResult();

                // Step 1: Update request status to In Progress
                request.Status = RequestStatus.InProgress;
                workflowSteps.Add($"Updated request status to In Progress");

                // Step 2: Update asset status to Under Repair
                request.RelatedAsset.Status = AssetStatus.UnderMaintenance;
                workflowSteps.Add($"Updated asset {request.RelatedAsset.AssetTag} status to Under Maintenance");

                // Step 3: Check if parts are needed (simulate business logic)
                var needsParts = await CheckIfPartsNeededAsync(request.RelatedAsset);
                if (needsParts.Any())
                {
                    result.RequiresProcurement = true;
                    workflowSteps.Add($"Identified {needsParts.Count} parts needed for repair");

                    // Step 4: Generate procurement requests
                    var procurementResult = await GenerateProcurementFromRepairAsync(requestId, needsParts, userId);
                    if (procurementResult.Success)
                    {
                        result.GeneratedProcurementIds = procurementResult.GeneratedProcurementRequestIds;
                        workflowSteps.Add($"Generated {procurementResult.GeneratedProcurementRequestIds.Count} procurement requests");
                    }
                }

                // Step 5: Check for temporary replacement
                var replacementAsset = await FindTemporaryReplacementAsync(request.RelatedAsset);
                if (replacementAsset != null)
                {
                    var replacementResult = await ReplaceAssetTemporarilyAsync(requestId, replacementAsset.Id, userId);
                    if (replacementResult.Success)
                    {
                        result.TemporaryReplacementAssetId = replacementAsset.Id;
                        workflowSteps.Add($"Temporarily replaced with asset {replacementAsset.AssetTag}");
                    }
                }

                // Step 6: Add workflow tracking
                await AddWorkflowTrackingAsync(requestId, workflowSteps, userId);

                // Step 7: Save changes
                await _context.SaveChangesAsync();

                // Step 8: Create audit log
                await _auditService.LogAsync(
                    AuditAction.Create,
                    "Request Workflow",
                    requestId,
                    userId,
                    $"Initiated asset repair workflow - Asset {request.RelatedAsset.AssetTag}");

                result.Success = true;
                result.Message = "Asset repair workflow initiated successfully";
                result.WorkflowSteps = workflowSteps;
                result.StatusUpdates = statusUpdates;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing asset repair workflow for request {RequestId}", requestId);
                return new AssetRepairWorkflowResult
                {
                    Success = false,
                    Message = $"Error processing workflow: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Replace asset temporarily from inventory during repair
        /// </summary>
        public async Task<AssetReplacementResult> ReplaceAssetTemporarilyAsync(int requestId, int replacementAssetId, string userId)
        {
            _logger.LogInformation("Replacing asset temporarily for request {RequestId} with asset {AssetId}", requestId, replacementAssetId);

            try
            {
                var request = await _context.ITRequests
                    .Include(r => r.RelatedAsset)
                    .FirstOrDefaultAsync(r => r.Id == requestId);

                var replacementAsset = await _context.Assets
                    .FirstOrDefaultAsync(a => a.Id == replacementAssetId);

                if (request?.RelatedAsset == null || replacementAsset == null)
                {
                    return new AssetReplacementResult
                    {
                        Success = false,
                        Message = "Asset or replacement asset not found"
                    };
                }

                // Move replacement asset to original location
                var originalLocationId = request.RelatedAsset.LocationId;
                replacementAsset.LocationId = originalLocationId;
                replacementAsset.Status = AssetStatus.InUse;

                // Move original asset to maintenance area (or inventory)
                var maintenanceLocation = await _context.Locations
                    .FirstOrDefaultAsync(l => l.Room.Contains("Maintenance") || l.Room.Contains("IT"));

                if (maintenanceLocation != null)
                {
                    request.RelatedAsset.LocationId = maintenanceLocation.Id;
                }

                // Create asset movement records
                var originalMovement = new AssetMovement
                {
                    AssetId = request.RelatedAsset.Id,
                    FromLocationId = originalLocationId,
                    ToLocationId = maintenanceLocation?.Id,
                    MovementDate = DateTime.UtcNow,
                    PerformedByUserId = userId,
                    Reason = $"Moved to maintenance for repair - Request #{requestId}",
                    MovementType = MovementType.Repair
                };

                var replacementMovement = new AssetMovement
                {
                    AssetId = replacementAssetId,
                    FromLocationId = replacementAsset.LocationId,
                    ToLocationId = originalLocationId,
                    MovementDate = DateTime.UtcNow,
                    PerformedByUserId = userId,
                    Reason = $"Temporary replacement for asset {request.RelatedAsset.AssetTag} - Request #{requestId}",
                    MovementType = MovementType.LocationTransfer
                };

                _context.AssetMovements.AddRange(originalMovement, replacementMovement);
                await _context.SaveChangesAsync();

                return new AssetReplacementResult
                {
                    Success = true,
                    Message = "Asset replaced temporarily",
                    OriginalAssetId = request.RelatedAsset.Id,
                    ReplacementAssetId = replacementAssetId,
                    LocationId = originalLocationId ?? 0,
                    ReplacementReason = $"Temporary replacement during repair - Request #{requestId}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error replacing asset temporarily for request {RequestId}", requestId);
                return new AssetReplacementResult
                {
                    Success = false,
                    Message = $"Error replacing asset: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Handle procurement request generation from repair needs
        /// </summary>
        public async Task<ProcurementGenerationResult> GenerateProcurementFromRepairAsync(int requestId, List<RepairPartRequest> partRequests, string userId)
        {
            _logger.LogInformation("Generating procurement requests for repair - Request {RequestId}", requestId);

            try
            {
                var request = await _context.ITRequests.FirstOrDefaultAsync(r => r.Id == requestId);
                if (request == null)
                {
                    return new ProcurementGenerationResult
                    {
                        Success = false,
                        Message = "Original request not found"
                    };
                }

                var generatedIds = new List<int>();
                var generatedItems = new List<string>();
                var totalCost = 0m;

                foreach (var partRequest in partRequests)
                {
                    var procurementRequest = new ProcurementRequest
                    {
                        RequestNumber = GenerateProcurementRequestNumber(),
                        Title = $"Repair Parts for {request.Title}",
                        Description = $"Parts needed for asset repair - Original Request #{requestId}\n\n{partRequest.Description}",
                        Category = ProcurementCategory.Maintenance,
                        Priority = ConvertRequestPriorityToProcurementPriority(partRequest.Priority),
                        Status = ProcurementStatus.Pending,
                        RequestedByUserId = userId,
                        Department = request.Department,
                        RequestDate = DateTime.UtcNow,
                        RequiredByDate = DateTime.UtcNow.AddDays(3), // Urgent for repairs
                        EstimatedBudget = partRequest.EstimatedPrice * partRequest.Quantity,
                        BusinessJustification = $"Required for asset repair - Request #{requestId}",
                        RelatedRequestId = requestId // Link to original IT request
                    };

                    _context.ProcurementRequests.Add(procurementRequest);
                    await _context.SaveChangesAsync();

                    generatedIds.Add(procurementRequest.Id);
                    generatedItems.Add($"{partRequest.PartName} (Qty: {partRequest.Quantity})");
                    totalCost += procurementRequest.EstimatedBudget;
                }

                // Update original request with procurement references
                request.Description += $"\n\nAuto-generated procurement requests: {string.Join(", ", generatedIds.Select(id => $"#{id}"))}";
                await _context.SaveChangesAsync();

                return new ProcurementGenerationResult
                {
                    Success = true,
                    Message = $"Generated {generatedIds.Count} procurement requests",
                    GeneratedProcurementRequestIds = generatedIds,
                    TotalEstimatedCost = totalCost,
                    GeneratedItems = generatedItems
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating procurement requests for repair - Request {RequestId}", requestId);
                return new ProcurementGenerationResult
                {
                    Success = false,
                    Message = $"Error generating procurement: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Update request status when procurement is completed
        /// </summary>
        public async Task<bool> UpdateRequestFromProcurementCompletionAsync(int requestId, int procurementRequestId, string userId)
        {
            try
            {
                var request = await _context.ITRequests.FirstOrDefaultAsync(r => r.Id == requestId);
                var procurement = await _context.ProcurementRequests.FirstOrDefaultAsync(p => p.Id == procurementRequestId);

                if (request == null || procurement == null)
                    return false;

                // Check if all related procurement requests are completed
                var relatedProcurements = await _context.ProcurementRequests
                    .Where(p => p.RelatedRequestId == requestId)
                    .ToListAsync();

                var allCompleted = relatedProcurements.All(p => p.Status == ProcurementStatus.Completed);

                if (allCompleted)
                {
                    // Update request status and add note
                    request.Status = RequestStatus.InProgress;
                    request.Description += $"\n\nProcurement completed on {DateTime.UtcNow:yyyy-MM-dd HH:mm}. Parts are now available for repair.";

                    await _context.SaveChangesAsync();

                    // Log audit
                    await _auditService.LogAsync(
                        AuditAction.Update,
                        "Request",
                        requestId,
                        userId,
                        "Updated request status after procurement completion - All parts available for repair");

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating request from procurement completion");
                return false;
            }
        }

        /// <summary>
        /// Complete repair and update asset status and location
        /// </summary>
        public async Task<RepairCompletionResult> CompleteAssetRepairAsync(int requestId, string completionNotes, int? finalLocationId, string userId)
        {
            _logger.LogInformation("Completing asset repair for request {RequestId}", requestId);

            try
            {
                var request = await _context.ITRequests
                    .Include(r => r.RelatedAsset)
                    .FirstOrDefaultAsync(r => r.Id == requestId);

                if (request?.RelatedAsset == null)
                {
                    return new RepairCompletionResult
                    {
                        Success = false,
                        Message = "Request or related asset not found"
                    };
                }

                // Update asset status
                request.RelatedAsset.Status = AssetStatus.InUse;
                
                // Move asset to final location
                if (finalLocationId.HasValue)
                {
                    var movement = new AssetMovement
                    {
                        AssetId = request.RelatedAsset.Id,
                        FromLocationId = request.RelatedAsset.LocationId,
                        ToLocationId = finalLocationId.Value,
                        MovementDate = DateTime.UtcNow,
                        PerformedByUserId = userId,
                        Reason = $"Returned after repair completion - Request #{requestId}",
                        MovementType = MovementType.Return
                    };

                    request.RelatedAsset.LocationId = finalLocationId.Value;
                    _context.AssetMovements.Add(movement);
                }

                // Update request status
                request.Status = RequestStatus.Completed;
                request.CompletedDate = DateTime.UtcNow;
                request.CompletionNotes = completionNotes;

                // Handle temporary replacement return
                var tempReplacement = await FindTemporaryReplacementForRequestAsync(requestId);
                if (tempReplacement != null)
                {
                    await ReturnTemporaryReplacementAsync(tempReplacement.Id, userId);
                }

                await _context.SaveChangesAsync();

                return new RepairCompletionResult
                {
                    Success = true,
                    Message = "Asset repair completed successfully",
                    AssetId = request.RelatedAsset.Id,
                    NewAssetStatus = AssetStatus.InUse,
                    FinalLocationId = finalLocationId,
                    CompletionNotes = completionNotes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing asset repair for request {RequestId}", requestId);
                return new RepairCompletionResult
                {
                    Success = false,
                    Message = $"Error completing repair: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get integrated workflow status for a request
        /// </summary>
        public async Task<WorkflowStatusResult> GetWorkflowStatusAsync(int requestId)
        {
            try
            {
                var request = await _context.ITRequests
                    .Include(r => r.RelatedAsset)
                    .FirstOrDefaultAsync(r => r.Id == requestId);

                if (request == null)
                {
                    return new WorkflowStatusResult
                    {
                        RequestId = requestId,
                        PendingActions = new List<string> { "Request not found" }
                    };
                }

                var workflowSteps = await GetWorkflowStepsAsync(requestId);
                var pendingActions = await GetPendingActionsAsync(requestId);
                var relatedProcurements = await GetRelatedProcurementIdsAsync(requestId);
                var temporaryAsset = await GetTemporaryAssetIdAsync(requestId);

                return new WorkflowStatusResult
                {
                    RequestId = requestId,
                    RequestStatus = request.Status,
                    WorkflowSteps = workflowSteps,
                    PendingActions = pendingActions,
                    RelatedProcurementIds = relatedProcurements,
                    TemporaryAssetId = temporaryAsset
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow status for request {RequestId}", requestId);
                return new WorkflowStatusResult
                {
                    RequestId = requestId,
                    PendingActions = new List<string> { "Error retrieving workflow status" }
                };
            }
        }

        #region Private Helper Methods

        private async Task<List<RepairPartRequest>> CheckIfPartsNeededAsync(Asset asset)
        {
            // Simulate checking if parts are needed based on asset type and issue
            await Task.Delay(1); // Add async operation
            var parts = new List<RepairPartRequest>();

            // For printers, common parts might be needed
            if (asset.Category.ToString().Contains("Printer"))
            {
                parts.Add(new RepairPartRequest
                {
                    PartName = "Printer Maintenance Kit",
                    PartNumber = $"MK-{asset.Brand}-{asset.Model}",
                    Quantity = 1,
                    EstimatedPrice = 150.00m,
                    Vendor = "Office Supplies Inc",
                    Description = "Standard maintenance kit for printer repair",
                    Priority = RequestPriority.Medium
                });
            }

            return parts;
        }

        private async Task<Asset?> FindTemporaryReplacementAsync(Asset originalAsset)
        {
            // Find similar asset in inventory that's available
            return await _context.Assets
                .Where(a => a.Category == originalAsset.Category && 
                           a.Status == AssetStatus.Available &&
                           a.Id != originalAsset.Id)
                .FirstOrDefaultAsync();
        }

        private async Task<Asset?> FindTemporaryReplacementForRequestAsync(int requestId)
        {
            // Find if there's a temporary replacement for this request
            var movements = await _context.AssetMovements
                .Where(m => m.Reason != null && m.Reason.Contains($"Request #{requestId}") && 
                           m.MovementType == MovementType.LocationTransfer)
                .ToListAsync();

            if (movements.Any())
            {
                var assetId = movements.First().AssetId;
                return await _context.Assets.FirstOrDefaultAsync(a => a.Id == assetId);
            }

            return null;
        }

        private async Task ReturnTemporaryReplacementAsync(int tempAssetId, string userId)
        {
            var tempAsset = await _context.Assets.FirstOrDefaultAsync(a => a.Id == tempAssetId);
            if (tempAsset != null)
            {
                // Return to inventory or original location
                var inventoryLocation = await _context.Locations
                    .FirstOrDefaultAsync(l => l.Room.Contains("Inventory") || l.Room.Contains("Storage"));

                if (inventoryLocation != null)
                {
                    var movement = new AssetMovement
                    {
                        AssetId = tempAssetId,
                        FromLocationId = tempAsset.LocationId,
                        ToLocationId = inventoryLocation.Id,
                        MovementDate = DateTime.UtcNow,
                        PerformedByUserId = userId,
                        Reason = "Returned temporary replacement to inventory",
                        MovementType = MovementType.Return
                    };

                    tempAsset.LocationId = inventoryLocation.Id;
                    tempAsset.Status = AssetStatus.Available;
                    _context.AssetMovements.Add(movement);
                }
            }
        }

        private async Task AddWorkflowTrackingAsync(int requestId, List<string> steps, string userId)
        {
            // Add workflow tracking entries (you might have a separate WorkflowTracking table)
            foreach (var step in steps)
            {
                await _auditService.LogAsync(
                    AuditAction.Update,
                    "Workflow",
                    requestId,
                    userId,
                    step);
            }
        }

        private async Task<List<WorkflowStep>> GetWorkflowStepsAsync(int requestId)
        {
            // Get workflow steps from audit logs
            var auditLogs = await _context.AuditLogs
                .Where(a => a.EntityId == requestId && a.EntityType == "Workflow")
                .OrderBy(a => a.Timestamp)
                .ToListAsync();

            return auditLogs.Select(log => new WorkflowStep
            {
                StepName = log.Action.ToString(),
                Description = log.Description ?? "",
                CompletedAt = log.Timestamp,
                CompletedBy = log.UserId,
                Status = "Completed"
            }).ToList();
        }

        private async Task<List<string>> GetPendingActionsAsync(int requestId)
        {
            var pendingActions = new List<string>();
            var request = await _context.ITRequests.FirstOrDefaultAsync(r => r.Id == requestId);

            if (request != null)
            {
                switch (request.Status)
                {
                    case RequestStatus.Submitted: // Changed from Open
                        pendingActions.Add("Assign to technician");
                        break;
                    case RequestStatus.InProgress:
                        pendingActions.Add("Complete repair work");
                        break;
                }

                // Check for pending procurement
                var pendingProcurement = await _context.ProcurementRequests
                    .Where(p => p.RelatedRequestId == requestId && p.Status != ProcurementStatus.Completed)
                    .AnyAsync();

                if (pendingProcurement)
                {
                    pendingActions.Add("Wait for procurement completion");
                }
            }

            return pendingActions;
        }

        private async Task<List<int>> GetRelatedProcurementIdsAsync(int requestId)
        {
            return await _context.ProcurementRequests
                .Where(p => p.RelatedRequestId == requestId)
                .Select(p => p.Id)
                .ToListAsync();
        }

        private async Task<int?> GetTemporaryAssetIdAsync(int requestId)
        {
            var movement = await _context.AssetMovements
                .Where(m => m.Reason != null && m.Reason.Contains($"Request #{requestId}") && 
                           m.MovementType == MovementType.LocationTransfer)
                .FirstOrDefaultAsync();

            return movement?.AssetId;
        }

        private string GenerateProcurementRequestNumber()
        {
            return $"PR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        }

        private ProcurementPriority ConvertRequestPriorityToProcurementPriority(RequestPriority requestPriority)
        {
            return requestPriority switch
            {
                RequestPriority.Low => ProcurementPriority.Low,
                RequestPriority.Medium => ProcurementPriority.Medium,
                RequestPriority.High => ProcurementPriority.High,
                RequestPriority.Critical => ProcurementPriority.High,
                _ => ProcurementPriority.Medium
            };
        }

        #endregion
    }
}
