using AccessControl_API.Data;
using AccessControl_API.Models;
using AccessControl_API.Models.DTO;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccessControl_API.Controllers
{
    [Route("api/vist-logs")]
    [ApiController]
    public class VisitLogsControlle : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        public VisitLogsControlle(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpPost("check-in")]
        public async Task<ActionResult<ApiResponse<VisitLogResponseDTO>>> CheckIn(CheckInDTO checkInDTO)
        {
            if (checkInDTO == null || checkInDTO.UserId <= 0)
            {
                return BadRequest(ApiResponse<VisitLogResponseDTO>.BadRequestResponse("Invalid check-in data."));
            }

            // Validate that the user exists
            var userExists = await _db.Users.AnyAsync(u => u.Id == checkInDTO.UserId);
            if (!userExists)
            {
                return BadRequest(ApiResponse<VisitLogResponseDTO>.BadRequestResponse($"User with ID {checkInDTO.UserId} does not exist."));
            }

            var VistLog = _mapper.Map<VisitLog>(checkInDTO);
            VistLog.CheckInTime = DateTime.UtcNow;
            VistLog.IsActive = true;

            _db.VisitLogs.Add(VistLog);
            await _db.SaveChangesAsync();

            return Ok(ApiResponse<VisitLogResponseDTO>.CreatedResponse(_mapper.Map<VisitLogResponseDTO>(VistLog), "Vistor Checked In Successfully"));
        }
    }
}
