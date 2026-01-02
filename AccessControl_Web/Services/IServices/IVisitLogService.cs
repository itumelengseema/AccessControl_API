using AccessControl_API.Models.DTO;

namespace AccessControl_Web.Services.IServices
{
    public interface IVisitLogService
    {
        Task<ApiResponse<VisitLogResponseDTO>?> CheckInAsync(CheckInDTO checkInDto);
        Task<ApiResponse<VisitLogResponseDTO>?> CheckOutAsync(int visitLogId);
        Task<ApiResponse<List<VisitLogResponseDTO>>?> GetActiveVisitorsAsync();
    }
}
