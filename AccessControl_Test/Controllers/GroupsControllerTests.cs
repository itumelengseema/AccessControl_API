using AccessControl_API.Controllers;
using AccessControl_API.Data;
using AccessControl_API.Models;
using AccessControl_API.Models.DTO;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AccessControl_Test.Controllers
{
    public class GroupsControllerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GroupsController _controller;

        public GroupsControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _mockMapper = new Mock<IMapper>();
            _controller = new GroupsController(_context, _mockMapper.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task CreateGroup_ValidGroup_ReturnsCreatedResult()
        {
            // Arrange
            var groupDto = new GroupDTO
            {
                Name = "Test Group"
            };

            var group = new Group
            {
                Id = 1,
                Name = "Test Group"
            };

            _mockMapper.Setup(m => m.Map<Group>(It.IsAny<GroupDTO>()))
                .Returns(group);

            _mockMapper.Setup(m => m.Map<GroupDTO>(It.IsAny<Group>()))
                .Returns(groupDto);

            // Act
            var result = await _controller.CreateGroup(groupDto);

            // Assert
            var response = Assert.IsType<ApiResponse<GroupDTO>>(result.Value);
            Assert.Equal(201, response.StatusCode);
            Assert.Equal("Group created successfully", response.Message);
        }

        [Fact]
        public async Task CreateGroup_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "Name is required");
            var groupDto = new GroupDTO { Name = "" };

            // Act
            var result = await _controller.CreateGroup(groupDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<GroupDTO>>(badRequestResult.Value);
            Assert.Equal(400, response.StatusCode);
            Assert.Contains("Validation failed", response.Message);
        }

        [Fact]
        public async Task GetGroups_ReturnsAllGroups()
        {
            // Arrange
            var groups = new List<Group>
            {
                new Group { Id = 1, Name = "Group 1" },
                new Group { Id = 2, Name = "Group 2" },
                new Group { Id = 3, Name = "Group 3" }
            };

            await _context.Groups.AddRangeAsync(groups);
            await _context.SaveChangesAsync();

            var groupDtos = groups.Select(g => new GroupDTO
            {
                Id = g.Id,
                Name = g.Name
            }).ToList();

            _mockMapper.Setup(m => m.Map<IEnumerable<GroupDTO>>(It.IsAny<List<Group>>()))
                .Returns(groupDtos);

            // Act
            var result = await _controller.GetGoups();

            // Assert
            var response = Assert.IsType<ApiResponse<IEnumerable<GroupDTO>>>(result.Value);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(3, response.Data!.Count());
        }

        [Fact]
        public async Task GetGroups_EmptyDatabase_ReturnsEmptyList()
        {
            // Arrange
            _mockMapper.Setup(m => m.Map<IEnumerable<GroupDTO>>(It.IsAny<List<Group>>()))
                .Returns(new List<GroupDTO>());

            // Act
            var result = await _controller.GetGoups();

            // Assert
            var response = Assert.IsType<ApiResponse<IEnumerable<GroupDTO>>>(result.Value);
            Assert.Equal(200, response.StatusCode);
            Assert.Empty(response.Data!);
        }

        [Fact]
        public async Task GetUsersPerGroupCount_ReturnsCorrectCounts()
        {
            // Arrange
            var group1 = new Group { Id = 1, Name = "Group 1" };
            var group2 = new Group { Id = 2, Name = "Group 2" };

            var user1 = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com", IdentificationNumber = "123", PasswordHash = "DummyHash" };
            var user2 = new User { Id = 2, FirstName = "Jane", LastName = "Doe", Email = "jane@test.com", IdentificationNumber = "124", PasswordHash = "DummyHash" };
            var user3 = new User { Id = 3, FirstName = "Bob", LastName = "Smith", Email = "bob@test.com", IdentificationNumber = "125", PasswordHash = "DummyHash" };

            await _context.Groups.AddRangeAsync(group1, group2);
            await _context.Users.AddRangeAsync(user1, user2, user3);
            await _context.SaveChangesAsync();

            await _context.UserGroups.AddRangeAsync(
                new UserGroup { UserId = 1, GroupId = 1 },
                new UserGroup { UserId = 2, GroupId = 1 },
                new UserGroup { UserId = 3, GroupId = 2 }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetUsersPerGroupCoun();

            // Assert
            var response = Assert.IsType<ApiResponse<IEnumerable<UsersPerGroupDTO>>>(result.Value);
            Assert.Equal(200, response.StatusCode);
            
            var data = response.Data!.ToList();
            Assert.Equal(2, data.Count);
            Assert.Contains(data, d => d.GroupName == "Group 1" && d.UserCount == 2);
            Assert.Contains(data, d => d.GroupName == "Group 2" && d.UserCount == 1);
        }

        [Fact]
        public async Task GetUsersPerGroupCount_GroupsWithNoUsers_ReturnsZeroCounts()
        {
            // Arrange
            var group1 = new Group { Id = 1, Name = "Empty Group" };
            await _context.Groups.AddAsync(group1);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetUsersPerGroupCoun();

            // Assert
            var response = Assert.IsType<ApiResponse<IEnumerable<UsersPerGroupDTO>>>(result.Value);
            
            var data = response.Data!.ToList();
            Assert.Single(data);
            Assert.Equal("Empty Group", data[0].GroupName);
            Assert.Equal(0, data[0].UserCount);
        }
    }
}
