using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Services
{
    public interface IReportService
    {
        Task<byte[]> GenerateAssetReportExcelAsync(IEnumerable<Asset> assets);
        Task<byte[]> GenerateAssetReportPdfAsync(IEnumerable<Asset> assets);
        Task<byte[]> GenerateMaintenanceReportPdfAsync(IEnumerable<MaintenanceRecord> maintenanceRecords);
        Task<byte[]> GenerateAuditReportPdfAsync(IEnumerable<AuditLog> auditLogs);
        Task<Dictionary<string, object>> GetDashboardDataAsync();
        Task<Dictionary<AssetCategory, int>> GetAssetsByCategoryAsync();
        Task<Dictionary<AssetStatus, int>> GetAssetsByStatusAsync();
        Task<Dictionary<string, int>> GetAssetsByLocationAsync();
    }
}
