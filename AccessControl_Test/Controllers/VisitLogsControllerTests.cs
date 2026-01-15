using AccessControl_API.Controllers;
using AccessControl_API.Data;
using AccessControl_API.Models;
using AccessControl_API.Models.DTO;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace AccessControl_Test.Controllers
{
    public class VisitLogsControllerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<VisitLogsControlle>> _mockLogger;
        private readonly VisitLogsControlle _controller;

        public VisitLogsControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<VisitLogsControlle>>();
            _controller = new VisitLogsControlle(_context, _mockMapper.Object, _mockLogger.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task CheckIn_ValidUser_ReturnsOkResult()
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
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var checkInDto = new CheckInDTO
            {
                UserId = 1,
                CarRegistrationNumber = "ABC123"
            };

            var visitLog = new VisitLog
            {
                Id = 1,
                UserId = 1,
                CarRegistrationNumber = "ABC123",
                CheckInTime = DateTime.UtcNow,
                IsActive = true
            };

            _mockMapper.Setup(m => m.Map<VisitLog>(It.IsAny<CheckInDTO>()))
                .Returns(visitLog);

            _mockMapper.Setup(m => m.Map<VisitLogResponseDTO>(It.IsAny<VisitLog>()))
                .Returns(new VisitLogResponseDTO
                {
                    Id = 1,
                    UserId = 1,
                    UserFirstName = "John",
                    UserLastName = "Doe",
                    CarRegistrationNumber = "ABC123",
                    CheckInTime = visitLog.CheckInTime,
                    IsActive = true
                });

            // Act
            var result = await _controller.CheckIn(checkInDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<VisitLogResponseDTO>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<VisitLogResponseDTO>>(okResult.Value);
            Assert.Equal(201, response.StatusCode);
            Assert.Contains("checked in successfully", response.Message);
        }

        [Fact]
        public async Task CheckIn_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("UserId", "UserId is required");
            var checkInDto = new CheckInDTO { UserId = 0 };

            // Act
            var result = await _controller.CheckIn(checkInDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<VisitLogResponseDTO>>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<VisitLogResponseDTO>>(badRequestResult.Value);
            Assert.Equal(400, response.StatusCode);
            Assert.Contains("Validation failed", response.Message);
        }

        [Fact]
        public async Task CheckIn_NullCheckInDTO_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.CheckIn(null!);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<VisitLogResponseDTO>>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<VisitLogResponseDTO>>(badRequestResult.Value);
            Assert.Equal(400, response.StatusCode);
            Assert.Contains("Invalid check-in data", response.Message);
        }

        [Fact]
        public async Task CheckIn_InvalidUserId_ReturnsBadRequest()
        {
            // Arrange
            var checkInDto = new CheckInDTO { UserId = 0 };

            // Act
            var result = await _controller.CheckIn(checkInDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<VisitLogResponseDTO>>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<VisitLogResponseDTO>>(badRequestResult.Value);
            Assert.Equal(400, response.StatusCode);
        }

        [Fact]
        public async Task CheckIn_UserNotFound_ReturnsBadRequest()
        {
            // Arrange
            var checkInDto = new CheckInDTO
            {
                UserId = 999
            };

            // Act
            var result = await _controller.CheckIn(checkInDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<VisitLogResponseDTO>>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<VisitLogResponseDTO>>(badRequestResult.Value);
            Assert.Equal(400, response.StatusCode);
            Assert.Contains("does not exist", response.Message);
        }

        [Fact]
        public async Task CheckIn_UserAlreadyCheckedIn_ReturnsBadRequest()
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
            await _context.Users.AddAsync(user);

            var existingVisitLog = new VisitLog
            {
                UserId = 1,
                CheckInTime = DateTime.UtcNow.AddHours(-1),
                IsActive = true
            };
            await _context.VisitLogs.AddAsync(existingVisitLog);
            await _context.SaveChangesAsync();

            var checkInDto = new CheckInDTO
            {
                UserId = 1
            };

            // Act
            var result = await _controller.CheckIn(checkInDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<VisitLogResponseDTO>>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<VisitLogResponseDTO>>(badRequestResult.Value);
            Assert.Equal(400, response.StatusCode);
            Assert.Contains("already has an active visit", response.Message);
        }

        [Fact]
        public async Task CheckOut_ValidVisitLog_ReturnsOkResult()
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
            await _context.Users.AddAsync(user);

            var visitLog = new VisitLog
            {
                Id = 1,
                UserId = 1,
                CheckInTime = DateTime.UtcNow.AddHours(-2),
                IsActive = true,
                User = user
            };
            await _context.VisitLogs.AddAsync(visitLog);
            await _context.SaveChangesAsync();

            _mockMapper.Setup(m => m.Map<VisitLogResponseDTO>(It.IsAny<VisitLog>()))
                .Returns(new VisitLogResponseDTO
                {
                    Id = 1,
                    UserId = 1,
                    UserFirstName = "John",
                    UserLastName = "Doe",
                    CheckInTime = visitLog.CheckInTime,
                    CheckOutTime = DateTime.UtcNow,
                    IsActive = false
                });

            // Act
            var result = await _controller.CheckOut(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<VisitLogResponseDTO>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<VisitLogResponseDTO>>(okResult.Value);
            Assert.Equal(200, response.StatusCode);
            Assert.Contains("checked out successfully", response.Message);

            // Verify visit log is no longer active
            var updatedVisitLog = await _context.VisitLogs.FindAsync(1);
            Assert.NotNull(updatedVisitLog);
            Assert.False(updatedVisitLog!.IsActive);
            Assert.NotNull(updatedVisitLog.CheckOutTime);
        }

        [Fact]
        public async Task CheckOut_VisitLogNotFound_ReturnsNotFound()
        {
            // Act
            var result = await _controller.CheckOut(999);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<VisitLogResponseDTO>>>(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<VisitLogResponseDTO>>(notFoundResult.Value);
            Assert.Equal(404, response.StatusCode);
            Assert.Contains("not found", response.Message);
        }

        [Fact]
        public async Task CheckOut_InactiveVisitLog_ReturnsNotFound()
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
            await _context.Users.AddAsync(user);

            var visitLog = new VisitLog
            {
                Id = 1,
                UserId = 1,
                CheckInTime = DateTime.UtcNow.AddHours(-3),
                CheckOutTime = DateTime.UtcNow.AddHours(-1),
                IsActive = false
            };
            await _context.VisitLogs.AddAsync(visitLog);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.CheckOut(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<VisitLogResponseDTO>>>(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<VisitLogResponseDTO>>(notFoundResult.Value);
            Assert.Equal(404, response.StatusCode);
        }

        [Fact]
        public async Task ActiveVisitors_ReturnsActiveVisitLogs()
        {
            // Arrange
            var user1 = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com", IdentificationNumber = "123", PasswordHash = "DummyHash" };
            var user2 = new User { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@test.com", IdentificationNumber = "124", PasswordHash = "DummyHash" };
            await _context.Users.AddRangeAsync(user1, user2);

            var activeVisit1 = new VisitLog
            {
                Id = 1,
                UserId = 1,
                CheckInTime = DateTime.UtcNow.AddHours(-1),
                IsActive = true,
                User = user1
            };

            var activeVisit2 = new VisitLog
            {
                Id = 2,
                UserId = 2,
                CheckInTime = DateTime.UtcNow.AddMinutes(-30),
                IsActive = true,
                User = user2
            };

            var inactiveVisit = new VisitLog
            {
                Id = 3,
                UserId = 1,
                CheckInTime = DateTime.UtcNow.AddDays(-1),
                CheckOutTime = DateTime.UtcNow.AddDays(-1).AddHours(2),
                IsActive = false,
                User = user1
            };

            await _context.VisitLogs.AddRangeAsync(activeVisit1, activeVisit2, inactiveVisit);
            await _context.SaveChangesAsync();

            _mockMapper.Setup(m => m.Map<List<VisitLogResponseDTO>>(It.IsAny<List<VisitLog>>()))
                .Returns(new List<VisitLogResponseDTO>
                {
                    new VisitLogResponseDTO { Id = 2, UserId = 2, UserFirstName = "Jane", UserLastName = "Smith", IsActive = true },
                    new VisitLogResponseDTO { Id = 1, UserId = 1, UserFirstName = "John", UserLastName = "Doe", IsActive = true }
                });

            // Act
            var result = await _controller.ActiveVisitors();

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<List<VisitLogResponseDTO>>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<List<VisitLogResponseDTO>>>(okResult.Value);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(2, response.Data!.Count);
            Assert.All(response.Data, v => Assert.True(v.IsActive));
        }

        [Fact]
        public async Task ActiveVisitors_NoActiveVisitors_ReturnsEmptyList()
        {
            // Arrange
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com", IdentificationNumber = "123", PasswordHash = "DummyHash" };
            await _context.Users.AddAsync(user);

            var inactiveVisit = new VisitLog
            {
                Id = 1,
                UserId = 1,
                CheckInTime = DateTime.UtcNow.AddDays(-1),
                CheckOutTime = DateTime.UtcNow.AddDays(-1).AddHours(2),
                IsActive = false
            };
            await _context.VisitLogs.AddAsync(inactiveVisit);
            await _context.SaveChangesAsync();

            _mockMapper.Setup(m => m.Map<List<VisitLogResponseDTO>>(It.IsAny<List<VisitLog>>()))
                .Returns(new List<VisitLogResponseDTO>());

            // Act
            var result = await _controller.ActiveVisitors();

            // Assert
            var actionResult = Assert.IsType<ActionResult<ApiResponse<List<VisitLogResponseDTO>>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<ApiResponse<List<VisitLogResponseDTO>>>(okResult.Value);
            Assert.Equal(200, response.StatusCode);
            Assert.Empty(response.Data!);
        }

        [Fact]
        public async Task ActiveVisitors_OrdersByCheckInTimeDescending()
        {
            // Arrange
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com", IdentificationNumber = "123", PasswordHash = "DummyHash" };
            await _context.Users.AddAsync(user);

            var visit1 = new VisitLog
            {
                Id = 1,
                UserId = 1,
                CheckInTime = DateTime.UtcNow.AddHours(-3),
                IsActive = true,
                User = user
            };

            var visit2 = new VisitLog
            {
                Id = 2,
                UserId = 1,
                CheckInTime = DateTime.UtcNow.AddMinutes(-5),
                IsActive = true,
                User = user
            };

            await _context.VisitLogs.AddRangeAsync(visit1, visit2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.ActiveVisitors();

            // Assert
            var logs = await _context.VisitLogs
                .Where(v => v.IsActive)
                .OrderByDescending(v => v.CheckInTime)
                .ToListAsync();

            Assert.Equal(2, logs[0].Id); // Latest should be first
            Assert.Equal(1, logs[1].Id);
        }
    }
}
