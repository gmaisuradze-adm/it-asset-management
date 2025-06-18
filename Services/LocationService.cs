using HospitalAssetTracker.Data;
using HospitalAssetTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalAssetTracker.Services
{
    public class LocationService : ILocationService 
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public LocationService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<Location> CreateLocationAsync(Location location, string userId)
        {
            location.CreatedDate = DateTime.UtcNow;
            location.IsActive = true;

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Create, "Location", location.Id, userId,
                $"Location created: {location.FullLocation}");

            return location;
        }

        public async Task<Location?> GetLocationByIdAsync(int id)
        {
            return await _context.Locations
                .Include(l => l.Assets)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<Location>> GetAllLocationsAsync()
        {
            return await _context.Locations
                .OrderBy(l => l.Building)
                .ThenBy(l => l.Floor)
                .ThenBy(l => l.Room)
                .ToListAsync();
        }

        public async Task<IEnumerable<Location>> GetActiveLocationsAsync()
        {
            return await _context.Locations
                .Where(l => l.IsActive)
                .OrderBy(l => l.Building)
                .ThenBy(l => l.Floor)
                .ThenBy(l => l.Room)
                .ToListAsync();
        }

        public async Task<Location> UpdateLocationAsync(Location location, string userId)
        {
            var existingLocation = await _context.Locations.AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == location.Id);

            _context.Locations.Update(location);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Update, "Location", location.Id, userId,
                $"Location updated: {location.FullLocation}", existingLocation, location);

            return location;
        }

        public async Task<bool> DeleteLocationAsync(int id, string userId)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null) return false;

            // Check if location is in use
            if (await IsLocationInUseAsync(id)) return false;

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Delete, "Location", id, userId,
                $"Location deleted: {location.FullLocation}");

            return true;
        }

        public async Task<bool> DeactivateLocationAsync(int id, string userId)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null) return false;

            location.IsActive = false;
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Update, "Location", id, userId,
                $"Location deactivated: {location.FullLocation}");

            return true;
        }

        public async Task<bool> ActivateLocationAsync(int id, string userId)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null) return false;

            location.IsActive = true;
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(AuditAction.Update, "Location", id, userId,
                $"Location activated: {location.FullLocation}");

            return true;
        }

        public async Task<bool> IsLocationInUseAsync(int id)
        {
            return await _context.Assets.AnyAsync(a => a.LocationId == id);
        }

        public async Task<Location?> FindLocationAsync(string building, string? floor, string room)
        {
            return await _context.Locations
                .FirstOrDefaultAsync(l => l.Building.ToLower() == building.ToLower() &&
                                         (floor == null ? l.Floor == null : l.Floor!.ToLower() == floor.ToLower()) &&
                                         l.Room.ToLower() == room.ToLower());
        }

        public async Task<Location> CreateLocationIfNotExistsAsync(string building, string? floor, string room, string userId)
        {
            var existingLocation = await FindLocationAsync(building, floor, room);
            if (existingLocation != null)
            {
                return existingLocation;
            }

            var newLocation = new Location
            {
                Building = building,
                Floor = floor,
                Room = room,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            return await CreateLocationAsync(newLocation, userId);
        }

        public async Task<IEnumerable<Location>> SearchLocationsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetActiveLocationsAsync();
            }

            var lowerSearchTerm = searchTerm.ToLower();

            return await _context.Locations
                .Where(l => l.IsActive &&
                           (l.Building.ToLower().Contains(lowerSearchTerm) ||
                            (l.Floor != null && l.Floor.ToLower().Contains(lowerSearchTerm)) ||
                            l.Room.ToLower().Contains(lowerSearchTerm) ||
                            (l.Description != null && l.Description.ToLower().Contains(lowerSearchTerm))))
                .OrderBy(l => l.Building)
                .ThenBy(l => l.Floor)
                .ThenBy(l => l.Room)
                .ToListAsync();
        }

        public async Task<LocationSummary> GetLocationSummaryAsync()
        {
            var locations = await _context.Locations.Include(l => l.Assets).ToListAsync();

            return new LocationSummary
            {
                TotalLocations = locations.Count,
                ActiveLocations = locations.Count(l => l.IsActive),
                InactiveLocations = locations.Count(l => !l.IsActive),
                LocationsWithAssets = locations.Count(l => l.Assets.Any()),
                LocationsWithoutAssets = locations.Count(l => !l.Assets.Any()),
                TotalAssetsInLocations = locations.Sum(l => l.Assets.Count),
                BuildingBreakdown = locations
                    .GroupBy(l => l.Building)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        public async Task<IEnumerable<object>> GetLocationSuggestionsAsync(string building, string floor)
        {
            var suggestions = new List<object>();

            if (string.IsNullOrEmpty(building))
            {
                // Return distinct buildings
                var buildings = await _context.Locations
                    .Where(l => l.IsActive)
                    .Select(l => l.Building)
                    .Distinct()
                    .OrderBy(b => b)
                    .ToListAsync();

                suggestions.AddRange(buildings.Select(b => new { type = "building", value = b }));
            }
            else if (string.IsNullOrEmpty(floor))
            {
                // Return floors for the given building
                var floors = await _context.Locations
                    .Where(l => l.IsActive && l.Building.ToLower() == building.ToLower() && l.Floor != null)
                    .Select(l => l.Floor)
                    .Distinct()
                    .OrderBy(f => f)
                    .ToListAsync();

                suggestions.AddRange(floors.Select(f => new { type = "floor", value = f }));
            }
            else
            {
                // Return rooms for the given building and floor
                var rooms = await _context.Locations
                    .Where(l => l.IsActive && 
                               l.Building.ToLower() == building.ToLower() && 
                               (l.Floor == null ? string.IsNullOrEmpty(floor) : l.Floor.ToLower() == floor.ToLower()))
                    .Select(l => l.Room)
                    .Distinct()
                    .OrderBy(r => r)
                    .ToListAsync();

                suggestions.AddRange(rooms.Select(r => new { type = "room", value = r }));
            }

            return suggestions;
        }
    }

    public class LocationSummary
    {
        public int TotalLocations { get; set; }
        public int ActiveLocations { get; set; }
        public int InactiveLocations { get; set; }
        public int LocationsWithAssets { get; set; }
        public int LocationsWithoutAssets { get; set; }
        public int TotalAssetsInLocations { get; set; }
        public Dictionary<string, int> BuildingBreakdown { get; set; } = new();
    }
}
