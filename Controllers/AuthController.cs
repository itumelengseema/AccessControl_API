using AccessControl_API.Data;
using AccessControl_API.Models.DTO;
using AccessControl_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccessControl_API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly AppDbContext _db;

        public AuthController(IAuthService authService, AppDbContext db)
        {
            _authService = authService;
            _db = db;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<UserDTO>>> Register([FromBody] RegistrationRequestDTO registrationRequestDTO)
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

            // Check if email exists first
            var emailExists = await _authService.IsEmailExistAsync(registrationRequestDTO.Email);
            if (emailExists)
            {
                return BadRequest(ApiResponse<UserDTO>.BadRequestResponse("Email already exists"));
            }

            // Check if group exists
            var group = await _db.Groups.FindAsync(registrationRequestDTO.GroupId);
            if (group == null)
            {
                var availableGroups = await _db.Groups.Select(g => new { g.Id, g.Name }).ToListAsync();
                var groupsList = string.Join(", ", availableGroups.Select(g => $"{g.Name} (ID: {g.Id})"));
                return BadRequest(ApiResponse<UserDTO>.BadRequestResponse(
                    $"Invalid group ID: {registrationRequestDTO.GroupId}. Available groups: {groupsList}"));
            }

            var result = await _authService.RegisterAsync(registrationRequestDTO);
            
            if (result == null)
            {
                return BadRequest(ApiResponse<UserDTO>.BadRequestResponse("Registration failed"));
            }

            return CreatedAtAction(nameof(Register), ApiResponse<UserDTO>.CreatedResponse(result, "User registered successfully"));
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDTO>>> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            // Validate model state
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<LoginResponseDTO>.BadRequestResponse(
                    "Validation failed. Please check your input.", 
                    errors));
            }

            var result = await _authService.LoginAsync(loginRequestDTO);
            
            if (result == null)
            {
                return Unauthorized(ApiResponse<LoginResponseDTO>.UnauthorizedResponse("Invalid credentials"));
            }
            
            return Ok(ApiResponse<LoginResponseDTO>.SuccessResponse(result, "Login successful"));
        }
    }
}
