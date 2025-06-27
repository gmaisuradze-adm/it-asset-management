using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using HospitalAssetTracker.Data;
using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(AuditAction action, string entityType, int? entityId, string userId, 
            string description, object? oldValues = null, object? newValues = null, int? assetId = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            
            var auditLog = new AuditLog
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                Description = description,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString(),
                AssetId = assetId
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(int page = 1, int pageSize = 50)
        {
            return await _context.AuditLogs
                .Include(a => a.User)
                .Include(a => a.Asset)
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetAssetAuditLogsAsync(int assetId)
        {
            return await _context.AuditLogs
                .Include(a => a.User)
                .Where(a => a.AssetId == assetId || (a.EntityType == "Asset" && a.EntityId == assetId))
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetUserAuditLogsAsync(string userId)
        {
            return await _context.AuditLogs
                .Include(a => a.User)
                .Include(a => a.Asset)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> SearchAuditLogsAsync(string searchTerm, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.AuditLogs
                .Include(a => a.User)
                .Include(a => a.Asset)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(a => 
                    (a.Description != null && a.Description.ToLower().Contains(searchTerm)) ||
                    (a.EntityType != null && a.EntityType.ToLower().Contains(searchTerm)) ||
                    (a.User != null && a.User.FirstName != null && a.User.FirstName.ToLower().Contains(searchTerm)) ||
                    (a.User != null && a.User.LastName != null && a.User.LastName.ToLower().Contains(searchTerm)) ||
                    (a.Asset != null && a.Asset.AssetTag != null && a.Asset.AssetTag.ToLower().Contains(searchTerm)));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(a => a.Timestamp <= toDate.Value.AddDays(1));
            }

            return await query
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }
    }
}
