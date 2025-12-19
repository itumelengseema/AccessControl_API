using AccessControl_API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccessControl_API.Controllers
{
    public class AccessControll : ControllerBase
    {

        private readonly AppDbContext _context;

        public AccessControll(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> CheckAccess(Guid userId, string permission)
        {

            var hasAccess = await _context.UserGroups
                .Where(ug => ug.UserId == userId)
                .SelectMany(ug => ug.Group.GroupPermission)
                .AnyAsync(gp => gp.Permission.Name == permission);

            return Ok(hasAccess);
        }
    }
}
