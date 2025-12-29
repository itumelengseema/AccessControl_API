using AccessControl_API.Models.DTO;
using AccessControl_Web.Services.IServices;

namespace AccessControl_Web.Services
{
    public class VisitLogService : IVisitLogService
    {
        private readonly IBaseServices _baseService;

        public VisitLogService(IBaseServices baseService)
        {
            _baseService = baseService;
        }

        public async Task<ApiResponse<VisitLogResponseDTO>?> CheckInAsync(CheckInDTO checkInDto)
        {
            return await _baseService.SendAsync<ApiResponse<VisitLogResponseDTO>>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Url = SD.AccessControlAPIBase + SD.VisitLogCheckIn,
                Data = checkInDto
            });
        }

        public async Task<ApiResponse<VisitLogResponseDTO>?> CheckOutAsync(int visitLogId)
        {
            return await _baseService.SendAsync<ApiResponse<VisitLogResponseDTO>>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Url = SD.AccessControlAPIBase + SD.VisitLogCheckOut + visitLogId
            });
        }

        public async Task<ApiResponse<List<VisitLogResponseDTO>>?> GetActiveVisitorsAsync()
        {
            return await _baseService.SendAsync<ApiResponse<List<VisitLogResponseDTO>>>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = SD.AccessControlAPIBase + SD.VisitLogActive
            });
        }
    }
}
