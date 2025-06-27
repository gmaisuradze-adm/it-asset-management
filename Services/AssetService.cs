using Microsoft.EntityFrameworkCore;
using HospitalAssetTracker.Data;
using HospitalAssetTracker.Models;
using System.Text;
using OfficeOpenXml;

namespace HospitalAssetTracker.Services
{
    public class AssetService : IAssetService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly IQRCodeService _qrCodeService;

        public AssetService(ApplicationDbContext context, IAuditService auditService, IQRCodeService qrCodeService)
        {
            _context = context;
            _auditService = auditService;
            _qrCodeService = qrCodeService;
        }

        public async Task<IEnumerable<Asset>> GetAllAssetsAsync()
        {
            return await _context.Assets
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .OrderBy(a => a.AssetTag)
                .ToListAsync();
        }

        public async Task<IEnumerable<Asset>> GetActiveAssetsAsync()
        {
            // Only show assets that are still in use (not decommissioned, written off, lost, or stolen)
            var excludedStatuses = new List<AssetStatus> {
                AssetStatus.Decommissioned,
                AssetStatus.Lost,
                AssetStatus.Stolen
            };

            return await _context.Assets
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .Where(a => !excludedStatuses.Contains(a.Status))
                .OrderBy(a => a.AssetTag)
                .ToListAsync();
        }

        public async Task<Asset?> GetAssetByIdAsync(int id)
        {
            return await _context.Assets
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .Include(a => a.Movements)
                    .ThenInclude(m => m.FromLocation)
                .Include(a => a.Movements)
                    .ThenInclude(m => m.ToLocation)
                .Include(a => a.MaintenanceRecords)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Asset?> GetAssetByTagAsync(string assetTag)
        {
            return await _context.Assets
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .FirstOrDefaultAsync(a => a.AssetTag == assetTag);
        }

        public async Task<IEnumerable<Asset>> SearchAssetsAsync(string searchTerm)
        {
            try
            {
                // DIAGNOSTIC LOG: Track search operations
                await _auditService.LogAsync(AuditAction.Update, "Asset", null, "System", 
                    $"[DIAGNOSTIC] SearchAssetsAsync called with term: '{searchTerm ?? "null"}'");

                var query = _context.Assets
                    .Include(a => a.Location)
                    .Include(a => a.AssignedToUser)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    
                    // DIAGNOSTIC LOG: Check for potential null reference scenarios
                    var assetsWithNullUser = await _context.Assets.CountAsync(a => a.AssignedToUser == null);
                    var assetsWithNullLocation = await _context.Assets.CountAsync(a => a.Location == null);
                    
                    await _auditService.LogAsync(AuditAction.Update, "Asset", null, "System", 
                        $"[DIAGNOSTIC] Assets with null AssignedToUser: {assetsWithNullUser}, null Location: {assetsWithNullLocation}");

                    query = query.Where(a => 
                        a.AssetTag.ToLower().Contains(searchTerm) ||
                        a.Brand.ToLower().Contains(searchTerm) ||
                        a.Model.ToLower().Contains(searchTerm) ||
                        a.SerialNumber.ToLower().Contains(searchTerm) ||
                        a.InternalSerialNumber.ToLower().Contains(searchTerm) ||
                        a.Description.ToLower().Contains(searchTerm) ||
                        (a.AssignedToUser != null && 
                         !string.IsNullOrEmpty(a.AssignedToUser.FirstName) && 
                         !string.IsNullOrEmpty(a.AssignedToUser.LastName) && 
                         (a.AssignedToUser.FirstName + " " + a.AssignedToUser.LastName).ToLower().Contains(searchTerm)) ||
                        (a.Location != null && 
                         !string.IsNullOrEmpty(a.Location.FullLocation) && 
                         a.Location.FullLocation.ToLower().Contains(searchTerm)));
                }

                var results = await query.OrderBy(a => a.AssetTag).ToListAsync();
                
                // DIAGNOSTIC LOG: Track search results
                await _auditService.LogAsync(AuditAction.Update, "Asset", null, "System", 
                    $"[DIAGNOSTIC] SearchAssetsAsync returned {results.Count()} results");

                return results;
            }
            catch (Exception ex)
            {
                // DIAGNOSTIC LOG: Capture any exceptions
                await _auditService.LogAsync(AuditAction.Error, "Asset", null, "System", 
                    $"[DIAGNOSTIC] SearchAssetsAsync exception: {ex.Message} | StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<IEnumerable<Asset>> GetAssetsByLocationAsync(int locationId)
        {
            return await _context.Assets
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .Where(a => a.LocationId == locationId)
                .OrderBy(a => a.AssetTag)
                .ToListAsync();
        }

        public async Task<IEnumerable<Asset>> GetAssetsByUserAsync(string userId)
        {
            return await _context.Assets
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .Where(a => a.AssignedToUserId == userId)
                .OrderBy(a => a.AssetTag)
                .ToListAsync();
        }

        public async Task<IEnumerable<Asset>> GetAssetsByStatusAsync(AssetStatus status)
        {
            return await _context.Assets
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .Where(a => a.Status == status)
                .OrderBy(a => a.AssetTag)
                .ToListAsync();
        }

        public async Task<IEnumerable<Asset>> GetAssetsByCategoryAsync(AssetCategory category)
        {
            return await _context.Assets
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .Where(a => a.Category == category)
                .OrderBy(a => a.AssetTag)
                .ToListAsync();
        }

        public async Task<Asset> CreateAssetAsync(Asset asset, string userId)
        {
            // Generate internal serial number if not provided
            if (string.IsNullOrEmpty(asset.InternalSerialNumber))
            {
                asset.InternalSerialNumber = $"INT-{DateTime.UtcNow.Ticks}";
            }

            // Ensure DateTime kinds are UTC before saving
            // For InstallationDate (non-nullable DateTime)
            asset.InstallationDate = asset.InstallationDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(asset.InstallationDate, DateTimeKind.Utc)
                : asset.InstallationDate.ToUniversalTime();

            // For WarrantyExpiry (nullable DateTime)
            if (asset.WarrantyExpiry.HasValue)
            {
                asset.WarrantyExpiry = asset.WarrantyExpiry.Value.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(asset.WarrantyExpiry.Value, DateTimeKind.Utc)
                    : asset.WarrantyExpiry.Value.ToUniversalTime();
            }
            
            asset.CreatedDate = DateTime.UtcNow; // Already UTC
            asset.LastUpdated = DateTime.UtcNow; // Already UTC

            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();

            // Create audit log
            await _auditService.LogAsync(
                AuditAction.Create,
                "Asset",
                asset.Id,
                userId,
                $"Created asset {asset.AssetTag}",
                null,
                asset
            );

            return asset;
        }

        public async Task<Asset> UpdateAssetAsync(Asset asset, string userId)
        {
            try
            {
                // DIAGNOSTIC LOG: Track update operations for concurrency analysis
                await _auditService.LogAsync(AuditAction.Update, "Asset", asset.Id, userId, 
                    $"[DIAGNOSTIC] UpdateAssetAsync called for Asset ID: {asset.Id}");

                var existingAsset = await _context.Assets.FindAsync(asset.Id);
                if (existingAsset == null)
                {
                    await _auditService.LogAsync(AuditAction.Error, "Asset", asset.Id, userId, 
                        $"[DIAGNOSTIC] UpdateAssetAsync - Asset {asset.Id} not found");
                    throw new ArgumentException("Asset not found");
                }

                // DIAGNOSTIC LOG: Check for potential concurrency conflicts
                var lastUpdatedBefore = existingAsset.LastUpdated;
                await _auditService.LogAsync(AuditAction.Update, "Asset", asset.Id, userId, 
                    $"[DIAGNOSTIC] Asset {asset.Id} LastUpdated before: {lastUpdatedBefore:yyyy-MM-dd HH:mm:ss.fff}");

            // Store original values for audit (using anonymous object to avoid EF tracking issues)
            var originalValues = new
            {
                Id = existingAsset.Id,
                AssetTag = existingAsset.AssetTag,
                Category = existingAsset.Category,
                Brand = existingAsset.Brand,
                Model = existingAsset.Model,
                Status = existingAsset.Status,
                LocationId = existingAsset.LocationId,
                AssignedToUserId = existingAsset.AssignedToUserId
            };

            // Update properties
            existingAsset.AssetTag = asset.AssetTag;
            existingAsset.Category = asset.Category;
            existingAsset.Brand = asset.Brand;
            existingAsset.Model = asset.Model;
            existingAsset.SerialNumber = asset.SerialNumber;
            existingAsset.Description = asset.Description;
            existingAsset.Status = asset.Status;
            existingAsset.LocationId = asset.LocationId;
            existingAsset.AssignedToUserId = asset.AssignedToUserId;
            existingAsset.ResponsiblePerson = asset.ResponsiblePerson;
            existingAsset.Department = asset.Department;
            
            // Ensure DateTime kinds are UTC before saving
            // For InstallationDate (non-nullable DateTime)
            existingAsset.InstallationDate = asset.InstallationDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(asset.InstallationDate, DateTimeKind.Utc)
                : asset.InstallationDate.ToUniversalTime();

            // For WarrantyExpiry (nullable DateTime)
            if (asset.WarrantyExpiry.HasValue)
            {
                existingAsset.WarrantyExpiry = asset.WarrantyExpiry.Value.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(asset.WarrantyExpiry.Value, DateTimeKind.Utc)
                    : asset.WarrantyExpiry.Value.ToUniversalTime();
            }
            else
            {
                existingAsset.WarrantyExpiry = null; 
            }
            
            existingAsset.Supplier = asset.Supplier;
            existingAsset.PurchasePrice = asset.PurchasePrice;
            existingAsset.Notes = asset.Notes;
            existingAsset.LastUpdated = DateTime.UtcNow; // Already UTC

            // Copy the RowVersion for concurrency control
            existingAsset.RowVersion = asset.RowVersion;
            
            // Ensure the entity is being tracked
            _context.Entry(existingAsset).State = EntityState.Modified;
            
            try
            {
                await _context.SaveChangesAsync();
                
                // DIAGNOSTIC LOG: Successful update
                await _auditService.LogAsync(AuditAction.Update, "Asset", asset.Id, userId, 
                    $"[DIAGNOSTIC] Asset {asset.Id} updated successfully. New LastUpdated: {existingAsset.LastUpdated:yyyy-MM-dd HH:mm:ss.fff}");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // DIAGNOSTIC LOG: Concurrency conflict detected
                await _auditService.LogAsync(AuditAction.Error, "Asset", asset.Id, userId, 
                    $"[DIAGNOSTIC] Concurrency conflict detected for Asset {asset.Id}. Another user modified this asset.");
                
                // Handle concurrency conflict
                var entry = ex.Entries.Single();
                var clientValues = (Asset)entry.Entity;
                var databaseEntry = entry.GetDatabaseValues();
                
                if (databaseEntry == null)
                {
                    throw new InvalidOperationException("The asset was deleted by another user.");
                }
                
                var databaseValues = (Asset)databaseEntry.ToObject();
                
                // Log the conflict details for debugging
                await _auditService.LogAsync(AuditAction.Error, "Asset", asset.Id, userId, 
                    $"[DIAGNOSTIC] Conflict details - Client LastUpdated: {clientValues.LastUpdated:yyyy-MM-dd HH:mm:ss.fff}, Database LastUpdated: {databaseValues.LastUpdated:yyyy-MM-dd HH:mm:ss.fff}");
                
                throw new InvalidOperationException($"The asset was modified by another user. Please refresh and try again. Current version: {databaseValues.LastUpdated:yyyy-MM-dd HH:mm:ss}");
            }

            // Create audit log
            await _auditService.LogAsync(
                AuditAction.Update,
                "Asset",
                asset.Id,
                userId,
                $"Updated asset {asset.AssetTag}",
                originalValues,
                existingAsset
            );

            return existingAsset;
            }
            catch (Exception ex)
            {
                // DIAGNOSTIC LOG: Capture concurrency and other update exceptions
                await _auditService.LogAsync(AuditAction.Error, "Asset", asset.Id, userId, 
                    $"[DIAGNOSTIC] UpdateAssetAsync exception: {ex.Message} | StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<bool> DeleteAssetAsync(int id, string userId)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null)
            {
                return false;
            }

            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();

            // Create audit log
            await _auditService.LogAsync(
                AuditAction.Delete,
                "Asset",
                id,
                userId,
                $"Deleted asset {asset.AssetTag}",
                asset,
                null
            );

            return true;
        }

        public async Task<bool> MoveAssetAsync(int assetId, int? newLocationId, string? newUserId, string reason, string performedByUserId)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null)
            {
                return false;
            }

            var oldLocationId = asset.LocationId;
            var oldUserId = asset.AssignedToUserId;

            asset.LocationId = newLocationId;
            asset.AssignedToUserId = newUserId;
            asset.LastUpdated = DateTime.UtcNow;

            // Create movement record
            var movement = new AssetMovement
            {
                AssetId = assetId,
                FromLocationId = oldLocationId,
                ToLocationId = newLocationId,
                FromUserId = oldUserId,
                ToUserId = newUserId,
                MovementDate = DateTime.UtcNow,
                Reason = reason,
                PerformedByUserId = performedByUserId
            };

            _context.AssetMovements.Add(movement);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ChangeAssetStatusAsync(int assetId, AssetStatus newStatus, string reason, string userId)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null)
            {
                return false;
            }

            var oldStatus = asset.Status;
            asset.Status = newStatus;
            asset.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Create audit log
            await _auditService.LogAsync(
                AuditAction.StatusChange,
                "Asset",
                assetId,
                userId,
                $"Changed status from {oldStatus} to {newStatus}. Reason: {reason}",
                null,
                null,
                assetId
            );

            return true;
        }

        public async Task<bool> AssignAssetAsync(int assetId, string userId, string assignedByUserId)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null)
            {
                return false;
            }

            asset.AssignedToUserId = userId;
            asset.Status = AssetStatus.InUse;
            asset.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Create audit log
            await _auditService.LogAsync(
                AuditAction.Assignment,
                "Asset",
                assetId,
                assignedByUserId,
                $"Assigned asset to user {userId}",
                null,
                null,
                assetId
            );

            return true;
        }

