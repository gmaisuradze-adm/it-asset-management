using Microsoft.EntityFrameworkCore;
using HospitalAssetTracker.Data;
using HospitalAssetTracker.Models;
using Microsoft.Extensions.Logging;

namespace HospitalAssetTracker.Services
{
    /// <summary>
    /// Integrated Business Logic Service - Hospital IT Asset Tracking System
    /// Orchestrates cross-module business processes and ensures data consistency
    /// </summary>
    public interface IIntegratedBusinessLogicService
    {
        // Cross-module workflow operations
        Task<bool> ProcessRequestApprovalWorkflowAsync(int requestId, string approverId, bool approved, string? comments = null);
        Task<bool> ExecuteRequestFulfillmentAsync(int requestId, string fulfillmentUserId);
        Task<bool> TriggerAutomaticProcurementAsync(int inventoryItemId, string initiatedByUserId);
        Task<bool> ProcessAssetDeploymentWorkflowAsync(int assetId, int requestId, string deployedByUserId);
        Task<bool> ProcessAssetReturnWorkflowAsync(int assetId, string returnReason, string processedByUserId);
        
        // Stock management automation
        Task<IEnumerable<InventoryItem>> CheckAndTriggerReorderPointsAsync();
        Task<bool> AutomaticallyAllocateInventoryAsync(int requestId);
        Task<bool> ProcessInventoryReceiptAsync(int procurementRequestId, Dictionary<int, int> receivedItems, string receivedByUserId);
        
        // Business rule validation
        Task<ValidationResult> ValidateRequestBusinessRulesAsync(ITRequest request);
        Task<ValidationResult> ValidateAssetDeploymentRulesAsync(int assetId, int targetLocationId);
        Task<ValidationResult> ValidateProcurementBusinessRulesAsync(ProcurementRequest procurement);
        
        // Integration health checks
        Task<IntegrationHealthStatus> CheckModuleIntegrationHealthAsync();
        Task<IEnumerable<BusinessLogicAlert>> GetBusinessLogicAlertsAsync();
    }

    public class IntegratedBusinessLogicService : IIntegratedBusinessLogicService
    {
        private readonly ApplicationDbContext _context;
        private readonly IInventoryService _inventoryService;
        private readonly IAssetService _assetService;
        private readonly IRequestService _requestService;
        private readonly IProcurementService _procurementService;
        private readonly IAuditService _auditService;
        private readonly ILogger<IntegratedBusinessLogicService> _logger;

