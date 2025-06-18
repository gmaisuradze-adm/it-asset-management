using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Services
{
    public interface ILocationService
    {
        Task<Location> CreateLocationAsync(Location location, string userId);
        Task<Location?> GetLocationByIdAsync(int id);
        Task<IEnumerable<Location>> GetAllLocationsAsync();
        Task<IEnumerable<Location>> GetActiveLocationsAsync();
        Task<Location> UpdateLocationAsync(Location location, string userId);
        Task<bool> DeleteLocationAsync(int id, string userId);
        Task<bool> DeactivateLocationAsync(int id, string userId);
        Task<bool> ActivateLocationAsync(int id, string userId);
        Task<bool> IsLocationInUseAsync(int id);
        Task<Location?> FindLocationAsync(string building, string? floor, string room);
        Task<Location> CreateLocationIfNotExistsAsync(string building, string? floor, string room, string userId);
        Task<IEnumerable<Location>> SearchLocationsAsync(string searchTerm);
        Task<LocationSummary> GetLocationSummaryAsync();
        Task<IEnumerable<object>> GetLocationSuggestionsAsync(string building, string floor);
    }
}