        public async Task<bool> UnassignAssetAsync(int assetId, string userId)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null)
            {
                return false;
            }

            asset.AssignedToUserId = null;
            asset.Status = AssetStatus.Available;
            asset.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Create audit log
            await _auditService.LogAsync(
                AuditAction.Assignment,
                "Asset",
                assetId,
                userId,
                "Unassigned asset from user",
                null,
                null,
                assetId
            );

            return true;
        }

        public async Task<IEnumerable<AssetMovement>> GetAssetMovementHistoryAsync(int assetId)
        {
            return await _context.AssetMovements
                .Include(m => m.FromLocation)
                .Include(m => m.ToLocation)
                .Include(m => m.PerformedByUser)
                .Where(m => m.AssetId == assetId)
                .OrderByDescending(m => m.MovementDate)
                .ToListAsync();
        }

        public async Task<bool> IsAssetTagUniqueAsync(string assetTag, int? excludeId = null)
        {
            var query = _context.Assets.AsQueryable();
            
            if (excludeId.HasValue)
                query = query.Where(a => a.Id != excludeId.Value);

            return !await query.AnyAsync(a => a.AssetTag == assetTag);
        }

        public async Task<IEnumerable<Asset>> GetAssetsForMaintenanceAsync()
        {
            var thirtyDaysFromNow = DateTime.UtcNow.AddDays(30);
            
            return await _context.Assets
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .Include(a => a.MaintenanceRecords)
                .Where(a => 
                    a.Status == AssetStatus.UnderMaintenance ||
                    a.MaintenanceRecords.Any(m => 
                        m.NextMaintenanceDate.HasValue && 
                        m.NextMaintenanceDate.Value <= thirtyDaysFromNow))
                .OrderBy(a => a.AssetTag)
                .ToListAsync();
        }

        /* // Removed GetExpiredWarrantyAssetsAsync
        public async Task<IEnumerable<Asset>> GetExpiredWarrantyAssetsAsync()
        {
            var today = DateTime.UtcNow.Date; // Changed from DateTime.Today to DateTime.UtcNow.Date
            
            var result = await _context.Assets
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .Where(a => a.WarrantyExpiry.HasValue && a.WarrantyExpiry.Value.Date <= today) // Ensure comparing Date parts
                .OrderBy(a => a.AssetTag)
                .ToListAsync();
                
            return result;
        }
        */

        // Extended functionality
        public async Task<Asset> CloneAssetAsync(int sourceAssetId, string userId)
        {
            var sourceAsset = await _context.Assets.FindAsync(sourceAssetId);
            if (sourceAsset == null)
            {
                throw new ArgumentException("Source asset not found");
            }

            var clonedAsset = new Asset
            {
                AssetTag = $"CLONE-{DateTime.UtcNow.Ticks}",
                Category = sourceAsset.Category,
                Brand = sourceAsset.Brand,
                Model = sourceAsset.Model,
                SerialNumber = "", // Clear serial number for clone
                InternalSerialNumber = $"INT-{DateTime.UtcNow.Ticks}",
                Description = sourceAsset.Description,
                Status = AssetStatus.Available,
                LocationId = sourceAsset.LocationId,
                ResponsiblePerson = sourceAsset.ResponsiblePerson,
                Department = sourceAsset.Department,
                Supplier = sourceAsset.Supplier,
                PurchasePrice = sourceAsset.PurchasePrice,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            _context.Assets.Add(clonedAsset);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                AuditAction.Create,
                "Asset",
                clonedAsset.Id,
                userId,
                $"Cloned from asset {sourceAsset.AssetTag}",
                null,
                clonedAsset
            );

            return clonedAsset;
        }

        /*
        public async Task<bool> BulkUpdateStatusAsync(List<int> assetIds, AssetStatus newStatus, string reason, string userId)
        {
            var assets = await _context.Assets.Where(a => assetIds.Contains(a.Id)).ToListAsync();
            
            foreach (var asset in assets)
            {
                asset.Status = newStatus;
                asset.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            foreach (var asset in assets)
            {
                await _auditService.LogAsync(
                    AuditAction.Update,
                    "Asset",
                    asset.Id,
                    userId,
                    $"Bulk updated status to {newStatus}. Reason: {reason}",
                    null,
                    null,
                    asset.Id
                );
            }

            return true;
        }

        public async Task<bool> BulkUpdateLocationAsync(List<int> assetIds, int? newLocationId, string reason, string userId)
        {
            var assets = await _context.Assets.Where(a => assetIds.Contains(a.Id)).ToListAsync();
            
            foreach (var asset in assets)
            {
                asset.LocationId = newLocationId;
                asset.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            foreach (var asset in assets)
            {
                await _auditService.LogAsync(
                    AuditAction.Move,
                    "Asset",
                    asset.Id,
                    userId,
                    $"Bulk updated location. Reason: {reason}",
                    null,
                    null,
                    asset.Id
                );
            }

            return true;
        }

        public async Task<bool> BulkAssignAsync(List<int> assetIds, string assignedToUserId, string assignedByUserId)
        {
            var assets = await _context.Assets.Where(a => assetIds.Contains(a.Id)).ToListAsync();
            
            foreach (var asset in assets)
            {
                asset.AssignedToUserId = assignedToUserId;
                asset.Status = AssetStatus.InUse;
                asset.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            foreach (var asset in assets)
            {
                await _auditService.LogAsync(
                    AuditAction.Assignment,
                    "Asset",
                    asset.Id,
                    assignedByUserId,
                    $"Bulk assigned to user {assignedToUserId}",
                    null,
                    null,
                    asset.Id
                );
            }

            return true;
        }
        */

        public async Task<bool> DecommissionAssetAsync(int assetId, string reason, string userId)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null)
            {
                return false;
            }

            asset.Status = AssetStatus.Decommissioned;
            asset.AssignedToUserId = null;
            asset.LocationId = null;
            asset.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                AuditAction.StatusChange,
                "Asset",
                assetId,
                userId,
                $"Decommissioned asset. Reason: {reason}",
                null,
                null,
                assetId
            );

            return true;
        }

        public async Task<bool> WriteOffAssetAsync(int assetId, string reason, string userId)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null)
            {
                return false;
            }

            asset.Status = AssetStatus.Decommissioned;
            asset.AssignedToUserId = null;
            asset.LocationId = null;
            asset.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                AuditAction.StatusChange,
                "Asset",
                assetId,
                userId,
                $"Written off asset. Reason: {reason}",
                null,
                null,
                assetId
            );

            return true;
        }

        public async Task<bool> AttachDocumentAsync(int assetId, string documentPath, string userId)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null)
            {
                return false;
            }

            // The documentPath parameter is assumed to be the relative path already constructed by the controller
            // after saving the file and sanitizing the original filename there.
            // So, no further sanitization of documentPath itself is needed here, as it's system-generated.

            var documents = GetDocumentList(asset.DocumentPaths);
            if (!documents.Contains(documentPath)) // Avoid adding duplicates
            {
                documents.Add(documentPath);
                asset.DocumentPaths = System.Text.Json.JsonSerializer.Serialize(documents);
                asset.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            await _auditService.LogAsync(
                AuditAction.Update,
                "Asset",
                assetId,
                userId,
                $"Attached document: {documentPath}",
                null,
                null,
                assetId
            );

            return true;
        }

        public async Task<bool> AttachImageAsync(int assetId, string imagePath, string userId)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null)
            {
                return false;
            }

            // The imagePath parameter is assumed to be the relative path already constructed by the controller
            // after saving the file and sanitizing the original filename there.
            // So, no further sanitization of imagePath itself is needed here.

            var images = GetImageList(asset.ImagePaths);
            if (!images.Contains(imagePath)) // Avoid adding duplicates
            {
                images.Add(imagePath);
                asset.ImagePaths = System.Text.Json.JsonSerializer.Serialize(images);
                asset.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            await _auditService.LogAsync(
                AuditAction.Update,
                "Asset",
                assetId,
                userId,
                $"Attached image: {imagePath}",
                null,
                null,
                assetId
            );

            return true;
        }

        public async Task<bool> RemoveDocumentAsync(int assetId, string documentPath, string userId)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null)
            {
                return false;
            }

            var documents = GetDocumentList(asset.DocumentPaths);
            documents.Remove(documentPath);
            asset.DocumentPaths = System.Text.Json.JsonSerializer.Serialize(documents);
            asset.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                AuditAction.Update,
                "Asset",
                assetId,
                userId,
                $"Removed document: {documentPath}",
                null,
                null,
                assetId
            );

            return true;
        }

        public async Task<bool> RemoveImageAsync(int assetId, string imagePath, string userId)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null)
            {
                return false;
            }

            var images = GetImageList(asset.ImagePaths);
            images.Remove(imagePath);
            asset.ImagePaths = System.Text.Json.JsonSerializer.Serialize(images);
            asset.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                AuditAction.Update,
                "Asset",
                assetId,
                userId,
                $"Removed image: {imagePath}",
                null,
                null,
                assetId
            );

            return true;
        }

        public async Task<List<string>> GetAssetDocumentsAsync(int assetId)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            return asset != null ? GetDocumentList(asset.DocumentPaths) : new List<string>();
        }

        public async Task<List<string>> GetAssetImagesAsync(int assetId)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            return asset != null ? GetImageList(asset.ImagePaths) : new List<string>();
        }

        public async Task<bool> GenerateAssetQRCodeAsync(int assetId)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null)
            {
                return false;
            }

            // Generate QR code file path instead of Base64 data
            var fileName = $"qr_{asset.AssetTag}_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
            var qrCodePath = await _qrCodeService.SaveQRCodeImageAsync(asset.AssetTag, fileName);
            
            // Store the file path instead of Base64 data (which is too long for 200 char limit)
            asset.QRCodeData = qrCodePath;
            asset.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<byte[]> GetAssetQRCodeAsync(int assetId)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null || string.IsNullOrEmpty(asset.QRCodeData))
            {
                return Array.Empty<byte>();
            }

            try
            {
                // Check if QRCodeData is a file path or Base64 data
                if (asset.QRCodeData.StartsWith("/uploads/") || asset.QRCodeData.StartsWith("uploads/"))
                {
                    // It's a file path, read the file
                    var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var filePath = Path.Combine(webRootPath, asset.QRCodeData.TrimStart('/'));
                    
                    if (File.Exists(filePath))
                    {
                        return await File.ReadAllBytesAsync(filePath);
                    }
                    else
                    {
                        // File doesn't exist, regenerate QR code
                        await GenerateAssetQRCodeAsync(assetId);
                        var updatedAsset = await _context.Assets.FindAsync(assetId);
                        if (updatedAsset != null && !string.IsNullOrEmpty(updatedAsset.QRCodeData))
                        {
                            var newFilePath = Path.Combine(webRootPath, updatedAsset.QRCodeData.TrimStart('/'));
                            if (File.Exists(newFilePath))
                            {
                                return await File.ReadAllBytesAsync(newFilePath);
                            }
                        }
                    }
                }
                else
                {
                    // It's Base64 data (legacy), convert to byte array
                    return Convert.FromBase64String(asset.QRCodeData);
                }
            }
            catch (Exception)
            {
                // If anything fails, return empty array
            }

            return Array.Empty<byte>();
        }

        public async Task<IEnumerable<Location>> GetActiveLocationsAsync()
        {
            return await _context.Locations
                .Where(l => l.IsActive)
                .OrderBy(l => l.Building)
                .ThenBy(l => l.Floor)
                .ThenBy(l => l.Room)
                .ToListAsync();
        }

        public async Task<Location> CreateLocationAsync(Location location, string userId)
        {
            try
            {
                location.CreatedDate = DateTime.UtcNow;
                location.IsActive = true;

                _context.Locations.Add(location);
                await _context.SaveChangesAsync();

                // Log the action
                await _auditService.LogAsync(AuditAction.Create, "Location", location.Id, userId, $"Created location: {location.FullLocation}");

                return location;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating location: {ex.Message}", ex);
            }
        }

        // Missing interface method implementations (stubs)
        public async Task<IEnumerable<Asset>> GetWriteOffAssetsAsync()
        {
            return await _context.Assets
                .Where(a => a.Status == AssetStatus.Decommissioned)
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .ToListAsync();
        }

        public async Task<PagedResult<Asset>> GetAssetsPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, AssetCategory? category = null, AssetStatus? status = null, int? locationId = null)
        {
            var query = _context.Assets.Include(a => a.Location).Include(a => a.AssignedToUser).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(a => a.AssetTag.Contains(searchTerm) || a.Brand.Contains(searchTerm) || a.Model.Contains(searchTerm));
            }

            if (category.HasValue)
                query = query.Where(a => a.Category == category.Value);

            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            if (locationId.HasValue)
                query = query.Where(a => a.LocationId == locationId.Value);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<Asset>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PagedResult<Asset>> GetActiveAssetsPagedAsync(int pageNumber, int pageSize)
        {
            return await GetAssetsPagedAsync(pageNumber, pageSize, null, null, AssetStatus.Available, null);
        }

        /// <summary>
        /// Performs an advanced search for assets based on multiple criteria with pagination.
        /// </summary>
        /// <param name="criteria">The criteria to filter assets by. Includes fields like SearchTerm, Category, Status, LocationId, date ranges, price ranges, etc.</param>
        /// <param name="pageNumber">The page number for pagination (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>A PagedResult containing the list of assets matching the criteria for the specified page, along with total count and pagination details.</returns>
        public async Task<PagedResult<Asset>> AdvancedSearchAsync(AssetSearchCriteria criteria, int pageNumber, int pageSize)
        {
            var query = _context.Assets
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
            {
                var searchTermLower = criteria.SearchTerm.ToLower();
                query = query.Where(a =>
                    (a.AssetTag != null && a.AssetTag.ToLower().Contains(searchTermLower)) ||
                    (a.Brand != null && a.Brand.ToLower().Contains(searchTermLower)) ||
                    (a.Model != null && a.Model.ToLower().Contains(searchTermLower)) ||
                    (a.SerialNumber != null && a.SerialNumber.ToLower().Contains(searchTermLower)) ||
                    (a.InternalSerialNumber != null && a.InternalSerialNumber.ToLower().Contains(searchTermLower)) ||
                    (a.Description != null && a.Description.ToLower().Contains(searchTermLower)) ||
                    (a.AssignedToUser != null && 
                     !string.IsNullOrEmpty(a.AssignedToUser.FirstName) && 
                     !string.IsNullOrEmpty(a.AssignedToUser.LastName) && 
                     (a.AssignedToUser.FirstName + " " + a.AssignedToUser.LastName).ToLower().Contains(searchTermLower)) ||
                    (a.Location != null && 
                     !string.IsNullOrEmpty(a.Location.FullLocation) && 
                     a.Location.FullLocation.ToLower().Contains(searchTermLower)));
            }

            if (criteria.Category.HasValue)
            {
                query = query.Where(a => a.Category == criteria.Category.Value);
            }

            if (criteria.Status.HasValue)
            {
                query = query.Where(a => a.Status == criteria.Status.Value);
            }

            if (criteria.LocationId.HasValue)
            {
                query = query.Where(a => a.LocationId == criteria.LocationId.Value);
            }

            if (!string.IsNullOrWhiteSpace(criteria.AssetTagFrom))
            {
                query = query.Where(a => string.Compare(a.AssetTag, criteria.AssetTagFrom) >= 0);
            }

            if (!string.IsNullOrWhiteSpace(criteria.AssetTagTo))
            {
                query = query.Where(a => string.Compare(a.AssetTag, criteria.AssetTagTo) <= 0);
            }

            if (criteria.PriceFrom.HasValue)
            {
                query = query.Where(a => a.PurchasePrice >= criteria.PriceFrom.Value);
            }

            if (criteria.PriceTo.HasValue)
            {
                query = query.Where(a => a.PurchasePrice <= criteria.PriceTo.Value);
            }

            if (criteria.InstallFrom.HasValue)
            {
                query = query.Where(a => a.InstallationDate >= criteria.InstallFrom.Value.Date);
            }

            if (criteria.InstallTo.HasValue)
            {
                query = query.Where(a => a.InstallationDate < criteria.InstallTo.Value.Date.AddDays(1));
            }

            if (!string.IsNullOrWhiteSpace(criteria.WarrantyStatus))
            {
                var today = DateTime.UtcNow.Date;
                if (criteria.WarrantyStatus.Equals("Expired", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(a => a.WarrantyExpiry.HasValue && a.WarrantyExpiry.Value.Date < today);
                }
                else if (criteria.WarrantyStatus.Equals("Active", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(a => a.WarrantyExpiry.HasValue && a.WarrantyExpiry.Value.Date >= today);
                }
                // Could add "ExpiringSoon" or "None" if needed
            }

            if (!string.IsNullOrWhiteSpace(criteria.Department))
            {
                query = query.Where(a => a.Department != null && a.Department.ToLower().Contains(criteria.Department.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(criteria.Supplier))
            {
                query = query.Where(a => a.Supplier != null && a.Supplier.ToLower().Contains(criteria.Supplier.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(criteria.ResponsiblePerson))
            {
                query = query.Where(a => a.ResponsiblePerson != null && a.ResponsiblePerson.ToLower().Contains(criteria.ResponsiblePerson.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(criteria.AssignedToUserId))
            {
                query = query.Where(a => a.AssignedToUserId == criteria.AssignedToUserId);
            }
            
            // Order before pagination for consistent results
            query = query.OrderBy(a => a.AssetTag);

            var totalCount = await query.CountAsync();
            // Ensure pageNumber and pageSize are valid
            var validPageNumber = pageNumber > 0 ? pageNumber : 1;
            var validPageSize = pageSize > 0 ? pageSize : 10; // Default to 10 if pageSize is invalid

            var items = await query.Skip((validPageNumber - 1) * validPageSize).Take(validPageSize).ToListAsync();

            return new PagedResult<Asset>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = validPageNumber,
                PageSize = validPageSize
            };
        }

        public async Task<AssetHealthDashboard> GetAssetHealthDashboardAsync()
        {
            var today = DateTime.UtcNow.Date;
            var threeMonthsFromNow = today.AddMonths(3);

            // Perform calculations on the database side where possible
            var totalAssets = await _context.Assets.CountAsync();
            var activeAssets = await _context.Assets.CountAsync(a => a.Status == AssetStatus.InUse || a.Status == AssetStatus.Available || a.Status == AssetStatus.Reserved);
            var inUseAssets = await _context.Assets.CountAsync(a => a.Status == AssetStatus.InUse);
            var availableAssets = await _context.Assets.CountAsync(a => a.Status == AssetStatus.Available);
            var underMaintenanceAssets = await _context.Assets.CountAsync(a => a.Status == AssetStatus.UnderMaintenance);
            var decommissionedAssets = await _context.Assets.CountAsync(a => a.Status == AssetStatus.Decommissioned);

            var expiredWarrantyAssets = await _context.Assets.CountAsync(a => a.WarrantyExpiry.HasValue && a.WarrantyExpiry.Value.Date < today);
            var expiringWarrantyAssets = await _context.Assets.CountAsync(a => a.WarrantyExpiry.HasValue && a.WarrantyExpiry.Value.Date >= today && a.WarrantyExpiry.Value.Date < threeMonthsFromNow);
            var noWarrantyAssets = await _context.Assets.CountAsync(a => !a.WarrantyExpiry.HasValue);

            var totalValue = await _context.Assets.SumAsync(a => a.PurchasePrice ?? 0);
            
            var assetsOlderThan5Years = await _context.Assets.CountAsync(a => (today - a.InstallationDate.Date).TotalDays > (365 * 5));
            var assetsOlderThan3Years = await _context.Assets.CountAsync(a => (today - a.InstallationDate.Date).TotalDays > (365 * 3));
            var assetsNewerThan1Year = await _context.Assets.CountAsync(a => (today - a.InstallationDate.Date).TotalDays <= 365);

            var assetsByCategory = await _context.Assets.GroupBy(a => a.Category).ToDictionaryAsync(g => g.Key, g => g.Count());
            var assetsByStatus = await _context.Assets.GroupBy(a => a.Status).ToDictionaryAsync(g => g.Key, g => g.Count());
            
            // AssetsByLocation might still benefit from fetching minimal data if FullLocation is complex or involves many joins not shown
            // For simplicity, keeping the original approach for AssetsByLocation for now, but it could be optimized further if needed.
            var allAssetsForLocationGrouping = await _context.Assets.Include(a => a.Location).Where(a => a.LocationId != null).ToListAsync(); 
            var assetsByLocation = allAssetsForLocationGrouping.GroupBy(a => a.Location!.FullLocation).ToDictionary(g => g.Key, g => g.Count());
            
            var assetsAddedByMonth = await _context.Assets
                .GroupBy(a => new { Year = a.CreatedDate.Year, Month = a.CreatedDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new { Key = $"{g.Key.Year}-{g.Key.Month:D2}", Count = g.Count() })
                .ToDictionaryAsync(g => g.Key, g => g.Count);

            var dashboard = new AssetHealthDashboard
            {
                TotalAssets = totalAssets,
                ActiveAssets = activeAssets,
                InUseAssets = inUseAssets,
                AvailableAssets = availableAssets,
                UnderMaintenanceAssets = underMaintenanceAssets,
                DecommissionedAssets = decommissionedAssets,
                ExpiredWarrantyAssets = expiredWarrantyAssets,
                ExpiringWarrantyAssets = expiringWarrantyAssets,
                NoWarrantyAssets = noWarrantyAssets,
                TotalValue = totalValue,
                AssetsOlderThan5Years = assetsOlderThan5Years,
                AssetsOlderThan3Years = assetsOlderThan3Years,
                AssetsNewerThan1Year = assetsNewerThan1Year,
                AssetsByCategory = assetsByCategory,
                AssetsByStatus = assetsByStatus,
                AssetsByLocation = assetsByLocation,
                AssetsAddedByMonth = assetsAddedByMonth,
                LastUpdated = DateTime.UtcNow
            };
            dashboard.AverageAssetValue = dashboard.TotalAssets > 0 ? dashboard.TotalValue / dashboard.TotalAssets : 0;

            return dashboard;
        }

        public async Task<IEnumerable<Asset>> GetAssetsNeedingAttentionAsync()
        {
            var thirtyDaysFromNow = DateTime.UtcNow.AddDays(30);
            var today = DateTime.UtcNow.Date;

            var assets = await _context.Assets
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .Where(a => 
                    (a.WarrantyExpiry.HasValue && a.WarrantyExpiry.Value <= thirtyDaysFromNow) || // Warranty expiring soon or expired
                    a.Status == AssetStatus.Lost ||
                    a.Status == AssetStatus.Stolen
                )
                .OrderBy(a => a.WarrantyExpiry) // Order by those expiring soonest first, then by status
                .ThenBy(a => a.Status)
                .ToListAsync();
            
            return assets;
        }

        public async Task<IEnumerable<ApplicationUser>> GetActiveUsersAsync()
        {
            // Fetch users from the UserManager or directly from DbContext if appropriate
            // Assuming ApplicationUser is part of the same DbContext or accessible via UserManager
            // For this example, let's assume _context.Users is available and is of type DbSet<ApplicationUser>
            // If ApplicationUser is managed by ASP.NET Core Identity, UserManager might be more appropriate
            // but AssetService might not have a direct dependency on UserManager for this specific query.
            // Let's use _context.Users if it's configured to include ApplicationUser.
            // If not, this would need adjustment based on how ApplicationUser is managed.

            // A common way if ApplicationUser is an IdentityUser and part of the DbContext:
            return await _context.Users.Where(u => u.IsActive).OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToListAsync();
        }

        public async Task<byte[]> ExportAssetsToExcelAsync(List<int>? assetIds = null)
        {
            IEnumerable<Asset> assetsToExport;
            if (assetIds != null && assetIds.Any())
            {
                assetsToExport = await _context.Assets
                    .Where(a => assetIds.Contains(a.Id))
                    .Include(a => a.Location)
                    .Include(a => a.AssignedToUser)
                    .OrderBy(a => a.AssetTag)
                    .ToListAsync();
            }
            else
            {
                // Default to exporting all active (non-decommissioned/lost/stolen) assets if no specific IDs are provided
                assetsToExport = await GetActiveAssetsAsync(); 
            }

            // Set EPPlus license context - TODO: Fix for EPPlus v8+
            // OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial; 

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Assets");

                // Headers
                string[] headers = { 
                    "Asset Tag", "Category", "Brand", "Model", "Serial Number", "Internal Serial Number", 
                    "Description", "Status", "Location", "Assigned To User", "Department", 
                    "Installation Date", "Acquisition Date", "Warranty Expiry", "Purchase Price", "Supplier", "Notes" 
                };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }

                // Data
                int row = 2;
                foreach (var asset in assetsToExport)
                {
                    worksheet.Cells[row, 1].Value = asset.AssetTag;
                    worksheet.Cells[row, 2].Value = asset.Category.ToString();
                    worksheet.Cells[row, 3].Value = asset.Brand;
                    worksheet.Cells[row, 4].Value = asset.Model;
                    worksheet.Cells[row, 5].Value = asset.SerialNumber;
                    worksheet.Cells[row, 6].Value = asset.InternalSerialNumber;
                    worksheet.Cells[row, 7].Value = asset.Description;
                    worksheet.Cells[row, 8].Value = asset.Status.ToString();
                    worksheet.Cells[row, 9].Value = asset.Location?.FullLocation;
                    worksheet.Cells[row, 10].Value = asset.AssignedToUser?.FullName;
                    worksheet.Cells[row, 11].Value = asset.Department;
                    worksheet.Cells[row, 12].Value = asset.InstallationDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 13].Value = asset.AcquisitionDate?.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 14].Value = asset.WarrantyExpiry?.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 15].Value = asset.PurchasePrice;
                    worksheet.Cells[row, 16].Value = asset.Supplier;
                    worksheet.Cells[row, 17].Value = asset.Notes;
                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return await package.GetAsByteArrayAsync();
            }
        }

        public async Task<byte[]> ExportAssetsToCsvAsync(List<int>? assetIds = null)
        {
            IEnumerable<Asset> assetsToExport;
            if (assetIds != null && assetIds.Any())
            {
                assetsToExport = await _context.Assets
                    .Where(a => assetIds.Contains(a.Id))
                    .Include(a => a.Location)
                    .Include(a => a.AssignedToUser)
                    .OrderBy(a => a.AssetTag)
                    .ToListAsync();
            }
            else
            {
                assetsToExport = await GetActiveAssetsAsync();
            }

            var csvBuilder = new StringBuilder();

            // Headers
            string[] headers = { 
                "Asset Tag", "Category", "Brand", "Model", "Serial Number", "Internal Serial Number", 
                "Description", "Status", "Location", "Assigned To User", "Department", 
                "Installation Date", "Acquisition Date", "Warranty Expiry", "Purchase Price", "Supplier", "Notes" 
            };
            csvBuilder.AppendLine(string.Join(",", headers.Select(h => $"\"{h.Replace("\"", "\"\"")}\""))); // Quote headers

            // Data
            foreach (var asset in assetsToExport)
            {
                var values = new string?[]
                {
                    asset.AssetTag,
                    asset.Category.ToString(),
                    asset.Brand,
                    asset.Model,
                    asset.SerialNumber,
                    asset.InternalSerialNumber,
                    asset.Description,
                    asset.Status.ToString(),
                    asset.Location?.FullLocation,
                    asset.AssignedToUser?.FullName,
                    asset.Department,
                    asset.InstallationDate.ToString("yyyy-MM-dd"),
                    asset.AcquisitionDate?.ToString("yyyy-MM-dd"),
                    asset.WarrantyExpiry?.ToString("yyyy-MM-dd"),
                    asset.PurchasePrice?.ToString(),
                    asset.Supplier,
                    asset.Notes
                };
                csvBuilder.AppendLine(string.Join(",", values.Select(v => $"\"{(v ?? string.Empty).Replace("\"", "\"\"")}\""))); // Quote values and handle nulls
            }

            return Encoding.UTF8.GetBytes(csvBuilder.ToString());
        }

        public async Task<byte[]> ExportAssetsWithFiltersAsync(AssetSearchCriteria criteria, string format = "excel")
        {
            // Get all assets matching the criteria (effectively no pagination for export)
            var pagedResult = await AdvancedSearchAsync(criteria, 1, int.MaxValue);
            var assetsToExport = pagedResult.Items;

            if (!assetsToExport.Any())
            {
                // Return empty byte array or handle as an error/empty report
                if (format.Equals("excel", StringComparison.OrdinalIgnoreCase))
                {
                    // Return an empty Excel file - TODO: Fix EPPlus license for v8+
                    // OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                    using (var package = new ExcelPackage())
                    {
                        package.Workbook.Worksheets.Add("Assets"); // Add an empty sheet
                        return await package.GetAsByteArrayAsync();
                    }
                }
                else if (format.Equals("csv", StringComparison.OrdinalIgnoreCase))
                {
                    // Return an empty CSV (just headers perhaps, or completely empty)
                     var csvBuilder = new StringBuilder();
                    string[] headers = { 
                        "Asset Tag", "Category", "Brand", "Model", "Serial Number", "Internal Serial Number", 
                        "Description", "Status", "Location", "Assigned To User", "Department", 
                        "Installation Date", "Acquisition Date", "Warranty Expiry", "Purchase Price", "Supplier", "Notes" 
                    };
                    csvBuilder.AppendLine(string.Join(",", headers.Select(h => $"\"{h.Replace("\"", "\"\"")}\"")));
                    return Encoding.UTF8.GetBytes(csvBuilder.ToString());
                }
                return Array.Empty<byte>();
            }

            var assetIdsToExport = assetsToExport.Select(a => a.Id).ToList();

            if (format.Equals("excel", StringComparison.OrdinalIgnoreCase))
            {
                return await ExportAssetsToExcelAsync(assetIdsToExport);
            }
            else if (format.Equals("csv", StringComparison.OrdinalIgnoreCase))
            {
                return await ExportAssetsToCsvAsync(assetIdsToExport);
            }
            else
            {
                // Unsupported format, return empty or throw exception
                return Array.Empty<byte>();
            }
        }

        public async Task<PagedResult<Asset>> GetAssetsAsync(AssetSearchModel searchModel)
        {
            var query = _context.Assets
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchModel.SearchTerm))
            {
                var searchTermLower = searchModel.SearchTerm.ToLower();
                query = query.Where(a =>
                    (a.AssetTag != null && a.AssetTag.ToLower().Contains(searchTermLower)) ||
                    (a.Brand != null && a.Brand.ToLower().Contains(searchTermLower)) ||
                    (a.Model != null && a.Model.ToLower().Contains(searchTermLower)) ||
                    (a.Description != null && a.Description.ToLower().Contains(searchTermLower)) || // Corrected: Added || operator
                    (a.SerialNumber != null && a.SerialNumber.ToLower().Contains(searchTermLower))
                );
            }

            if (searchModel.Category.HasValue)
            {
                query = query.Where(a => a.Category == searchModel.Category.Value);
            }

            if (searchModel.Status.HasValue)
            {
                query = query.Where(a => a.Status == searchModel.Status.Value);
            }

            if (searchModel.LocationId.HasValue)
            {
                query = query.Where(a => a.LocationId == searchModel.LocationId.Value);
            }
            
            // Add other filters from AssetSearchModel if they are relevant for this specific search method
            // For example, AssetTagFrom/To, PriceFrom/To, InstallFrom/To, etc.
            // For now, keeping it to the most common ones.

            query = query.OrderBy(a => a.AssetTag); // Default ordering

            var totalCount = await query.CountAsync();
            
            var pageNumber = searchModel.PageNumber > 0 ? searchModel.PageNumber : 1;
            var pageSize = searchModel.PageSize > 0 ? searchModel.PageSize : 10;

            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<Asset>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<bool> UpdateAssetStatusAsync(int assetId, AssetStatus status, string userId)
        {
            return await ChangeAssetStatusAsync(assetId, status, "Status updated", userId);
        }

        public async Task<string> GenerateAssetTagAsync()
        {
            // Generate a 7-digit asset tag
            var random = new Random();
            string assetTag;
            
            do
            {
                assetTag = random.Next(1000000, 9999999).ToString();
            }
            while (await _context.Assets.AnyAsync(a => a.AssetTag == assetTag));
            
            return assetTag;
        }

        // Helper methods (stubs)
        private List<string> GetDocumentList(string? documentPaths)
        {
            if (string.IsNullOrEmpty(documentPaths))
                return new List<string>();
            
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(documentPaths) ?? new List<string>();
            }
            catch
            {
                return documentPaths.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }

        private List<string> GetImageList(string? imagePaths)
        {
            if (string.IsNullOrEmpty(imagePaths))
                return new List<string>();
            
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(imagePaths) ?? new List<string>();
            }
            catch
            {
                return imagePaths.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }

        public async Task ScheduleMaintenanceAsync(MaintenanceRecord record, string userId)
        {
            var asset = await _context.Assets.FindAsync(record.AssetId);
            if (asset == null)
            {
                throw new KeyNotFoundException("Asset not found.");
            }

            // Add the maintenance record
            _context.MaintenanceRecords.Add(record);

            // Update asset status
            asset.Status = AssetStatus.UnderMaintenance;

            await _context.SaveChangesAsync();

            // Create audit log
            await _auditService.LogAsync(
                AuditAction.Maintenance,
                "Asset",
                asset.Id,
                userId,
                $"Scheduled '{record.MaintenanceType}' for asset {asset.AssetTag}."
            );
        }

        /*
        public async Task ScheduleBulkMaintenanceAsync(IEnumerable<int> assetIds, MaintenanceRecord record, string userId)
        {
            var assets = await _context.Assets.Where(a => assetIds.Contains(a.Id)).ToListAsync();
            if (!assets.Any())
            {
                throw new KeyNotFoundException("No valid assets found.");
            }

            foreach (var asset in assets)
            {
                var newRecord = new MaintenanceRecord
                {
                    AssetId = asset.Id,
                    MaintenanceType = record.MaintenanceType,
                    Title = record.Title,
                    Description = record.Description,
                    ScheduledDate = record.ScheduledDate,
                    Status = MaintenanceStatus.Scheduled
                };
                _context.MaintenanceRecords.Add(newRecord);

                asset.Status = AssetStatus.UnderMaintenance;
            }

            await _context.SaveChangesAsync();

            foreach (var asset in assets)
            {
                await _auditService.LogAsync(
                    AuditAction.Maintenance,
                    "Asset",
                    asset.Id,
                    userId,
                    $"Scheduled '{record.MaintenanceType}' for asset {asset.AssetTag} as part of bulk operation."
                );
            }
        }
        */

        public async Task<Asset?> GetLatestAssetByPrefixAsync(string prefix)
        {
            return await _context.Assets
                .Where(a => a.AssetTag.StartsWith(prefix + "-"))
                .OrderByDescending(a => a.AssetTag)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> AssetTagExistsAsync(string assetTag)
        {
            return await _context.Assets.AnyAsync(a => a.AssetTag == assetTag);
        }

        public async Task<Location> CreateLocationAsync(string name, string? description = null, 
            string? building = null, string? floor = null, string? room = null)
        {
            // Since Location doesn't have a Name property, use the building and room
            var location = new Location
            {
                Building = building ?? name,
                Room = room ?? "General",
                Floor = floor,
                Description = description,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            
            _context.Locations.Add(location);
            await _context.SaveChangesAsync();
            
            // Log the creation
            await _auditService.LogAsync(AuditAction.Create, "Location", location.Id, 
                "System", $"Created location: {location.FullLocation}");
            
            return location;
        }

        // Advanced Search and Management methods
        public async Task<AssetSearchResult> AdvancedSearchAsync(AdvancedAssetSearchModel searchModel)
        {
            var query = _context.Assets
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .AsQueryable();

            // Apply search filters
            if (!string.IsNullOrWhiteSpace(searchModel.SearchTerm))
            {
                var searchTerm = searchModel.SearchTerm.ToLower();
                query = query.Where(a => 
                    (searchModel.SearchInAssetTag && a.AssetTag.ToLower().Contains(searchTerm)) ||
                    (searchModel.SearchInBrand && a.Brand.ToLower().Contains(searchTerm)) ||
                    (searchModel.SearchInModel && a.Model.ToLower().Contains(searchTerm)) ||
                    (searchModel.SearchInSerialNumber && !string.IsNullOrEmpty(a.SerialNumber) && a.SerialNumber.ToLower().Contains(searchTerm)) ||
                    (searchModel.SearchInDescription && !string.IsNullOrEmpty(a.Description) && a.Description.ToLower().Contains(searchTerm))
                );
            }

            // Category filters
            if (searchModel.Categories != null && searchModel.Categories.Any())
            {
                query = query.Where(a => searchModel.Categories.Contains(a.Category));
            }

            // Status filters
            if (searchModel.Statuses != null && searchModel.Statuses.Any())
            {
                query = query.Where(a => searchModel.Statuses.Contains(a.Status));
            }

            // Location filters
            if (searchModel.LocationIds != null && searchModel.LocationIds.Any())
            {
                query = query.Where(a => a.LocationId.HasValue && searchModel.LocationIds.Contains(a.LocationId.Value));
            }

            // Date range filters
            if (searchModel.PurchaseDateFrom.HasValue)
            {
                query = query.Where(a => a.AcquisitionDate >= searchModel.PurchaseDateFrom.Value);
            }
            
            if (searchModel.PurchaseDateTo.HasValue)
            {
                query = query.Where(a => a.AcquisitionDate <= searchModel.PurchaseDateTo.Value);
            }

            // Price range filters
            if (searchModel.PriceFrom.HasValue)
            {
                query = query.Where(a => a.PurchasePrice >= searchModel.PriceFrom.Value);
            }
            
            if (searchModel.PriceTo.HasValue)
            {
                query = query.Where(a => a.PurchasePrice <= searchModel.PriceTo.Value);
            }

            // Assignment filters
            if (searchModel.IsAssigned.HasValue)
            {
                if (searchModel.IsAssigned.Value)
                {
                    query = query.Where(a => !string.IsNullOrEmpty(a.AssignedToUserId));
                }
                else
                {
                    query = query.Where(a => string.IsNullOrEmpty(a.AssignedToUserId));
                }
            }

            // Warranty filters
            if (searchModel.WarrantyExpired.HasValue)
            {
                var today = DateTime.Today;
                if (searchModel.WarrantyExpired.Value)
                {
                    query = query.Where(a => a.WarrantyExpiry.HasValue && a.WarrantyExpiry.Value < today);
                }
                else
                {
                    query = query.Where(a => !a.WarrantyExpiry.HasValue || a.WarrantyExpiry.Value >= today);
                }
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();
            
            // Apply sorting
            query = searchModel.SortDirection.ToLower() == "desc" 
                ? ApplySortingDescending(query, searchModel.SortBy)
                : ApplySortingAscending(query, searchModel.SortBy);

            // Apply pagination
            var assets = await query
                .Skip((searchModel.Page - 1) * searchModel.PageSize)
                .Take(searchModel.PageSize)
                .ToListAsync();

            // Generate statistics
            var allAssets = await _context.Assets.ToListAsync();
            var categoryCounts = allAssets.GroupBy(a => a.Category).ToDictionary(g => g.Key, g => g.Count());
            var statusCounts = allAssets.GroupBy(a => a.Status).ToDictionary(g => g.Key, g => g.Count());
            var locationCounts = allAssets.Where(a => a.Location != null)
                .GroupBy(a => a.Location!.FullLocation).ToDictionary(g => g.Key, g => g.Count());

            return new AssetSearchResult
            {
                Assets = assets,
                TotalCount = allAssets.Count,
                FilteredCount = totalCount,
                Page = searchModel.Page,
                PageSize = searchModel.PageSize,
                CategoryCounts = categoryCounts,
                StatusCounts = statusCounts,
                LocationCounts = locationCounts,
                TotalValue = assets.Where(a => a.PurchasePrice.HasValue).Sum(a => a.PurchasePrice!.Value),
                WarrantyExpiringCount = allAssets.Count(a => a.WarrantyExpiry.HasValue && 
                    a.WarrantyExpiry.Value >= DateTime.Today && 
                    a.WarrantyExpiry.Value <= DateTime.Today.AddDays(90)),
                UnassignedCount = allAssets.Count(a => string.IsNullOrEmpty(a.AssignedToUserId))
            };
        }

        public async Task<HospitalAssetTracker.Models.BulkOperationResult> ProcessBulkOperationAsync(BulkOperationModel operationModel)
        {
            var result = new HospitalAssetTracker.Models.BulkOperationResult
            {
                ProcessedCount = operationModel.AssetIds.Count
            };

            try
            {
                switch (operationModel.Operation.ToLower())
                {
                    case "status":
                        result = await ProcessBulkStatusUpdate(operationModel);
                        break;
                    case "location":
                        result = await ProcessBulkLocationUpdate(operationModel);
                        break;
                    case "assign":
                        result = await ProcessBulkAssignment(operationModel);
                        break;
                    case "delete":
                        result = await ProcessBulkDelete(operationModel);
                        break;
                    default:
                        result.Success = false;
                        result.Message = "Unknown operation type";
                        break;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error processing bulk operation: {ex.Message}";
            }

            return result;
        }

        public async Task<Microsoft.AspNetCore.Mvc.FileResult?> ExportAssetsAsync(AssetExportModel exportModel)
        {
            try
            {
                var searchModel = exportModel.SearchCriteria ?? new AdvancedAssetSearchModel();
                searchModel.PageSize = int.MaxValue; // Get all results for export
                searchModel.Page = 1;
                
                var searchResult = await AdvancedSearchAsync(searchModel);
                var assets = searchResult.Assets;

                switch (exportModel.Format.ToLower())
                {
                    case "excel":
                        return ExportToExcel(assets, exportModel);
                    case "csv":
                        return ExportToCsv(assets, exportModel);
                    case "pdf":
                        return ExportToPdf(assets, exportModel);
                    default:
                        return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<AssetComparisonModel> CompareAssetsAsync(List<int> assetIds)
        {
            var assets = await _context.Assets
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .Where(a => assetIds.Contains(a.Id))
                .ToListAsync();

            var comparison = new AssetComparisonModel
            {
                Assets = assets
            };

            // Build comparison data
            var properties = new[] { "AssetTag", "Category", "Brand", "Model", "SerialNumber", "Status", 
                "Location", "AssignedTo", "PurchasePrice", "WarrantyExpiry", "AcquisitionDate", 
                "InstallationDate", "Description", "Department", "Supplier" };

            foreach (var property in properties)
            {
                var values = new List<object?>();
                foreach (var asset in assets)
                {
                    values.Add(GetPropertyValue(asset, property));
                }
                comparison.ComparisonData[property] = values;

                // Check if all values are the same
                if (values.Distinct().Count() > 1)
                {
                    comparison.DifferentFields.Add(property);
                }
            }

            return comparison;
        }

        public async Task<List<object>> GetSearchSuggestionsAsync(string term, string type = "all")
        {
            var suggestions = new List<object>();
            var searchTerm = term.ToLower();

            if (type == "all" || type == "assettag")
            {
                var assetTags = await _context.Assets
                    .Where(a => a.AssetTag.ToLower().Contains(searchTerm))
                    .Select(a => new { value = a.AssetTag, type = "Asset Tag" })
                    .Take(10)
                    .ToListAsync();
                suggestions.AddRange(assetTags);
            }

            if (type == "all" || type == "brand")
            {
                var brands = await _context.Assets
                    .Where(a => a.Brand.ToLower().Contains(searchTerm))
                    .Select(a => a.Brand)
                    .Distinct()
                    .Take(10)
                    .Select(b => new { value = b, type = "Brand" })
                    .ToListAsync();
                suggestions.AddRange(brands);
            }

            if (type == "all" || type == "model")
            {
                var models = await _context.Assets
                    .Where(a => a.Model.ToLower().Contains(searchTerm))
                    .Select(a => a.Model)
                    .Distinct()
                    .Take(10)
                    .Select(m => new { value = m, type = "Model" })
                    .ToListAsync();
                suggestions.AddRange(models);
            }

            return suggestions.Cast<object>().ToList();
        }

        public async Task<List<string>> GetDepartmentsAsync()
        {
            return await _context.Assets
                .Where(a => !string.IsNullOrEmpty(a.Department))
                .Select(a => a.Department!)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();
        }

        public async Task<List<string>> GetSuppliersAsync()
        {
            return await _context.Assets
                .Where(a => !string.IsNullOrEmpty(a.Supplier))
                .Select(a => a.Supplier!)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
        }

        // Helper methods for advanced search
        private IQueryable<Asset> ApplySortingAscending(IQueryable<Asset> query, string sortBy)
        {
            return sortBy.ToLower() switch
            {
                "assettag" => query.OrderBy(a => a.AssetTag),
                "category" => query.OrderBy(a => a.Category),
                "brand" => query.OrderBy(a => a.Brand),
                "model" => query.OrderBy(a => a.Model),
                "status" => query.OrderBy(a => a.Status),
                "location" => query.OrderBy(a => a.Location!.FullLocation),
                "purchaseprice" => query.OrderBy(a => a.PurchasePrice),
                "warrantyexpiry" => query.OrderBy(a => a.WarrantyExpiry),
                _ => query.OrderBy(a => a.AssetTag)
            };
        }

        private IQueryable<Asset> ApplySortingDescending(IQueryable<Asset> query, string sortBy)
        {
            return sortBy.ToLower() switch
            {
                "assettag" => query.OrderByDescending(a => a.AssetTag),
                "category" => query.OrderByDescending(a => a.Category),
                "brand" => query.OrderByDescending(a => a.Brand),
                "model" => query.OrderByDescending(a => a.Model),
                "status" => query.OrderByDescending(a => a.Status),
                "location" => query.OrderByDescending(a => a.Location!.FullLocation),
                "purchaseprice" => query.OrderByDescending(a => a.PurchasePrice),
                "warrantyexpiry" => query.OrderByDescending(a => a.WarrantyExpiry),
                _ => query.OrderByDescending(a => a.AssetTag)
            };
        }

        private object? GetPropertyValue(Asset asset, string propertyName)
        {
            return propertyName.ToLower() switch
            {
                "assettag" => asset.AssetTag,
                "category" => asset.Category.ToString(),
                "brand" => asset.Brand,
                "model" => asset.Model,
                "serialnumber" => asset.SerialNumber,
                "status" => asset.Status.ToString(),
                "location" => asset.Location?.FullLocation,
                "assignedto" => asset.AssignedToUser?.UserName,
                "purchaseprice" => asset.PurchasePrice?.ToString("C"),
                "warrantyexpiry" => asset.WarrantyExpiry?.ToString("MMM dd, yyyy"),
                "acquisitiondate" => asset.AcquisitionDate?.ToString("MMM dd, yyyy"),
                "installationdate" => asset.InstallationDate.ToString("MMM dd, yyyy"),
                "description" => asset.Description,
                "department" => asset.Department,
                "supplier" => asset.Supplier,
                _ => null
            };
        }

        // Bulk operation helper methods
        private async Task<HospitalAssetTracker.Models.BulkOperationResult> ProcessBulkStatusUpdate(BulkOperationModel operationModel)
        {
            var result = new HospitalAssetTracker.Models.BulkOperationResult { ProcessedCount = operationModel.AssetIds.Count };
            
            if (!operationModel.Parameters.ContainsKey("status"))
            {
                result.Success = false;
                result.Message = "Status parameter is required";
                return result;
            }

            if (!Enum.TryParse<AssetStatus>(operationModel.Parameters["status"].ToString(), out var newStatus))
            {
                result.Success = false;
                result.Message = "Invalid status value";
                return result;
            }

            foreach (var assetId in operationModel.AssetIds)
            {
                try
                {
                    var asset = await _context.Assets.FindAsync(assetId);
                    if (asset != null)
                    {
                        asset.Status = newStatus;
                        asset.LastUpdated = DateTime.UtcNow;
                        result.SuccessCount++;
                        
                        result.Results.Add(new BulkOperationItem 
                        { 
                            AssetId = assetId, 
                            AssetTag = asset.AssetTag, 
                            Success = true 
                        });
                    }
                    else
                    {
                        result.FailureCount++;
                        result.Errors.Add($"Asset {assetId} not found");
                    }
                }
                catch (Exception ex)
                {
                    result.FailureCount++;
                    result.Errors.Add($"Error updating asset {assetId}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            result.Success = result.SuccessCount > 0;
            result.Message = $"Updated {result.SuccessCount} assets successfully";
            
            return result;
        }

        private async Task<HospitalAssetTracker.Models.BulkOperationResult> ProcessBulkLocationUpdate(BulkOperationModel operationModel)
        {
            var result = new HospitalAssetTracker.Models.BulkOperationResult { ProcessedCount = operationModel.AssetIds.Count };
            
            if (!operationModel.Parameters.ContainsKey("locationId"))
            {
                result.Success = false;
                result.Message = "Location ID parameter is required";
                return result;
            }

            if (!int.TryParse(operationModel.Parameters["locationId"].ToString(), out var locationId))
            {
                result.Success = false;
                result.Message = "Invalid location ID";
                return result;
            }

            foreach (var assetId in operationModel.AssetIds)
            {
                try
                {
                    var asset = await _context.Assets.FindAsync(assetId);
                    if (asset != null)
                    {
                        asset.LocationId = locationId;
                        asset.LastUpdated = DateTime.UtcNow;
                        result.SuccessCount++;
                        
                        result.Results.Add(new BulkOperationItem 
                        { 
                            AssetId = assetId, 
                            AssetTag = asset.AssetTag, 
                            Success = true 
                        });
                    }
                    else
                    {
                        result.FailureCount++;
                        result.Errors.Add($"Asset {assetId} not found");
                    }
                }
                catch (Exception ex)
                {
                    result.FailureCount++;
                    result.Errors.Add($"Error updating asset {assetId}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            result.Success = result.SuccessCount > 0;
            result.Message = $"Updated location for {result.SuccessCount} assets successfully";
            
            return result;
        }

        private async Task<HospitalAssetTracker.Models.BulkOperationResult> ProcessBulkAssignment(BulkOperationModel operationModel)
        {
            var result = new HospitalAssetTracker.Models.BulkOperationResult { ProcessedCount = operationModel.AssetIds.Count };
            
            if (!operationModel.Parameters.ContainsKey("userId"))
            {
                result.Success = false;
                result.Message = "User ID parameter is required";
                return result;
            }

            var userId = operationModel.Parameters["userId"].ToString();

            foreach (var assetId in operationModel.AssetIds)
            {
                try
                {
                    var asset = await _context.Assets.FindAsync(assetId);
                    if (asset != null)
                    {
                        asset.AssignedToUserId = userId;
                        asset.Status = AssetStatus.InUse;
                        asset.LastUpdated = DateTime.UtcNow;
                        result.SuccessCount++;
                        
                        result.Results.Add(new BulkOperationItem 
                        { 
                            AssetId = assetId, 
                            AssetTag = asset.AssetTag, 
                            Success = true 
                        });
                    }
                    else
                    {
                        result.FailureCount++;
                        result.Errors.Add($"Asset {assetId} not found");
                    }
                }
                catch (Exception ex)
                {
                    result.FailureCount++;
                    result.Errors.Add($"Error assigning asset {assetId}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            result.Success = result.SuccessCount > 0;
            result.Message = $"Assigned {result.SuccessCount} assets successfully";
            
            return result;
        }

        private async Task<HospitalAssetTracker.Models.BulkOperationResult> ProcessBulkDelete(BulkOperationModel operationModel)
        {
            var result = new HospitalAssetTracker.Models.BulkOperationResult { ProcessedCount = operationModel.AssetIds.Count };

            foreach (var assetId in operationModel.AssetIds)
            {
                try
                {
                    var asset = await _context.Assets.FindAsync(assetId);
                    if (asset != null)
                    {
                        _context.Assets.Remove(asset);
                        result.SuccessCount++;
                        
                        result.Results.Add(new BulkOperationItem 
                        { 
                            AssetId = assetId, 
                            AssetTag = asset.AssetTag, 
                            Success = true 
                        });
                    }
                    else
                    {
                        result.FailureCount++;
                        result.Errors.Add($"Asset {assetId} not found");
                    }
                }
                catch (Exception ex)
                {
                    result.FailureCount++;
                    result.Errors.Add($"Error deleting asset {assetId}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            result.Success = result.SuccessCount > 0;
            result.Message = $"Deleted {result.SuccessCount} assets successfully";
            
            return result;
        }

        // Export helper methods
        private Microsoft.AspNetCore.Mvc.FileResult ExportToExcel(List<Asset> assets, AssetExportModel exportModel)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Assets");

            // Add headers
            var headers = exportModel.Columns.Any() ? exportModel.Columns : 
                new List<string> { "AssetTag", "Category", "Brand", "Model", "Status", "Location", "AssignedTo", "PurchasePrice" };
            
            for (int i = 0; i < headers.Count; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
            }

            // Add data
            for (int row = 0; row < assets.Count; row++)
            {
                var asset = assets[row];
                for (int col = 0; col < headers.Count; col++)
                {
                    worksheet.Cells[row + 2, col + 1].Value = GetPropertyValue(asset, headers[col]);
                }
            }

            var fileBytes = package.GetAsByteArray();
            var fileName = $"assets_export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            
            return new Microsoft.AspNetCore.Mvc.FileContentResult(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = fileName
            };
        }

        private Microsoft.AspNetCore.Mvc.FileResult ExportToCsv(List<Asset> assets, AssetExportModel exportModel)
        {
            var csv = new StringBuilder();
            
            // Add headers
            var headers = exportModel.Columns.Any() ? exportModel.Columns : 
                new List<string> { "AssetTag", "Category", "Brand", "Model", "Status", "Location", "AssignedTo", "PurchasePrice" };
            
            csv.AppendLine(string.Join(",", headers));

            // Add data
            foreach (var asset in assets)
            {
                var values = headers.Select(h => GetPropertyValue(asset, h)?.ToString() ?? "").ToArray();
                csv.AppendLine(string.Join(",", values.Select(v => $"\"{v}\"")));
            }

            var fileBytes = Encoding.UTF8.GetBytes(csv.ToString());
            var fileName = $"assets_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            
            return new Microsoft.AspNetCore.Mvc.FileContentResult(fileBytes, "text/csv")
            {
                FileDownloadName = fileName
            };
        }

        private Microsoft.AspNetCore.Mvc.FileResult ExportToPdf(List<Asset> assets, AssetExportModel exportModel)
        {
            // For now, return CSV format - PDF export would require additional libraries
            return ExportToCsv(assets, exportModel);
        }
    }
}