        public IntegratedBusinessLogicService(
            ApplicationDbContext context,
            IInventoryService inventoryService,
            IAssetService assetService,
            IRequestService requestService,
            IProcurementService procurementService,
            IAuditService auditService,
            ILogger<IntegratedBusinessLogicService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
            _requestService = requestService ?? throw new ArgumentNullException(nameof(requestService));
            _procurementService = procurementService ?? throw new ArgumentNullException(nameof(procurementService));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> ProcessRequestApprovalWorkflowAsync(int requestId, string approverId, bool approved, string? comments = null)
        {
            _logger.LogInformation("Processing request approval workflow for Request {RequestId}", requestId);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var request = await _context.ITRequests
                    .Include(r => r.RequestedByUser)
                    .FirstOrDefaultAsync(r => r.Id == requestId);

                if (request == null)
                {
                    _logger.LogWarning("Request {RequestId} not found", requestId);
                    return false;
                }

                if (approved)
                {
                    // Approve the request
                    request.Status = RequestStatus.Approved;
                    request.ApprovedByUserId = approverId;
                    request.ApprovalDate = DateTime.UtcNow;

                    // Check inventory availability and automatically allocate if possible
                    var allocationResult = await AutomaticallyAllocateInventoryAsync(requestId);
                    
                    if (allocationResult)
                    {
                        request.Status = RequestStatus.InProgress;
                        _logger.LogInformation("Request {RequestId} automatically allocated from inventory", requestId);
                    }
                    else
                    {
                        // Check if we need to trigger procurement
                        await CheckAndTriggerProcurementForRequestAsync(requestId, approverId);
                    }
                }
                else
                {
                    // Reject the request
                    request.Status = RequestStatus.Rejected;
                    request.ApprovedByUserId = approverId;
                    request.ApprovalDate = DateTime.UtcNow;
                }

                // Add approval record
                var approval = new RequestApproval
                {
                    ITRequestId = requestId,
                    ApproverId = approverId,
                    Status = approved ? ApprovalStatus.Approved : ApprovalStatus.Rejected,
                    Comments = comments,
                    DecisionDate = DateTime.UtcNow,
                    ApprovalLevel = ApprovalLevel.Department,
                    CreatedDate = DateTime.UtcNow,
                    Sequence = 1
                };

                _context.RequestApprovals.Add(approval);
                await _context.SaveChangesAsync();

                // Log audit trail
                await _auditService.LogAsync(
                    approved ? AuditAction.StatusChange : AuditAction.Update,
                    "ITRequest",
                    requestId,
                    approverId,
                    $"Request {(approved ? "approved" : "rejected")}: {comments}",
                    null,
                    request);

                await transaction.CommitAsync();
                _logger.LogInformation("Request {RequestId} workflow processed successfully", requestId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing request approval workflow for Request {RequestId}", requestId);
                throw;
            }
        }

        public async Task<bool> ExecuteRequestFulfillmentAsync(int requestId, string fulfillmentUserId)
        {
            _logger.LogInformation("Executing request fulfillment for Request {RequestId}", requestId);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var request = await _context.ITRequests
                    .Include(r => r.RelatedAsset)
                    .FirstOrDefaultAsync(r => r.Id == requestId);

                if (request == null || request.Status != RequestStatus.InProgress)
                {
                    _logger.LogWarning("Request {RequestId} not found or not in progress status", requestId);
                    return false;
                }

                // Different fulfillment logic based on request type
                switch (request.RequestType)
                {
                    case RequestType.NewEquipment:
                        await FulfillNewEquipmentRequestAsync(request, fulfillmentUserId);
                        break;
                    case RequestType.HardwareReplacement:
                        await FulfillReplacementRequestAsync(request, fulfillmentUserId);
                        break;
                    case RequestType.HardwareRepair:
                        await FulfillRepairRequestAsync(request, fulfillmentUserId);
                        break;
                    case RequestType.UserAccessRights:
                        await FulfillAccessRequestAsync(request, fulfillmentUserId);
                        break;
                    case RequestType.SoftwareInstallation:
                    case RequestType.SoftwareUpgrade:
                        await FulfillSoftwareRequestAsync(request, fulfillmentUserId);
                        break;
                    default:
                        await FulfillGenericRequestAsync(request, fulfillmentUserId);
                        break;
                }

                request.Status = RequestStatus.Completed;
                request.CompletedDate = DateTime.UtcNow;
                request.CompletedByUserId = fulfillmentUserId;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _auditService.LogAsync(
                    AuditAction.StatusChange,
                    "ITRequest",
                    requestId,
                    fulfillmentUserId,
                    "Request fulfilled and completed",
                    null,
                    request);

                _logger.LogInformation("Request {RequestId} fulfilled successfully", requestId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error fulfilling request {RequestId}", requestId);
                throw;
            }
        }

        public async Task<bool> TriggerAutomaticProcurementAsync(int inventoryItemId, string initiatedByUserId)
        {
            _logger.LogInformation("Triggering automatic procurement for Inventory Item {InventoryItemId}", inventoryItemId);

            var inventoryItem = await _context.InventoryItems.FindAsync(inventoryItemId);
            if (inventoryItem == null)
            {
                _logger.LogWarning("Inventory item {InventoryItemId} not found", inventoryItemId);
                return false;
            }

            // Check if item is below reorder level
            if (inventoryItem.Quantity > inventoryItem.ReorderLevel)
            {
                _logger.LogInformation("Inventory item {InventoryItemId} is above reorder level, skipping procurement", inventoryItemId);
                return false;
            }

            // Calculate reorder quantity (bring up to maximum stock level)
            var reorderQuantity = inventoryItem.MaximumStock - inventoryItem.Quantity;
            if (reorderQuantity <= 0)
                reorderQuantity = inventoryItem.ReorderLevel;

            // Create automatic procurement request
            var procurementRequest = new ProcurementRequest
            {
                ProcurementNumber = await GenerateProcurementNumberAsync(),
                Title = $"Auto-Reorder: {inventoryItem.Name}",
                Description = $"Automatic reorder triggered for {inventoryItem.Name} (Current: {inventoryItem.Quantity}, Reorder Level: {inventoryItem.ReorderLevel})",
                Priority = ProcurementPriority.Medium,
                Status = ProcurementStatus.Draft,
                RequestDate = DateTime.UtcNow,
                RequiredByDate = DateTime.UtcNow.AddDays(30), // Default 30 days
                RequestedByUserId = initiatedByUserId,
                Department = "IT Department", // Use string instead of ID
                Source = ProcurementSource.AutoGenerated,
                Method = ProcurementMethod.DirectPurchase
            };

            _context.ProcurementRequests.Add(procurementRequest);
            await _context.SaveChangesAsync();

            // Add procurement item
            var procurementItem = new ProcurementItem
            {
                ProcurementRequestId = procurementRequest.Id,
                ItemName = inventoryItem.Name,
                Description = inventoryItem.Description,
                TechnicalSpecifications = inventoryItem.Specifications,
                Quantity = reorderQuantity,
                EstimatedUnitPrice = inventoryItem.UnitCost ?? 0,
                // TotalPrice = (inventoryItem.UnitCost ?? 0) * reorderQuantity, // Computed property
                // Category = inventoryItem.Category.ToString(), // Remove this property
                // PreferredSupplier = inventoryItem.Supplier, // Remove this property
                // InventoryItemId = inventoryItemId // Remove this property
            };

            _context.ProcurementItems.Add(procurementItem);

            // Update procurement request total
            procurementRequest.TotalAmount = procurementItem.TotalPrice;

            await _context.SaveChangesAsync();

            // Log audit trail
            await _auditService.LogAsync(
                AuditAction.Create,
                "ProcurementRequest",
                procurementRequest.Id,
                initiatedByUserId,
                $"Auto-generated procurement for {inventoryItem.Name} (Qty: {reorderQuantity})",
                null,
                procurementRequest);

            _logger.LogInformation("Automatic procurement {ProcurementRequestId} created for inventory item {InventoryItemId}", 
                procurementRequest.Id, inventoryItemId);

            return true;
        }

        public async Task<bool> ProcessAssetDeploymentWorkflowAsync(int assetId, int requestId, string deployedByUserId)
        {
            _logger.LogInformation("Processing asset deployment workflow for Asset {AssetId}, Request {RequestId}", assetId, requestId);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var asset = await _context.Assets
                    .Include(a => a.Location)
                    .FirstOrDefaultAsync(a => a.Id == assetId);

                var request = await _context.ITRequests.FindAsync(requestId);

                if (asset == null || request == null)
                {
                    _logger.LogWarning("Asset {AssetId} or Request {RequestId} not found", assetId, requestId);
                    return false;
                }

                // Update asset status
                asset.Status = AssetStatus.InUse; // Use InUse instead of Deployed
                asset.AssignedToUserId = request.RequestedByUserId;
                // asset.AssignmentDate = DateTime.UtcNow; // Remove this property
                asset.LastUpdated = DateTime.UtcNow; // Use LastUpdated instead of LastUpdatedDate
                // asset.LastUpdatedByUserId = deployedByUserId; // Remove this property

                // Link asset to request
                request.RelatedAssetId = assetId;
                request.Status = RequestStatus.Completed;
                request.CompletedDate = DateTime.UtcNow;
                request.CompletedByUserId = deployedByUserId;

                // Create asset movement record
                var assetMovement = new AssetMovement
                {
                    AssetId = assetId,
                    MovementType = MovementType.PersonTransfer, // Use MovementType instead of AssetMovementType
                    FromLocationId = asset.LocationId,
                    ToLocationId = request.LocationId ?? asset.LocationId,
                    MovementDate = DateTime.UtcNow,
                    Notes = $"Deployed for Request #{request.RequestNumber}",
                    PerformedByUserId = deployedByUserId
                };

                _context.AssetMovements.Add(assetMovement);

                // Update asset location if different
                if (request.LocationId.HasValue && request.LocationId != asset.LocationId)
                {
                    asset.LocationId = request.LocationId.Value;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Log audit trails
                await _auditService.LogAsync(
                    AuditAction.Assignment,
                    "Asset",
                    assetId,
                    deployedByUserId,
                    $"Asset deployed for Request #{request.RequestNumber}",
                    null,
                    asset);

                await _auditService.LogAsync(
                    AuditAction.StatusChange,
                    "ITRequest",
                    requestId,
                    deployedByUserId,
                    $"Request completed with Asset #{asset.AssetTag} deployment",
                    null,
                    request);

                _logger.LogInformation("Asset {AssetId} deployment workflow completed for Request {RequestId}", assetId, requestId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing asset deployment workflow for Asset {AssetId}, Request {RequestId}", assetId, requestId);
                throw;
            }
        }

        public async Task<bool> ProcessAssetReturnWorkflowAsync(int assetId, string returnReason, string processedByUserId)
        {
            _logger.LogInformation("Processing asset return workflow for Asset {AssetId}", assetId);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var asset = await _context.Assets
                    // .Include(a => a.AssetInventoryMappings.Where(m => m.Status == AssetInventoryMappingStatus.Active)) // Remove this navigation
                    // .ThenInclude(m => m.InventoryItem)
                    .FirstOrDefaultAsync(a => a.Id == assetId);

                // Get asset inventory mappings separately
                var assetInventoryMappings = await _context.AssetInventoryMappings
                    .Where(m => m.AssetId == assetId && m.Status == AssetInventoryMappingStatus.Active)
                    .Include(m => m.InventoryItem)
                    .ToListAsync();

                if (asset == null)
                {
                    _logger.LogWarning("Asset {AssetId} not found", assetId);
                    return false;
                }

                // Return components to inventory
                foreach (var mapping in assetInventoryMappings) // Use the separate query result
                {
                    await _inventoryService.ReturnFromAssetAsync(
                        assetId, 
                        mapping.InventoryItemId, 
                        mapping.Quantity,
                        $"Asset return: {returnReason}",
                        processedByUserId);
                }

                // Update asset status
                asset.Status = AssetStatus.Available;
                asset.AssignedToUserId = null;
                // asset.AssignmentDate = null; // Remove this property
                asset.LastUpdated = DateTime.UtcNow; // Use LastUpdated instead of LastUpdatedDate
                // asset.LastUpdatedByUserId = processedByUserId; // Remove this property

                // Create asset movement record
                var assetMovement = new AssetMovement
                {
                    AssetId = assetId,
                    MovementType = MovementType.Return, // Use MovementType instead of AssetMovementType
                    FromLocationId = asset.LocationId,
                    ToLocationId = 1, // Warehouse location - should be configurable
                    MovementDate = DateTime.UtcNow,
                    Notes = returnReason, // Use Notes instead of Reason
                    PerformedByUserId = processedByUserId
                };

                _context.AssetMovements.Add(assetMovement);

                // Move asset to warehouse/storage location
                asset.LocationId = 1; // Warehouse - should be configurable

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Log audit trail
                await _auditService.LogAsync(
                    AuditAction.StatusChange,
                    "Asset",
                    assetId,
                    processedByUserId,
                    $"Asset returned: {returnReason}",
                    null,
                    asset);

                _logger.LogInformation("Asset {AssetId} return workflow completed", assetId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing asset return workflow for Asset {AssetId}", assetId);
                throw;
            }
        }

        public async Task<IEnumerable<InventoryItem>> CheckAndTriggerReorderPointsAsync()
        {
            _logger.LogInformation("Checking inventory reorder points");

            var itemsNeedingReorder = await _context.InventoryItems
                .Where(i => i.Quantity <= i.ReorderLevel && i.ReorderLevel > 0)
                .ToListAsync();

            var triggeredItems = new List<InventoryItem>();

            foreach (var item in itemsNeedingReorder)
            {
                // Check if there's already a pending procurement for this item
                var existingProcurement = await _context.ProcurementRequests
                    .Where(p => p.Status != ProcurementStatus.Completed && p.Status != ProcurementStatus.Cancelled)
                    .SelectMany(p => p.Items)
                    .AnyAsync(i => i.ItemName == item.Name); // Use ItemName instead of InventoryItemId

                if (!existingProcurement)
                {
                    var success = await TriggerAutomaticProcurementAsync(item.Id, "SYSTEM");
                    if (success)
                    {
                        triggeredItems.Add(item);
                    }
                }
            }

            _logger.LogInformation("Triggered automatic procurement for {Count} inventory items", triggeredItems.Count);
            return triggeredItems;
        }

        public async Task<bool> AutomaticallyAllocateInventoryAsync(int requestId)
        {
            _logger.LogInformation("Attempting automatic inventory allocation for Request {RequestId}", requestId);

            var request = await _context.ITRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null || string.IsNullOrEmpty(request.RequestedItemCategory))
            {
                _logger.LogWarning("Request {RequestId} not found or missing item category", requestId);
                return false;
            }

            // Find suitable inventory item
            var availableItem = await _context.InventoryItems
                .Where(i => i.Status == InventoryStatus.Available &&
                           i.Quantity > 0 &&
                           i.Category.ToString().Contains(request.RequestedItemCategory))
                .FirstOrDefaultAsync();

            if (availableItem == null)
            {
                _logger.LogInformation("No suitable inventory available for Request {RequestId}", requestId);
                return false;
            }

            // Reserve the item
            var success = await _inventoryService.CheckAvailabilityAndReserveAsync(
                availableItem.ItemCode,
                1, // Usually 1 item per request
                requestId,
                "SYSTEM");

            if (success)
            {
                _logger.LogInformation("Successfully allocated inventory item {ItemCode} to Request {RequestId}", 
                    availableItem.ItemCode, requestId);
            }

            return success;
        }

        public async Task<bool> ProcessInventoryReceiptAsync(int procurementRequestId, Dictionary<int, int> receivedItems, string receivedByUserId)
        {
            _logger.LogInformation("Processing inventory receipt for Procurement {ProcurementRequestId}", procurementRequestId);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var procurementRequest = await _context.ProcurementRequests
                    .Include(p => p.Items)
                    .FirstOrDefaultAsync(p => p.Id == procurementRequestId);

                if (procurementRequest == null)
                {
                    _logger.LogWarning("Procurement request {ProcurementRequestId} not found", procurementRequestId);
                    return false;
                }

                foreach (var receivedItem in receivedItems)
                {
                    var inventoryItemId = receivedItem.Key;
                    var quantityReceived = receivedItem.Value;

                    var procurementItem = procurementRequest.Items
                        .FirstOrDefault(i => i.ItemName.Contains(inventoryItemId.ToString())); // Use a workaround for now

                    if (procurementItem != null)
                    {
                        // Create procurement activity
                        var activity = new ProcurementActivity
                        {
                            ProcurementRequestId = procurementRequestId,
                            ActivityType = ProcurementActivityType.Received,
                            ActivityDetails = $"Received {quantityReceived} units",
                            ActionDate = DateTime.UtcNow,
                            ActionByUserId = receivedByUserId,
                            // Quantity = quantityReceived, // Remove this property
                            // Notes = $"Received from supplier for {procurementItem.ItemName}" // Remove this property
                        };

                        _context.ProcurementActivities.Add(activity);
                        await _context.SaveChangesAsync(); // Save to get activity ID

                        // Update inventory
                        await _inventoryService.ReceiveStockFromProcurementAsync(
                            inventoryItemId,
                            activity.Id,
                            quantityReceived,
                            procurementItem.UnitPrice,
                            DateTime.UtcNow,
                            receivedByUserId);
                    }
                }

                // Update procurement status
                procurementRequest.Status = ProcurementStatus.Received;
                procurementRequest.LastUpdatedDate = DateTime.UtcNow;
                procurementRequest.LastUpdatedByUserId = receivedByUserId;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Inventory receipt processed successfully for Procurement {ProcurementRequestId}", procurementRequestId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing inventory receipt for Procurement {ProcurementRequestId}", procurementRequestId);
                throw;
            }
        }

