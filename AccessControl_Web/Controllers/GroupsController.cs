using AccessControl_API.Models.DTO;
using AccessControl_Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace AccessControl_Web.Controllers
{
    public class GroupsController : Controller
    {
        private readonly IGroupService _groupService;

        public GroupsController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        // GET: Groups
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

        // GET: Groups/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Groups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // GET: Groups/Statistics
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

        // POST: Groups/CreateDefaults
        [HttpPost]
        public async Task<IActionResult> CreateDefaults()
        {
            var defaultGroups = new[] { "Admin", "Security", "Employee", "Visitor" };
            var createdCount = 0;
            var errors = new List<string>();
            var detailedErrors = new List<string>();

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
                    var errorMsg = response?.Message ?? "Unknown error";
                    errors.Add($"{groupName}: {errorMsg}");
                    
                    // Log detailed error to console
                    Console.WriteLine($"Failed to create group '{groupName}': {errorMsg}");
                    Console.WriteLine($"Response Status: {response?.Status}");
                    Console.WriteLine($"Response Data: {response?.Data}");
                    Console.WriteLine($"Response Errors: {response?.Errors}");
                    
                    detailedErrors.Add($"{groupName} - Status: {response?.Status}, Message: {errorMsg}");
                }
            }

            if (createdCount > 0)
            {
                TempData["Success"] = $"Successfully created {createdCount} out of {defaultGroups.Length} group(s)";
            }

            if (errors.Any())
            {
                var errorMessage = "Failed to create some groups:\n" + string.Join("\n", detailedErrors);
                TempData["Error"] = errorMessage;
                
                // Also log to console for debugging
                Console.WriteLine("=== GROUP CREATION ERRORS ===");
                Console.WriteLine(errorMessage);
                Console.WriteLine("=============================");
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Groups/TestConnection
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var response = await _groupService.GetAllGroupsAsync();
                
                var diagnostics = new
                {
                    ApiReachable = response != null,
                    Success = response?.Success,
                    Status = response?.Status.ToString(),
                    Message = response?.Message,
                    DataCount = response?.Data?.Count() ?? 0,
                    Errors = response?.Errors
                };

                ViewBag.Diagnostics = diagnostics;
                ViewBag.RawResponse = System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.StackTrace = ex.StackTrace;
                return View();
            }
        }
    }
}
