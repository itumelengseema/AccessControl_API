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
        private readonly ILogger<VisitLogsControlle> _logger;

        public VisitLogsControlle(AppDbContext db, IMapper mapper, ILogger<VisitLogsControlle> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost("check-in")]
        public async Task<ActionResult<ApiResponse<VisitLogResponseDTO>>> CheckIn(CheckInDTO checkInDTO)
        {
            try
            {
                _logger.LogInformation("Check-in request received for UserId: {UserId}", checkInDTO?.UserId);

                if (checkInDTO == null || checkInDTO.UserId <= 0)
                {
                    _logger.LogWarning("Invalid check-in data received");
                    return BadRequest(ApiResponse<VisitLogResponseDTO>.BadRequestResponse("Invalid check-in data."));
                }

                // Validate that the user exists
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == checkInDTO.UserId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} does not exist", checkInDTO.UserId);
                    return BadRequest(ApiResponse<VisitLogResponseDTO>.BadRequestResponse($"User with ID {checkInDTO.UserId} does not exist."));
                }

                // Check if user already has an active visit log
                var existingActiveVisit = await _db.VisitLogs
                    .FirstOrDefaultAsync(v => v.UserId == checkInDTO.UserId && v.IsActive);

                if (existingActiveVisit != null)
                {
                    _logger.LogWarning("User {UserId} already has an active visit log (ID: {VisitLogId})", 
                        checkInDTO.UserId, existingActiveVisit.Id);
                    return BadRequest(ApiResponse<VisitLogResponseDTO>.BadRequestResponse(
                        $"User {user.FirstName} {user.LastName} already has an active visit. Please check them out first."));
                }

                // Create new visit log
                var visitLog = _mapper.Map<VisitLog>(checkInDTO);
                visitLog.CheckInTime = DateTime.UtcNow;
                visitLog.IsActive = true;

                _logger.LogInformation("Creating visit log for User {UserId}: {FirstName} {LastName}", 
                    user.Id, user.FirstName, user.LastName);

                _db.VisitLogs.Add(visitLog);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Visit log created successfully with ID: {VisitLogId}", visitLog.Id);

                // Load user details for response
                var visitLogWithUser = await _db.VisitLogs
                    .Include(v => v.User)
                    .FirstOrDefaultAsync(v => v.Id == visitLog.Id);

                var responseDTO = _mapper.Map<VisitLogResponseDTO>(visitLogWithUser);

                return Ok(ApiResponse<VisitLogResponseDTO>.CreatedResponse(responseDTO, "Visitor checked in successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during check-in for UserId: {UserId}", checkInDTO?.UserId);
                return StatusCode(500, ApiResponse<VisitLogResponseDTO>.InternalServerErrorResponse(
                    $"An error occurred during check-in: {ex.Message}"));
            }
        }

        [HttpPost("check-out/{visitLogId}")]
        public async Task<ActionResult<ApiResponse<VisitLogResponseDTO>>> CheckOut(int visitLogId)
        {
            try
            {
                _logger.LogInformation("Check-out request received for VisitLogId: {VisitLogId}", visitLogId);

                var visitLog = await _db.VisitLogs
                    .Include(v => v.User)
                    .FirstOrDefaultAsync(v => v.Id == visitLogId && v.IsActive);
                
                if (visitLog == null)
                {
                    _logger.LogWarning("Active visit log with ID {VisitLogId} not found", visitLogId);
                    return NotFound(ApiResponse<VisitLogResponseDTO>.NotFoundResponse("Active visit log not found."));
                }

                visitLog.CheckOutTime = DateTime.UtcNow;
                visitLog.IsActive = false;

                _db.VisitLogs.Update(visitLog);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Visitor checked out successfully: {VisitLogId}", visitLogId);
                
                return Ok(ApiResponse<VisitLogResponseDTO>.SuccessResponse(
                    _mapper.Map<VisitLogResponseDTO>(visitLog), "Visitor checked out successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during check-out for VisitLogId: {VisitLogId}", visitLogId);
                return StatusCode(500, ApiResponse<VisitLogResponseDTO>.InternalServerErrorResponse(
                    $"An error occurred during check-out: {ex.Message}"));
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<List<VisitLogResponseDTO>>>> ActiveVisitors()
        {
            try
            {
                _logger.LogInformation("Fetching active visitors");

                var activeVisitLogs = await _db.VisitLogs
                    .Include(v => v.User) // Include user details
                    .Where(v => v.IsActive)
                    .OrderByDescending(v => v.CheckInTime)
                    .ToListAsync();
                
                var visitLogDTOs = _mapper.Map<List<VisitLogResponseDTO>>(activeVisitLogs);

                _logger.LogInformation("Found {Count} active visitors", visitLogDTOs.Count);
                
                return Ok(ApiResponse<List<VisitLogResponseDTO>>.SuccessResponse(
                    visitLogDTOs, "Active visit logs retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active visitors");
                return StatusCode(500, ApiResponse<List<VisitLogResponseDTO>>.InternalServerErrorResponse(
                    $"An error occurred while retrieving active visitors: {ex.Message}"));
            }
        }
    }
}