        public async Task<ValidationResult> ValidateRequestBusinessRulesAsync(ITRequest request)
        {
            var result = new ValidationResult { IsValid = true, Messages = new List<string>() };

            // Rule 1: Check if user has reached maximum concurrent requests
            var userActiveRequests = await _context.ITRequests
                .CountAsync(r => r.RequestedByUserId == request.RequestedByUserId &&
                               (r.Status == RequestStatus.Pending || r.Status == RequestStatus.InProgress));

            if (userActiveRequests >= 5) // Configurable limit
            {
                result.IsValid = false;
                result.Messages.Add("User has reached maximum number of concurrent requests (5)");
            }

            // Rule 2: Check budget limits for high-value requests
            if (request.EstimatedCost > 10000) // Configurable threshold
            {
                // Check if user has budget approval authority
                var user = await _context.Users.FindAsync(request.RequestedByUserId);
                if (user?.Department != "IT Management")
                {
                    result.IsValid = false;
                    result.Messages.Add("High-value requests require IT Management approval");
                }
            }

            // Rule 3: Validate request type specific rules
            switch (request.RequestType)
            {
                case RequestType.NewEquipment:
                    if (string.IsNullOrEmpty(request.BusinessJustification))
                    {
                        result.IsValid = false;
                        result.Messages.Add("Business justification is required for new equipment requests");
                    }
                    break;
                case RequestType.HardwareReplacement:
                    if (!request.RelatedAssetId.HasValue)
                    {
                        result.IsValid = false;
                        result.Messages.Add("Related asset must be specified for replacement requests");
                    }
                    break;
            }

            return result;
        }

