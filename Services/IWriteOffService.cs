using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Services
{
    public interface IWriteOffService
    {
        Task<WriteOffRecord> CreateWriteOffRecordAsync(WriteOffRecord writeOffRecord, string userId);
        Task<WriteOffRecord?> GetWriteOffRecordByIdAsync(int id);
        Task<WriteOffRecord?> GetWriteOffRecordByAssetIdAsync(int assetId);
        Task<IEnumerable<WriteOffRecord>> GetAllWriteOffRecordsAsync();
        Task<IEnumerable<WriteOffRecord>> GetPendingWriteOffRecordsAsync();
        Task<IEnumerable<WriteOffRecord>> GetApprovedWriteOffRecordsAsync();
        Task<IEnumerable<WriteOffRecord>> GetRejectedWriteOffRecordsAsync();
        Task<WriteOffRecord?> UpdateWriteOffRecordAsync(WriteOffRecord writeOffRecord, string userId);
        Task<bool> ApproveWriteOffAsync(int id, string approvedBy, string approvalNotes);
        Task<bool> RejectWriteOffAsync(int id, string rejectedBy, string rejectionReason);
        Task<bool> ProcessWriteOffAsync(int id, string processedBy, string processNotes);
        Task<bool> DeleteWriteOffRecordAsync(int id, string userId);
        Task<bool> SubmitWriteOffRequestAsync(int assetId, WriteOffReason reason, string description, string requestedBy);
        Task<IEnumerable<WriteOffRecord>> GetWriteOffHistoryByAssetAsync(int assetId);
        Task<WriteOffSummary> GetWriteOffSummaryAsync();
        
        // Additional methods required by controllers
        Task<IEnumerable<WriteOffRecord>> GetWriteOffRecordsByAssetAsync(int assetId);
        Task<IEnumerable<WriteOffRecord>> GetPendingApprovalWriteOffsAsync();
        Task<WriteOffSummary> GetWriteOffSummaryAsync(DateTime? startDate, DateTime? endDate, WriteOffReason? reason);
    }
}
