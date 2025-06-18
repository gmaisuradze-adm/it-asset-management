using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace HospitalAssetTracker.Services
{
    public class QRCodeService : IQRCodeService
    {
        private readonly IWebHostEnvironment _environment;

        public QRCodeService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public string GenerateQRCode(string data)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new Base64QRCode(qrCodeData))
                {
                    return qrCode.GetGraphic(20);
                }
            }
        }

        public byte[] GenerateQRCodeImage(string data, int pixelsPerModule = 20)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new PngByteQRCode(qrCodeData))
                {
                    return qrCode.GetGraphic(pixelsPerModule);
                }
            }
        }

        public async Task<string> SaveQRCodeImageAsync(string data, string fileName)
        {
            try
            {
                var qrCodeBytes = GenerateQRCodeImage(data);
                
                var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "qrcodes");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                var filePath = Path.Combine(uploadsDir, $"{fileName}.png");
                await File.WriteAllBytesAsync(filePath, qrCodeBytes);

                return $"/uploads/qrcodes/{fileName}.png";
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