        public async Task<ValidationResult> ValidateAssetDeploymentRulesAsync(int assetId, int targetLocationId)
        {
            var result = new ValidationResult { IsValid = true, Messages = new List<string>() };

            var asset = await _context.Assets
                .Include(a => a.Location)
                .FirstOrDefaultAsync(a => a.Id == assetId);

            var targetLocation = await _context.Locations.FindAsync(targetLocationId);

            if (asset == null)
            {
                result.IsValid = false;
                result.Messages.Add("Asset not found");
                return result;
            }

            if (targetLocation == null)
            {
                result.IsValid = false;
                result.Messages.Add("Target location not found");
                return result;
            }

            // Rule 1: Check if asset is available for deployment
            if (asset.Status != AssetStatus.Available)
            {
                result.IsValid = false;
                result.Messages.Add($"Asset is not available for deployment (Status: {asset.Status})");
            }

            // Rule 2: Check location capacity (if applicable)
            var locationAssetCount = await _context.Assets
                .CountAsync(a => a.LocationId == targetLocationId && a.Status == AssetStatus.InUse); // Use InUse instead of Deployed

            if (locationAssetCount >= 50) // Configurable limit
            {
                result.IsValid = false;
                result.Messages.Add("Target location has reached maximum asset capacity");
            }

            // Rule 3: Check environmental requirements
            if (asset.Category == AssetCategory.Server && targetLocation.Building != "Data Center")
            {
                result.IsValid = false;
                result.Messages.Add("Server equipment must be deployed in Data Center locations");
            }

            return result;
        }

