using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Models
{
    public class ProcurementSearchModel : PagedSearchModel
    {
        public string? SearchTerm { get; set; }
        public ProcurementType? ProcurementType { get; set; }
        public ProcurementStatus? Status { get; set; }
        public ProcurementCategory? Category { get; set; }
        public int? VendorId { get; set; }
        public decimal? AmountFrom { get; set; }
        public decimal? AmountTo { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
