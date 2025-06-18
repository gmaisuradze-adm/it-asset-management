using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Services
{
    public interface IAuditService
    {
        Task LogAsync(AuditAction action, string entityType, int? entityId, string userId, 
            string description, object? oldValues = null, object? newValues = null, int? assetId = null);
        Task<IEnumerable<AuditLog>> GetAuditLogsAsync(int page = 1, int pageSize = 50);
        Task<IEnumerable<AuditLog>> GetAssetAuditLogsAsync(int assetId);
        Task<IEnumerable<AuditLog>> GetUserAuditLogsAsync(string userId);
        Task<IEnumerable<AuditLog>> SearchAuditLogsAsync(string searchTerm, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
