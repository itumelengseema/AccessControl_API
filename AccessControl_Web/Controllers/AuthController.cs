using AccessControl_API.Models.DTO;
using AccessControl_Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AccessControl_Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IGroupService _groupService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, IGroupService groupService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _groupService = groupService;
            _logger = logger;
        }

        // GET: Auth/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDTO model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            _logger.LogInformation("Login attempt for email: {Email}", model.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login failed: ModelState invalid");
                return View(model);
            }

            try
            {
                var response = await _authService.LoginAsync(model);

                _logger.LogInformation("Login API response - Success: {Success}, Message: {Message}", 
                    response?.Success, response?.Message);

                if (response?.Success == true && response.Data != null)
                {
                    // Store authentication data in session
                    HttpContext.Session.SetString("Token", response.Data.Token);
                    HttpContext.Session.SetString("UserEmail", response.Data.User.Email);
                    HttpContext.Session.SetString("UserName", $"{response.Data.User.FirstName} {response.Data.User.LastName}");
                    HttpContext.Session.SetString("UserGroup", response.Data.User.GroupName);
                    HttpContext.Session.SetInt32("UserId", response.Data.User.Id);
                    HttpContext.Session.SetInt32("GroupId", response.Data.User.GroupId);

                    // Store permissions in session as JSON
                    var permissionsJson = JsonSerializer.Serialize(response.Data.Permissions);
                    HttpContext.Session.SetString("UserPermissions", permissionsJson);

                    _logger.LogInformation("User {Email} logged in successfully with {PermissionCount} permissions: {Permissions}", 
                        model.Email, response.Data.Permissions.Count, string.Join(", ", response.Data.Permissions));
                    
                    TempData["Success"] = $"Welcome back, {response.Data.User.FirstName}!";
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }

                var errorMessage = response?.Message ?? "Invalid login attempt.";
                _logger.LogWarning("Login failed for {Email}: {Message}", model.Email, errorMessage);
                ModelState.AddModelError("", errorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during login for {Email}", model.Email);
                ModelState.AddModelError("", $"Cannot connect to API server. Please ensure the API is running on {SD.AccessControlAPIBase}");
                ModelState.AddModelError("", "To start the API: Open a terminal and run 'dotnet run --project AccessControl_API'");
            }

            return View(model);
        }

        // GET: Auth/Register
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            // Load groups for dropdown
            var groupsResponse = await _groupService.GetAllGroupsAsync();
            ViewBag.Groups = groupsResponse?.Data ?? new List<GroupDTO>();

            return View();
        }

        // POST: Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistrationRequestDTO model)
        {
            if (!ModelState.IsValid)
            {
                // Reload groups for dropdown
                var groupsResponse = await _groupService.GetAllGroupsAsync();
                ViewBag.Groups = groupsResponse?.Data ?? new List<GroupDTO>();
                return View(model);
            }

            var response = await _authService.RegisterAsync(model);

            if (response?.Success == true)
            {
                TempData["Success"] = "Registration successful! Please login.";
                return RedirectToAction(nameof(Login));
            }

            ModelState.AddModelError("", response?.Message ?? "Registration failed.");

            // Reload groups for dropdown
            var groupsReload = await _groupService.GetAllGroupsAsync();
            ViewBag.Groups = groupsReload?.Data ?? new List<GroupDTO>();

            return View(model);
        }

        // GET: Auth/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction(nameof(Login));
        }

        // GET: Auth/Diagnostic
        public async Task<IActionResult> Diagnostic()
        {
            var diagnostics = new
            {
                ApiUrl = SD.AccessControlAPIBase,
                SessionAvailable = HttpContext.Session.IsAvailable,
                IsHttps = Request.IsHttps,
                Host = Request.Host.ToString(),
                Scheme = Request.Scheme
            };

            ViewBag.Diagnostics = diagnostics;

            try
            {
                // Test API connection
                var testResponse = await _groupService.GetAllGroupsAsync();
                ViewBag.ApiConnected = testResponse != null;
                ViewBag.ApiResponse = testResponse?.Message ?? "No message";
                ViewBag.ApiSuccess = testResponse?.Success ?? false;
            }
            catch (Exception ex)
            {
                ViewBag.ApiConnected = false;
                ViewBag.ApiError = ex.Message;
            }

            return View();
        }

        // Check if user is authenticated (helper for views)
        public bool IsAuthenticated()
        {
            return HttpContext.Session.GetString("Token") != null;
        }
    }
}
