using AccessControl_API.Models.DTO;
using AccessControl_Web.Services.IServices;
using AccessControl_Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace AccessControl_Web.Controllers
{
    [AuthorizeSession]
    public class VisitLogsController : Controller
    {
        private readonly IVisitLogService _visitLogService;
        private readonly IUserService _userService;
        private readonly IGroupService _groupService;
        private readonly ILogger<VisitLogsController> _logger;

        public VisitLogsController(
            IVisitLogService visitLogService, 
            IUserService userService,
            IGroupService groupService,
            ILogger<VisitLogsController> logger)
        {
            _visitLogService = visitLogService;
            _userService = userService;
            _groupService = groupService;
            _logger = logger;
        }

        // GET: VisitLogs - Requires VIEW_ACTIVE_VISITORS permission
        [RequirePermission(PermissionHelper.VIEW_ACTIVE_VISITORS)]
        public async Task<IActionResult> Index()
        {
            var response = await _visitLogService.GetActiveVisitorsAsync();

            if (response?.Success == true && response.Data != null)
            {
                return View(response.Data);
            }

            TempData["Error"] = response?.Message ?? "Failed to load active visitors";
            return View(new List<VisitLogResponseDTO>());
        }

        // GET: VisitLogs/CheckIn - Requires CHECK_IN_VISITOR permission
        [RequirePermission(PermissionHelper.CHECK_IN_VISITOR)]
        public IActionResult CheckIn()
        {
            return View();
        }

        // POST: VisitLogs/CheckIn - Requires CHECK_IN_VISITOR permission
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(PermissionHelper.CHECK_IN_VISITOR)]
        public async Task<IActionResult> CheckIn(
            string VisitorFirstName,
            string VisitorLastName,
            string? VisitorEmail,
            string? VisitorPhone,
            string VisitorIdNumber,
            string? CarRegistration,
            string? PurposeOfVisit,
            string? PersonToVisit,
            string? Notes)
        {
            try
            {
                _logger.LogInformation("Check-in attempt for visitor: {FirstName} {LastName}, ID: {IdNumber}, Email: {Email}", 
                    VisitorFirstName, VisitorLastName, VisitorIdNumber, VisitorEmail);

                // Validate required fields
                if (string.IsNullOrWhiteSpace(VisitorFirstName) || 
                    string.IsNullOrWhiteSpace(VisitorLastName) || 
                    string.IsNullOrWhiteSpace(VisitorIdNumber))
                {
                    ModelState.AddModelError("", "First Name, Last Name, and ID Number are required.");
                    return View();
                }

                // Get all users to check for existing visitor
                var usersResponse = await _userService.GetAllUsersAsync();
                UserDTO? existingUser = null;

                if (usersResponse?.Success == true && usersResponse.Data != null)
                {
                    // First, try to find user by ID number (most reliable)
                    existingUser = usersResponse.Data.FirstOrDefault(u => 
                        u.IdentificationNumber == VisitorIdNumber);

                    // If not found by ID but email is provided, check by email
                    if (existingUser == null && !string.IsNullOrWhiteSpace(VisitorEmail))
                    {
                        existingUser = usersResponse.Data.FirstOrDefault(u => 
                            u.Email.Equals(VisitorEmail, StringComparison.OrdinalIgnoreCase));
                        
                        if (existingUser != null)
                        {
                            _logger.LogInformation("Found existing user by email: {Email}, User ID: {UserId}", 
                                VisitorEmail, existingUser.Id);
                        }
                    }
                }

                int userId;

                if (existingUser != null)
                {
                    // User exists - use existing user ID
                    userId = existingUser.Id;
                    _logger.LogInformation("Returning visitor found: {FirstName} {LastName} (ID: {UserId})", 
                        existingUser.FirstName, existingUser.LastName, existingUser.Id);
                    
                    TempData["Info"] = $"Welcome back, {existingUser.FirstName} {existingUser.LastName}!";
                }
                else
                {
                    // Get visitor group ID (find a group named "Visitor" or use first available)
                    int visitorGroupId = await GetVisitorGroupIdAsync();

                    if (visitorGroupId == 0)
                    {
                        ModelState.AddModelError("", "No groups available. Please create a 'Visitor' group first.");
                        _logger.LogError("No groups available for visitor creation");
                        return View();
                    }

                    // Generate a unique email if not provided
                    string emailToUse = VisitorEmail;
                    if (string.IsNullOrWhiteSpace(emailToUse))
                    {
                        // Generate unique email using ID number and timestamp
                        emailToUse = $"visitor.{VisitorIdNumber}.{DateTime.Now.Ticks}@temp.local";
                    }

                    // New visitor - create user record first
                    var newUser = new UserCreateUpdateDTO
                    {
                        FirstName = VisitorFirstName,
                        LastName = VisitorLastName,
                        Email = emailToUse,
                        IdentificationNumber = VisitorIdNumber,
                        GroupId = visitorGroupId
                    };

                    _logger.LogInformation("Creating new visitor user with Email: {Email}, GroupId: {GroupId}", 
                        emailToUse, visitorGroupId);

                    var createUserResponse = await _userService.CreateUserAsync(newUser);

                    if (createUserResponse?.Success != true || createUserResponse.Data == null)
                    {
                        var errorMsg = createUserResponse?.Message ?? "Unknown error";
                        _logger.LogError("Failed to create visitor user: {Error}", errorMsg);
                        
                        // Check if it's a duplicate email error
                        if (errorMsg.Contains("email") && errorMsg.Contains("already exists"))
                        {
                            ModelState.AddModelError("", $"A user with the email '{emailToUse}' already exists. Please use a different email or leave it blank.");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Failed to create visitor record: " + errorMsg);
                        }
                        return View();
                    }

                    userId = createUserResponse.Data.Id;
                    _logger.LogInformation("New visitor created: {FirstName} {LastName} (ID: {UserId})", 
                        VisitorFirstName, VisitorLastName, userId);
                }

                // Create check-in record
                var checkInDto = new CheckInDTO
                {
                    UserId = userId,
                    CarRegistrationNumber = CarRegistration
                };

                _logger.LogInformation("Attempting to check in user ID: {UserId}", userId);

                var response = await _visitLogService.CheckInAsync(checkInDto);

                if (response?.Success == true)
                {
                    TempData["Success"] = $"? Visitor {VisitorFirstName} {VisitorLastName} checked in successfully! " +
                        $"{BuildVisitorSummary(PurposeOfVisit, PersonToVisit)}";
                    _logger.LogInformation("Visitor checked in successfully: {UserId}", userId);
                    return RedirectToAction(nameof(Index));
                }

                var errorMessage = response?.Message ?? "Failed to check in visitor";
                _logger.LogWarning("Check-in failed for {FirstName} {LastName}: {Error}", 
                    VisitorFirstName, VisitorLastName, errorMessage);
                
                // Check if user already has an active visit
                if (errorMessage.Contains("already has an active visit"))
                {
                    ModelState.AddModelError("", $"? {VisitorFirstName} {VisitorLastName} is already checked in. Please check them out first.");
                }
                else
                {
                    ModelState.AddModelError("", "? " + errorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during visitor check-in for {FirstName} {LastName}", 
                    VisitorFirstName, VisitorLastName);
                ModelState.AddModelError("", "? An unexpected error occurred: " + ex.Message);
            }

            return View();
        }

        // POST: VisitLogs/CheckOut/{id} - Requires CHECK_OUT_VISITOR permission
        [HttpPost]
        [RequirePermission(PermissionHelper.CHECK_OUT_VISITOR)]
        public async Task<IActionResult> CheckOut(int id)
        {
            _logger.LogInformation("Check-out attempt for visit log ID: {VisitLogId}", id);

            var response = await _visitLogService.CheckOutAsync(id);

            if (response?.Success == true)
            {
                TempData["Success"] = "? Visitor checked out successfully";
                _logger.LogInformation("Visitor checked out successfully: {VisitLogId}", id);
            }
            else
            {
                var errorMsg = response?.Message ?? "Failed to check out visitor";
                TempData["Error"] = "? " + errorMsg;
                _logger.LogWarning("Check-out failed for {VisitLogId}: {Error}", id, errorMsg);
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper: Get visitor group ID
        private async Task<int> GetVisitorGroupIdAsync()
        {
            try
            {
                var groupsResponse = await _groupService.GetAllGroupsAsync();
                
                if (groupsResponse?.Success == true && groupsResponse.Data != null && groupsResponse.Data.Any())
                {
                    // Try to find a group named "Visitor" first
                    var visitorGroup = groupsResponse.Data.FirstOrDefault(g => 
                        g.Name.Equals("Visitor", StringComparison.OrdinalIgnoreCase));
                    
                    if (visitorGroup != null)
                    {
                        _logger.LogInformation("Found 'Visitor' group with ID: {GroupId}", visitorGroup.Id);
                        return visitorGroup.Id;
                    }

                    // If no "Visitor" group, use the last group (usually least privileged)
                    var lastGroup = groupsResponse.Data.Last();
                    _logger.LogInformation("Using group '{GroupName}' (ID: {GroupId}) for visitor", 
                        lastGroup.Name, lastGroup.Id);
                    return lastGroup.Id;
                }

                _logger.LogWarning("No groups found in the system");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting visitor group ID");
                return 0;
            }
        }

        // Helper: Build summary message
        private string BuildVisitorSummary(string? purpose, string? personToVisit)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(purpose))
                parts.Add($"Purpose: {purpose}");

            if (!string.IsNullOrWhiteSpace(personToVisit))
                parts.Add($"Visiting: {personToVisit}");

            return parts.Any() ? "(" + string.Join(", ", parts) + ")" : "";
        }
    }
}