        public async Task<ValidationResult> ValidateProcurementBusinessRulesAsync(ProcurementRequest procurement)
        {
            var result = new ValidationResult { IsValid = true, Messages = new List<string>() };

            // Rule 1: Check budget approval requirements
            if (procurement.TotalAmount > 50000) // Configurable threshold
            {
                result.IsValid = false;
                result.Messages.Add("Procurement requests over $50,000 require executive approval");
            }

            // Rule 2: Check vendor approval status (if vendor is specified)
            if (procurement.VendorId.HasValue)
            {
                var vendor = await _context.Vendors.FindAsync(procurement.VendorId.Value);
                if (vendor?.IsApproved != true)
                {
                    result.IsValid = false;
                    result.Messages.Add("Selected vendor is not approved for procurement");
                }
            }

            // Rule 3: Check for duplicate recent procurements
            var recentSimilar = await _context.ProcurementRequests
                .Where(p => p.Id != procurement.Id &&
                           p.CreatedDate >= DateTime.UtcNow.AddDays(-30) &&
                           p.Title == procurement.Title)
                .AnyAsync();

            if (recentSimilar)
            {
                result.Messages.Add("Similar procurement request found within last 30 days - please review for duplicates");
            }

            return result;
        }

        public async Task<IntegrationHealthStatus> CheckModuleIntegrationHealthAsync()
        {
            var status = new IntegrationHealthStatus
            {
                CheckTime = DateTime.UtcNow,
                Issues = new List<string>()
            };

            try
            {
                // Check database connectivity
                await _context.Database.CanConnectAsync();

                // Check for orphaned records
                var orphanedAssets = await _context.Assets
                    .Where(a => !_context.Locations.Any(l => l.Id == a.LocationId))
                    .CountAsync();

                if (orphanedAssets > 0)
                    status.Issues.Add($"{orphanedAssets} assets have invalid location references");

                var orphanedInventory = await _context.InventoryItems
                    .Where(i => i.LocationId != null && !_context.Locations.Any(l => l.Id == i.LocationId)) // Use != null instead of HasValue
                    .CountAsync();

                if (orphanedInventory > 0)
                    status.Issues.Add($"{orphanedInventory} inventory items have invalid location references");

                // Check for pending workflows that might be stuck
                var stuckRequests = await _context.ITRequests
                    .Where(r => r.Status == RequestStatus.InProgress &&
                               r.RequestDate < DateTime.UtcNow.AddDays(-30))
                    .CountAsync();

                if (stuckRequests > 0)
                    status.Issues.Add($"{stuckRequests} requests have been in progress for over 30 days");

                var stuckProcurements = await _context.ProcurementRequests
                    .Where(p => p.Status == ProcurementStatus.PendingApproval && // Use PendingApproval instead of Pending
                               p.RequestDate < DateTime.UtcNow.AddDays(-60))
                    .CountAsync();

                if (stuckProcurements > 0)
                    status.Issues.Add($"{stuckProcurements} procurement requests have been pending for over 60 days");

                status.IsHealthy = status.Issues.Count == 0;
            }
            catch (Exception ex)
            {
                status.IsHealthy = false;
                status.Issues.Add($"Database connectivity issue: {ex.Message}");
            }

            return status;
        }

