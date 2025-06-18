using Xunit;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using HospitalAssetTracker.Services;
using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Tests
{
    public class AssetServiceTests
    {
        private readonly Mock<IAssetService> _assetServiceMock;

        public AssetServiceTests()
        {
            _assetServiceMock = new Mock<IAssetService>();
        }

        [Fact]
        public async Task GetAssetByIdAsync_ReturnsAsset()
        {
            // Arrange
            var assetId = 1;
            var expectedAsset = new Asset {
                Id = assetId,
                AssetTag = "A001",
                Category = AssetCategory.Laptop,
                Brand = "Dell",
                Model = "Latitude",
                SerialNumber = "SN123",
                Status = AssetStatus.InUse,
                LocationId = 1
            };
            _assetServiceMock.Setup(s => s.GetAssetByIdAsync(assetId)).ReturnsAsync(expectedAsset);

            // Act
            var result = await _assetServiceMock.Object.GetAssetByIdAsync(assetId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(assetId, result.Id);
        }

        [Fact]
        public async Task GetAllAssetsAsync_ReturnsList()
        {
            // Arrange
            var assets = new List<Asset> {
                new Asset {
                    Id = 1,
                    AssetTag = "A001",
                    Category = AssetCategory.Laptop,
                    Brand = "Dell",
                    Model = "Latitude",
                    SerialNumber = "SN123",
                    Status = AssetStatus.InUse,
                    LocationId = 1
                }
            };
            _assetServiceMock.Setup(s => s.GetAllAssetsAsync()).ReturnsAsync(assets);

            // Act
            var result = await _assetServiceMock.Object.GetAllAssetsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }
    }
}
