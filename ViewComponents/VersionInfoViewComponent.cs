using Microsoft.AspNetCore.Mvc;
using HospitalAssetTracker.Services;

namespace HospitalAssetTracker.ViewComponents
{
    public class VersionInfoViewComponent : ViewComponent
    {
        private readonly VersionService _versionService;

        public VersionInfoViewComponent(VersionService versionService)
        {
            _versionService = versionService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var currentVersion = await _versionService.GetCurrentVersionAsync();
                return View("Default", currentVersion);
            }
            catch
            {
                return View("Default", "1.0.0");
            }
        }
    }
}
