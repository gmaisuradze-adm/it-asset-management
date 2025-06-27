using HospitalAssetTracker.Models;
using HospitalAssetTracker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HospitalAssetTracker.ViewComponents
{
    public class NotificationViewComponent : ViewComponent
    {
        private readonly IRequestService _requestService;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationViewComponent(IRequestService requestService, UserManager<ApplicationUser> userManager)
        {
            _requestService = requestService;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(UserClaimsPrincipal);
            if (user == null)
            {
                return Content(string.Empty);
            }

            var userId = user.Id;
            var userRoles = await _userManager.GetRolesAsync(user);

            var canViewAllPending = userRoles.Contains("Admin") || userRoles.Contains("IT Support") || userRoles.Contains("Asset Manager");

            var model = new NotificationViewModel
            {
                ShowPendingRequests = canViewAllPending,
                PendingRequestsCount = canViewAllPending ? await _requestService.GetPendingRequestsCountAsync() : 0,
                MyActiveRequestsCount = await _requestService.GetMyActiveRequestsCountAsync(userId)
            };

            return View(model);
        }
    }
}
