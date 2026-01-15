using AccessControl_API.Authorization;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AccessControl_Test.Authorization
{
    public class PermissionHandlerTests
    {
        private readonly PermissionHandler _handler;

        public PermissionHandlerTests()
        {
            _handler = new PermissionHandler();
        }

        [Fact]
        public async Task HandleRequirementAsync_UserHasPermission_Succeeds()
        {
            // Arrange
            var requirement = new PermissionRequirement("READ_USERS");
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim("permission", "READ_USERS")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);
            var context = new AuthorizationHandlerContext(
                new[] { requirement },
                user,
                null);

            // Act
            await _handler.HandleAsync(context);

            // Assert
            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task HandleRequirementAsync_UserDoesNotHavePermission_Fails()
        {
            // Arrange
            var requirement = new PermissionRequirement("WRITE_USERS");
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim("permission", "READ_USERS")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);
            var context = new AuthorizationHandlerContext(
                new[] { requirement },
                user,
                null);

            // Act
            await _handler.HandleAsync(context);

            // Assert
            Assert.False(context.HasSucceeded);
        }

        [Fact]
        public async Task HandleRequirementAsync_UserHasMultiplePermissions_SucceedsForRequiredOne()
        {
            // Arrange
            var requirement = new PermissionRequirement("WRITE_USERS");
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim("permission", "READ_USERS"),
                new Claim("permission", "WRITE_USERS"),
                new Claim("permission", "DELETE_USERS")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);
            var context = new AuthorizationHandlerContext(
                new[] { requirement },
                user,
                null);

            // Act
            await _handler.HandleAsync(context);

            // Assert
            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task HandleRequirementAsync_UserHasNoPermissions_Fails()
        {
            // Arrange
            var requirement = new PermissionRequirement("READ_USERS");
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Email, "test@example.com")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);
            var context = new AuthorizationHandlerContext(
                new[] { requirement },
                user,
                null);

            // Act
            await _handler.HandleAsync(context);

            // Assert
            Assert.False(context.HasSucceeded);
        }

        [Fact]
        public async Task HandleRequirementAsync_PermissionCaseExactMatch_Succeeds()
        {
            // Arrange
            var requirement = new PermissionRequirement("Read_Users");
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("permission", "Read_Users")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);
            var context = new AuthorizationHandlerContext(
                new[] { requirement },
                user,
                null);

            // Act
            await _handler.HandleAsync(context);

            // Assert
            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task HandleRequirementAsync_PermissionCaseMismatch_Fails()
        {
            // Arrange
            var requirement = new PermissionRequirement("READ_USERS");
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("permission", "read_users")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);
            var context = new AuthorizationHandlerContext(
                new[] { requirement },
                user,
                null);

            // Act
            await _handler.HandleAsync(context);

            // Assert
            Assert.False(context.HasSucceeded);
        }

        [Fact]
        public async Task HandleRequirementAsync_EmptyPermissionClaims_Fails()
        {
            // Arrange
            var requirement = new PermissionRequirement("READ_USERS");
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Email, "test@example.com")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);
            var context = new AuthorizationHandlerContext(
                new[] { requirement },
                user,
                null);

            // Act
            await _handler.HandleAsync(context);

            // Assert
            Assert.False(context.HasSucceeded);
            Assert.False(context.HasFailed); // Should not explicitly fail, just not succeed
        }

        [Fact]
        public async Task HandleRequirementAsync_UserNotAuthenticated_Fails()
        {
            // Arrange
            var requirement = new PermissionRequirement("READ_USERS");
            var user = new ClaimsPrincipal(); // No identity
            var context = new AuthorizationHandlerContext(
                new[] { requirement },
                user,
                null);

            // Act
            await _handler.HandleAsync(context);

            // Assert
            Assert.False(context.HasSucceeded);
        }

        [Fact]
        public async Task HandleRequirementAsync_MultipleRequirements_OnlyHandlesPermissionRequirement()
        {
            // Arrange
            var permissionRequirement = new PermissionRequirement("READ_USERS");
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("permission", "READ_USERS")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);
            var context = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] { permissionRequirement },
                user,
                null);

            // Act
            await _handler.HandleAsync(context);

            // Assert
            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task HandleRequirementAsync_SpecialCharactersInPermission_HandlesCorrectly()
        {
            // Arrange
            var requirement = new PermissionRequirement("READ_USERS:ALL");
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("permission", "READ_USERS:ALL")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);
            var context = new AuthorizationHandlerContext(
                new[] { requirement },
                user,
                null);

            // Act
            await _handler.HandleAsync(context);

            // Assert
            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task HandleRequirementAsync_SimilarButDifferentPermission_Fails()
        {
            // Arrange
            var requirement = new PermissionRequirement("READ_USERS");
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("permission", "READ_USER") // Missing 'S'
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);
            var context = new AuthorizationHandlerContext(
                new[] { requirement },
                user,
                null);

            // Act
            await _handler.HandleAsync(context);

            // Assert
            Assert.False(context.HasSucceeded);
        }

        [Fact]
        public async Task HandleRequirementAsync_WhitespaceInPermission_Fails()
        {
            // Arrange
            var requirement = new PermissionRequirement("READ_USERS");
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("permission", " READ_USERS ") // Has whitespace
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);
            var context = new AuthorizationHandlerContext(
                new[] { requirement },
                user,
                null);

            // Act
            await _handler.HandleAsync(context);

            // Assert
            Assert.False(context.HasSucceeded);
        }
    }
}
