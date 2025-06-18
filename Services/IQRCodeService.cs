namespace HospitalAssetTracker.Services
{
    public interface IQRCodeService
    {
        string GenerateQRCode(string data);
        byte[] GenerateQRCodeImage(string data, int pixelsPerModule = 20);
        Task<string> SaveQRCodeImageAsync(string data, string fileName);
    }
}
