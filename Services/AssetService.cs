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
            var excludedStatuses = new[] {
                AssetStatus.Decommissioned,
                AssetStatus.WriteOff,
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
            var query = _context.Assets
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(a => 
                    a.AssetTag.ToLower().Contains(searchTerm) ||
                    a.Brand.ToLower().Contains(searchTerm) ||
                    a.Model.ToLower().Contains(searchTerm) ||
                    a.SerialNumber.ToLower().Contains(searchTerm) ||
                    a.InternalSerialNumber.ToLower().Contains(searchTerm) ||
                    a.Description.ToLower().Contains(searchTerm) ||
                    (a.AssignedToUser != null && (a.AssignedToUser.FirstName + " " + a.AssignedToUser.LastName).ToLower().Contains(searchTerm)) ||
                    (a.Location != null && a.Location.FullLocation.ToLower().Contains(searchTerm)));
            }

            return await query.OrderBy(a => a.AssetTag).ToListAsync();
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
            var existingAsset = await _context.Assets.FindAsync(asset.Id);
            if (existingAsset == null)
            {
                throw new ArgumentException("Asset not found");
            }

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

            // Ensure the entity is being tracked
            _context.Entry(existingAsset).State = EntityState.Modified;
            await _context.SaveChangesAsync();

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
                    a.Status == AssetStatus.Maintenance ||
                    a.MaintenanceRecords.Any(m => 
                        m.NextMaintenanceDate.HasValue && 
                        m.NextMaintenanceDate.Value <= thirtyDaysFromNow))
                .OrderBy(a => a.AssetTag)
                .ToListAsync();
        }

        public async Task<IEnumerable<Asset>> GetExpiredWarrantyAssetsAsync()
        {
            var today = DateTime.Today;
            Console.WriteLine($"GetExpiredWarrantyAssetsAsync: Checking for expired warranties as of {today:yyyy-MM-dd}");
            
            var result = await _context.Assets
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .Where(a => a.WarrantyExpiry.HasValue && a.WarrantyExpiry.Value <= today)
                .OrderBy(a => a.AssetTag)
                .ToListAsync();
                
            Console.WriteLine($"GetExpiredWarrantyAssetsAsync: Found {result.Count} expired warranty assets");
            return result;
        }

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

            asset.Status = AssetStatus.WriteOff;
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

            var documents = GetDocumentList(asset.DocumentPaths);
            documents.Add(documentPath);
            asset.DocumentPaths = System.Text.Json.JsonSerializer.Serialize(documents);
            asset.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

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

            var images = GetImageList(asset.ImagePaths);
            images.Add(imagePath);
            asset.ImagePaths = System.Text.Json.JsonSerializer.Serialize(images);
            asset.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

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
                .Where(a => a.Status == AssetStatus.WriteOff)
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

        public async Task<PagedResult<Asset>> AdvancedSearchAsync(AssetSearchCriteria criteria, int pageNumber, int pageSize)
        {
            return new PagedResult<Asset> { Items = new List<Asset>(), TotalCount = 0, PageNumber = pageNumber, PageSize = pageSize };
        }

        public async Task<AssetHealthDashboard> GetAssetHealthDashboardAsync()
        {
            return new AssetHealthDashboard();
        }

        public async Task<IEnumerable<Asset>> GetAssetsNeedingAttentionAsync()
        {
            return new List<Asset>();
        }

        public async Task<IEnumerable<ApplicationUser>> GetActiveUsersAsync()
        {
            return new List<ApplicationUser>();
        }

        public async Task<byte[]> ExportAssetsToExcelAsync(List<int>? assetIds = null)
        {
            return Array.Empty<byte>();
        }

        public async Task<byte[]> ExportAssetsToCsvAsync(List<int>? assetIds = null)
        {
            return Array.Empty<byte>();
        }

        public async Task<byte[]> ExportAssetsWithFiltersAsync(AssetSearchCriteria criteria, string format = "excel")
        {
            return Array.Empty<byte>();
        }

        public async Task<PagedResult<Asset>> GetAssetsAsync(AssetSearchModel searchModel)
        {
            return new PagedResult<Asset> { Items = new List<Asset>(), TotalCount = 0, PageNumber = 1, PageSize = 10 };
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
    }
}
