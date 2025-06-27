using Microsoft.EntityFrameworkCore;
using HospitalAssetTracker.Data;
using HospitalAssetTracker.Models;
using static HospitalAssetTracker.Models.InventorySearchModels; // Reverted to using static
using ClosedXML.Excel;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace HospitalAssetTracker.Services
{
    /// <summary>
    /// Professional Inventory Service - Hospital IT Asset Tracking System
    /// Provides comprehensive inventory management with integrated business logic
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private const string EntityName = "InventoryItem";
        private const string UnknownValue = "Unknown";
        private const string SystemUser = "System";
        
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(
            ApplicationDbContext context, 
            IAuditService auditService,
            ILogger<InventoryService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Basic CRUD Operations

        public async Task<InventoryItem?> GetInventoryItemByIdAsync(int id)
        {
            return await _context.InventoryItems
                .Include(i => i.Location)
                .Include(i => i.CreatedByUser)
                .Include(i => i.LastUpdatedByUser)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<InventoryItem?> GetInventoryItemByItemCodeAsync(string itemCode)
        {
            return await _context.InventoryItems
                .Include(i => i.Location)
                .Include(i => i.CreatedByUser)
                .Include(i => i.LastUpdatedByUser)
                .FirstOrDefaultAsync(i => i.ItemCode == itemCode);
        }

        public async Task<IEnumerable<InventoryItem>> GetAllInventoryItemsAsync()
        {
            return await _context.InventoryItems
                .Include(i => i.Location)
                .Include(i => i.CreatedByUser)
                .OrderBy(i => i.Name)
                .ToListAsync();
        }

        public async Task<PagedResult<InventoryItem>> GetInventoryItemsPagedAsync(InventorySearchCriteria criteria)
        {
            var query = _context.InventoryItems
                .Include(i => i.Location)
                .Include(i => i.CreatedByUser)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
            {
                var searchTerm = criteria.SearchTerm.ToLower();
                query = query.Where(i => 
                    i.Name.ToLower().Contains(searchTerm) ||
                    i.ItemCode.ToLower().Contains(searchTerm) ||
                    i.Brand.ToLower().Contains(searchTerm) ||
                    i.Model.ToLower().Contains(searchTerm) ||
                    (i.SerialNumber != null && i.SerialNumber.ToLower().Contains(searchTerm)) ||
                    (i.Description != null && i.Description.ToLower().Contains(searchTerm)));
            }

            if (criteria.Category.HasValue)
                query = query.Where(i => i.Category == criteria.Category.Value);

            if (criteria.ItemType.HasValue)
                query = query.Where(i => i.ItemType == criteria.ItemType.Value);

            if (criteria.Status.HasValue)
                query = query.Where(i => i.Status == criteria.Status.Value);

            if (criteria.Condition.HasValue)
                query = query.Where(i => i.Condition == criteria.Condition.Value);

            if (criteria.LocationId.HasValue)
                query = query.Where(i => i.LocationId == criteria.LocationId.Value);

            if (!string.IsNullOrWhiteSpace(criteria.Brand))
                query = query.Where(i => i.Brand.ToLower().Contains(criteria.Brand.ToLower()));

            if (!string.IsNullOrWhiteSpace(criteria.Model))
                query = query.Where(i => i.Model.ToLower().Contains(criteria.Model.ToLower()));

            if (criteria.IsLowStock.HasValue && criteria.IsLowStock.Value)
                query = query.Where(i => i.Quantity <= i.ReorderLevel);

            if (criteria.IsCriticalStock.HasValue && criteria.IsCriticalStock.Value)
                query = query.Where(i => i.Quantity <= i.MinimumStock);

            if (criteria.IsConsumable.HasValue)
                query = query.Where(i => i.IsConsumable == criteria.IsConsumable.Value);

            if (criteria.RequiresCalibration.HasValue)
                query = query.Where(i => i.RequiresCalibration == criteria.RequiresCalibration.Value);

            if (criteria.CreatedFrom.HasValue)
                query = query.Where(i => i.CreatedDate >= criteria.CreatedFrom.Value);

            if (criteria.CreatedTo.HasValue)
                query = query.Where(i => i.CreatedDate <= criteria.CreatedTo.Value);

            if (criteria.PurchaseDateFrom.HasValue)
                query = query.Where(i => i.PurchaseDate >= criteria.PurchaseDateFrom.Value);

            if (criteria.PurchaseDateTo.HasValue)
                query = query.Where(i => i.PurchaseDate <= criteria.PurchaseDateTo.Value);

            if (criteria.WarrantyExpiryFrom.HasValue)
                query = query.Where(i => i.WarrantyExpiry >= criteria.WarrantyExpiryFrom.Value);

            if (criteria.WarrantyExpiryTo.HasValue)
                query = query.Where(i => i.WarrantyExpiry <= criteria.WarrantyExpiryTo.Value);

            if (criteria.MinUnitCost.HasValue)
                query = query.Where(i => i.UnitCost >= criteria.MinUnitCost.Value);

            if (criteria.MaxUnitCost.HasValue)
                query = query.Where(i => i.UnitCost <= criteria.MaxUnitCost.Value);

            if (criteria.MinQuantity.HasValue)
                query = query.Where(i => i.Quantity >= criteria.MinQuantity.Value);

            if (criteria.MaxQuantity.HasValue)
                query = query.Where(i => i.Quantity <= criteria.MaxQuantity.Value);

            if (!string.IsNullOrWhiteSpace(criteria.Supplier))
                query = query.Where(i => i.Supplier != null && i.Supplier.ToLower().Contains(criteria.Supplier.ToLower()));

            if (!string.IsNullOrWhiteSpace(criteria.StorageZone))
                query = query.Where(i => i.StorageZone != null && i.StorageZone.ToLower().Contains(criteria.StorageZone.ToLower()));

            // Apply sorting
            query = criteria.SortBy.ToLower() switch
            {
                "name" => criteria.SortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(i => i.Name) 
                    : query.OrderBy(i => i.Name),
                "itemcode" => criteria.SortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(i => i.ItemCode) 
                    : query.OrderBy(i => i.ItemCode),
                "category" => criteria.SortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(i => i.Category) 
                    : query.OrderBy(i => i.Category),
                "quantity" => criteria.SortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(i => i.Quantity) 
                    : query.OrderBy(i => i.Quantity),
                "unitcost" => criteria.SortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(i => i.UnitCost) 
                    : query.OrderBy(i => i.UnitCost),
                "createddate" => criteria.SortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(i => i.CreatedDate) 
                    : query.OrderBy(i => i.CreatedDate),
                _ => query.OrderBy(i => i.Name)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((criteria.PageNumber - 1) * criteria.PageSize)
                .Take(criteria.PageSize)
                .ToListAsync();

            return new PagedResult<InventoryItem>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = criteria.PageNumber,
                PageSize = criteria.PageSize
            };
        }

        public async Task<InventoryItem> CreateInventoryItemAsync(InventoryItem item, string userId)
        {
            // Set audit fields
            item.CreatedDate = DateTime.UtcNow;
            item.CreatedByUserId = userId;
            
            // Generate item code if not provided
            if (string.IsNullOrEmpty(item.ItemCode))
            {
                item.ItemCode = await GenerateItemCodeAsync(item.Category);
            }

            // Calculate total value
            if (item.UnitCost.HasValue && item.Quantity > 0)
            {
                item.TotalValue = item.UnitCost.Value * item.Quantity;
            }

            _context.InventoryItems.Add(item);
            await _context.SaveChangesAsync();

            // Create initial stock movement
            var stockInMovement = new InventoryMovement
            {
                InventoryItemId = item.Id,
                MovementType = InventoryMovementType.StockIn,
                Quantity = item.Quantity,
                ToLocationId = item.LocationId,
                ToZone = item.StorageZone,
                ToShelf = item.StorageShelf,
                ToBin = item.StorageBin,
                MovementDate = DateTime.UtcNow,
                Reason = "Initial stock entry",
                PerformedByUserId = userId,
                CreatedDate = DateTime.UtcNow
            };

            _context.InventoryMovements.Add(stockInMovement);

            // Create initial transaction if purchase details provided
            if (item.UnitCost.HasValue && !string.IsNullOrEmpty(item.Supplier))
            {
                var transaction = new InventoryTransaction
                {
                    InventoryItemId = item.Id,
                    TransactionType = InventoryTransactionType.Purchase,
                    Quantity = item.Quantity,
                    UnitCost = item.UnitCost.Value,
                    TotalCost = item.TotalValue,
                    Supplier = item.Supplier,
                    PurchaseDate = item.PurchaseDate,
                    TransactionDate = DateTime.UtcNow,
                    Description = "Initial purchase entry",
                    CreatedByUserId = userId,
                    CreatedDate = DateTime.UtcNow
                };

                _context.InventoryTransactions.Add(transaction);
            }

            await _context.SaveChangesAsync();

            // Create audit log
            await _auditService.LogAsync(
                AuditAction.Create,
                "InventoryItem",
                item.Id,
                userId,
                $"Created inventory item {item.ItemCode} - {item.Name}",
                null,
                item
            );

            return item;
        }

        public async Task<InventoryItem> UpdateInventoryItemAsync(InventoryItem item, string userId)
        {
            var existingItem = await _context.InventoryItems.FindAsync(item.Id);
            if (existingItem == null)
            {
                throw new ArgumentException("Inventory item not found");
            }

            // Store original values for audit
            var originalValues = new
            {
                existingItem.Name,
                existingItem.ItemCode,
                existingItem.Category,
                existingItem.Status,
                existingItem.Condition,
                existingItem.Quantity,
                existingItem.UnitCost,
                existingItem.LocationId
            };

            // Update properties
            existingItem.Name = item.Name;
            existingItem.Description = item.Description;
            existingItem.Category = item.Category;
            existingItem.ItemType = item.ItemType;
            existingItem.Brand = item.Brand;
            existingItem.Model = item.Model;
            existingItem.SerialNumber = item.SerialNumber;
            existingItem.PartNumber = item.PartNumber;
            existingItem.Status = item.Status;
            existingItem.Condition = item.Condition;
            existingItem.MinimumStock = item.MinimumStock;
            existingItem.MaximumStock = item.MaximumStock;
            existingItem.ReorderLevel = item.ReorderLevel;
            existingItem.UnitCost = item.UnitCost;
            existingItem.Supplier = item.Supplier;
            existingItem.SupplierPartNumber = item.SupplierPartNumber;
            existingItem.PurchaseDate = item.PurchaseDate;
            existingItem.WarrantyExpiry = item.WarrantyExpiry;
            existingItem.WarrantyPeriodMonths = item.WarrantyPeriodMonths;
            existingItem.StorageZone = item.StorageZone;
            existingItem.StorageShelf = item.StorageShelf;
            existingItem.StorageBin = item.StorageBin;
            existingItem.Specifications = item.Specifications;
            existingItem.CompatibleWith = item.CompatibleWith;
            existingItem.Notes = item.Notes;
            existingItem.IsConsumable = item.IsConsumable;
            existingItem.RequiresCalibration = item.RequiresCalibration;
            existingItem.LastCalibrationDate = item.LastCalibrationDate;
            existingItem.NextCalibrationDate = item.NextCalibrationDate;
            existingItem.CalibrationCertificate = item.CalibrationCertificate;
            existingItem.LastUpdatedDate = DateTime.UtcNow;
            existingItem.LastUpdatedByUserId = userId;

            // Recalculate total value
            if (existingItem.UnitCost.HasValue && existingItem.Quantity > 0)
            {
                existingItem.TotalValue = existingItem.UnitCost.Value * existingItem.Quantity;
            }

            await _context.SaveChangesAsync();

            // Create audit log
            await _auditService.LogAsync(
                AuditAction.Update,
                "InventoryItem",
                item.Id,
                userId,
                $"Updated inventory item {item.ItemCode} - {item.Name}",
                originalValues,
                existingItem
            );

            return existingItem;
        }

        public async Task<bool> DeleteInventoryItemAsync(int id, string userId)
        {
            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null)
            {
                return false;
            }

            // Check if item has any movements or is currently deployed
            var hasMovements = await _context.InventoryMovements
                .AnyAsync(m => m.InventoryItemId == id);

            var hasAssetMappings = await _context.AssetInventoryMappings
                .AnyAsync(m => m.InventoryItemId == id && m.Status == AssetInventoryMappingStatus.Deployed);

            if (hasMovements || hasAssetMappings)
            {
                // Soft delete - just mark as disposed
                item.Status = InventoryStatus.Disposed;
                item.LastUpdatedDate = DateTime.UtcNow;
                item.LastUpdatedByUserId = userId;
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(
                    AuditAction.Delete,
                    "InventoryItem",
                    id,
                    userId,
                    $"Soft deleted inventory item {item.ItemCode} - {item.Name} (marked as disposed)",
                    item,
                    null
                );
            }
            else
            {
                // Hard delete
                _context.InventoryItems.Remove(item);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(
                    AuditAction.Delete,
                    "InventoryItem",
                    id,
                    userId,
                    $"Deleted inventory item {item.ItemCode} - {item.Name}",
                    item,
                    null
                );
            }

            return true;
        }

        public async Task<bool> IsItemCodeUniqueAsync(string itemCode, int? excludeId = null)
        {
            var query = _context.InventoryItems.AsQueryable();
            
            if (excludeId.HasValue)
                query = query.Where(i => i.Id != excludeId.Value);

            return !await query.AnyAsync(i => i.ItemCode == itemCode);
        }        public async Task<bool> CheckAvailabilityAsync(int itemId, int quantity)
        {
            var item = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null) return false;

            return item.Quantity >= quantity;
        }

        public async Task<bool> CheckAvailabilityAsync(string itemName, int requiredQuantity)
        {
            var item = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.Name == itemName);
            
            if (item == null) return false;
            
            return item.Quantity >= requiredQuantity;
        }

        #endregion

        #region Stock Management

        public async Task<bool> AdjustStockAsync(int itemId, int adjustmentQuantity, string reason, string userId)
        {
            var item = await _context.InventoryItems.FindAsync(itemId);
            if (item == null)
            {
                return false;
            }

            var oldQuantity = item.Quantity;
            item.Quantity += adjustmentQuantity;
            
            // Ensure quantity doesn't go negative
            if (item.Quantity < 0)
            {
                item.Quantity = 0;
            }

            item.LastUpdatedDate = DateTime.UtcNow;
            item.LastUpdatedByUserId = userId;

            // Recalculate total value
            if (item.UnitCost.HasValue)
            {
                item.TotalValue = item.UnitCost.Value * item.Quantity;
            }

            // Create movement record
            var movement = new InventoryMovement
            {
                InventoryItemId = itemId,
                MovementType = InventoryMovementType.Adjustment,
                Quantity = Math.Abs(adjustmentQuantity),
                MovementDate = DateTime.UtcNow,
                Reason = reason,
                PerformedByUserId = userId,
                CreatedDate = DateTime.UtcNow
            };

            _context.InventoryMovements.Add(movement);
            await _context.SaveChangesAsync();

            // Create audit log
            await _auditService.LogAsync(
                AuditAction.Update,
                "InventoryItem",
                itemId,
                userId,
                $"Stock adjustment: {oldQuantity} → {item.Quantity} (change: {adjustmentQuantity:+#;-#;0}). Reason: {reason}",
                new { OldQuantity = oldQuantity },
                new { NewQuantity = item.Quantity }
            );

            return true;
        }

        public async Task<bool> StockInAsync(int itemId, int quantity, decimal? unitCost, string supplier, string reason, string userId, string? purchaseOrderNumber = null, string? invoiceNumber = null)
        {
            var item = await _context.InventoryItems.FindAsync(itemId);
            if (item == null)
                return false;
            
            // Update inventory
            item.Quantity += quantity;
            item.Status = InventoryStatus.InStock;
            
            // Update cost if provided
            if (unitCost.HasValue)
            {
                // Calculate weighted average cost
                var currentValue = (item.UnitCost ?? 0) * (item.Quantity - quantity);
                var newValue = unitCost.Value * quantity;
                var totalQuantity = item.Quantity;
                
                if (totalQuantity > 0)
                {
                    item.UnitCost = (currentValue + newValue) / totalQuantity;
                    item.TotalValue = item.UnitCost * totalQuantity;
                }
            }
            
            // Create movement record
            var movement = new InventoryMovement
            {
                InventoryItemId = itemId,
                MovementType = InventoryMovementType.StockIn,
                Quantity = quantity,
                MovementDate = DateTime.UtcNow,
                Reason = $"{reason} - Supplier: {supplier}",
                PerformedByUserId = userId,
                CreatedDate = DateTime.UtcNow
            };
            
            _context.InventoryMovements.Add(movement);
            
            // Create transaction record if cost is provided
            if (unitCost.HasValue)
            {
                var transaction = new InventoryTransaction
                {
                    InventoryItemId = itemId,
                    TransactionType = InventoryTransactionType.Purchase,
                    Quantity = quantity,
                    UnitCost = unitCost.Value,
                    TotalCost = unitCost.Value * quantity,
                    Supplier = supplier,
                    PurchaseOrderNumber = purchaseOrderNumber,
                    InvoiceNumber = invoiceNumber,
                    TransactionDate = DateTime.UtcNow,
                    Description = reason,
                    CreatedByUserId = userId,
                    CreatedDate = DateTime.UtcNow
                };
                
                _context.InventoryTransactions.Add(transaction);
            }
            
            await _context.SaveChangesAsync();
            
            // Create audit log
            await _auditService.LogAsync(
                AuditAction.Update,
                "InventoryItem",
                itemId,
                userId,
                $"Stock in: {quantity} units added",
                null,
                new { Quantity = quantity, UnitCost = unitCost, Supplier = supplier, Reason = reason }
            );
            
            return true;
        }

        public async Task<bool> StockOutAsync(int itemId, int quantity, string reason, string userId)
        {
            var item = await _context.InventoryItems.FindAsync(itemId);
            if (item == null)
                return false;
            
            // Check if enough stock is available
            if (item.Quantity < quantity)
                throw new InvalidOperationException($"Insufficient stock. Available: {item.Quantity}, Required: {quantity}");
            
            // Update inventory
            item.Quantity -= quantity;
            
            // Create movement record
            var movement = new InventoryMovement
            {
                InventoryItemId = itemId,
                MovementType = InventoryMovementType.StockOut,
                Quantity = quantity,
                MovementDate = DateTime.UtcNow,
                Reason = reason,
                PerformedByUserId = userId,
                CreatedDate = DateTime.UtcNow
            };
            
            _context.InventoryMovements.Add(movement);
            
            await _context.SaveChangesAsync();
            
            // Create audit log
            await _auditService.LogAsync(
                AuditAction.Update,
                "InventoryItem",
                itemId,
                userId,
                $"Stock out: {quantity} units removed",
                null,
                new { Quantity = quantity, Reason = reason }
            );
            
            return true;
        }

        public async Task<IEnumerable<InventoryMovement>> GetInventoryMovementHistoryAsync(int itemId)
        {
            return await _context.InventoryMovements
                .Where(m => m.InventoryItemId == itemId)
                .Include(m => m.PerformedByUser)
                .Include(m => m.ApprovedByUser)
                .Include(m => m.FromLocation)
                .Include(m => m.ToLocation)
                .Include(m => m.RelatedAsset)
                .OrderByDescending(m => m.MovementDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryMovement>> GetRecentMovementsAsync(int days = 30)
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);
            return await _context.InventoryMovements
                .Where(m => m.MovementDate >= fromDate)
                .Include(m => m.InventoryItem)
                .Include(m => m.PerformedByUser)
                .Include(m => m.FromLocation)
                .Include(m => m.ToLocation)
                .Include(m => m.RelatedAsset)
                .OrderByDescending(m => m.MovementDate)
                .ToListAsync();
        }

        public async Task<bool> DeployToAssetAsync(int assetId, int inventoryItemId, int quantity, string reason, string userId)
        {
            var inventoryItem = await _context.InventoryItems.FindAsync(inventoryItemId);
            var asset = await _context.Assets.FindAsync(assetId);
            
            if (inventoryItem == null || asset == null)
                return false;
                
            // Check if enough stock is available
            if (inventoryItem.Quantity < quantity)
                throw new InvalidOperationException($"Insufficient stock. Available: {inventoryItem.Quantity}, Required: {quantity}");
            
            // Reduce inventory quantity
            inventoryItem.Quantity -= quantity;
            
            // Update inventory status
            inventoryItem.Status = InventoryStatus.Deployed;
            
            // Create asset-inventory mapping
            var mapping = new AssetInventoryMapping
            {
                AssetId = assetId,
                InventoryItemId = inventoryItemId,
                Quantity = quantity,
                Status = AssetInventoryMappingStatus.Deployed,
                DeploymentDate = DateTime.UtcNow,
                DeploymentReason = reason,
                DeployedByUserId = userId,
                CreatedDate = DateTime.UtcNow
            };
            
            _context.AssetInventoryMappings.Add(mapping);
            
            // Create inventory movement record
            var movement = new InventoryMovement
            {
                InventoryItemId = inventoryItemId,
                MovementType = InventoryMovementType.AssetDeployment,
                Quantity = quantity,
                MovementDate = DateTime.UtcNow,
                Reason = $"Deployed to Asset {asset.AssetTag}: {reason}",
                PerformedByUserId = userId,
                CreatedDate = DateTime.UtcNow,
                RelatedAssetId = assetId
            };
            
            _context.InventoryMovements.Add(movement);
            
            await _context.SaveChangesAsync();
            
            // Create audit log
            await _auditService.LogAsync(
                AuditAction.Update,
                "InventoryItem",
                inventoryItemId,
                userId,
                $"Deployed {quantity} units to Asset {asset.AssetTag}",
                null,
                new { AssetId = assetId, Quantity = quantity, Reason = reason }
            );
            
            return true;
        }

        public async Task<bool> ReturnFromAssetAsync(int assetId, int inventoryItemId, int quantity, string reason, string userId)
        {
            var inventoryItem = await _context.InventoryItems.FindAsync(inventoryItemId);
            var asset = await _context.Assets.FindAsync(assetId);
            
            if (inventoryItem == null || asset == null)
                return false;
            
            // Find active mapping
            var mapping = await _context.AssetInventoryMappings
                .FirstOrDefaultAsync(m => m.AssetId == assetId && 
                                        m.InventoryItemId == inventoryItemId && 
                                        m.Status == AssetInventoryMappingStatus.Deployed);
            
            if (mapping == null)
                throw new InvalidOperationException("No active deployment found for this asset-inventory combination");
            
            if (mapping.Quantity < quantity)
                throw new InvalidOperationException($"Cannot return more than deployed. Deployed: {mapping.Quantity}, Requested: {quantity}");
            
            // Update inventory - return as used condition if coming from asset
            inventoryItem.Quantity += quantity;
            inventoryItem.Status = InventoryStatus.InStock;
            
            // If the item was new when deployed, mark it as used when returned
            if (inventoryItem.Condition == InventoryCondition.New)
            {
                inventoryItem.Condition = InventoryCondition.Good; // Used but functional
            }
            
            // Update mapping
            if (mapping.Quantity == quantity)
            {
                // Full return - mark mapping as returned
                mapping.Status = AssetInventoryMappingStatus.Returned;
                mapping.ReturnDate = DateTime.UtcNow;
                mapping.ReturnReason = reason;
                mapping.ReturnedByUserId = userId;
                mapping.LastUpdatedDate = DateTime.UtcNow;
                mapping.LastUpdatedByUserId = userId;
            }
            else
            {
                // Partial return - reduce deployed quantity and create new mapping for returned portion
                mapping.Quantity -= quantity;
                mapping.LastUpdatedDate = DateTime.UtcNow;
                mapping.LastUpdatedByUserId = userId;
                
                var returnMapping = new AssetInventoryMapping
                {
                    AssetId = assetId,
                    InventoryItemId = inventoryItemId,
                    Quantity = quantity,
                    Status = AssetInventoryMappingStatus.Returned,
                    DeploymentDate = mapping.DeploymentDate,
                    ReturnDate = DateTime.UtcNow,
                    DeploymentReason = mapping.DeploymentReason,
                    ReturnReason = reason,
                    DeployedByUserId = mapping.DeployedByUserId,
                    ReturnedByUserId = userId,
                    CreatedDate = DateTime.UtcNow
                };
                
                _context.AssetInventoryMappings.Add(returnMapping);
            }
            
            // Create inventory movement record
            var movement = new InventoryMovement
            {
                InventoryItemId = inventoryItemId,
                MovementType = InventoryMovementType.AssetReturn,
                Quantity = quantity,
                MovementDate = DateTime.UtcNow,
                Reason = $"Returned from Asset {asset.AssetTag}: {reason}",
                PerformedByUserId = userId,
                CreatedDate = DateTime.UtcNow,
                RelatedAssetId = assetId
            };
            
            _context.InventoryMovements.Add(movement);
            
            await _context.SaveChangesAsync();
            
            // Create audit log
            await _auditService.LogAsync(
                AuditAction.Update,
                "InventoryItem",
                inventoryItemId,
                userId,
                $"Returned {quantity} units from Asset {asset.AssetTag}",
                null,
                new { AssetId = assetId, Quantity = quantity, Reason = reason }
            );
            
            return true;
        }

        public async Task<IEnumerable<AssetInventoryMapping>> GetAssetInventoryMappingsAsync(int assetId)
        {
            return await _context.AssetInventoryMappings
                .Where(m => m.AssetId == assetId)
                .Include(m => m.InventoryItem)
                .Include(m => m.DeployedByUser)
                .Include(m => m.ReturnedByUser)
                .OrderByDescending(m => m.DeploymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<AssetInventoryMapping>> GetInventoryAssetMappingsAsync(int inventoryItemId)
        {
            return await _context.AssetInventoryMappings
                .Where(m => m.InventoryItemId == inventoryItemId)
                .Include(m => m.Asset)
                .Include(m => m.DeployedByUser)
                .Include(m => m.ReturnedByUser)
                .OrderByDescending(m => m.DeploymentDate)
                .ToListAsync();
        }

        public Task<bool> ReplaceAssetComponentAsync(int assetId, int oldInventoryItemId, int newInventoryItemId, string reason, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<InventoryTransaction> CreateTransactionAsync(InventoryTransaction transaction, string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<InventoryTransaction>> GetTransactionHistoryAsync(int itemId)
        {
            return await _context.InventoryTransactions
                .Where(t => t.InventoryItemId == itemId)
                .Include(t => t.CreatedByUser)
                .Include(t => t.ApprovedByUser)
                .Include(t => t.QualityCheckedByUser)
                .Include(t => t.RelatedAsset)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public Task<IEnumerable<InventoryTransaction>> GetTransactionsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<InventoryTransaction>> GetPurchaseTransactionsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            throw new NotImplementedException();
        }

        /*
        public Task<bool> BulkUpdateStatusAsync(List<int> itemIds, InventoryStatus newStatus, string reason, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> BulkUpdateLocationAsync(List<int> itemIds, int newLocationId, string reason, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> BulkUpdateConditionAsync(List<int> itemIds, InventoryCondition newCondition, string reason, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> BulkAdjustStockAsync(List<(int ItemId, int AdjustmentQuantity)> adjustments, string reason, string userId)
        {
            throw new NotImplementedException();
        }
        */

        public Task<InventoryStockReport> GetStockReportAsync()
        {
            throw new NotImplementedException();
        }

        public Task<InventoryMovementReport> GetMovementReportAsync(DateTime fromDate, DateTime toDate)
        {
            throw new NotImplementedException();
        }

        public Task<InventoryValuationReport> GetValuationReportAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateInventoryFromProcurementAsync(ProcurementItemReceived receivedItem, string userId)
        {
            if (receivedItem == null) throw new ArgumentNullException(nameof(receivedItem));

            // Find an existing inventory item or create a new one
            var inventoryItem = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.Name == receivedItem.ItemName && i.Brand == receivedItem.Brand && i.Model == receivedItem.Model);

            if (inventoryItem == null)
            {
                inventoryItem = new InventoryItem
                {
                    Name = receivedItem.ItemName,
                    Description = receivedItem.Description,
                    Brand = receivedItem.Brand ?? "N/A",
                    Model = receivedItem.Model ?? "N/A",
                    UnitCost = receivedItem.UnitPrice,
                    Quantity = receivedItem.ReceivedQuantity,
                    // Set other properties as needed, e.g., Category from receivedItem
                };
                await CreateInventoryItemAsync(inventoryItem, userId);
            }
            else
            {
                await StockInAsync(inventoryItem.Id, receivedItem.ReceivedQuantity, receivedItem.UnitPrice, receivedItem.SupplierName ?? "Procurement", "Received from procurement", userId, receivedItem.PurchaseOrderNumber);
            }

            return true;
        }

        public async Task<IEnumerable<StockLevelAlert>> GetStockLevelAlertsAsync()
        {
            var alerts = new List<StockLevelAlert>();

            // Critical stock alerts (quantity <= minimum stock)
            var criticalItems = await _context.InventoryItems
                .AsNoTracking()
                .Where(i => i.Quantity <= i.MinimumStock && i.MinimumStock > 0)
                .Include(i => i.Location)
                .Select(i => new StockLevelAlert
                {
                    InventoryItemId = i.Id,
                    ItemName = i.Name,
                    ItemCode = i.ItemCode,
                    CurrentStock = i.Quantity,
                    MinimumLevel = i.MinimumStock,
                    ReorderLevel = i.ReorderLevel,
                    AlertType = "CriticalStock",
                    LocationName = i.Location != null ? i.Location.Name : "N/A",
                    CreatedDate = DateTime.UtcNow
                })
                .ToListAsync();
            
            alerts.AddRange(criticalItems);

            // Low stock alerts (reorder level)
            var lowStockItems = await _context.InventoryItems
                .AsNoTracking()
                .Where(i => i.Quantity <= i.ReorderLevel && i.ReorderLevel > 0 && i.Quantity > i.MinimumStock)
                 .Include(i => i.Location)
                .Select(i => new StockLevelAlert
                {
                    InventoryItemId = i.Id,
                    ItemName = i.Name,
                    ItemCode = i.ItemCode,
                    CurrentStock = i.Quantity,
                    MinimumLevel = i.MinimumStock,
                    ReorderLevel = i.ReorderLevel,
                    AlertType = "LowStock",
                    LocationName = i.Location != null ? i.Location.Name : "N/A",
                    CreatedDate = DateTime.UtcNow
                })
                .ToListAsync();

            alerts.AddRange(lowStockItems);

            return alerts.DistinctBy(a => a.InventoryItemId).ToList();
        }

        public async Task<InventoryReservationResult> CheckAvailabilityAndReserveAsync(int itemId, int quantity, string reason, string userId)
        {
            var item = await _context.InventoryItems.FindAsync(itemId);
            if (item == null)
            {
                return new InventoryReservationResult { Success = false, Message = "Inventory item not found." };
            }

            if (item.Quantity < quantity)
            {
                return new InventoryReservationResult 
                { 
                    Success = false, 
                    Message = $"Insufficient stock for {item.Name}. Required: {quantity}, Available: {item.Quantity}."
                };
            }

            // Reserve the stock
            item.Quantity -= quantity;
            item.ReservedQuantity += quantity;
            item.LastUpdatedDate = DateTime.UtcNow;
            item.LastUpdatedByUserId = userId;

            // Create a reservation movement record
            var movement = new InventoryMovement
            {
                InventoryItemId = itemId,
                MovementType = InventoryMovementType.Reservation,
                Quantity = quantity,
                MovementDate = DateTime.UtcNow,
                Reason = reason,
                PerformedByUserId = userId,
                CreatedDate = DateTime.UtcNow
            };
            _context.InventoryMovements.Add(movement);

            await _context.SaveChangesAsync();

            // Create audit log
            await _auditService.LogAsync(
                AuditAction.Update,
                "InventoryItem",
                itemId,
                userId,
                $"Reserved {quantity} units of {item.ItemCode} for: {reason}. Available quantity now: {item.Quantity}.",
                new { OldQuantity = item.Quantity + quantity, OldReservedQuantity = item.ReservedQuantity - quantity },
                new { NewQuantity = item.Quantity, NewReservedQuantity = item.ReservedQuantity }
            );

            return new InventoryReservationResult 
            { 
                Success = true, 
                QuantityReserved = quantity, 
                Message = $"Successfully reserved {quantity} units of {item.Name}."
            };
        }

        public async Task<bool> UpdateInventoryQuantityAsync(int itemId, int newQuantity, string reason, string userId)
        {
            var item = await _context.InventoryItems.FindAsync(itemId);
            if (item == null)
            {
                _logger.LogWarning("UpdateInventoryQuantityAsync: Item with ID {ItemId} not found.", itemId);
                return false;
            }

            var originalQuantity = item.Quantity;
            var quantityChange = newQuantity - originalQuantity;

            // Update the quantity
            item.Quantity = newQuantity;

            if (item.Quantity < 0)
            {
                _logger.LogWarning("UpdateInventoryQuantityAsync: Attempted to set negative quantity for item {ItemId}. Setting to 0.", itemId);
                item.Quantity = 0; // Prevent negative inventory
            }

            item.LastUpdatedDate = DateTime.UtcNow;
            item.LastUpdatedByUserId = userId;

            // Recalculate total value
            if (item.UnitCost.HasValue)
            {
                item.TotalValue = item.UnitCost.Value * item.Quantity;
            }

            // Create movement record
            var movementType = quantityChange > 0 ? InventoryMovementType.StockIn : InventoryMovementType.StockOut;
            var movement = new InventoryMovement
            {
                InventoryItemId = itemId,
                MovementType = movementType,
                Quantity = Math.Abs(quantityChange),
                MovementDate = DateTime.UtcNow,
                Reason = reason,
                PerformedByUserId = userId,
                CreatedDate = DateTime.UtcNow
            };
            _context.InventoryMovements.Add(movement);

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                AuditAction.Update,
                "InventoryItem",
                itemId,
                userId,
                $"Quantity updated for {item.ItemCode}. Change: {quantityChange:+#;-#;0}. Reason: {reason}.",
                new { OldQuantity = originalQuantity },
                new { NewQuantity = item.Quantity }
            );

            return true;
        }

        public async Task<bool> ReceiveStockFromProcurementAsync(int inventoryItemId, int quantity, decimal unitCost, string supplier, string purchaseOrderNumber, string userId)
        {
            var item = await _context.InventoryItems.FindAsync(inventoryItemId);
            if (item == null)
            {
                _logger.LogError("ReceiveStockFromProcurementAsync: Inventory item with ID {InventoryItemId} not found.", inventoryItemId);
                return false;
            }

            var oldQuantity = item.Quantity;

            // Update item details
            item.Quantity += quantity;
            item.UnitCost = unitCost; // Update with the latest cost
            item.Supplier = supplier;
            item.PurchaseDate = DateTime.UtcNow;
            item.LastUpdatedDate = DateTime.UtcNow;
            item.LastUpdatedByUserId = userId;

            // Recalculate total value
            item.TotalValue = item.UnitCost.Value * item.Quantity;

            // Create Stock-In Movement
            var stockInMovement = new InventoryMovement
            {
                InventoryItemId = item.Id,
                MovementType = InventoryMovementType.StockIn,
                Quantity = quantity,
                MovementDate = DateTime.UtcNow,
                Reason = $"Received from procurement. PO: {purchaseOrderNumber}",
                PerformedByUserId = userId,
                CreatedDate = DateTime.UtcNow
            };
            _context.InventoryMovements.Add(stockInMovement);

            // Create Purchase Transaction
            var transaction = new InventoryTransaction
            {
                InventoryItemId = item.Id,
                TransactionType = InventoryTransactionType.Purchase,
                Quantity = quantity,
                UnitCost = unitCost,
                TotalCost = quantity * unitCost,
                Supplier = supplier,
                PurchaseOrderNumber = purchaseOrderNumber,
                PurchaseDate = DateTime.UtcNow,
                TransactionDate = DateTime.UtcNow,
                Description = $"Received stock from PO: {purchaseOrderNumber}",
                CreatedByUserId = userId,
                CreatedDate = DateTime.UtcNow
            };
            _context.InventoryTransactions.Add(transaction);

            await _context.SaveChangesAsync();

            // Create Audit Log
            await _auditService.LogAsync(
                AuditAction.Update,
                "InventoryItem",
                item.Id,
                userId,
                $"Received {quantity} units of {item.ItemCode} from PO {purchaseOrderNumber}. New quantity: {item.Quantity}.",
                new { OldQuantity = oldQuantity, OldUnitCost = item.UnitCost },
                new { NewQuantity = item.Quantity, NewUnitCost = unitCost }
            );

            return true;
        }


        // Continue with other methods...
        #endregion

        #region Helper Methods

        private async Task<string> GenerateItemCodeAsync(InventoryCategory category)
        {
            var prefix = category switch
            {
                InventoryCategory.Desktop => "DT",
                InventoryCategory.Laptop => "LT",
                InventoryCategory.Server => "SV",
                InventoryCategory.NetworkDevice => "ND",
                InventoryCategory.Printer => "PR",
                InventoryCategory.Monitor => "MN",
                InventoryCategory.Peripherals => "PE",
                InventoryCategory.Components => "CP",
                InventoryCategory.Storage => "ST",
                InventoryCategory.Memory => "MM",
                InventoryCategory.PowerSupply => "PS",
                InventoryCategory.Cables => "CB",
                InventoryCategory.Software => "SW",
                InventoryCategory.Accessories => "AC",
                InventoryCategory.Consumables => "CS",
                InventoryCategory.MedicalDevice => "MD",
                InventoryCategory.Telephone => "TL",
                InventoryCategory.Audio => "AD",
                InventoryCategory.Video => "VD",
                InventoryCategory.Security => "SC",
                InventoryCategory.Backup => "BK",
                _ => "IT"
            };

            var currentYear = DateTime.Now.Year.ToString()[2..];
            var latestItem = await _context.InventoryItems
                .Where(i => i.ItemCode.StartsWith($"{prefix}{currentYear}"))
                .OrderByDescending(i => i.ItemCode)
                .FirstOrDefaultAsync();

            var sequence = 1;
            if (latestItem != null && latestItem.ItemCode.Length >= 6)
            {
                var sequencePart = latestItem.ItemCode[4..];
                if (int.TryParse(sequencePart, out var lastSequence))
                {
                    sequence = lastSequence + 1;
                }
            }

            return $"{prefix}{currentYear}{sequence:D4}";
        }

        #endregion

        // Placeholder implementations for remaining interface methods
        // These would be implemented with similar patterns as above

        public async Task<InventoryDashboardData> GetInventoryDashboardDataAsync()
        {
            var data = new InventoryDashboardData();

            var items = await _context.InventoryItems.AsNoTracking().ToListAsync();

            data.TotalItems = items.Count;
            data.OutOfStockItems = items.Count(i => i.Quantity == 0);
            data.LowStockItems = items.Count(i => i.Quantity > 0 && i.Quantity <= i.MinimumStock);
            data.TotalValue = items.Sum(i => i.TotalValue ?? 0);

            data.CategoryDistribution = items.GroupBy(i => i.Category.ToString())
                                             .ToDictionary(g => g.Key, g => g.Count());

            data.StatusDistribution = items.GroupBy(i => i.Status.ToString())
                                           .ToDictionary(g => g.Key, g => g.Count());

            data.RecentMovements = await _context.InventoryMovements
                .AsNoTracking()
                .OrderByDescending(m => m.MovementDate)
                .Take(10)
                .Select(m => new InventoryMovementViewModel
                {
                    Id = m.Id,
                    ItemName = m.InventoryItem != null ? m.InventoryItem.Name : "N/A",
                    MovementType = m.MovementType,
                    QuantityChanged = m.Quantity,
                    MovementDate = m.MovementDate,
                    MovedBy = m.PerformedByUser != null ? m.PerformedByUser.UserName : "System"
                }).ToListAsync();

            return data;
        }

        public async Task<IEnumerable<InventoryExpiryAlert>> GetExpiryAlertsAsync()
        {
            var upcomingExpiryDate = DateTime.UtcNow.AddDays(30);
            return await _context.InventoryItems
                .AsNoTracking()
                .Where(i => i.WarrantyExpiry.HasValue && i.WarrantyExpiry.Value <= upcomingExpiryDate)
                .Include(i => i.Location)
                .Select(i => new InventoryExpiryAlert
                {
                    InventoryItemId = i.Id,
                    ItemName = i.Name,
                    ItemCode = i.ItemCode,
                    WarrantyExpiry = i.WarrantyExpiry,
                    DaysUntilExpiry = i.WarrantyExpiry.HasValue 
                        ? (int)(i.WarrantyExpiry.Value - DateTime.UtcNow).TotalDays 
                        : 0,
                    AlertType = i.WarrantyExpiry.HasValue && i.WarrantyExpiry.Value < DateTime.UtcNow 
                        ? "Expired" 
                        : "Expiring",
                    LocationName = i.Location != null ? i.Location.Name : "Unknown",
                    CreatedDate = DateTime.UtcNow
                })
                .ToListAsync();
        }

        public Task<bool> ReserveStockAsync(int itemId, int quantity, string reason, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ReleaseReservationAsync(int itemId, int quantity, string reason, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AllocateStockAsync(int itemId, int quantity, string reason, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetStockLevelsAsync(int itemId, int minStock, int maxStock, int reorderLevel, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TransferInventoryAsync(int itemId, int quantity, int fromLocationId, int toLocationId, string reason, string userId, string? fromZone = null, string? toZone = null, string? fromShelf = null, string? toShelf = null, string? fromBin = null, string? toBin = null)
        {
            throw new NotImplementedException();
        }

        #region Enhanced Advanced Search & Bulk Operations

        public async Task<PagedResult<AdvancedInventorySearchResult>> GetInventoryItemsAdvancedAsync(AdvancedInventorySearchModel searchModel)
        {
            var query = _context.InventoryItems
                .Include(i => i.Location)
                .Include(i => i.CreatedByUser)
                .Include(i => i.LastUpdatedByUser)
                .AsQueryable();

            // Apply search filters
            if (!string.IsNullOrEmpty(searchModel.SearchTerm))
            {
                var searchTerm = searchModel.SearchTerm.ToLower();
                query = query.Where(i => 
                    i.Name.ToLower().Contains(searchTerm) ||
                    i.ItemCode.ToLower().Contains(searchTerm) ||
                    i.Brand.ToLower().Contains(searchTerm) ||
                    i.Model.ToLower().Contains(searchTerm) ||
                    (i.Description != null && i.Description.ToLower().Contains(searchTerm)) ||
                    (i.SerialNumber != null && i.SerialNumber.ToLower().Contains(searchTerm)));
            }

            if (!string.IsNullOrEmpty(searchModel.ItemCode))
            {
                query = query.Where(i => i.ItemCode.Contains(searchModel.ItemCode));
            }

            if (searchModel.Category.HasValue)
            {
                query = query.Where(i => i.Category == searchModel.Category.Value);
            }

            if (searchModel.Status.HasValue)
            {
                query = query.Where(i => i.Status == searchModel.Status.Value);
            }

            if (searchModel.Condition.HasValue)
            {
                query = query.Where(i => i.Condition == searchModel.Condition.Value);
            }

            if (searchModel.LocationId.HasValue)
            {
                query = query.Where(i => i.LocationId == searchModel.LocationId.Value);
            }

            if (!string.IsNullOrEmpty(searchModel.Brand))
            {
                query = query.Where(i => i.Brand.Contains(searchModel.Brand));
            }

            if (!string.IsNullOrEmpty(searchModel.Model))
            {
                query = query.Where(i => i.Model.Contains(searchModel.Model));
            }

            if (!string.IsNullOrEmpty(searchModel.Supplier))
            {
                query = query.Where(i => i.Supplier != null && i.Supplier.Contains(searchModel.Supplier));
            }

            // Stock level filters
            if (searchModel.StockLevelFilter.HasValue)
            {
                switch (searchModel.StockLevelFilter.Value)
                {
                    case StockLevelFilter.LowStock:
                        query = query.Where(i => i.Quantity <= i.ReorderLevel);
                        break;
                    case StockLevelFilter.CriticalStock:
                        query = query.Where(i => i.Quantity <= i.MinimumStock);
                        break;
                    case StockLevelFilter.OutOfStock:
                        query = query.Where(i => i.Quantity == 0);
                        break;
                    case StockLevelFilter.Overstocked:
                        query = query.Where(i => i.Quantity >= i.MaximumStock);
                        break;
                    case StockLevelFilter.Normal:
                        query = query.Where(i => i.Quantity > i.ReorderLevel && i.Quantity < i.MaximumStock);
                        break;
                }
            }

            // Quick filters
            if (searchModel.ShowLowStockOnly)
            {
                query = query.Where(i => i.Quantity <= i.ReorderLevel);
            }

            if (searchModel.ShowCriticalStockOnly)
            {
                query = query.Where(i => i.Quantity <= i.MinimumStock);
            }

            if (searchModel.ShowOverstockedOnly)
            {
                query = query.Where(i => i.Quantity >= i.MaximumStock);
            }

            // Quantity filters
            if (searchModel.MinQuantity.HasValue)
            {
                query = query.Where(i => i.Quantity >= searchModel.MinQuantity.Value);
            }

            if (searchModel.MaxQuantity.HasValue)
            {
                query = query.Where(i => i.Quantity <= searchModel.MaxQuantity.Value);
            }

            // Value filters
            if (searchModel.MinUnitCost.HasValue)
            {
                query = query.Where(i => i.UnitCost >= searchModel.MinUnitCost.Value);
            }

            if (searchModel.MaxUnitCost.HasValue)
            {
                query = query.Where(i => i.UnitCost <= searchModel.MaxUnitCost.Value);
            }

            if (searchModel.MinTotalValue.HasValue)
            {
                query = query.Where(i => i.TotalValue >= searchModel.MinTotalValue.Value);
            }

            if (searchModel.MaxTotalValue.HasValue)
            {
                query = query.Where(i => i.TotalValue <= searchModel.MaxTotalValue.Value);
            }

            // Date filters
            if (searchModel.CreatedFrom.HasValue)
            {
                query = query.Where(i => i.CreatedDate >= searchModel.CreatedFrom.Value);
            }

            if (searchModel.CreatedTo.HasValue)
            {
                query = query.Where(i => i.CreatedDate <= searchModel.CreatedTo.Value.AddDays(1));
            }

            if (searchModel.PurchaseDateFrom.HasValue)
            {
                query = query.Where(i => i.PurchaseDate >= searchModel.PurchaseDateFrom.Value);
            }

            if (searchModel.PurchaseDateTo.HasValue)
            {
                query = query.Where(i => i.PurchaseDate <= searchModel.PurchaseDateTo.Value.AddDays(1));
            }

            // Boolean filters
            if (searchModel.IsConsumable.HasValue)
            {
                query = query.Where(i => i.IsConsumable == searchModel.IsConsumable.Value);
            }

            if (searchModel.HasSerialNumber.HasValue)
            {
                if (searchModel.HasSerialNumber.Value)
                {
                    query = query.Where(i => !string.IsNullOrEmpty(i.SerialNumber));
                }
                else
                {
                    query = query.Where(i => string.IsNullOrEmpty(i.SerialNumber));
                }
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = searchModel.SortOrder.ToLower() == "desc"
                ? ApplySortingDescending(query, searchModel.SortBy)
                : ApplySortingAscending(query, searchModel.SortBy);

            // Apply pagination
            var items = await query
                .Skip((searchModel.PageNumber - 1) * searchModel.PageSize)
                .Take(searchModel.PageSize)
                .Select(i => new AdvancedInventorySearchResult
                {
                    Id = i.Id,
                    ItemCode = i.ItemCode,
                    Name = i.Name,
                    Description = i.Description,
                    Category = i.Category,
                    ItemType = i.ItemType,
                    Status = i.Status,
                    Condition = i.Condition,
                    Brand = i.Brand,
                    Model = i.Model,
                    SerialNumber = i.SerialNumber,
                    Quantity = i.Quantity,
                    ReservedQuantity = i.ReservedQuantity,
                    MinimumStock = i.MinimumStock,
                    MaximumStock = i.MaximumStock,
                    ReorderLevel = i.ReorderLevel,
                    UnitCost = i.UnitCost,
                    TotalValue = i.TotalValue,
                    Supplier = i.Supplier,
                    PurchaseDate = i.PurchaseDate,
                    WarrantyExpiry = i.WarrantyExpiry,
                    LocationName = i.Location != null ? i.Location.Name : "Unknown",
                    StorageLocation = GetStorageLocationString(i.StorageZone, i.StorageShelf, i.StorageBin) ?? UnknownValue,
                    AbcClassification = i.AbcClassification,
                    CreatedDate = i.CreatedDate,
                    LastUpdatedDate = i.LastUpdatedDate,
                    CreatedByUserName = i.CreatedByUser != null ? (i.CreatedByUser.UserName ?? SystemUser) : SystemUser,
                    LastUpdatedByUserName = i.LastUpdatedByUser != null ? (i.LastUpdatedByUser.UserName ?? UnknownValue) : UnknownValue
                })
                .ToListAsync();

            return new PagedResult<AdvancedInventorySearchResult>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = searchModel.PageNumber,
                PageSize = searchModel.PageSize
            };
        }

        public async Task<IEnumerable<AdvancedInventorySearchResult>> SearchInventoryItemsAsync(string searchTerm, int maxResults = 50)
        {
            if (string.IsNullOrEmpty(searchTerm)) return new List<AdvancedInventorySearchResult>();

            var searchModel = new AdvancedInventorySearchModel
            {
                SearchTerm = searchTerm,
                PageSize = maxResults
            };

            var result = await GetInventoryItemsAdvancedAsync(searchModel);
            return result.Items;
        }

        public async Task<IEnumerable<InventoryQuickFilterModel>> GetQuickFiltersAsync()
        {
            var filters = new List<InventoryQuickFilterModel>();

            // Add system filters
            filters.Add(new InventoryQuickFilterModel
            {
                Name = "Low Stock Items",
                Description = "Items at or below reorder level",
                FilterCriteria = new AdvancedInventorySearchModel { ShowLowStockOnly = true },
                ItemCount = await _context.InventoryItems.CountAsync(i => i.Quantity <= i.ReorderLevel),
                IsSystemFilter = true
            });

            filters.Add(new InventoryQuickFilterModel
            {
                Name = "Critical Stock Items",
                Description = "Items at or below minimum stock level",
                FilterCriteria = new AdvancedInventorySearchModel { ShowCriticalStockOnly = true },
                ItemCount = await _context.InventoryItems.CountAsync(i => i.Quantity <= i.MinimumStock),
                IsSystemFilter = true
            });

            filters.Add(new InventoryQuickFilterModel
            {
                Name = "High Value Items",
                Description = "Items with total value over $1000",
                FilterCriteria = new AdvancedInventorySearchModel { MinTotalValue = 1000 },
                ItemCount = await _context.InventoryItems.CountAsync(i => i.TotalValue >= 1000),
                IsSystemFilter = true
            });

            filters.Add(new InventoryQuickFilterModel
            {
                Name = "Expiring Warranty",
                Description = "Items with warranty expiring in 30 days",
                FilterCriteria = new AdvancedInventorySearchModel { ShowExpiringWarrantyOnly = true },
                ItemCount = await _context.InventoryItems.CountAsync(i => 
                    i.WarrantyExpiry.HasValue && 
                    i.WarrantyExpiry.Value >= DateTime.Now && 
                    i.WarrantyExpiry.Value <= DateTime.Now.AddDays(30)),
                IsSystemFilter = true
            });

            return filters;
        }

        public async Task<int> GetInventoryCountAsync(AdvancedInventorySearchModel searchModel)
        {
            var result = await GetInventoryItemsAdvancedAsync(searchModel);
            return result.TotalCount;
        }

        public async Task<BulkOperationResult> ExecuteBulkOperationAsync(BulkInventoryOperationModel operationModel, string userId)
        {
            var result = new BulkOperationResult
            {
                TotalItems = operationModel.SelectedItemIds.Count,
                ExecutedBy = userId
            };

            try
            {
                var items = await _context.InventoryItems
                    .Where(i => operationModel.SelectedItemIds.Contains(i.Id))
                    .ToListAsync();

                switch (operationModel.OperationType)
                {
                    case BulkOperationType.UpdateStatus:
                        await ProcessBulkStatusUpdate(items, operationModel, userId, result);
                        break;
                    case BulkOperationType.UpdateLocation:
                        await ProcessBulkLocationUpdate(items, operationModel, userId, result);
                        break;
                    case BulkOperationType.AdjustStock:
                        await ProcessBulkStockAdjustment(items, operationModel, userId, result);
                        break;
                    default:
                        result.ErrorMessages.Add($"Bulk operation type {operationModel.OperationType} is not implemented");
                        break;
                }

                if (result.SuccessfulItems > 0)
                {
                    await _context.SaveChangesAsync();
                    result.Success = true;
                    result.Summary = $"Successfully processed {result.SuccessfulItems} of {result.TotalItems} items";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing bulk operation");
                result.ErrorMessages.Add($"Unexpected error: {ex.Message}");
            }

            return result;
        }

        // Advanced bulk operations implementation
        public async Task<InventorySearchModels.BulkOperationResult> BulkUpdateInventoryAsync(InventorySearchModels.BulkInventoryUpdateRequest request, string userId)
        {
            var result = new InventorySearchModels.BulkOperationResult();
            
            try
            {
                var items = await _context.InventoryItems
                    .Where(i => request.ItemIds.Contains(i.Id))
                    .ToListAsync();

                if (!items.Any())
                {
                    result.Message = "No items found to update";
                    return result;
                }

                foreach (var item in items)
                {
                    try
                    {
                        switch (request.UpdateType.ToLower())
                        {
                            case "status":
                                if (request.UpdateValue is InventoryStatus status)
                                {
                                    item.Status = status;
                                }
                                break;
                            case "location":
                                if (request.UpdateValue is int locationId)
                                {
                                    item.LocationId = locationId;
                                }
                                break;
                            case "category":
                                if (request.UpdateValue is InventoryCategory category)
                                {
                                    item.Category = category;
                                }
                                break;
                        }

                        item.LastUpdatedDate = DateTime.UtcNow;
                        item.LastUpdatedByUserId = userId;
                        result.AffectedItems++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Error updating item {item.ItemCode}: {ex.Message}");
                    }
                }

                await _context.SaveChangesAsync();
                result.Success = true;
                result.Message = $"Successfully updated {result.AffectedItems} items";
            }
            catch (Exception ex)
            {
                result.Message = $"Bulk update failed: {ex.Message}";
                _logger.LogError(ex, "Bulk update operation failed");
            }

            return result;
        }

        public async Task<byte[]?> ExportInventoryAsync(InventorySearchModels.InventoryExportRequest request)
        {
            try
            {
                // This is a placeholder implementation
                // In a real implementation, you would use libraries like EPPlus for Excel or iTextSharp for PDF
                var items = new List<InventoryItem>();

                if (request.ItemIds?.Any() == true)
                {
                    items = await _context.InventoryItems
                        .Where(i => request.ItemIds.Contains(i.Id))
                        .Include(i => i.Location)
                        .ToListAsync();
                }
                else if (request.SearchCriteria != null)
                {
                    var searchResult = await GetInventoryItemsAdvancedAsync(request.SearchCriteria);
                    // Would need to convert AdvancedInventorySearchResult back to InventoryItem
                    // This is simplified for now
                }

                // For now, return null - implement actual export logic based on request.Format
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Export operation failed");
                return null;
            }
        }

        public async Task<IEnumerable<InventorySearchModels.AdvancedInventorySearchResult>> GetLowStockItemsAsync()
        {
            var items = await _context.InventoryItems
                .Include(i => i.Location)
                .Where(i => i.Quantity <= i.MinimumStock && i.MinimumStock > 0)
                .OrderBy(i => i.Quantity)
                .Take(50)
                .ToListAsync();

            return items.Select(i => new InventorySearchModels.AdvancedInventorySearchResult
            {
                Id = i.Id,
                ItemCode = i.ItemCode,
                Name = i.Name,
                Category = i.Category,
                Quantity = i.Quantity,
                UnitCost = i.UnitCost,
                TotalValue = i.TotalValue,
                LocationName = i.Location?.Name ?? UnknownValue,
                Status = i.Status,
                MinimumStock = i.MinimumStock,
                ReorderLevel = i.ReorderLevel
            });
        }

        public async Task<IEnumerable<InventorySearchModels.AdvancedInventorySearchResult>> GetOutOfStockItemsAsync()
        {
            var items = await _context.InventoryItems
                .Include(i => i.Location)
                .Where(i => i.Quantity <= 0)
                .OrderBy(i => i.LastUpdatedDate)
                .Take(50)
                .ToListAsync();

            return items.Select(i => new InventorySearchModels.AdvancedInventorySearchResult
            {
                Id = i.Id,
                ItemCode = i.ItemCode,
                Name = i.Name,
                Category = i.Category,
                Quantity = i.Quantity,
                UnitCost = i.UnitCost,
                TotalValue = i.TotalValue,
                LocationName = i.Location?.Name ?? UnknownValue,
                Status = i.Status,
                MinimumStock = i.MinimumStock,
                ReorderLevel = i.ReorderLevel
            });
        }

        public async Task<IEnumerable<InventorySearchModels.AdvancedInventorySearchResult>> GetExpiringSoonItemsAsync()
        {
            var cutoffDate = DateTime.UtcNow.AddDays(30); // 30 days from now
            
            var items = await _context.InventoryItems
                .Include(i => i.Location)
                .Where(i => i.WarrantyExpiry.HasValue && i.WarrantyExpiry.Value <= cutoffDate)
                .OrderBy(i => i.WarrantyExpiry)
                .Take(50)
                .ToListAsync();

            return items.Select(i => new InventorySearchModels.AdvancedInventorySearchResult
            {
                Id = i.Id,
                ItemCode = i.ItemCode,
                Name = i.Name,
                Category = i.Category,
                Quantity = i.Quantity,
                UnitCost = i.UnitCost,
                TotalValue = i.TotalValue,
                LocationName = i.Location?.Name ?? UnknownValue,
                Status = i.Status,
                WarrantyExpiry = i.WarrantyExpiry
            });
        }

        public async Task<IEnumerable<InventorySearchModels.AdvancedInventorySearchResult>> GetHighValueItemsAsync()
        {
            var items = await _context.InventoryItems
                .Include(i => i.Location)
                .Where(i => i.TotalValue.HasValue)
                .OrderByDescending(i => i.TotalValue)
                .Take(50)
                .ToListAsync();

            return items.Select(i => new InventorySearchModels.AdvancedInventorySearchResult
            {
                Id = i.Id,
                ItemCode = i.ItemCode,
                Name = i.Name,
                Category = i.Category,
                Quantity = i.Quantity,
                UnitCost = i.UnitCost,
                TotalValue = i.TotalValue,
                LocationName = i.Location?.Name ?? UnknownValue,
                Status = i.Status
            });
        }

        public async Task<IEnumerable<InventorySearchModels.AdvancedInventorySearchResult>> GetRecentlyAddedItemsAsync()
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-7); // Last 7 days
            
            var items = await _context.InventoryItems
                .Include(i => i.Location)
                .Where(i => i.CreatedDate >= cutoffDate)
                .OrderByDescending(i => i.CreatedDate)
                .Take(50)
                .ToListAsync();

            return items.Select(i => new InventorySearchModels.AdvancedInventorySearchResult
            {
                Id = i.Id,
                ItemCode = i.ItemCode,
                Name = i.Name,
                Category = i.Category,
                Quantity = i.Quantity,
                UnitCost = i.UnitCost,
                TotalValue = i.TotalValue,
                LocationName = i.Location?.Name ?? UnknownValue,
                Status = i.Status,
                CreatedDate = i.CreatedDate
            });
        }

        #endregion

        #region Private Helper Methods for Advanced Operations

        private static IQueryable<InventoryItem> ApplySortingAscending(IQueryable<InventoryItem> query, string sortBy)
        {
            return sortBy.ToLower() switch
            {
                "name" => query.OrderBy(i => i.Name),
                "itemcode" => query.OrderBy(i => i.ItemCode),
                "category" => query.OrderBy(i => i.Category),
                "brand" => query.OrderBy(i => i.Brand),
                "model" => query.OrderBy(i => i.Model),
                "quantity" => query.OrderBy(i => i.Quantity),
                "unitcost" => query.OrderBy(i => i.UnitCost),
                "totalvalue" => query.OrderBy(i => i.TotalValue),
                "status" => query.OrderBy(i => i.Status),
                "location" => query.OrderBy(i => i.Location!.Name),
                "createdate" => query.OrderBy(i => i.CreatedDate),
                _ => query.OrderBy(i => i.Name)
            };
        }

        private static IQueryable<InventoryItem> ApplySortingDescending(IQueryable<InventoryItem> query, string sortBy)
        {
            return sortBy.ToLower() switch
            {
                "name" => query.OrderByDescending(i => i.Name),
                "itemcode" => query.OrderByDescending(i => i.ItemCode),
                "category" => query.OrderByDescending(i => i.Category),
                "brand" => query.OrderByDescending(i => i.Brand),
                "model" => query.OrderByDescending(i => i.Model),
                "quantity" => query.OrderByDescending(i => i.Quantity),
                "unitcost" => query.OrderByDescending(i => i.UnitCost),
                "totalvalue" => query.OrderByDescending(i => i.TotalValue),
                "status" => query.OrderByDescending(i => i.Status),
                "location" => query.OrderByDescending(i => i.Location!.Name),
                "createdate" => query.OrderByDescending(i => i.CreatedDate),
                _ => query.OrderByDescending(i => i.Name)
            };
        }

        private static string? GetStorageLocationString(string? zone, string? shelf, string? bin)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(zone)) parts.Add($"Zone: {zone}");
            if (!string.IsNullOrEmpty(shelf)) parts.Add($"Shelf: {shelf}");
            if (!string.IsNullOrEmpty(bin)) parts.Add($"Bin: {bin}");
            return parts.Count > 0 ? string.Join(", ", parts) : null;
        }

        private async Task ProcessBulkStatusUpdate(List<InventoryItem> items, BulkInventoryOperationModel operationModel, string userId, BulkOperationResult result)
        {
            if (!operationModel.NewStatus.HasValue)
            {
                result.ErrorMessages.Add("New status is required for status update operation");
                return;
            }

            foreach (var item in items)
            {
                try
                {
                    var oldStatus = item.Status;
                    item.Status = operationModel.NewStatus.Value;
                    item.LastUpdatedDate = DateTime.UtcNow;
                    item.LastUpdatedByUserId = userId;

                    await _auditService.LogAsync(
                        AuditAction.Update,
                        "InventoryItem",
                        item.Id,
                        userId,
                        $"Bulk status update: {oldStatus} → {item.Status}",
                        new { OldStatus = oldStatus },
                        new { NewStatus = item.Status }
                    );

                    result.SuccessfulItems++;
                }
                catch (Exception ex)
                {
                    result.FailedItems++;
                    result.ErrorMessages.Add($"Failed to update item {item.ItemCode}: {ex.Message}");
                }
            }
        }

        private async Task ProcessBulkLocationUpdate(List<InventoryItem> items, BulkInventoryOperationModel operationModel, string userId, BulkOperationResult result)
        {
            if (!operationModel.NewLocationId.HasValue)
            {
                result.ErrorMessages.Add("New location is required for location update operation");
                return;
            }

            foreach (var item in items)
            {
                try
                {
                    var oldLocationId = item.LocationId;
                    item.LocationId = operationModel.NewLocationId.Value;
                    item.StorageZone = operationModel.NewStorageZone;
                    item.StorageShelf = operationModel.NewStorageShelf;
                    item.LastUpdatedDate = DateTime.UtcNow;
                    item.LastUpdatedByUserId = userId;

                    // Create movement record
                    var movement = new InventoryMovement
                    {
                        InventoryItemId = item.Id,
                        MovementType = InventoryMovementType.Transfer,
                        Quantity = item.Quantity,
                        FromLocationId = oldLocationId,
                        ToLocationId = item.LocationId,
                        FromZone = null, // Would need to track previous zone
                        ToZone = operationModel.NewStorageZone,
                        MovementDate = DateTime.UtcNow,
                        Reason = operationModel.TransferReason ?? "Bulk location update",
                        PerformedByUserId = userId,
                        CreatedDate = DateTime.UtcNow
                    };
                    _context.InventoryMovements.Add(movement);

                    await _auditService.LogAsync(
                        AuditAction.Update,
                        "InventoryItem",
                        item.Id,
                        userId,
                        $"Bulk location update: Location changed to {operationModel.NewLocationId}",
                        new { OldLocationId = oldLocationId },
                        new { NewLocationId = item.LocationId, NewZone = item.StorageZone }
                    );

                    result.SuccessfulItems++;
                }
                catch (Exception ex)
                {
                    result.FailedItems++;
                    result.ErrorMessages.Add($"Failed to update location for item {item.ItemCode}: {ex.Message}");
                }
            }
        }

        private async Task ProcessBulkStockAdjustment(List<InventoryItem> items, BulkInventoryOperationModel operationModel, string userId, BulkOperationResult result)
        {
            if (!operationModel.QuantityAdjustment.HasValue || !operationModel.AdjustmentType.HasValue)
            {
                result.ErrorMessages.Add("Quantity adjustment and adjustment type are required for stock adjustment operation");
                return;
            }

            foreach (var item in items)
            {
                try
                {
                    var oldQuantity = item.Quantity;
                    var adjustment = operationModel.QuantityAdjustment.Value;

                    switch (operationModel.AdjustmentType.Value)
                    {
                        case StockAdjustmentType.Increase:
                            item.Quantity += adjustment;
                            break;
                        case StockAdjustmentType.Decrease:
                            item.Quantity = Math.Max(0, item.Quantity - adjustment);
                            break;
                        case StockAdjustmentType.SetAbsolute:
                            item.Quantity = adjustment;
                            break;
                    }

                    // Recalculate total value
                    if (item.UnitCost.HasValue)
                    {
                        item.TotalValue = item.UnitCost.Value * item.Quantity;
                    }

                    item.LastUpdatedDate = DateTime.UtcNow;
                    item.LastUpdatedByUserId = userId;

                    // Create movement record
                    var movementType = item.Quantity > oldQuantity ? InventoryMovementType.StockIn : InventoryMovementType.StockOut;
                    var movement = new InventoryMovement
                    {
                        InventoryItemId = item.Id,
                        MovementType = movementType,
                        Quantity = Math.Abs(item.Quantity - oldQuantity),
                        MovementDate = DateTime.UtcNow,
                        Reason = operationModel.Reason ?? "Bulk stock adjustment",
                        PerformedByUserId = userId,
                        CreatedDate = DateTime.UtcNow
                    };
                    _context.InventoryMovements.Add(movement);

                    await _auditService.LogAsync(
                        AuditAction.Update,
                        "InventoryItem",
                        item.Id,
                        userId,
                        $"Bulk stock adjustment: {oldQuantity} → {item.Quantity}",
                        new { OldQuantity = oldQuantity },
                        new { NewQuantity = item.Quantity, AdjustmentType = operationModel.AdjustmentType }
                    );

                    result.SuccessfulItems++;
                }
                catch (Exception ex)
                {
                    result.FailedItems++;
                    result.ErrorMessages.Add($"Failed to adjust stock for item {item.ItemCode}: {ex.Message}");
                }
            }
        }

        public async Task<IEnumerable<AdvancedInventorySearchResult>> GetItemsForBulkOperationAsync(List<int> itemIds)
        {
            return await _context.InventoryItems
                .Where(i => itemIds.Contains(i.Id))
                .Include(i => i.Location)
                .Select(i => new AdvancedInventorySearchResult
                {
                    Id = i.Id,
                    ItemCode = i.ItemCode,
                    Name = i.Name,
                    Category = i.Category,
                    Status = i.Status,
                    Quantity = i.Quantity,
                    LocationName = i.Location != null ? i.Location.Name : "Unknown"
                })
                .ToListAsync();
        }

        public async Task<bool> ValidateBulkOperationAsync(BulkInventoryOperationModel operationModel)
        {
            if (operationModel.SelectedItemIds == null || !operationModel.SelectedItemIds.Any())
                return false;

            var itemsExist = await _context.InventoryItems
                .CountAsync(i => operationModel.SelectedItemIds.Contains(i.Id));

            return itemsExist == operationModel.SelectedItemIds.Count;
        }

        // Stub implementations for new interface methods - will be implemented in later phases
        public Task<byte[]> ExportInventoryToExcelAsync(AdvancedInventorySearchModel searchModel, bool includeDetails = true)
        {
            throw new NotImplementedException("Excel export will be implemented in Phase 2");
        }

        public Task<byte[]> ExportInventoryToPdfAsync(AdvancedInventorySearchModel searchModel, bool includeDetails = true)
        {
            throw new NotImplementedException("PDF export will be implemented in Phase 2");
        }

        public Task<byte[]> ExportInventoryToCsvAsync(AdvancedInventorySearchModel searchModel)
        {
            throw new NotImplementedException("CSV export will be implemented in Phase 2");
        }

        public Task<InventoryAnalyticsSummary> GetInventoryAnalyticsSummaryAsync()
        {
            throw new NotImplementedException("Analytics will be implemented in Phase 2");
        }

        public Task<IEnumerable<InventoryTrendData>> GetInventoryTrendsAsync(int months = 12)
        {
            throw new NotImplementedException("Trend analysis will be implemented in Phase 2");
        }

        public Task<IEnumerable<InventoryAlertSummary>> GetInventoryAlertSummaryAsync()
        {
            throw new NotImplementedException("Alert summary will be implemented in Phase 2");
        }

        public Task<bool> TransferInventoryAsync(InventoryTransferRequest transferRequest, string userId)
        {
            throw new NotImplementedException("Enhanced transfer will be implemented in Phase 2");
        }

        public Task<bool> StockInAsync(StockInRequest stockInRequest, string userId)
        {
            throw new NotImplementedException("Enhanced stock-in will be implemented in Phase 2");
        }

        #endregion
    }
}
