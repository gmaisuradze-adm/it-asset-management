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
                asset.InternalSerialNumber = await GenerateInternalSerialNumberAsync();
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
                AssetTag = await GenerateUniqueAssetTagAsync(sourceAsset.AssetTag),
                Category = sourceAsset.Category,
                Brand = sourceAsset.Brand,
                Model = sourceAsset.Model,
                SerialNumber = "", // Clear serial number for clone
                InternalSerialNumber = await GenerateInternalSerialNumberAsync(),
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

        public async Task<IEnumerable<ApplicationUser>> GetActiveUsersAsync()
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();
        }

        // Private helper methods
        private async Task<string> GenerateInternalSerialNumberAsync()
        {
            var prefix = "HSP";
            var year = DateTime.UtcNow.Year.ToString();
            var counter = await _context.Assets.CountAsync() + 1;
            var serial = $"{prefix}{year}{counter:D6}";
            
            // Ensure uniqueness
            bool isUnique;
            do
            {
                counter++;
                serial = $"{prefix}{year}{counter:D6}";
                
                isUnique = !await _context.Assets.AnyAsync(a => a.InternalSerialNumber == serial);
            } while (!isUnique);

            return serial;
        }

        private async Task<string> GenerateUniqueAssetTagAsync(string baseTag)
        {
            var baseName = baseTag.Split('-')[0];
            var counter = 1;
            string newTag;

            do
            {
                newTag = $"{baseName}-{counter:D3}";
                counter++;
            } while (!await IsAssetTagUniqueAsync(newTag));

            return newTag;
        }

        private List<string> GetDocumentList(string? jsonDocuments)
        {
            if (string.IsNullOrEmpty(jsonDocuments))
                return new List<string>();

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(jsonDocuments) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private List<string> GetImageList(string? jsonImages)
        {
            if (string.IsNullOrEmpty(jsonImages))
                return new List<string>();

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(jsonImages) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<IEnumerable<Asset>> GetWriteOffAssetsAsync()
        {
            var writeOffStatuses = new[] { 
                AssetStatus.WriteOff, 
                AssetStatus.Lost, 
                AssetStatus.Stolen, 
                AssetStatus.Decommissioned 
            };
            
            return await _context.Assets
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .Where(a => writeOffStatuses.Contains(a.Status))
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        // Pagination methods implementation
        public async Task<PagedResult<Asset>> GetAssetsPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, AssetCategory? category = null, AssetStatus? status = null, int? locationId = null)
        {
            var query = _context.Assets
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .AsQueryable();

            // Apply filters
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

            if (category.HasValue)
            {
                query = query.Where(a => a.Category == category.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }
            else
            {
                // Default: exclude inactive assets
                var excludedStatuses = new[] {
                    AssetStatus.Decommissioned,
                    AssetStatus.WriteOff,
                    AssetStatus.Lost,
                    AssetStatus.Stolen
                };
                query = query.Where(a => !excludedStatuses.Contains(a.Status));
            }

            if (locationId.HasValue)
            {
                query = query.Where(a => a.LocationId == locationId.Value);
            }

            query = query.OrderBy(a => a.AssetTag);

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<PagedResult<Asset>> GetActiveAssetsPagedAsync(int pageNumber, int pageSize)
        {
            var excludedStatuses = new[] {
                AssetStatus.Decommissioned,
                AssetStatus.WriteOff,
                AssetStatus.Lost,
                AssetStatus.Stolen
            };

            var query = _context.Assets
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .Where(a => !excludedStatuses.Contains(a.Status))
                .OrderBy(a => a.AssetTag);

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        // Advanced Search
        public async Task<PagedResult<Asset>> AdvancedSearchAsync(AssetSearchCriteria criteria, int pageNumber, int pageSize)
        {
            var query = _context.Assets
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .AsQueryable();

            // Apply advanced search filters
            if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
            {
                var searchTerm = criteria.SearchTerm.ToLower();
                query = query.Where(a => 
                    a.AssetTag.ToLower().Contains(searchTerm) ||
                    a.Brand.ToLower().Contains(searchTerm) ||
                    a.Model.ToLower().Contains(searchTerm) ||
                    a.Description.ToLower().Contains(searchTerm));
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

            if (!string.IsNullOrWhiteSpace(criteria.Department))
            {
                query = query.Where(a => a.Department != null && a.Department.ToLower().Contains(criteria.Department.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(criteria.Supplier))
            {
                query = query.Where(a => a.Supplier != null && a.Supplier.ToLower().Contains(criteria.Supplier.ToLower()));
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
                query = query.Where(a => a.InstallationDate >= criteria.InstallFrom.Value);
            }

            if (criteria.InstallTo.HasValue)
            {
                query = query.Where(a => a.InstallationDate <= criteria.InstallTo.Value);
            }

            // Warranty status filter
            if (!string.IsNullOrWhiteSpace(criteria.WarrantyStatus))
            {
                var now = DateTime.UtcNow;
                var threeMonthsFromNow = now.AddMonths(3);

                switch (criteria.WarrantyStatus.ToLower())
                {
                    case "active":
                        query = query.Where(a => a.WarrantyExpiry.HasValue && a.WarrantyExpiry.Value > now);
                        break;
                    case "expired":
                        query = query.Where(a => a.WarrantyExpiry.HasValue && a.WarrantyExpiry.Value <= now);
                        break;
                    case "expiring":
                        query = query.Where(a => a.WarrantyExpiry.HasValue && a.WarrantyExpiry.Value > now && a.WarrantyExpiry.Value <= threeMonthsFromNow);
                        break;
                    case "none":
                        query = query.Where(a => !a.WarrantyExpiry.HasValue);
                        break;
                }
            }

            query = query.OrderBy(a => a.AssetTag);
            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        // Asset Health Dashboard
        public async Task<AssetHealthDashboard> GetAssetHealthDashboardAsync()
        {
            var now = DateTime.UtcNow;
            var threeMonthsFromNow = now.AddMonths(3);
            var oneYearAgo = now.AddYears(-1);
            var threeYearsAgo = now.AddYears(-3);
            var fiveYearsAgo = now.AddYears(-5);

            var dashboard = new AssetHealthDashboard();

            // Basic counts
            dashboard.TotalAssets = await _context.Assets.CountAsync();
            dashboard.ActiveAssets = await _context.Assets.CountAsync(a => 
                a.Status != AssetStatus.Decommissioned && 
                a.Status != AssetStatus.WriteOff && 
                a.Status != AssetStatus.Lost && 
                a.Status != AssetStatus.Stolen);
            dashboard.InUseAssets = await _context.Assets.CountAsync(a => a.Status == AssetStatus.InUse);
            dashboard.AvailableAssets = await _context.Assets.CountAsync(a => a.Status == AssetStatus.Available);
            dashboard.UnderRepairAssets = await _context.Assets.CountAsync(a => a.Status == AssetStatus.UnderRepair);
            dashboard.MaintenanceAssets = await _context.Assets.CountAsync(a => a.Status == AssetStatus.Maintenance);
            dashboard.DecommissionedAssets = await _context.Assets.CountAsync(a => 
                a.Status == AssetStatus.Decommissioned || 
                a.Status == AssetStatus.WriteOff);

            // Warranty status
            dashboard.ExpiredWarrantyAssets = await _context.Assets.CountAsync(a => 
                a.WarrantyExpiry.HasValue && a.WarrantyExpiry.Value <= now);
            dashboard.ExpiringWarrantyAssets = await _context.Assets.CountAsync(a => 
                a.WarrantyExpiry.HasValue && a.WarrantyExpiry.Value > now && a.WarrantyExpiry.Value <= threeMonthsFromNow);
            dashboard.NoWarrantyAssets = await _context.Assets.CountAsync(a => !a.WarrantyExpiry.HasValue);

            // Financial
            dashboard.TotalValue = await _context.Assets.Where(a => a.PurchasePrice.HasValue).SumAsync(a => a.PurchasePrice.Value);
            dashboard.AverageAssetValue = dashboard.TotalAssets > 0 ? dashboard.TotalValue / dashboard.TotalAssets : 0;

            // Age analysis
            dashboard.AssetsOlderThan5Years = await _context.Assets.CountAsync(a => a.InstallationDate <= fiveYearsAgo);
            dashboard.AssetsOlderThan3Years = await _context.Assets.CountAsync(a => a.InstallationDate <= threeYearsAgo);
            dashboard.AssetsNewerThan1Year = await _context.Assets.CountAsync(a => a.InstallationDate >= oneYearAgo);

            return dashboard;
        }

        // Assets needing attention
        public async Task<IEnumerable<Asset>> GetAssetsNeedingAttentionAsync()
        {
            var now = DateTime.UtcNow;
            var threeMonthsFromNow = now.AddMonths(3);

            return await _context.Assets
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .Where(a => 
                    a.Status == AssetStatus.UnderRepair ||
                    a.Status == AssetStatus.Maintenance ||
                    (a.WarrantyExpiry.HasValue && a.WarrantyExpiry.Value <= threeMonthsFromNow))
                .OrderBy(a => a.WarrantyExpiry)
                .ThenBy(a => a.AssetTag)
                .ToListAsync();
        }

        // Bulk Export methods
        public async Task<byte[]> ExportAssetsToExcelAsync(List<int>? assetIds = null)
        {
            var query = _context.Assets
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .AsQueryable();

            if (assetIds != null && assetIds.Any())
            {
                query = query.Where(a => assetIds.Contains(a.Id));
            }

            var assets = await query.OrderBy(a => a.AssetTag).ToListAsync();

            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Assets");

            // Headers
            worksheet.Cells[1, 1].Value = "Asset Tag";
            worksheet.Cells[1, 2].Value = "Category";
            worksheet.Cells[1, 3].Value = "Brand";
            worksheet.Cells[1, 4].Value = "Model";
            worksheet.Cells[1, 5].Value = "Serial Number";
            worksheet.Cells[1, 6].Value = "Description";
            worksheet.Cells[1, 7].Value = "Status";
            worksheet.Cells[1, 8].Value = "Department";
            worksheet.Cells[1, 9].Value = "Location";
            worksheet.Cells[1, 10].Value = "Assigned To";
            worksheet.Cells[1, 11].Value = "Installation Date";
            worksheet.Cells[1, 12].Value = "Purchase Price";
            worksheet.Cells[1, 13].Value = "Supplier";
            worksheet.Cells[1, 14].Value = "Warranty Expiry";
            worksheet.Cells[1, 15].Value = "Installation Date";
            worksheet.Cells[1, 16].Value = "Notes";

            // Style headers
            using (var range = worksheet.Cells[1, 1, 1, 16])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // Data
            for (int i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                var row = i + 2;

                worksheet.Cells[row, 1].Value = asset.AssetTag;
                worksheet.Cells[row, 2].Value = asset.Category.ToString();
                worksheet.Cells[row, 3].Value = asset.Brand;
                worksheet.Cells[row, 4].Value = asset.Model;
                worksheet.Cells[row, 5].Value = asset.SerialNumber;
                worksheet.Cells[row, 6].Value = asset.Description;
                worksheet.Cells[row, 7].Value = asset.Status.ToString();
                worksheet.Cells[row, 8].Value = asset.Department;
                worksheet.Cells[row, 9].Value = asset.Location?.Building + " - " + asset.Location?.Floor + " - " + asset.Location?.Room;
                worksheet.Cells[row, 10].Value = asset.AssignedToUser?.UserName;
                worksheet.Cells[row, 11].Value = asset.InstallationDate.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 12].Value = asset.PurchasePrice;
                worksheet.Cells[row, 13].Value = asset.Supplier;
                worksheet.Cells[row, 14].Value = asset.WarrantyExpiry?.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 15].Value = asset.InstallationDate.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 16].Value = asset.Notes;
            }

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            return package.GetAsByteArray();
        }

        public async Task<byte[]> ExportAssetsToCsvAsync(List<int>? assetIds = null)
        {
            var query = _context.Assets
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .AsQueryable();

            if (assetIds != null && assetIds.Any())
            {
                query = query.Where(a => assetIds.Contains(a.Id));
            }

            var assets = await query.OrderBy(a => a.AssetTag).ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Asset Tag,Category,Brand,Model,Serial Number,Description,Status,Department,Location,Assigned To,Installation Date,Purchase Price,Supplier,Warranty Expiry,Installation Date,Notes");

            foreach (var asset in assets)
            {
                csv.AppendLine($"\"{asset.AssetTag}\"," +
                              $"\"{asset.Category}\"," +
                              $"\"{asset.Brand}\"," +
                              $"\"{asset.Model}\"," +
                              $"\"{asset.SerialNumber}\"," +
                              $"\"{asset.Description}\"," +
                              $"\"{asset.Status}\"," +
                              $"\"{asset.Department}\"," +
                              $"\"{asset.Location?.Building} - {asset.Location?.Floor} - {asset.Location?.Room}\"," +
                              $"\"{asset.AssignedToUser?.UserName}\"," +
                              $"\"{asset.InstallationDate:yyyy-MM-dd}\"," +
                              $"\"{asset.PurchasePrice}\"," +
                              $"\"{asset.Supplier}\"," +
                              $"\"{asset.WarrantyExpiry?.ToString("yyyy-MM-dd")}\"," +
                              $"\"{asset.InstallationDate:yyyy-MM-dd}\"," +
                              $"\"{asset.Notes}\"");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        public async Task<byte[]> ExportAssetsWithFiltersAsync(AssetSearchCriteria criteria, string format = "excel")
        {
            var result = await AdvancedSearchAsync(criteria, 1, int.MaxValue);
            var assetIds = result.Items.Select(a => a.Id).ToList();

            return format.ToLower() switch
            {
                "csv" => await ExportAssetsToCsvAsync(assetIds),
                _ => await ExportAssetsToExcelAsync(assetIds)
            };
        }

        // Additional methods for Request Service integration
        public async Task<PagedResult<Asset>> GetAssetsAsync(AssetSearchModel searchModel)
        {
            var query = _context.Assets
                .Include(a => a.Location)
                .Include(a => a.AssignedToUser)
                .AsQueryable();

            // Apply filters from search model
            if (!string.IsNullOrWhiteSpace(searchModel.SearchTerm))
            {
                var searchTerm = searchModel.SearchTerm.ToLower();
                query = query.Where(a => 
                    a.AssetTag.ToLower().Contains(searchTerm) ||
                    a.Brand.ToLower().Contains(searchTerm) ||
                    a.Model.ToLower().Contains(searchTerm) ||
                    a.Description.ToLower().Contains(searchTerm));
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

            query = query.OrderBy(a => a.AssetTag);
            return await query.ToPagedResultAsync(1, searchModel.PageSize);
        }

        public async Task<bool> UpdateAssetStatusAsync(int assetId, AssetStatus status, string userId)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null)
            {
                return false;
            }

            var oldStatus = asset.Status;
            asset.Status = status;
            asset.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                AuditAction.StatusChange,
                "Asset",
                assetId,
                userId,
                $"Status changed from {oldStatus} to {status}",
                null,
                null,
                assetId
            );

            return true;
        }

        public async Task<string> GenerateAssetTagAsync()
        {
            var year = DateTime.UtcNow.Year.ToString().Substring(2);
            var prefix = $"AST{year}";
            
            var lastAsset = await _context.Assets
                .Where(a => a.AssetTag.StartsWith(prefix))
                .OrderByDescending(a => a.AssetTag)
                .FirstOrDefaultAsync();

            var nextNumber = 1;
            if (lastAsset != null && lastAsset.AssetTag.Length > prefix.Length)
            {
                var numberPart = lastAsset.AssetTag.Substring(prefix.Length);
                if (int.TryParse(numberPart, out var num))
                {
                    nextNumber = num + 1;
                }
            }

            return $"{prefix}{nextNumber:D4}";
        }
    }
}
