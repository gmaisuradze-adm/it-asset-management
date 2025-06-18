using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Services;

namespace HospitalAssetTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LocationsController : Controller
    {
        private readonly ILocationService _locationService;
        private readonly IAuditService _auditService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LocationsController(
            ILocationService locationService,
            IAuditService auditService,
            UserManager<ApplicationUser> userManager)
        {
            _locationService = locationService;
            _auditService = auditService;
            _userManager = userManager;
        }

        // GET: Locations
        public async Task<IActionResult> Index()
        {
            var locations = await _locationService.GetAllLocationsAsync();
            return View(locations);
        }

        // GET: Locations/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var location = await _locationService.GetLocationByIdAsync(id);
            if (location == null)
            {
                return NotFound();
            }

            return View(location);
        }

        // GET: Locations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Locations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Building,Floor,Room,Description")] Location location)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = _userManager.GetUserId(User) ?? string.Empty;
                    await _locationService.CreateLocationAsync(location, userId);
                    
                    await _auditService.LogAsync(
                        AuditAction.Create,
                        "Location",
                        location.Id,
                        userId,
                        $"Location created: {location.FullLocation}"
                    );

                    TempData["SuccessMessage"] = "Location created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error creating location: {ex.Message}";
                }
            }
            return View(location);
        }

        // GET: Locations/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var location = await _locationService.GetLocationByIdAsync(id);
            if (location == null)
            {
                return NotFound();
            }
            return View(location);
        }

        // POST: Locations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Building,Floor,Room,Description,IsActive,CreatedDate")] Location location)
        {
            if (id != location.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = _userManager.GetUserId(User) ?? string.Empty;
                    await _locationService.UpdateLocationAsync(location, userId);
                    
                    await _auditService.LogAsync(
                        AuditAction.Update,
                        "Location",
                        location.Id,
                        userId,
                        $"Location updated: {location.FullLocation}"
                    );

                    TempData["SuccessMessage"] = "Location updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error updating location: {ex.Message}";
                }
            }
            return View(location);
        }

        // GET: Locations/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var location = await _locationService.GetLocationByIdAsync(id);
            if (location == null)
            {
                return NotFound();
            }

            return View(location);
        }

        // POST: Locations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var location = await _locationService.GetLocationByIdAsync(id);
                if (location != null)
                {
                    var userId = _userManager.GetUserId(User) ?? string.Empty;
                    await _locationService.DeleteLocationAsync(id, userId);
                    
                    await _auditService.LogAsync(
                        AuditAction.Delete,
                        "Location",
                        id,
                        userId,
                        $"Location deleted: {location.FullLocation}"
                    );

                    TempData["SuccessMessage"] = "Location deleted successfully!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting location: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Locations/ToggleActive/5
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            try
            {
                var location = await _locationService.GetLocationByIdAsync(id);
                if (location != null)
                {
                    location.IsActive = !location.IsActive;
                    var userId = _userManager.GetUserId(User) ?? string.Empty;
                    await _locationService.UpdateLocationAsync(location, userId);
                    
                    await _auditService.LogAsync(
                        AuditAction.Update,
                        "Location",
                        id,
                        userId,
                        $"Location {(location.IsActive ? "activated" : "deactivated")}: {location.FullLocation}"
                    );

                    TempData["SuccessMessage"] = $"Location {(location.IsActive ? "activated" : "deactivated")} successfully!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating location: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