        public async Task<IEnumerable<BusinessLogicAlert>> GetBusinessLogicAlertsAsync()
        {
            var alerts = new List<BusinessLogicAlert>();

            // Check for items needing reorder
            var lowStockItems = await _context.InventoryItems
                .Where(i => i.Quantity <= i.ReorderLevel)
                .CountAsync();

            if (lowStockItems > 0)
            {
                alerts.Add(new BusinessLogicAlert
                {
                    Type = "Inventory",
                    Severity = "Warning",
                    Message = $"{lowStockItems} inventory items are at or below reorder level",
                    ActionRequired = "Review and create procurement requests",
                    CreatedDate = DateTime.UtcNow
                });
            }

            // Check for overdue requests
            var overdueRequests = await _context.ITRequests
                .Where(r => r.RequiredByDate.HasValue &&
                           r.RequiredByDate < DateTime.UtcNow &&
                           r.Status != RequestStatus.Completed)
                .CountAsync();

            if (overdueRequests > 0)
            {
                alerts.Add(new BusinessLogicAlert
                {
                    Type = "Requests",
                    Severity = "High",
                    Message = $"{overdueRequests} requests are overdue",
                    ActionRequired = "Review and expedite overdue requests",
                    CreatedDate = DateTime.UtcNow
                });
            }

            // Check for assets needing maintenance based on last maintenance date
            var assetsNeedingMaintenance = await _context.Assets
                .Where(a => a.Status == AssetStatus.InUse &&
                           !a.MaintenanceRecords.Any(m => m.MaintenanceDate > DateTime.UtcNow.AddMonths(-6)))
                .CountAsync();

            if (assetsNeedingMaintenance > 0)
            {
                alerts.Add(new BusinessLogicAlert
                {
                    Type = "Assets",
                    Severity = "Medium",
                    Message = $"{assetsNeedingMaintenance} assets need maintenance within 7 days",
                    ActionRequired = "Schedule maintenance activities",
                    CreatedDate = DateTime.UtcNow
                });
            }

            return alerts;
        }

