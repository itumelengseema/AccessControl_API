using AccessControl_API.Data;
using AccessControl_API.Models;
using AccessControl_API.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AccessControl_Test.Utilities
{
    public class JwtTokenGeneratorTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly JwtTokenGenerator _tokenGenerator;

        public JwtTokenGeneratorTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            var configDict = new Dictionary<string, string?>
            {
                {"JwtSettings:Key", "YourSuperSecretKeyHere_MustBeAtLeast32CharactersLongForTesting"},
                {"JwtSettings:Issuer", "TestIssuer"},
                {"JwtSettings:Audience", "TestAudience"},
                {"JwtSettings:DurationInMinutes", "60"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            _tokenGenerator = new JwtTokenGenerator(_configuration, _context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void GenerateToken_ValidUser_ReturnsToken()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                IdentificationNumber = "123",
                PasswordHash = "DummyHash"
            };

            // Act
            var token = _tokenGenerator.GenerateToken(user);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        public void GenerateToken_ValidUser_TokenIsValidJwt()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                IdentificationNumber = "123",
                PasswordHash = "DummyHash"
            };

            // Act
            var token = _tokenGenerator.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            Assert.True(handler.CanReadToken(token));
        }

        [Fact]
        public void GenerateToken_ContainsUserIdClaim()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                IdentificationNumber = "123",
                PasswordHash = "DummyHash"
            };

            // Act
            var token = _tokenGenerator.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            
            Assert.NotNull(userIdClaim);
            Assert.Equal("1", userIdClaim.Value);
        }

        [Fact]
        public void GenerateToken_ContainsEmailClaim()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                IdentificationNumber = "123",
                PasswordHash = "DummyHash"
            };

            // Act
            var token = _tokenGenerator.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            
            Assert.NotNull(emailClaim);
            Assert.Equal("john@test.com", emailClaim.Value);
        }

        [Fact]
        public async Task GenerateToken_UserWithPermissions_ContainsPermissionClaims()
        {
            // Arrange
            var group = new Group { Id = 1, Name = "TestGroup" };
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                IdentificationNumber = "123",
                PasswordHash = "DummyHash"
            };
            var permission1 = new Permission { Id = 1, Name = "READ_USERS" };
            var permission2 = new Permission { Id = 2, Name = "WRITE_USERS" };

            await _context.Groups.AddAsync(group);
            await _context.Users.AddAsync(user);
            await _context.Permissions.AddRangeAsync(permission1, permission2);
            await _context.SaveChangesAsync();

            await _context.UserGroups.AddAsync(new UserGroup { UserId = 1, GroupId = 1 });
            await _context.GroupPermissions.AddRangeAsync(
                new GroupPermission { GroupId = 1, PermissionId = 1 },
                new GroupPermission { GroupId = 1, PermissionId = 2 }
            );
            await _context.SaveChangesAsync();

            // Act
            var token = _tokenGenerator.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var permissionClaims = jwtToken.Claims.Where(c => c.Type == "permission").ToList();
            
            Assert.Equal(2, permissionClaims.Count);
            Assert.Contains(permissionClaims, c => c.Value == "READ_USERS");
            Assert.Contains(permissionClaims, c => c.Value == "WRITE_USERS");
        }

        [Fact]
        public async Task GenerateToken_UserInMultipleGroups_CombinesPermissions()
        {
            // Arrange
            var group1 = new Group { Id = 1, Name = "Group1" };
            var group2 = new Group { Id = 2, Name = "Group2" };
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                IdentificationNumber = "123",
                PasswordHash = "DummyHash"
            };
            var permission1 = new Permission { Id = 1, Name = "READ_USERS" };
            var permission2 = new Permission { Id = 2, Name = "WRITE_USERS" };

            await _context.Groups.AddRangeAsync(group1, group2);
            await _context.Users.AddAsync(user);
            await _context.Permissions.AddRangeAsync(permission1, permission2);
            await _context.SaveChangesAsync();

            await _context.UserGroups.AddRangeAsync(
                new UserGroup { UserId = 1, GroupId = 1 },
                new UserGroup { UserId = 1, GroupId = 2 }
            );
            await _context.GroupPermissions.AddRangeAsync(
                new GroupPermission { GroupId = 1, PermissionId = 1 },
                new GroupPermission { GroupId = 2, PermissionId = 2 }
            );
            await _context.SaveChangesAsync();

            // Act
            var token = _tokenGenerator.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var permissionClaims = jwtToken.Claims.Where(c => c.Type == "permission").ToList();
            
            Assert.Equal(2, permissionClaims.Count);
            Assert.Contains(permissionClaims, c => c.Value == "READ_USERS");
            Assert.Contains(permissionClaims, c => c.Value == "WRITE_USERS");
        }

        [Fact]
        public async Task GenerateToken_DuplicatePermissions_IncludesOnlyDistinct()
        {
            // Arrange
            var group1 = new Group { Id = 1, Name = "Group1" };
            var group2 = new Group { Id = 2, Name = "Group2" };
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                IdentificationNumber = "123",
                PasswordHash = "DummyHash"
            };
            var permission = new Permission { Id = 1, Name = "READ_USERS" };

            await _context.Groups.AddRangeAsync(group1, group2);
            await _context.Users.AddAsync(user);
            await _context.Permissions.AddAsync(permission);
            await _context.SaveChangesAsync();

            await _context.UserGroups.AddRangeAsync(
                new UserGroup { UserId = 1, GroupId = 1 },
                new UserGroup { UserId = 1, GroupId = 2 }
            );
            await _context.GroupPermissions.AddRangeAsync(
                new GroupPermission { GroupId = 1, PermissionId = 1 },
                new GroupPermission { GroupId = 2, PermissionId = 1 }
            );
            await _context.SaveChangesAsync();

            // Act
            var token = _tokenGenerator.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var permissionClaims = jwtToken.Claims.Where(c => c.Type == "permission").ToList();
            
            Assert.Single(permissionClaims);
            Assert.Equal("READ_USERS", permissionClaims[0].Value);
        }

        [Fact]
        public void GenerateToken_UserWithoutPermissions_GeneratesTokenWithoutPermissionClaims()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                IdentificationNumber = "123",
                PasswordHash = "DummyHash"
            };

            // Act
            var token = _tokenGenerator.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var permissionClaims = jwtToken.Claims.Where(c => c.Type == "permission").ToList();
            
            Assert.Empty(permissionClaims);
        }

        [Fact]
        public void GenerateToken_TokenHasCorrectIssuer()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                IdentificationNumber = "123",
                PasswordHash = "DummyHash"
            };

            // Act
            var token = _tokenGenerator.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            Assert.Equal("TestIssuer", jwtToken.Issuer);
        }

        [Fact]
        public void GenerateToken_TokenHasCorrectAudience()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                IdentificationNumber = "123",
                PasswordHash = "DummyHash"
            };

            // Act
            var token = _tokenGenerator.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            Assert.Contains("TestAudience", jwtToken.Audiences);
        }

        [Fact]
        public void GenerateToken_TokenHasExpirationTime()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                IdentificationNumber = "123",
                PasswordHash = "DummyHash"
            };

            var beforeGeneration = DateTime.UtcNow;

            // Act
            var token = _tokenGenerator.GenerateToken(user);

            var afterGeneration = DateTime.UtcNow;

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            Assert.True(jwtToken.ValidTo > beforeGeneration.AddMinutes(59));
            Assert.True(jwtToken.ValidTo < afterGeneration.AddMinutes(61));
        }

        [Fact]
        public void GenerateToken_DifferentUsers_GenerateDifferentTokens()
        {
            // Arrange
            var user1 = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                IdentificationNumber = "123",
                PasswordHash = "DummyHash"
            };

            var user2 = new User
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@test.com",
                IdentificationNumber = "124",
                PasswordHash = "DummyHash"
            };

            // Act
            var token1 = _tokenGenerator.GenerateToken(user1);
            var token2 = _tokenGenerator.GenerateToken(user2);

            // Assert
            Assert.NotEqual(token1, token2);
        }

        [Fact]
        public void GenerateToken_SameUserCalledTwice_GeneratesDifferentTokensDueToTimestamp()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                IdentificationNumber = "123",
                PasswordHash = "DummyHash"
            };

            // Act
            var token1 = _tokenGenerator.GenerateToken(user);
            System.Threading.Thread.Sleep(1000); // Ensure different timestamp
            var token2 = _tokenGenerator.GenerateToken(user);

            // Assert
            Assert.NotEqual(token1, token2);
        }
    }
}
