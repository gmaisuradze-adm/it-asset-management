using Microsoft.EntityFrameworkCore;
using HospitalAssetTracker.Data;
using HospitalAssetTracker.Models;
using static HospitalAssetTracker.Models.InventorySearchModels;
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
        }

        public async Task<bool> CheckAvailabilityAsync(int inventoryItemId, int requiredQuantity)
        {
            var item = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.Id == inventoryItemId);
            
            if (item == null) return false;
            
            return item.Quantity >= requiredQuantity;
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

        public async Task<IEnumerable<StockLevelAlert>> GetStockLevelAlertsAsync()
        {
            var alerts = new List<StockLevelAlert>();

            // Critical stock alerts (quantity <= minimum stock)
            var criticalItems = await _context.InventoryItems
                .Where(i => i.Quantity <= i.MinimumStock && i.MinimumStock > 0)
                .Include(i => i.Location)
                .Select(i => new StockLevelAlert
                {
                    InventoryItemId = i.Id,
                    ItemCode = i.ItemCode,
                    ItemName = i.Name,
                    Category = i.Category,
                    CurrentStock = i.Quantity,
                    MinimumStock = i.MinimumStock,
                    ReorderLevel = i.ReorderLevel,
                    AlertType = "Critical",
                    LocationName = i.Location != null ? i.Location.FullLocation : "Unknown"
                })
                .ToListAsync();

            alerts.AddRange(criticalItems);

            // Low stock alerts (quantity <= reorder level but > minimum stock)
            var lowStockItems = await _context.InventoryItems
                .Where(i => i.Quantity <= i.ReorderLevel && i.Quantity > i.MinimumStock && i.ReorderLevel > 0)
                .Include(i => i.Location)
                .Select(i => new StockLevelAlert
                {
                    InventoryItemId = i.Id,
                    ItemCode = i.ItemCode,
                    ItemName = i.Name,
                    Category = i.Category,
                    CurrentStock = i.Quantity,
                    MinimumStock = i.MinimumStock,
                    ReorderLevel = i.ReorderLevel,
                    AlertType = "Low",
                    LocationName = i.Location != null ? i.Location.FullLocation : "Unknown"
                })
                .ToListAsync();

            alerts.AddRange(lowStockItems);

            // Zero stock alerts
            var zeroStockItems = await _context.InventoryItems
                .Where(i => i.Quantity == 0)
                .Include(i => i.Location)
                .Select(i => new StockLevelAlert
                {
                    InventoryItemId = i.Id,
                    ItemCode = i.ItemCode,
                    ItemName = i.Name,
                    Category = i.Category,
                    CurrentStock = i.Quantity,
                    MinimumStock = i.MinimumStock,
                    ReorderLevel = i.ReorderLevel,
                    AlertType = "Out of Stock",
                    LocationName = i.Location != null ? i.Location.FullLocation : "Unknown"
                })
                .ToListAsync();

            alerts.AddRange(zeroStockItems);

            return alerts.OrderBy(a => a.CurrentStock).ThenBy(a => a.ItemName);
        }

        public Task<IEnumerable<ExpiryAlert>> GetExpiryAlertsAsync(int daysAhead = 30)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync()
        {
            return await _context.InventoryItems
                .Where(i => i.Quantity <= i.ReorderLevel)
                .Include(i => i.Location)
                .OrderBy(i => i.Quantity)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryItem>> GetCriticalStockItemsAsync()
        {
            return await _context.InventoryItems
                .Where(i => i.Quantity <= i.MinimumStock)
                .Include(i => i.Location)
                .OrderBy(i => i.Quantity)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryItem>> GetOverstockedItemsAsync()
        {
            return await _context.InventoryItems
                .Where(i => i.Quantity > i.MaximumStock)
                .Include(i => i.Location)
                .OrderByDescending(i => i.Quantity)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryItem>> GetExpiredItemsAsync()
        {
            return await _context.InventoryItems
                .Where(i => i.WarrantyExpiry.HasValue && i.WarrantyExpiry <= DateTime.UtcNow)
                .Include(i => i.Location)
                .OrderBy(i => i.WarrantyExpiry)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryItem>> GetItemsNearingExpiryAsync(int daysAhead = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);
            return await _context.InventoryItems
                .Where(i => i.WarrantyExpiry.HasValue && 
                           i.WarrantyExpiry > DateTime.UtcNow && 
                           i.WarrantyExpiry <= cutoffDate)
                .Include(i => i.Location)
                .OrderBy(i => i.WarrantyExpiry)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryItem>> SearchInventoryAsync(string searchTerm)
        {
            var term = searchTerm.ToLower();
            return await _context.InventoryItems
                .Where(i => i.Name.ToLower().Contains(term) ||
                           i.ItemCode.ToLower().Contains(term) ||
                           i.Brand.ToLower().Contains(term) ||
                           i.Model.ToLower().Contains(term) ||
                           (i.SerialNumber != null && i.SerialNumber.ToLower().Contains(term)) ||
                           (i.Description != null && i.Description.ToLower().Contains(term)))
                .Include(i => i.Location)
                .OrderBy(i => i.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryItem>> GetInventoryByLocationAsync(int locationId)
        {
            return await _context.InventoryItems
                .Where(i => i.LocationId == locationId)
                .OrderBy(i => i.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryItem>> GetInventoryByCategoryAsync(InventoryCategory category)
        {
            return await _context.InventoryItems
                .Where(i => i.Category == category)
                .Include(i => i.Location)
                .OrderBy(i => i.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryItem>> GetInventoryByStatusAsync(InventoryStatus status)
        {
            return await _context.InventoryItems
                .Where(i => i.Status == status)
                .Include(i => i.Location)
                .OrderBy(i => i.Name)
                .ToListAsync();
        }

        public Task<IEnumerable<InventoryItem>> GetInventoryBySupplierAsync(string supplier)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<InventoryItem>> GetConsumableItemsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<InventoryItem>> GetItemsRequiringCalibrationAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateStockLevelsAsync(int itemId, int quantity, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ValidateStockAvailabilityAsync(int itemId, int requiredQuantity)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetAvailableStockAsync(int itemId)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetReservedStockAsync(int itemId)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetAllocatedStockAsync(int itemId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> MarkQualityCheckedAsync(int transactionId, bool passed, string notes, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<InventoryTransaction>> GetPendingQualityChecksAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> ScheduleCalibrationAsync(int itemId, DateTime calibrationDate, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CompleteCalibrationAsync(int itemId, DateTime calibrationDate, string certificateNumber, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<InventoryItem>> GetItemsDueForCalibrationAsync(int daysAhead = 30)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateStorageLocationAsync(int itemId, string? zone, string? shelf, string? bin, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<InventoryItem>> GetItemsByStorageLocationAsync(int locationId, string? zone = null, string? shelf = null, string? bin = null)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> ExportInventoryToExcelAsync(InventorySearchCriteria? criteria = null)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> ExportMovementHistoryToExcelAsync(int itemId)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> ExportStockReportToExcelAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> ImportInventoryFromExcelAsync(byte[] fileData, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<int> CleanupOldMovementsAsync(int daysToKeep = 365)
        {
            throw new NotImplementedException();
        }

        public Task<int> CleanupOldTransactionsAsync(int daysToKeep = 365)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RecalculateInventoryValuesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAllTotalValuesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<InventoryDashboardData> GetInventoryDashboardDataAsync()
        {
            var dashboardData = new InventoryDashboardData();

            // Total counts
            dashboardData.TotalItems = await _context.InventoryItems.CountAsync();
            dashboardData.AvailableItems = await _context.InventoryItems.CountAsync(i => i.Status == InventoryStatus.Available);
            dashboardData.LowStockItems = await _context.InventoryItems.CountAsync(i => i.Quantity <= i.MinimumLevel);
            dashboardData.OutOfStockItems = await _context.InventoryItems.CountAsync(i => i.Quantity == 0);
            dashboardData.TotalValue = await _context.InventoryItems.SumAsync(i => (i.Quantity * i.UnitCost) ?? 0);

            // Category breakdown
            dashboardData.CategoryData = await _context.InventoryItems
                .GroupBy(i => i.Category.ToString())
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Category, x => x.Count);

            // Recent movements (last 10)
            var recentMovementsData = await _context.InventoryMovements
                .Include(m => m.InventoryItem)
                .Include(m => m.User)
                .OrderByDescending(m => m.MovementDate)
                .Take(10)
                .ToListAsync();

            dashboardData.RecentMovements = recentMovementsData.Select(m => new InventoryMovementViewModel
            {
                Id = m.Id,
                ItemName = m.InventoryItem.Name,
                MovementType = m.MovementType,
                QuantityChanged = m.Quantity,
                MovementDate = m.MovementDate,
                MovedBy = m.User?.FirstName + " " + m.User?.LastName,
                Reason = m.Reason
            }).ToList();

            // Low stock alerts
            var lowStockItems = await _context.InventoryItems
                .Where(i => i.Quantity <= i.MinimumLevel)
                .OrderBy(i => i.Quantity)
                .Take(10)
                .ToListAsync();

            dashboardData.LowStockAlerts = lowStockItems.Select(i => new InventoryAlertViewModel
            {
                ItemId = i.Id,
                ItemName = i.Name,
                CurrentQuantity = i.Quantity,
                MinimumStock = i.MinimumLevel,
                Category = i.Category.ToString(),
                Status = i.Quantity == 0 ? "Out of Stock" : "Low Stock"
            }).ToList();

            return dashboardData;
        }

        public Task<Dictionary<InventoryCategory, int>> GetInventoryCountByCategoryAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<InventoryStatus, int>> GetInventoryCountByStatusAsync()
        {
            throw new NotImplementedException();
        }

        public Task<decimal> GetTotalInventoryValueAsync()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetTotalInventoryItemsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetUniqueInventoryItemsAsync()
        {
            throw new NotImplementedException();
        }

        #region New Integration Methods - Required by Interface

        public async Task<IEnumerable<Location>> GetAllInventoryLocationsAsync()
        {
            return await _context.Locations
                .Where(l => _context.InventoryItems.Any(i => i.LocationId == l.Id))
                .OrderBy(l => l.Building)
                .ThenBy(l => l.Floor)
                .ThenBy(l => l.Room)
                .ToListAsync();
        }

        public async Task<bool> CheckAvailabilityAndReserveAsync(string itemCode, int quantity, int requestId, string userId)
        {
            var item = await GetInventoryItemByItemCodeAsync(itemCode);
            if (item == null || item.Quantity < quantity)
                return false;

            // Create a reservation record (this could be a new entity or use existing movement system)
            var movement = new InventoryMovement
            {
                InventoryItemId = item.Id,
                MovementType = InventoryMovementType.Out,
                Quantity = quantity,
                MovementDate = DateTime.UtcNow,
                Reason = $"Reserved for Request #{requestId}",
                PerformedByUserId = userId,
                ReferenceNumber = requestId.ToString()
            };

            item.Quantity -= quantity; // Reserve the stock
            _context.InventoryMovements.Add(movement);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Update, "InventoryItem", item.Id, userId, 
                $"Reserved {quantity} units for Request #{requestId}");

            return true;
        }

        public async Task<bool> ReleaseReservedStockForRequestAsync(int requestId, string userId)
        {
            // Find movements related to this request
            var reservationMovements = await _context.InventoryMovements
                .Where(m => m.ReferenceNumber == requestId.ToString() && 
                           m.MovementType == InventoryMovementType.Out &&
                           m.Reason.Contains("Reserved"))
                .ToListAsync();

            foreach (var movement in reservationMovements)
            {
                var item = await GetInventoryItemByIdAsync(movement.InventoryItemId);
                if (item != null)
                {
                    // Create a return movement
                    var returnMovement = new InventoryMovement
                    {
                        InventoryItemId = item.Id,
                        MovementType = InventoryMovementType.In,
                        Quantity = movement.Quantity,
                        MovementDate = DateTime.UtcNow,
                        Reason = $"Released reservation for Request #{requestId}",
                        PerformedByUserId = userId,
                        ReferenceNumber = requestId.ToString()
                    };

                    item.Quantity += movement.Quantity; // Return the stock
                    _context.InventoryMovements.Add(returnMovement);

                    await _auditService.LogAsync(AuditAction.Update, "InventoryItem", item.Id, userId, 
                        $"Released {movement.Quantity} reserved units for Request #{requestId}");
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<InventoryItem?> GetItemBySKUAsync(string sku)
        {
            return await _context.InventoryItems
                .Include(i => i.Location)
                .Include(i => i.CreatedByUser)
                .Include(i => i.LastUpdatedByUser)
                .FirstOrDefaultAsync(i => i.SKU == sku);
        }

        public async Task<bool> ReceiveStockFromProcurementAsync(int inventoryItemId, int procurementActivityId, int quantityReceived, decimal unitCost, DateTime receivedDate, string userId, string? batchNumber = null, DateTime? expiryDate = null)
        {
            var item = await GetInventoryItemByIdAsync(inventoryItemId);
            if (item == null)
                return false;

            // Update inventory quantity
            item.Quantity += quantityReceived;
            item.UnitCost = unitCost;
            item.TotalValue = item.Quantity * unitCost;
            item.LastUpdatedDate = DateTime.UtcNow;
            item.LastUpdatedByUserId = userId;

            // Create movement record
            var movement = new InventoryMovement
            {
                InventoryItemId = inventoryItemId,
                MovementType = InventoryMovementType.In,
                Quantity = quantityReceived,
                MovementDate = receivedDate,
                Reason = $"Received from Procurement Activity #{procurementActivityId}",
                PerformedByUserId = userId,
                ReferenceNumber = procurementActivityId.ToString()
            };

            // Create transaction record
            var transaction = new InventoryTransaction
            {
                InventoryItemId = inventoryItemId,
                TransactionType = InventoryTransactionType.Purchase,
                TransactionDate = receivedDate,
                Quantity = quantityReceived,
                UnitCost = unitCost,
                TotalCost = quantityReceived * unitCost,
                CreatedByUserId = userId,
                BatchNumber = batchNumber,
                ExpiryDate = expiryDate,
                Notes = $"Received from Procurement Activity #{procurementActivityId}"
            };

            _context.InventoryMovements.Add(movement);
            _context.InventoryTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Create, "InventoryItem", inventoryItemId, userId, 
                $"Received {quantityReceived} units from procurement");

            return true;
        }

        public async Task<bool> AssignComponentToAssetAsync(int assetId, int inventoryItemId, int quantity, string? serialNumber, DateTime installationDate, string userId)
        {
            var item = await GetInventoryItemByIdAsync(inventoryItemId);
            var asset = await _context.Assets.FindAsync(assetId);
            
            if (item == null || asset == null || item.Quantity < quantity)
                return false;

            // Create asset-inventory mapping
            var mapping = new AssetInventoryMapping
            {
                AssetId = assetId,
                InventoryItemId = inventoryItemId,
                Quantity = quantity,
                SerialNumber = serialNumber,
                Status = AssetInventoryMappingStatus.Active,
                DeploymentDate = installationDate,
                DeployedByUserId = userId,
                CreatedDate = DateTime.UtcNow
            };

            // Update inventory quantity
            item.Quantity -= quantity;

            // Create movement record
            var movement = new InventoryMovement
            {
                InventoryItemId = inventoryItemId,
                MovementType = InventoryMovementType.Out,
                Quantity = quantity,
                MovementDate = installationDate,
                Reason = $"Assigned to Asset #{asset.AssetTag}",
                PerformedByUserId = userId,
                RelatedAssetId = assetId
            };

            _context.AssetInventoryMappings.Add(mapping);
            _context.InventoryMovements.Add(movement);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Assignment, "AssetInventoryMapping", mapping.Id, userId, 
                $"Assigned {quantity} units of {item.Name} to Asset {asset.AssetTag}");

            return true;
        }

        public async Task<bool> RemoveComponentFromAssetAsync(int assetId, int inventoryItemId, int quantity, DateTime removalDate, string reason, string userId)
        {
            var mapping = await _context.AssetInventoryMappings
                .FirstOrDefaultAsync(m => m.AssetId == assetId && 
                                         m.InventoryItemId == inventoryItemId && 
                                         m.Status == AssetInventoryMappingStatus.Active);
            
            if (mapping == null || mapping.Quantity < quantity)
                return false;

            var item = await GetInventoryItemByIdAsync(inventoryItemId);
            var asset = await _context.Assets.FindAsync(assetId);

            if (item == null || asset == null)
                return false;

            // Update mapping
            if (mapping.Quantity == quantity)
            {
                mapping.Status = AssetInventoryMappingStatus.Removed;
                mapping.ReturnDate = removalDate;
                mapping.ReturnedByUserId = userId;
            }
            else
            {
                mapping.Quantity -= quantity;
            }

            // Return to inventory
            item.Quantity += quantity;

            // Create movement record
            var movement = new InventoryMovement
            {
                InventoryItemId = inventoryItemId,
                MovementType = InventoryMovementType.In,
                Quantity = quantity,
                MovementDate = removalDate,
                Reason = $"Removed from Asset #{asset.AssetTag}: {reason}",
                PerformedByUserId = userId,
                RelatedAssetId = assetId
            };

            _context.InventoryMovements.Add(movement);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Update, "AssetInventoryMapping", mapping.Id, userId, 
                $"Removed {quantity} units of {item.Name} from Asset {asset.AssetTag}");

            return true;
        }

        public async Task<IEnumerable<InventoryItem>> GetCompatibleComponentsAsync(int assetId)
        {
            var asset = await _context.Assets
                .Include(a => a.Location)
                .FirstOrDefaultAsync(a => a.Id == assetId);
            
            if (asset == null)
                return new List<InventoryItem>();

            // This is a simplified implementation - in reality, you'd have more complex compatibility logic
            // based on asset category, model, specifications, etc.
            return await _context.InventoryItems
                .Where(i => i.Status == InventoryStatus.Available && 
                           i.Quantity > 0 &&
                           (i.Category == InventoryCategory.Component || 
                            i.Category == InventoryCategory.Accessory))
                .OrderBy(i => i.Name)
                .ToListAsync();
        }

        #endregion

        public async Task<bool> UpdateInventoryQuantityAsync(int itemId, int newQuantity, string userId)
        {
            try
            {
                var item = await _context.InventoryItems.FindAsync(itemId);
                if (item == null)
                {
                    _logger.LogWarning("Inventory item with ID {ItemId} not found", itemId);
                    return false;
                }

                var oldQuantity = item.Quantity;
                var adjustmentQuantity = newQuantity - oldQuantity;

                item.Quantity = newQuantity;
                if (item.UnitCost.HasValue)
                {
                    item.TotalValue = item.UnitCost.Value * item.Quantity;
                }

                // Create movement record
                var movement = new InventoryMovement
                {
                    InventoryItemId = itemId,
                    MovementType = adjustmentQuantity > 0 ? InventoryMovementType.StockIn : InventoryMovementType.StockOut,
                    Quantity = Math.Abs(adjustmentQuantity),
                    MovementDate = DateTime.UtcNow,
                    Notes = $"Quantity updated from {oldQuantity} to {newQuantity}"
                };

                _context.InventoryMovements.Add(movement);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Inventory quantity updated for item {ItemId} from {OldQuantity} to {NewQuantity} by user {UserId}", 
                    itemId, oldQuantity, newQuantity, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inventory quantity for item {ItemId}", itemId);
                return false;
            }
        }
    }
}
