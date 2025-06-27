using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Models
{
    public class RequestSearchModel : PagedSearchModel
    {
        public string? SearchTerm { get; set; }
        public RequestStatus? Status { get; set; }
        public RequestPriority? Priority { get; set; }
        public int? CategoryId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public RequestType? RequestType { get; set; }
        public string? Department { get; set; }
    }
}
