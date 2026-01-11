using AccessControl_API.Controllers;
using AccessControl_API.Data;
using AccessControl_API.Models;
using AccessControl_API.Models.DTO;
using AccessControl_API.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace AccessControl_Test.Controllers
{
    public class UserControllerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<UserController>> _mockLogger;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new AppDbContext(options);
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<UserController>>();
            
            _controller = new UserController(_context, _mockMapper.Object, _mockLogger.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task CreateUser_ValidUser_ReturnsOkResult()
        {
            // Arrange
            var group = new Group { Id = 1, Name = "TestGroup" };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            var userDto = new UserCreateUpdateDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IdentificationNumber = "123456",
                GroupId = 1
            };

            var expectedUser = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IdentificationNumber = "123456",
                PasswordHash = "hashedpassword"
            };

            var expectedUserDTO = new UserDTO
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IdentificationNumber = "123456",
                GroupId = 1,
                GroupName = "TestGroup"
            };

            _mockMapper.Setup(m => m.Map<User>(It.IsAny<UserCreateUpdateDTO>()))
                .Returns(expectedUser);
            
            _mockMapper.Setup(m => m.Map<UserDTO>(It.IsAny<User>()))
                .Returns(expectedUserDTO);

            // Act
            var result = await _controller.CreateUser(userDto);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UserDTO>>(actionResult.Value);
            Assert.Equal(201, response.StatusCode);
            Assert.Equal("john@example.com", response.Data!.Email);
            Assert.Equal("TestGroup", response.Data.GroupName);
        }

        [Fact]
        public async Task CreateUser_NullUser_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.CreateUser(null!);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UserDTO>>(actionResult.Value);
            Assert.Equal(400, response.StatusCode);
            Assert.Contains("User data is required", response.Message);
        }

        [Fact]
        public async Task CreateUser_InvalidGroupId_ReturnsBadRequest()
        {
            // Arrange
            var userDto = new UserCreateUpdateDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IdentificationNumber = "123456",
                GroupId = 999 // Non-existent group
            };

            // Act
            var result = await _controller.CreateUser(userDto);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UserDTO>>(actionResult.Value);
            Assert.Equal(400, response.StatusCode);
            Assert.Contains("Group with ID 999 does not exist", response.Message);
        }

        [Fact]
        public async Task CreateUser_DuplicateEmail_ReturnsBadRequest()
        {
            // Arrange
            var group = new Group { Id = 1, Name = "TestGroup" };
            _context.Groups.Add(group);
            
            var existingUser = new User
            {
                FirstName = "Existing",
                LastName = "User",
                Email = "john@example.com",
                IdentificationNumber = "111111",
                PasswordHash = "hashedpassword"
            };
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var userDto = new UserCreateUpdateDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IdentificationNumber = "222222",
                GroupId = 1
            };

            // Act
            var result = await _controller.CreateUser(userDto);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UserDTO>>(actionResult.Value);
            Assert.Equal(400, response.StatusCode);
            Assert.Contains("User with email john@example.com already exists", response.Message);
        }

        [Fact]
        public async Task CreateUser_DuplicateIdentificationNumber_ReturnsBadRequest()
        {
            // Arrange
            var group = new Group { Id = 1, Name = "TestGroup" };
            _context.Groups.Add(group);
            
            var existingUser = new User
            {
                FirstName = "Existing",
                LastName = "User",
                Email = "existing@example.com",
                IdentificationNumber = "123456",
                PasswordHash = "hashedpassword"
            };
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var userDto = new UserCreateUpdateDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IdentificationNumber = "123456",
                GroupId = 1
            };

            // Act
            var result = await _controller.CreateUser(userDto);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UserDTO>>(actionResult.Value);
            Assert.Equal(400, response.StatusCode);
            Assert.Contains("User with ID number 123456 already exists", response.Message);
        }

        [Fact]
        public async Task UpdateUser_ValidUser_ReturnsOkResult()
        {
            // Arrange
            var group1 = new Group { Id = 1, Name = "Group1" };
            var group2 = new Group { Id = 2, Name = "Group2" };
            _context.Groups.AddRange(group1, group2);
            
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IdentificationNumber = "123456",
                PasswordHash = "hashedpassword"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userGroup = new UserGroup { UserId = 1, GroupId = 1 };
            _context.UserGroups.Add(userGroup);
            await _context.SaveChangesAsync();

            var updateDto = new UserCreateUpdateDTO
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                IdentificationNumber = "654321",
                GroupId = 2
            };

            var expectedUserDTO = new UserDTO
            {
                Id = 1,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                IdentificationNumber = "654321",
                GroupId = 2,
                GroupName = "Group2"
            };

            _mockMapper.Setup(m => m.Map<UserDTO>(It.IsAny<User>()))
                .Returns(expectedUserDTO);

            // Act
            var result = await _controller.UpdateUser(1, updateDto);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UserDTO>>(actionResult.Value);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal("Jane", response.Data!.FirstName);
            Assert.Equal("Group2", response.Data.GroupName);
        }

        [Fact]
        public async Task UpdateUser_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var updateDto = new UserCreateUpdateDTO
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                IdentificationNumber = "654321",
                GroupId = 1
            };

            // Act
            var result = await _controller.UpdateUser(999, updateDto);

            // Assert
            var actionResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<UserDTO>>(actionResult.Value);
            Assert.Equal(404, response.StatusCode);
            Assert.Contains("User not found", response.Message);
        }

        [Fact]
        public async Task DeleteUser_ValidUser_ReturnsOkResult()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IdentificationNumber = "123456",
                PasswordHash = "hashedpassword"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteUser(1);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<object>>(actionResult.Value);
            Assert.Equal(200, response.StatusCode);
            Assert.Contains("User deleted successfully", response.Message);
            
            // Verify user was deleted
            var deletedUser = await _context.Users.FindAsync(1);
            Assert.Null(deletedUser);
        }

        [Fact]
        public async Task DeleteUser_UserNotFound_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteUser(999);

            // Assert
            var actionResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<object>>(actionResult.Value);
            Assert.Equal(404, response.StatusCode);
            Assert.Contains("User not found", response.Message);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsListOfUsers()
        {
            // Arrange
            var group = new Group { Id = 1, Name = "TestGroup" };
            _context.Groups.Add(group);
            
            var user1 = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IdentificationNumber = "123456",
                PasswordHash = "hashedpassword"
            };
            var user2 = new User
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                IdentificationNumber = "654321",
                PasswordHash = "hashedpassword"
            };
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var userGroup1 = new UserGroup { UserId = 1, GroupId = 1 };
            var userGroup2 = new UserGroup { UserId = 2, GroupId = 1 };
            _context.UserGroups.AddRange(userGroup1, userGroup2);
            await _context.SaveChangesAsync();

            var userDtos = new List<UserDTO>
            {
                new UserDTO { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", IdentificationNumber = "123456", GroupId = 1, GroupName = "TestGroup" },
                new UserDTO { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", IdentificationNumber = "654321", GroupId = 1, GroupName = "TestGroup" }
            };

            _mockMapper.Setup(m => m.Map<List<UserDTO>>(It.IsAny<List<User>>()))
                .Returns(userDtos);

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<List<UserDTO>>>(actionResult.Value);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(2, response.Data!.Count);
        }

        [Fact]
        public async Task GetUserCount_ReturnsCorrectCount()
        {
            // Arrange
            var user1 = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IdentificationNumber = "123456",
                PasswordHash = "hashedpassword"
            };
            var user2 = new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                IdentificationNumber = "654321",
                PasswordHash = "hashedpassword"
            };
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetUserCount();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<int>>(actionResult.Value);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(2, response.Data);
        }

        [Fact]
        public async Task CreateUser_SetsDefaultPassword()
        {
            // Arrange
            var group = new Group { Id = 1, Name = "TestGroup" };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            var userDto = new UserCreateUpdateDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IdentificationNumber = "123456",
                GroupId = 1
            };

            _mockMapper.Setup(m => m.Map<User>(It.IsAny<UserCreateUpdateDTO>()))
                .Returns(new User
                {
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    Email = userDto.Email,
                    IdentificationNumber = userDto.IdentificationNumber
                });

            _mockMapper.Setup(m => m.Map<UserDTO>(It.IsAny<User>()))
                .Returns(new UserDTO
                {
                    Id = 1,
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    Email = userDto.Email,
                    IdentificationNumber = userDto.IdentificationNumber,
                    GroupId = 1,
                    GroupName = "TestGroup"
                });

            // Act
            await _controller.CreateUser(userDto);

            // Assert
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "john@example.com");
            Assert.NotNull(user);
            Assert.True(PasswordHasher.Verify("Visitor@123", user!.PasswordHash));
        }
    }
}
