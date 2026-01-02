using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text.Json;
using AccessControl_Web.Filters;

namespace AccessControl_Web.Helpers
{
    public static class PermissionExtensions
    {
        /// <summary>
        /// Extension method to check permission in Razor views
        /// Usage: @if(ViewContext.HasPermission("MANAGE_USERS")) { ... }
        /// </summary>
        public static bool HasPermission(this ViewContext viewContext, string permission)
        {
            return PermissionHelper.HasPermission(viewContext.HttpContext, permission);
        }

        /// <summary>
        /// Check if user has any of the specified permissions
        /// </summary>
        public static bool HasAnyPermission(this ViewContext viewContext, params string[] permissions)
        {
            return PermissionHelper.HasAnyPermission(viewContext.HttpContext, permissions);
        }

        /// <summary>
        /// Get all user permissions
        /// </summary>
        public static List<string> GetUserPermissions(this ViewContext viewContext)
        {
            return PermissionHelper.GetUserPermissions(viewContext.HttpContext);
        }
    }
}
