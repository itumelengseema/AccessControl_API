using AccessControl_API.Data;
using AccessControl_API.Models;
using AccessControl_API.Models.DTO;
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

        public UserController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserDTO>>> CreateUser(UserDTO userDto)
        {
            if (userDto == null)
            {
                return BadRequest(ApiResponse<UserDTO>.BadRequestResponse("User data is required."));
            }



            var user = _mapper.Map<User>(userDto);
            user.Id = 0; // Ensure EF treats this as a new entity
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var responseDto = _mapper.Map<UserDTO>(user);
            return Ok(ApiResponse<UserDTO>.CreatedResponse(responseDto, "User created successfully"));
        }

        [HttpPost("id: int")]
        public async Task<ActionResult<ApiResponse<UserDTO>>> UpdateUser(UserCreateUpdateDTO userDto, int id)
        {
            var user = await _db.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(ApiResponse<UserDTO>.NotFoundResponse("User not found."));
            }

            _mapper.Map(userDto, user);
            await _db.SaveChangesAsync();

            var responseDto = _mapper.Map<UserDTO>(user);
            return Ok(ApiResponse<UserDTO>.SuccessResponse(responseDto, "User updated successfully"));
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteUser(int id)
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

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<UserDTO>>>> GetAllUsers()
        {
            var users = await _db.Users.ToListAsync();
            var userDtos = _mapper.Map<List<UserDTO>>(users);
            return Ok(ApiResponse<List<UserDTO>>.SuccessResponse(userDtos, "Users retrieved successfully"));
        }

        [HttpGet("count")]
        public async Task<ActionResult<ApiResponse<int>>> GetUserCount()
        {
            var count = await _db.Users.CountAsync();
            return Ok(ApiResponse<int>.SuccessResponse(count, "User count retrieved successfully"));

        }
    }
}
