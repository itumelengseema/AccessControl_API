using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace AccessControl_Web.Filters
{
    /// <summary>
    /// Authorization attribute that checks if user has required permission
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : ActionFilterAttribute
    {
        private readonly string _permission;

        public RequirePermissionAttribute(string permission)
        {
            _permission = permission;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Check if user is authenticated
            var token = context.HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", 
                    new { returnUrl = context.HttpContext.Request.Path });
                return;
            }

            // Get user permissions from session
            var permissionsJson = context.HttpContext.Session.GetString("UserPermissions");
            if (string.IsNullOrEmpty(permissionsJson))
            {
                // No permissions - redirect to access denied
                context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
                return;
            }

            // Parse permissions
            var permissions = JsonSerializer.Deserialize<List<string>>(permissionsJson);
            
            // Check if user has the required permission
            if (permissions == null || !permissions.Contains(_permission, StringComparer.OrdinalIgnoreCase))
            {
                // User doesn't have permission - redirect to access denied
                context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }

    /// <summary>
    /// Helper class to check permissions in views and controllers
    /// </summary>
    public static class PermissionHelper
    {
        // Permission constants
        public const string MANAGE_USERS = "MANAGE_USERS";
        public const string CHECK_IN_VISITOR = "CHECK_IN_VISITOR";
        public const string CHECK_OUT_VISITOR = "CHECK_OUT_VISITOR";
        public const string VIEW_ACTIVE_VISITORS = "VIEW_ACTIVE_VISITORS";
        public const string MANAGE_GROUPS = "MANAGE_GROUPS";
        public const string VIEW_STATISTICS = "VIEW_STATISTICS";

        /// <summary>
        /// Check if current user has specific permission
        /// </summary>
        public static bool HasPermission(HttpContext httpContext, string permission)
        {
            var permissionsJson = httpContext.Session.GetString("UserPermissions");
            if (string.IsNullOrEmpty(permissionsJson))
                return false;

            try
            {
                var permissions = JsonSerializer.Deserialize<List<string>>(permissionsJson);
                return permissions != null && permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if current user has any of the specified permissions
        /// </summary>
        public static bool HasAnyPermission(HttpContext httpContext, params string[] requiredPermissions)
        {
            var permissionsJson = httpContext.Session.GetString("UserPermissions");
            if (string.IsNullOrEmpty(permissionsJson))
                return false;

            try
            {
                var permissions = JsonSerializer.Deserialize<List<string>>(permissionsJson);
                if (permissions == null)
                    return false;

                return requiredPermissions.Any(req => 
                    permissions.Contains(req, StringComparer.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get all permissions for current user
        /// </summary>
        public static List<string> GetUserPermissions(HttpContext httpContext)
        {
            var permissionsJson = httpContext.Session.GetString("UserPermissions");
            if (string.IsNullOrEmpty(permissionsJson))
                return new List<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(permissionsJson) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
