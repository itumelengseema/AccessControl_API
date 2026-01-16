using AccessControl_API.Models;
using AccessControl_API.Utilities;

namespace AccessControl_API.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            // Only seed if no groups exist
            if (!context.Groups.Any())
            {
                var adminGroup = new Group { Name = "Admin" };
                var securityGroup = new Group { Name = "Security" };

                context.Groups.AddRange(adminGroup, securityGroup);
                context.SaveChanges();

                var permissions = new List<Permission>
                {
                    new Permission { Name = "MANAGE_USERS" },
                    new Permission { Name = "CHECK_IN_VISITOR" },
                    new Permission { Name = "CHECK_OUT_VISITOR" },
                    new Permission { Name = "VIEW_ACTIVE_VISITORS" }
                };

                context.Permissions.AddRange(permissions);
                context.SaveChanges();

                context.GroupPermissions.AddRange(
                    // Admin gets all permissions
                    permissions.Select(p => new GroupPermission
                    {
                        GroupId = adminGroup.Id,
                        PermissionId = p.Id
                    })
                );

                context.GroupPermissions.AddRange(
                    // Security limited (no MANAGE_USERS)
                    permissions
                        .Where(p => p.Name != "MANAGE_USERS")
                        .Select(p => new GroupPermission
                        {
                            GroupId = securityGroup.Id,
                            PermissionId = p.Id
                        })
                );

                context.SaveChanges();

                // Create default admin user
                var adminUser = new User
                {
                    FirstName = "System",
                    LastName = "Admin",
                    Email = "admin@access.local",
                    IdentificationNumber = "ADMIN-001",
                    PasswordHash = PasswordHasher.Hash("Admin@123"),
                    IsApproved = true, // Default admin is pre-approved
                    ApprovedAt = DateTime.UtcNow
                };
                var securityUser = new User
                {
                    FirstName = "Default",
                    LastName = "Security",
                    Email = "secure@access.com",
                    IdentificationNumber = "SEC-001",
                    PasswordHash = PasswordHasher.Hash("Secure@123"),
                    IsApproved = true,
                    ApprovedAt = DateTime.UtcNow
                };
                

                context.Users.Add(adminUser);
                context.Users.Add(securityUser);
                context.SaveChanges();

                // Assign admin user to Admin group
                context.UserGroups.Add(new UserGroup
                {
                    UserId = adminUser.Id,
                    GroupId = adminGroup.Id
                });

                // Assign security user to Security group
                context.UserGroups.Add(new UserGroup
                {
                    UserId = securityUser.Id,
                    GroupId = securityGroup.Id
                });

                context.SaveChanges();

                Console.WriteLine("Database seeded successfully!");
                Console.WriteLine("Default Admin User:");
                Console.WriteLine($"  Email: {adminUser.Email}");
                Console.WriteLine("  Password: Admin@123");

                Console.WriteLine($"Default Security User:{securityUser.Email}");
                Console.WriteLine("  Password: Secure@123");
            }
        }
    }
}
