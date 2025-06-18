using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HospitalAssetTracker.Models;
using HospitalAssetTracker.Services;
using System.Diagnostics;

namespace HospitalAssetTracker.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IReportService _reportService;
        private readonly IWriteOffService _writeOffService;

        public HomeController(ILogger<HomeController> logger, IReportService reportService, IWriteOffService writeOffService)
        {
            _logger = logger;
            _reportService = reportService;
            _writeOffService = writeOffService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboardData = await _reportService.GetDashboardDataAsync();
                var assetsByCategory = await _reportService.GetAssetsByCategoryAsync();
                var assetsByStatus = await _reportService.GetAssetsByStatusAsync();
                var assetsByLocation = await _reportService.GetAssetsByLocationAsync();
                var writeOffSummary = await _writeOffService.GetWriteOffSummaryAsync();

                ViewBag.AssetsByCategory = assetsByCategory;
                ViewBag.AssetsByStatus = assetsByStatus;
                ViewBag.AssetsByLocation = assetsByLocation;
                ViewBag.WriteOffSummary = writeOffSummary;

                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                ViewBag.ErrorMessage = "Unable to load dashboard data. Please try again.";
                return View(new Dictionary<string, object>());
            }
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new Models.ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
