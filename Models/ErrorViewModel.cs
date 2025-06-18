namespace HospitalAssetTracker.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string? ErrorMessage { get; set; }

        public string? ErrorDetails { get; set; }

        public Exception? Exception { get; set; }
    }
}
