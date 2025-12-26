using Microsoft.AspNetCore.Authorization;

namespace AccessControl_API.Authorization
{
    public class RequirePermissionAttribute : AuthorizeAttribute
    {
        public RequirePermissionAttribute(string permission)
        {
            Policy = permission;
        }
    }
}
