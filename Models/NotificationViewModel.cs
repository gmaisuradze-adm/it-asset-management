namespace HospitalAssetTracker.Models
{
    public class NotificationViewModel
    {
        public int PendingRequestsCount { get; set; }
        public int MyActiveRequestsCount { get; set; }
        public bool ShowPendingRequests { get; set; }
    }
}
