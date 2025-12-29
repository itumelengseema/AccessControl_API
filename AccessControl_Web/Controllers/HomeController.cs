using AccessControl_Web.Models;
using AccessControl_Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AccessControl_Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserService _userService;
        private readonly IGroupService _groupService;
        private readonly IVisitLogService _visitLogService;

        public HomeController(IUserService userService, IGroupService groupService, IVisitLogService visitLogService)
        {
            _userService = userService;
            _groupService = groupService;
            _visitLogService = visitLogService;
        }

        public async Task<IActionResult> Index()
        {
            // Check if user is authenticated
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Fetch dashboard data
            var userCountResponse = await _userService.GetUserCountAsync();
            var groupsResponse = await _groupService.GetAllGroupsAsync();
            var activeVisitorsResponse = await _visitLogService.GetActiveVisitorsAsync();

            ViewBag.UserCount = userCountResponse?.Data ?? 0;
            ViewBag.GroupCount = groupsResponse?.Data?.Count() ?? 0;
            ViewBag.ActiveVisitorCount = activeVisitorsResponse?.Data?.Count ?? 0;
            ViewBag.UserName = HttpContext.Session.GetString("UserName") ?? "User";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
