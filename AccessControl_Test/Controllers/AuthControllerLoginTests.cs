using AccessControl_API.Controllers;
using AccessControl_API.Data;
using AccessControl_API.Models;
using AccessControl_API.Models.DTO;
using AccessControl_API.Services;
using AccessControl_API.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AccessControl_Test.Controllers
{
    public class AuthControllerLoginStatusTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AppDbContext _context;
        private readonly AuthController _controller;

        public AuthControllerLoginStatusTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new AppDbContext(options);
            _controller = new AuthController(_mockAuthService.Object, _context);
        }

        [Fact]
        public async Task Login_UnapprovedUser_Returns403Forbidden()
        {
            // Arrange
            var loginRequest = new LoginRequestDTO
            {
                Email = "unapproved@example.com",
                Password = "Password123"
            };

            var loginResponse = new LoginResponseDTO
            {
                Result = LoginResult.AccountNotApproved,
                Message = "Your account is pending approval by an administrator. You will be notified once your account is approved."
            };

            _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<LoginRequestDTO>()))
                .ReturnsAsync(loginResponse);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(403, objectResult.StatusCode); // Forbidden
            
            var response = Assert.IsType<ApiResponse<LoginResponseDTO>>(objectResult.Value);
            Assert.False(response.Success);
            Assert.Contains("pending approval", response.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Login_InvalidCredentials_Returns401Unauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequestDTO
            {
                Email = "wrong@example.com",
                Password = "WrongPassword"
            };

            var loginResponse = new LoginResponseDTO
            {
                Result = LoginResult.InvalidCredentials,
                Message = "Invalid email or password."
            };

            _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<LoginRequestDTO>()))
                .ReturnsAsync(loginResponse);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var objectResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal(401, objectResult.StatusCode);
            
            var response = Assert.IsType<ApiResponse<LoginResponseDTO>>(objectResult.Value);
            Assert.False(response.Success);
        }

        [Fact]
        public async Task Login_SuccessfulLogin_Returns200Ok()
        {
            // Arrange
            var loginRequest = new LoginRequestDTO
            {
                Email = "approved@example.com",
                Password = "Password123"
            };

            var loginResponse = new LoginResponseDTO
            {
                Result = LoginResult.Success,
                Message = "Login successful.",
                User = new UserDTO
                {
                    Id = 1,
                    Email = "approved@example.com",
                    FirstName = "John",
                    LastName = "Doe"
                },
                Token = "fake-jwt-token",
                Permissions = new List<string> { "READ_USERS" }
            };

            _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<LoginRequestDTO>()))
                .ReturnsAsync(loginResponse);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            
            var response = Assert.IsType<ApiResponse<LoginResponseDTO>>(okResult.Value);
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal("approved@example.com", response.Data.User!.Email);
        }
    }
}
