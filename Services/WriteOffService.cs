using HospitalAssetTracker.Data;
using HospitalAssetTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalAssetTracker.Services
{
    public class WriteOffService : IWriteOffService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly IAssetService _assetService;

        public WriteOffService(ApplicationDbContext context, IAuditService auditService, IAssetService assetService)
        {
            _context = context;
            _auditService = auditService;
            _assetService = assetService;
        }

        public async Task<WriteOffRecord> CreateWriteOffRecordAsync(WriteOffRecord writeOffRecord, string userId)
        {
            writeOffRecord.CreatedDate = DateTime.UtcNow;
            writeOffRecord.LastUpdated = DateTime.UtcNow;
            writeOffRecord.RequestDate = DateTime.UtcNow;
            writeOffRecord.RequestedByUserId = userId;
            writeOffRecord.Status = WriteOffStatus.Pending;
            
            // Generate unique write-off number
            writeOffRecord.WriteOffNumber = await GenerateWriteOffNumberAsync();

            _context.WriteOffRecords.Add(writeOffRecord);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Create, "WriteOffRecord", writeOffRecord.Id, userId,
                $"Write-off request created for asset {writeOffRecord.Asset?.AssetTag}");

            return writeOffRecord;
        }

        public async Task<WriteOffRecord?> GetWriteOffRecordByIdAsync(int id)
        {
            return await _context.WriteOffRecords
                .Include(w => w.Asset)
                .Include(w => w.RequestedByUser)
                .Include(w => w.ReviewedByUser)
                .Include(w => w.ApprovedByUser)
                .Include(w => w.ProcessedByUser)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<WriteOffRecord?> GetWriteOffRecordByAssetIdAsync(int assetId)
        {
            return await _context.WriteOffRecords
                .Include(w => w.Asset)
                .Include(w => w.RequestedByUser)
                .Include(w => w.ReviewedByUser)
                .Include(w => w.ApprovedByUser)
                .Include(w => w.ProcessedByUser)
                .Where(w => w.AssetId == assetId)
                .OrderByDescending(w => w.CreatedDate)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<WriteOffRecord>> GetAllWriteOffRecordsAsync()
        {
            return await _context.WriteOffRecords
                .Include(w => w.Asset)
                .Include(w => w.RequestedByUser)
                .Include(w => w.ReviewedByUser)
                .Include(w => w.ApprovedByUser)
                .Include(w => w.ProcessedByUser)
                .OrderByDescending(w => w.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<WriteOffRecord>> GetPendingWriteOffRecordsAsync()
        {
            return await _context.WriteOffRecords
                .Include(w => w.Asset)
                .Include(w => w.RequestedByUser)
                .Where(w => w.Status == WriteOffStatus.Pending || w.Status == WriteOffStatus.UnderReview)
                .OrderBy(w => w.RequestDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<WriteOffRecord>> GetApprovedWriteOffRecordsAsync()
        {
            return await _context.WriteOffRecords
                .Include(w => w.Asset)
                .Include(w => w.RequestedByUser)
                .Include(w => w.ApprovedByUser)
                .Where(w => w.Status == WriteOffStatus.Approved)
                .OrderByDescending(w => w.ApprovalDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<WriteOffRecord>> GetRejectedWriteOffRecordsAsync()
        {
            return await _context.WriteOffRecords
                .Include(w => w.Asset)
                .Include(w => w.RequestedByUser)
                .Include(w => w.ReviewedByUser)
                .Where(w => w.Status == WriteOffStatus.Rejected)
                .OrderByDescending(w => w.ReviewDate)
                .ToListAsync();
        }

        public async Task<WriteOffRecord?> UpdateWriteOffRecordAsync(WriteOffRecord writeOffRecord, string userId)
        {
            var existingRecord = await _context.WriteOffRecords.AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == writeOffRecord.Id);

            if (existingRecord == null) return null;

            writeOffRecord.LastUpdated = DateTime.UtcNow;
            _context.WriteOffRecords.Update(writeOffRecord);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Update, "WriteOffRecord", writeOffRecord.Id, userId,
                $"Write-off record updated", existingRecord, writeOffRecord);

            return writeOffRecord;
        }

        public async Task<bool> ApproveWriteOffAsync(int id, string approvedBy, string approvalNotes)
        {
            var writeOffRecord = await _context.WriteOffRecords
                .Include(w => w.Asset)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (writeOffRecord == null) return false;

            writeOffRecord.Status = WriteOffStatus.Approved;
            writeOffRecord.ApprovedByUserId = approvedBy;
            writeOffRecord.ApprovalDate = DateTime.UtcNow;
            writeOffRecord.ApprovalNotes = approvalNotes;
            writeOffRecord.LastUpdated = DateTime.UtcNow;

            // Change asset status to Decommissioned (write-off)
            if (writeOffRecord.Asset != null)
            {
                writeOffRecord.Asset.Status = AssetStatus.Decommissioned;
                writeOffRecord.Asset.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Update, "WriteOffRecord", id, approvedBy,
                $"Write-off approved for asset {writeOffRecord.Asset?.AssetTag}");

            return true;
        }

        public async Task<bool> RejectWriteOffAsync(int id, string rejectedBy, string rejectionReason)
        {
            var writeOffRecord = await _context.WriteOffRecords
                .Include(w => w.Asset)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (writeOffRecord == null) return false;

            writeOffRecord.Status = WriteOffStatus.Rejected;
            writeOffRecord.ReviewedByUserId = rejectedBy;
            writeOffRecord.ReviewDate = DateTime.UtcNow;
            writeOffRecord.ReviewNotes = rejectionReason;
            writeOffRecord.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Update, "WriteOffRecord", id, rejectedBy,
                $"Write-off rejected for asset {writeOffRecord.Asset?.AssetTag}");

            return true;
        }

        public async Task<bool> ProcessWriteOffAsync(int id, string processedBy, string processNotes)
        {
            var writeOffRecord = await _context.WriteOffRecords
                .Include(w => w.Asset)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (writeOffRecord == null || writeOffRecord.Status != WriteOffStatus.Approved) return false;

            writeOffRecord.Status = WriteOffStatus.Processed;
            writeOffRecord.ProcessedByUserId = processedBy;
            writeOffRecord.ProcessingDate = DateTime.UtcNow;
            writeOffRecord.ProcessingNotes = processNotes;
            writeOffRecord.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Update, "WriteOffRecord", id, processedBy,
                $"Write-off processed for asset {writeOffRecord.Asset?.AssetTag}");

            return true;
        }

        public async Task<bool> DeleteWriteOffRecordAsync(int id, string userId)
        {
            var writeOffRecord = await _context.WriteOffRecords.FindAsync(id);
            if (writeOffRecord == null) return false;

            // Only allow deletion if it's pending or rejected
            if (writeOffRecord.Status != WriteOffStatus.Pending && writeOffRecord.Status != WriteOffStatus.Rejected)
            {
                return false;
            }

            _context.WriteOffRecords.Remove(writeOffRecord);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Delete, "WriteOffRecord", id, userId,
                $"Write-off record deleted");

            return true;
        }

        public async Task<bool> SubmitWriteOffRequestAsync(int assetId, WriteOffReason reason, string description, string requestedBy)
        {
            var asset = await _assetService.GetAssetByIdAsync(assetId);
            if (asset == null) return false;

            // Check if there's already a pending write-off for this asset
            var existingPending = await _context.WriteOffRecords
                .AnyAsync(w => w.AssetId == assetId && 
                             (w.Status == WriteOffStatus.Pending || w.Status == WriteOffStatus.UnderReview));

            if (existingPending) return false;

            var writeOffRecord = new WriteOffRecord
            {
                AssetId = assetId,
                Reason = reason,
                Description = description,
                EstimatedValue = asset.PurchasePrice,
                RequestedByUserId = requestedBy,
                RequestDate = DateTime.UtcNow,
                Status = WriteOffStatus.Pending,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                WriteOffNumber = await GenerateWriteOffNumberAsync()
            };

            _context.WriteOffRecords.Add(writeOffRecord);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Create, "WriteOffRecord", writeOffRecord.Id, requestedBy,
                $"Write-off request submitted for asset {asset.AssetTag}");

            return true;
        }

        public async Task<IEnumerable<WriteOffRecord>> GetWriteOffHistoryByAssetAsync(int assetId)
        {
            return await _context.WriteOffRecords
                .Include(w => w.RequestedByUser)
                .Include(w => w.ReviewedByUser)
                .Include(w => w.ApprovedByUser)
                .Include(w => w.ProcessedByUser)
                .Where(w => w.AssetId == assetId)
                .OrderByDescending(w => w.CreatedDate)
                .ToListAsync();
        }

        // Additional methods required by controllers
        public async Task<IEnumerable<WriteOffRecord>> GetWriteOffRecordsByAssetAsync(int assetId)
        {
            return await GetWriteOffHistoryByAssetAsync(assetId);
        }

        public async Task<IEnumerable<WriteOffRecord>> GetPendingApprovalWriteOffsAsync()
        {
            return await GetPendingWriteOffRecordsAsync();
        }

        public async Task<WriteOffSummary> GetWriteOffSummaryAsync(DateTime? startDate, DateTime? endDate, WriteOffReason? reason)
        {
            var query = _context.WriteOffRecords.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(w => w.CreatedDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(w => w.CreatedDate <= endDate.Value);

            if (reason.HasValue)
                query = query.Where(w => w.Reason == reason.Value);

            var writeOffs = await query.ToListAsync();

            var summary = new WriteOffSummary
            {
                TotalWriteOffs = writeOffs.Count,
                PendingRequests = writeOffs.Count(w => w.Status == WriteOffStatus.Pending || w.Status == WriteOffStatus.UnderReview),
                ApprovedRequests = writeOffs.Count(w => w.Status == WriteOffStatus.Approved),
                ProcessedRequests = writeOffs.Count(w => w.Status == WriteOffStatus.Processed),
                RejectedRequests = writeOffs.Count(w => w.Status == WriteOffStatus.Rejected),
                TotalEstimatedValue = writeOffs.Sum(w => w.EstimatedValue) ?? 0M,
                TotalSalvageValue = writeOffs.Sum(w => w.SalvageValue) ?? 0M
            };

            // Reason breakdown
            summary.ReasonBreakdown = writeOffs
                .GroupBy(w => w.Reason)
                .ToDictionary(g => g.Key, g => g.Count());

            // Monthly trends
            summary.MonthlyTrends = writeOffs
                .GroupBy(w => w.CreatedDate.ToString("yyyy-MM"))
                .ToDictionary(g => g.Key, g => g.Count());

            return summary;
        }

        public async Task<WriteOffSummary> GetWriteOffSummaryAsync()
        {
            var writeOffs = await _context.WriteOffRecords.ToListAsync();

            var summary = new WriteOffSummary
            {
                TotalWriteOffs = writeOffs.Count,
                PendingRequests = writeOffs.Count(w => w.Status == WriteOffStatus.Pending || w.Status == WriteOffStatus.UnderReview),
                ApprovedRequests = writeOffs.Count(w => w.Status == WriteOffStatus.Approved),
                ProcessedRequests = writeOffs.Count(w => w.Status == WriteOffStatus.Processed),
                RejectedRequests = writeOffs.Count(w => w.Status == WriteOffStatus.Rejected),
                TotalEstimatedValue = writeOffs.Sum(w => w.EstimatedValue) ?? 0M,
                TotalSalvageValue = writeOffs.Sum(w => w.SalvageValue) ?? 0M
            };

            // Reason breakdown
            summary.ReasonBreakdown = writeOffs
                .GroupBy(w => w.Reason)
                .ToDictionary(g => g.Key, g => g.Count());

            // Monthly trends (last 12 months)
            var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-12);
            summary.MonthlyTrends = writeOffs
                .Where(w => w.CreatedDate >= twelveMonthsAgo)
                .GroupBy(w => w.CreatedDate.ToString("yyyy-MM"))
                .ToDictionary(g => g.Key, g => g.Count());

            return summary;
        }

        private async Task<string> GenerateWriteOffNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var month = DateTime.UtcNow.Month;
            var prefix = $"WO-{year:0000}{month:00}-";
            
            var lastNumber = await _context.WriteOffRecords
                .Where(w => w.WriteOffNumber.StartsWith(prefix))
                .OrderByDescending(w => w.WriteOffNumber)
                .Select(w => w.WriteOffNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastNumber))
            {
                var lastNumberPart = lastNumber.Substring(prefix.Length);
                if (int.TryParse(lastNumberPart, out int lastNum))
                {
                    nextNumber = lastNum + 1;
                }
            }

            return $"{prefix}{nextNumber:0000}";
        }
    }
}
