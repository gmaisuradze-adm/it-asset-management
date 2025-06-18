namespace HospitalAssetTracker.Models
{
    public class AssetImportResult
    {
        public int LineNumber { get; set; }
        public string RawData { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string AssetTag { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
