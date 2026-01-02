using AccessControl_API.Models.DTO;
using AccessControl_Web.Services.IServices;
using AccessControl_Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace AccessControl_Web.Controllers
{
    [AuthorizeSession]
    public class GroupsController : Controller
    {
        private readonly IGroupService _groupService;

        public GroupsController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        // GET: Groups - Anyone can view groups
        public async Task<IActionResult> Index()
        {
            var response = await _groupService.GetAllGroupsAsync();

            if (response?.Success == true && response.Data != null)
            {
                return View(response.Data);
            }

            TempData["Error"] = response?.Message ?? "Failed to load groups";
            return View(new List<GroupDTO>());
        }

        // GET: Groups/Create - Only MANAGE_USERS permission
        [RequirePermission(PermissionHelper.MANAGE_USERS)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Groups/Create - Only MANAGE_USERS permission
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionHelper.MANAGE_USERS)]
        public async Task<IActionResult> Create(GroupDTO groupDto)
        {
            if (!ModelState.IsValid)
            {
                return View(groupDto);
            }

            var response = await _groupService.CreateGroupAsync(groupDto);

            if (response?.Success == true)
            {
                TempData["Success"] = "Group created successfully";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", response?.Message ?? "Failed to create group");
            return View(groupDto);
        }

        // GET: Groups/Statistics - Anyone can view statistics
        public async Task<IActionResult> Statistics()
        {
            var response = await _groupService.GetUsersPerGroupCountAsync();

            if (response?.Success == true && response.Data != null)
            {
                return View(response.Data);
            }

            TempData["Error"] = response?.Message ?? "Failed to load statistics";
            return View(new List<UsersPerGroupDTO>());
        }

        // POST: Groups/CreateDefaults - Only MANAGE_USERS permission
        [HttpPost]
        [RequirePermission(PermissionHelper.MANAGE_USERS)]
        public async Task<IActionResult> CreateDefaults()
        {
            var defaultGroups = new[] { "Admin", "Security", "Employee", "Visitor" };
            var createdCount = 0;
            var errors = new List<string>();

            foreach (var groupName in defaultGroups)
            {
                var groupDto = new GroupDTO { Name = groupName };
                var response = await _groupService.CreateGroupAsync(groupDto);

                if (response?.Success == true)
                {
                    createdCount++;
                }
                else
                {
                    errors.Add($"{groupName}: {response?.Message ?? "Unknown error"}");
                }
            }

            if (createdCount > 0)
            {
                TempData["Success"] = $"Successfully created {createdCount} out of {defaultGroups.Length} group(s)";
            }

            if (errors.Any())
            {
                TempData["Error"] = "Failed to create some groups: " + string.Join(", ", errors);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