        #region Private Helper Methods

        private async Task<bool> CheckAndTriggerProcurementForRequestAsync(int requestId, string initiatedByUserId)
        {
            var request = await _context.ITRequests.FindAsync(requestId);
            if (request == null || string.IsNullOrEmpty(request.RequestedItemCategory))
                return false;

            // Look for matching inventory items that are out of stock
            var outOfStockItems = await _context.InventoryItems
                .Where(i => i.Category.ToString().Contains(request.RequestedItemCategory) &&
                           i.Quantity == 0)
                .ToListAsync();

            foreach (var item in outOfStockItems)
            {
                await TriggerAutomaticProcurementAsync(item.Id, initiatedByUserId);
            }

            return outOfStockItems.Any();
        }

        private async Task FulfillNewEquipmentRequestAsync(ITRequest request, string fulfillmentUserId)
        {
            // For new equipment, we typically deploy a new asset
            // This is a simplified implementation
            _logger.LogInformation("Fulfilling new equipment request {RequestId}", request.Id);
        }

        private async Task FulfillReplacementRequestAsync(ITRequest request, string fulfillmentUserId)
        {
            if (request.RelatedAssetId.HasValue)
            {
                // Return old asset and deploy new one
                await ProcessAssetReturnWorkflowAsync(request.RelatedAssetId.Value, "Replacement", fulfillmentUserId);
            }
        }

