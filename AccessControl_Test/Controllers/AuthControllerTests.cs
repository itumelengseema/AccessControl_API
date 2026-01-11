using AccessControl_API.Controllers;
using AccessControl_API.Data;
using AccessControl_API.Models;
using AccessControl_API.Models.DTO;
using AccessControl_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AccessControl_Test.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AppDbContext _context;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new AppDbContext(options);
            _controller = new AuthController(_mockAuthService.Object, _context);
        }

        [Fact]
        public async Task Register_ValidRequest_ReturnsCreatedResult()
        {
            // Arrange
            var group = new Group { Id = 1, Name = "TestGroup" };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            var registrationRequest = new RegistrationRequestDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IdentificationNumber = "123456",
                Password = "Password123",
                GroupId = 1
            };

            var userDTO = new UserDTO
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IdentificationNumber = "123456",
                GroupId = 1,
                GroupName = "TestGroup"
            };

            _mockAuthService.Setup(s => s.IsEmailExistAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            
            _mockAuthService.Setup(s => s.RegisterAsync(It.IsAny<RegistrationRequestDTO>()))
                .ReturnsAsync(userDTO);

            // Act
            var result = await _controller.Register(registrationRequest);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UserDTO>>(actionResult.Value);
            Assert.Equal(201, response.StatusCode);
            Assert.Equal("john@example.com", response.Data!.Email);
        }

        [Fact]
        public async Task Register_EmailAlreadyExists_ReturnsBadRequest()
        {
            // Arrange
            var registrationRequest = new RegistrationRequestDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "existing@example.com",
                IdentificationNumber = "123456",
                Password = "Password123",
                GroupId = 1
            };

            _mockAuthService.Setup(s => s.IsEmailExistAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Register(registrationRequest);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UserDTO>>(actionResult.Value);
            Assert.Equal(400, response.StatusCode);
            Assert.Contains("Email already exists", response.Message);
        }

        [Fact]
        public async Task Register_InvalidGroupId_ReturnsBadRequest()
        {
            // Arrange
            var registrationRequest = new RegistrationRequestDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IdentificationNumber = "123456",
                Password = "Password123",
                GroupId = 999 // Non-existent group
            };

            _mockAuthService.Setup(s => s.IsEmailExistAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Register(registrationRequest);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UserDTO>>(actionResult.Value);
            Assert.Equal(400, response.StatusCode);
            Assert.Contains("Invalid group ID", response.Message);
        }

        [Fact]
        public async Task Register_RegistrationFails_ReturnsBadRequest()
        {
            // Arrange
            var group = new Group { Id = 1, Name = "TestGroup" };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            var registrationRequest = new RegistrationRequestDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IdentificationNumber = "123456",
                Password = "Password123",
                GroupId = 1
            };

            _mockAuthService.Setup(s => s.IsEmailExistAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            
            _mockAuthService.Setup(s => s.RegisterAsync(It.IsAny<RegistrationRequestDTO>()))
                .ReturnsAsync((UserDTO?)null);

            // Act
            var result = await _controller.Register(registrationRequest);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UserDTO>>(actionResult.Value);
            Assert.Equal(400, response.StatusCode);
            Assert.Contains("Registration failed", response.Message);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkResult()
        {
            // Arrange
            var loginRequest = new LoginRequestDTO
            {
                Email = "john@example.com",
                Password = "Password123"
            };

            var loginResponse = new LoginResponseDTO
            {
                User = new UserDTO
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john@example.com",
                    IdentificationNumber = "123456",
                    GroupId = 1,
                    GroupName = "TestGroup"
                },
                Token = "fake-jwt-token",
                Permissions = new List<string> { "Read", "Write" }
            };

            _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<LoginRequestDTO>()))
                .ReturnsAsync(loginResponse);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<LoginResponseDTO>>(actionResult.Value);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal("fake-jwt-token", response.Data!.Token);
            Assert.Equal("john@example.com", response.Data.User.Email);
            Assert.Contains("Read", response.Data.Permissions);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequestDTO
            {
                Email = "john@example.com",
                Password = "WrongPassword"
            };

            _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<LoginRequestDTO>()))
                .ReturnsAsync((LoginResponseDTO?)null);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var actionResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<LoginResponseDTO>>(actionResult.Value);
            Assert.Equal(401, response.StatusCode);
            Assert.Contains("Invalid credentials", response.Message);
        }
    }
}
