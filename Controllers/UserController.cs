using AccessControl_API.Data;
using AccessControl_API.Models;
using AccessControl_API.Models.DTO;
using AccessControl_API.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccessControl_API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<UserController> _logger;

        public UserController(AppDbContext db, IMapper mapper, ILogger<UserController> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserDTO>>> CreateUser(UserCreateUpdateDTO userDto)
        {
            try
            {
                // Validate model state
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(ApiResponse<UserDTO>.BadRequestResponse(
                        "Validation failed. Please check your input.", 
                        errors));
                }

                if (userDto == null)
                {
                    return BadRequest(ApiResponse<UserDTO>.BadRequestResponse("User data is required."));
                }

                _logger.LogInformation("Creating user: {FirstName} {LastName}, Email: {Email}, GroupId: {GroupId}", 
                    userDto.FirstName, userDto.LastName, userDto.Email, userDto.GroupId);

                // Check if group exists
                var groupExists = await _db.Groups.AnyAsync(g => g.Id == userDto.GroupId);
                if (!groupExists)
                {
                    _logger.LogError("Group with ID {GroupId} does not exist", userDto.GroupId);
                    return BadRequest(ApiResponse<UserDTO>.BadRequestResponse($"Group with ID {userDto.GroupId} does not exist."));
                }

                // Check if user with same email already exists
                var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("User with email {Email} already exists", userDto.Email);
                    return BadRequest(ApiResponse<UserDTO>.BadRequestResponse($"User with email {userDto.Email} already exists."));
                }

                // Check if user with same ID number already exists
                var existingIdUser = await _db.Users.FirstOrDefaultAsync(u => u.IdentificationNumber == userDto.IdentificationNumber);
                if (existingIdUser != null)
                {
                    _logger.LogWarning("User with ID number {IdNumber} already exists", userDto.IdentificationNumber);
                    return BadRequest(ApiResponse<UserDTO>.BadRequestResponse($"User with ID number {userDto.IdentificationNumber} already exists."));
                }

                // Map to User entity
                var user = _mapper.Map<User>(userDto);
                
                // Generate a default password hash for visitors (they won't login anyway)
                user.PasswordHash = PasswordHasher.Hash("Visitor@123");
                
                _logger.LogInformation("Adding user to database");
                
                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                _logger.LogInformation("User created with ID: {UserId}", user.Id);

                // Add user to group
                var userGroup = new UserGroup
                {
                    UserId = user.Id,
                    GroupId = userDto.GroupId
                };

                _db.UserGroups.Add(userGroup);
                await _db.SaveChangesAsync();

                _logger.LogInformation("User added to group {GroupId}", userDto.GroupId);

                // Reload user with groups for response
                var createdUser = await _db.Users
                    .Include(u => u.UserGroups)
                    .ThenInclude(ug => ug.Group)
                    .FirstOrDefaultAsync(u => u.Id == user.Id);

                var responseDto = _mapper.Map<UserDTO>(createdUser);
                
                return Ok(ApiResponse<UserDTO>.CreatedResponse(responseDto, "User created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<UserDTO>.InternalServerErrorResponse($"An error occurred while creating the user: {ex.Message}"));
            }
        }

        [HttpPost("{id:int}")]
        public async Task<ActionResult<ApiResponse<UserDTO>>> UpdateUser(int id, UserCreateUpdateDTO userDto)
        {
            try
            {
                // Validate model state
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(ApiResponse<UserDTO>.BadRequestResponse(
                        "Validation failed. Please check your input.", 
                        errors));
                }

                var user = await _db.Users
                    .Include(u => u.UserGroups)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound(ApiResponse<UserDTO>.NotFoundResponse("User not found."));
                }

                // Update basic fields
                user.FirstName = userDto.FirstName;
                user.LastName = userDto.LastName;
                user.Email = userDto.Email;
                user.IdentificationNumber = userDto.IdentificationNumber;

                // Update group membership
                // Remove existing groups
                _db.UserGroups.RemoveRange(user.UserGroups);
                
                // Add new group
                var userGroup = new UserGroup
                {
                    UserId = user.Id,
                    GroupId = userDto.GroupId
                };
                _db.UserGroups.Add(userGroup);

                await _db.SaveChangesAsync();

                // Reload with groups
                var updatedUser = await _db.Users
                    .Include(u => u.UserGroups)
                    .ThenInclude(ug => ug.Group)
                    .FirstOrDefaultAsync(u => u.Id == id);

                var responseDto = _mapper.Map<UserDTO>(updatedUser);
                return Ok(ApiResponse<UserDTO>.SuccessResponse(responseDto, "User updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}: {Message}", id, ex.Message);
                return StatusCode(500, ApiResponse<UserDTO>.InternalServerErrorResponse($"An error occurred while updating the user: {ex.Message}"));
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteUser(int id)
        {
            try
            {
                var user = await _db.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse<object>.NotFoundResponse("User not found."));
                }
                
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();
                
                return Ok(ApiResponse<object>.SuccessResponse(null!, "User deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}: {Message}", id, ex.Message);
                return StatusCode(500, ApiResponse<object>.InternalServerErrorResponse($"An error occurred while deleting the user: {ex.Message}"));
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<UserDTO>>>> GetAllUsers()
        {
            try
            {
                var users = await _db.Users
                    .Include(u => u.UserGroups)
                    .ThenInclude(ug => ug.Group)
                    .ToListAsync();
                    
                var userDtos = _mapper.Map<List<UserDTO>>(users);
                return Ok(ApiResponse<List<UserDTO>>.SuccessResponse(userDtos, "Users retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<List<UserDTO>>.InternalServerErrorResponse($"An error occurred while retrieving users: {ex.Message}"));
            }
        }

        [HttpGet("count")]
        public async Task<ActionResult<ApiResponse<int>>> GetUserCount()
        {
            try
            {
                var count = await _db.Users.CountAsync();
                return Ok(ApiResponse<int>.SuccessResponse(count, "User count retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user count: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<int>.InternalServerErrorResponse($"An error occurred while getting user count: {ex.Message}"));
            }
        }
    }
}