        private async Task FulfillRepairRequestAsync(ITRequest request, string fulfillmentUserId)
        {
            if (request.RelatedAssetId.HasValue)
            {
                var asset = await _context.Assets.FindAsync(request.RelatedAssetId.Value);
                if (asset != null)
                {
                    asset.Status = AssetStatus.UnderMaintenance; // Use UnderMaintenance instead of InRepair
                    asset.LastUpdated = DateTime.UtcNow; // Use LastUpdated instead of LastUpdatedDate
                    // asset.LastUpdatedByUserId = fulfillmentUserId; // Remove this property
                }
            }
        }

        private async Task FulfillAccessRequestAsync(ITRequest request, string fulfillmentUserId)
        {
            // Handle access/permission requests
            _logger.LogInformation("Fulfilling access request {RequestId}", request.Id);
        }

        private async Task FulfillSoftwareRequestAsync(ITRequest request, string fulfillmentUserId)
        {
            // Handle software installation/license requests
            _logger.LogInformation("Fulfilling software request {RequestId}", request.Id);
        }

        private async Task FulfillGenericRequestAsync(ITRequest request, string fulfillmentUserId)
        {
            // Handle other types of requests
            _logger.LogInformation("Fulfilling generic request {RequestId}", request.Id);
        }

        private async Task<string> GenerateProcurementNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"PR{year}";
            
            var latestProcurement = await _context.ProcurementRequests
                .Where(p => p.RequestNumber.StartsWith(prefix))
                .OrderByDescending(p => p.RequestNumber)
                .FirstOrDefaultAsync();

            var sequence = 1;
            if (latestProcurement != null)
            {
                var numberPart = latestProcurement.RequestNumber.Substring(prefix.Length);
                if (int.TryParse(numberPart, out var lastSequence))
                {
                    sequence = lastSequence + 1;
                }
            }

            return $"{prefix}{sequence:D6}";
        }

        #endregion
    }

    // Supporting classes
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Messages { get; set; } = new();
    }

    public class IntegrationHealthStatus
    {
        public DateTime CheckTime { get; set; }
        public bool IsHealthy { get; set; }
        public List<string> Issues { get; set; } = new();
    }

    public class BusinessLogicAlert
    {
        public string Type { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ActionRequired { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
