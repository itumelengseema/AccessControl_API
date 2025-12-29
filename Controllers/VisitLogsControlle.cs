using AccessControl_API.Data;
using AccessControl_API.Models;
using AccessControl_API.Models.DTO;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccessControl_API.Controllers
{
    [Route("api/vist-logs")]
    [ApiController]
    [Authorize]
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

            // Load user details for response
            var visitLogWithUser = await _db.VisitLogs
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.Id == VistLog.Id);

            var responseDTO = _mapper.Map<VisitLogResponseDTO>(visitLogWithUser);

            return Ok(ApiResponse<VisitLogResponseDTO>.CreatedResponse(responseDTO, "Vistor Checked In Successfully"));
        }

        [HttpPost("check-out/{visitLogId}")]
        public async Task<ActionResult<ApiResponse<VisitLogResponseDTO>>> CheckOut(int visitLogId)
        {
            var visitLog = await _db.VisitLogs
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.Id == visitLogId && v.IsActive);
                
            if (visitLog == null)
            {
                return NotFound(ApiResponse<VisitLogResponseDTO>.BadRequestResponse("Active visit log not found."));
            }
            
            visitLog.CheckOutTime = DateTime.UtcNow;
            visitLog.IsActive = false;
            _db.VisitLogs.Update(visitLog);
            await _db.SaveChangesAsync();
            
            return Ok(ApiResponse<VisitLogResponseDTO>.SuccessResponse(_mapper.Map<VisitLogResponseDTO>(visitLog), "Vistor Checked Out Successfully"));
        }

        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<List<VisitLogResponseDTO>>>> ActiveVisitors()
        {
            var activeVisitLogs = await _db.VisitLogs
                .Include(v => v.User) // Include user details
                .Where(v => v.IsActive)
                .ToListAsync();
                
            var visitLogDTOs = _mapper.Map<List<VisitLogResponseDTO>>(activeVisitLogs);
            
            return Ok(ApiResponse<List<VisitLogResponseDTO>>.SuccessResponse(visitLogDTOs, "Active visit logs retrieved successfully."));
        }
    }
}
