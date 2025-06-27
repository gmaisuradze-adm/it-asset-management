using System.Collections.Generic;

namespace HospitalAssetTracker.Models
{
    public class RequestDashboardData
    {
        public int TotalRequests { get; set; }
        public int SubmittedRequests { get; set; }
        public int InProgressRequests { get; set; }
        public int OnHoldRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int CancelledRequests { get; set; }
        public int OverdueRequests { get; set; }
        public int CompletedToday { get; set; } // Added this property

        public Dictionary<string, int> RequestsByType { get; set; } = new();
        public Dictionary<string, int> RequestsByPriority { get; set; } = new();

        public List<RequestSummaryViewModel> RecentRequests { get; set; } = new();
        public List<RequestSummaryViewModel> HighPriorityRequests { get; set; } = new(); // Added this property
    }
}
