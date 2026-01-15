using AccessControl_API.Models.DTO;
using AccessControl_Web.Services.IServices;

namespace AccessControl_Web.Services
{
    public class UserService : IUserService
    {
        private readonly IBaseServices _baseService;

        public UserService(IBaseServices baseService)
        {
            _baseService = baseService;
        }

        public async Task<ApiResponse<UserDTO>?> CreateUserAsync(UserCreateUpdateDTO userDto)
        {
            return await _baseService.SendAsync<ApiResponse<UserDTO>>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Url = SD.AccessControlAPIBase + SD.UserAPIBase,
                Data = userDto
            });
        }

        public async Task<ApiResponse<object>?> DeleteUserAsync(int id)
        {
            return await _baseService.SendAsync<ApiResponse<object>>(new ApiRequest
            {
                ApiType = SD.ApiType.DELETE,
                Url = SD.AccessControlAPIBase + SD.UserAPIBase + id
            });
        }

        public async Task<ApiResponse<List<UserDTO>>?> GetAllUsersAsync()
        {
            return await _baseService.SendAsync<ApiResponse<List<UserDTO>>>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = SD.AccessControlAPIBase + SD.UserAPIBase
            });
        }

        public async Task<ApiResponse<int>?> GetUserCountAsync()
        {
            return await _baseService.SendAsync<ApiResponse<int>>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = SD.AccessControlAPIBase + SD.UserCount
            });
        }

        public async Task<ApiResponse<UserDTO>?> UpdateUserAsync(int id, UserCreateUpdateDTO userDto)
        {
            return await _baseService.SendAsync<ApiResponse<UserDTO>>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Url = SD.AccessControlAPIBase + SD.UserAPIBase + id,
                Data = userDto
            });
        }

        public async Task<ApiResponse<List<UserDTO>>?> GetPendingApprovalsAsync()
        {
            return await _baseService.SendAsync<ApiResponse<List<UserDTO>>>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = SD.AccessControlAPIBase + SD.UserAPIBase + "pending-approvals"
            });
        }

        public async Task<ApiResponse<UserDTO>?> ApproveUserAsync(int userId)
        {
            return await _baseService.SendAsync<ApiResponse<UserDTO>>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Url = SD.AccessControlAPIBase + SD.UserAPIBase + userId + "/approve"
            });
        }

        public async Task<ApiResponse<object>?> RejectUserAsync(int userId)
        {
            return await _baseService.SendAsync<ApiResponse<object>>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Url = SD.AccessControlAPIBase + SD.UserAPIBase + userId + "/reject"
            });
        }
    }
}

