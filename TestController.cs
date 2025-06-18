using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using HospitalAssetTracker.Services;
using HospitalAssetTracker.Models;

namespace HospitalAssetTracker.Controllers
{
    [Route("[controller]")]
    public class TestController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILocationService _locationService;

        public TestController(IInventoryService inventoryService, ILocationService locationService)
        {
            _inventoryService = inventoryService;
            _locationService = locationService;
        }

        [HttpGet("inventory-test")]
        public async Task<IActionResult> InventoryTest()
        {
            try
            {
                var result = await _inventoryService.GetInventoryItemsPagedAsync(new HospitalAssetTracker.Models.InventorySearchModels.InventorySearchCriteria());
                return Json(new { success = true, count = result.TotalCount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message, stack = ex.StackTrace });
            }
        }

        [HttpGet("location-test")]
        public async Task<IActionResult> LocationTest()
        {
            try
            {
                var locations = await _locationService.GetAllLocationsAsync();
                return Json(new { success = true, count = locations.Count() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message, stack = ex.StackTrace });
            }
        }

        [HttpGet("populate-viewdata-test")]
        public async Task<IActionResult> PopulateViewDataTest()
        {
            try
            {
                await PopulateViewData();
                return Json(new { 
                    success = true, 
                    locationsCount = (ViewBag.Locations as SelectList)?.Count() ?? 0,
                    categoriesCount = (ViewBag.Categories as SelectList)?.Count() ?? 0,
                    itemTypesCount = (ViewBag.ItemTypes as SelectList)?.Count() ?? 0,
                    statusesCount = (ViewBag.Statuses as SelectList)?.Count() ?? 0,
                    conditionsCount = (ViewBag.Conditions as SelectList)?.Count() ?? 0
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message, stack = ex.StackTrace });
            }
        }

        private async Task PopulateViewData()
        {
            var locations = await _locationService.GetAllLocationsAsync();
            ViewBag.Locations = new SelectList(locations, "Id", "FullLocation");

            ViewBag.Categories = new SelectList(Enum.GetValues<InventoryCategory>()
                .Select(c => new { Value = (int)c, Text = c.ToString() }), "Value", "Text");

            ViewBag.ItemTypes = new SelectList(Enum.GetValues<InventoryItemType>()
                .Select(t => new { Value = (int)t, Text = t.ToString() }), "Value", "Text");

            ViewBag.Statuses = new SelectList(Enum.GetValues<InventoryStatus>()
                .Select(s => new { Value = (int)s, Text = s.ToString() }), "Value", "Text");

            ViewBag.Conditions = new SelectList(Enum.GetValues<InventoryCondition>()
                .Select(c => new { Value = (int)c, Text = c.ToString() }), "Value", "Text");
        }
    }
}
