namespace HospitalAssetTracker.Models
{
    public class AssetTagRequest
    {
        public string Prefix { get; set; } = string.Empty;
    }

    public class AssetTagCheckRequest
    {
        public string AssetTag { get; set; } = string.Empty;
    }

    public class LocationCreateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Building { get; set; }
        public string? Floor { get; set; }
        public string? Room { get; set; }
    }
}
