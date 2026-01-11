using AccessControl_API.Data;
using AccessControl_API.Models;
using AccessControl_API.Models.DTO;
using AccessControl_API.Services;
using AccessControl_API.Utilities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AccessControl_Test.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IMapper> _mockMapper;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly AppDbContext _context;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // Setup InMemory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _mockMapper = new Mock<IMapper>();
            
            // Setup IConfiguration for JwtTokenGenerator
            var configDict = new Dictionary<string, string?>
            {
                {"JwtSettings:Key", "YourSuperSecretKeyHere_MustBeAtLeast32CharactersLongForTesting"},
                {"JwtSettings:Issuer", "TestIssuer"},
                {"JwtSettings:Audience", "TestAudience"},
                {"JwtSettings:DurationInMinutes", "60"}
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            _jwtTokenGenerator = new JwtTokenGenerator(configuration, _context);
            
            _authService = new AuthService(_context, _mockMapper.Object, _jwtTokenGenerator);
        }

        [Fact]
        public async Task IsEmailExistAsync_EmailExists_ReturnsTrue()
        {
            // Arrange
            var email = "test@example.com";
            _context.Users.Add(new User 
            { 
                FirstName = "Test",
                LastName = "User",
                Email = email,
                IdentificationNumber = "123456",
                PasswordHash = "hashedpassword"
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _authService.IsEmailExistAsync(email);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsEmailExistAsync_EmailDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var email = "nonexistent@example.com";

            // Act
            var result = await _authService.IsEmailExistAsync(email);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RegisterAsync_ValidRequest_ReturnsUserDTO()
        {
            // Arrange
            var group = new Group { Id = 1, Name = "TestGroup" };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            var registrationRequest = new RegistrationRequestDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                IdentificationNumber = "987654",
                Password = "Password123",
                GroupId = 1
            };

            var expectedUserDTO = new UserDTO
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                IdentificationNumber = "987654",
                GroupId = 1,
                GroupName = "TestGroup"
            };

            _mockMapper.Setup(m => m.Map<UserDTO>(It.IsAny<User>()))
                .Returns(expectedUserDTO);

            // Act
            var result = await _authService.RegisterAsync(registrationRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedUserDTO.Email, result.Email);
            Assert.Equal(expectedUserDTO.FirstName, result.FirstName);
            
            // Verify user was added to database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == registrationRequest.Email);
            Assert.NotNull(user);
            Assert.True(PasswordHasher.Verify(registrationRequest.Password, user.PasswordHash));
        }

        [Fact]
        public async Task RegisterAsync_EmailAlreadyExists_ReturnsNull()
        {
            // Arrange
            var existingUser = new User
            {
                FirstName = "Existing",
                LastName = "User",
                Email = "existing@example.com",
                IdentificationNumber = "111111",
                PasswordHash = "hashedpassword"
            };
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var registrationRequest = new RegistrationRequestDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "existing@example.com",
                IdentificationNumber = "222222",
                Password = "Password123",
                GroupId = 1
            };

            // Act
            var result = await _authService.RegisterAsync(registrationRequest);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task RegisterAsync_InvalidGroupId_ReturnsNull()
        {
            // Arrange
            var registrationRequest = new RegistrationRequestDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IdentificationNumber = "333333",
                Password = "Password123",
                GroupId = 999 // Non-existent group
            };

            // Act
            var result = await _authService.RegisterAsync(registrationRequest);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsLoginResponseDTO()
        {
            // Arrange
            var group = new Group { Id = 1, Name = "TestGroup" };
            var permission = new Permission { Id = 1, Name = "Read" };
            
            _context.Groups.Add(group);
            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            var passwordHash = PasswordHasher.Hash("Password123");
            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IdentificationNumber = "444444",
                PasswordHash = passwordHash
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userGroup = new UserGroup { UserId = user.Id, GroupId = group.Id };
            _context.UserGroups.Add(userGroup);
            
            var groupPermission = new GroupPermission { GroupId = group.Id, PermissionId = permission.Id };
            _context.GroupPermissions.Add(groupPermission);
            await _context.SaveChangesAsync();

            var loginRequest = new LoginRequestDTO
            {
                Email = "john@example.com",
                Password = "Password123"
            };

            var userDTO = new UserDTO
            {
                Id = user.Id,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IdentificationNumber = "444444",
                GroupId = 1,
                GroupName = "TestGroup"
            };

            _mockMapper.Setup(m => m.Map<UserDTO>(It.IsAny<User>()))
                .Returns(userDTO);

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Token);  // Token will be generated by real JwtTokenGenerator
            Assert.Equal("john@example.com", result.User.Email);
            Assert.Contains("Read", result.Permissions);
        }

        [Fact]
        public async Task LoginAsync_InvalidEmail_ReturnsNull()
        {
            // Arrange
            var loginRequest = new LoginRequestDTO
            {
                Email = "nonexistent@example.com",
                Password = "Password123"
            };

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ReturnsNull()
        {
            // Arrange
            var passwordHash = PasswordHasher.Hash("CorrectPassword");
            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IdentificationNumber = "555555",
                PasswordHash = passwordHash
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var loginRequest = new LoginRequestDTO
            {
                Email = "john@example.com",
                Password = "WrongPassword"
            };

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.Null(result);
        }
    }
}
