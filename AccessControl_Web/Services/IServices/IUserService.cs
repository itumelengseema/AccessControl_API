using AccessControl_API.Models.DTO;

namespace AccessControl_Web.Services.IServices
{
    public interface IUserService
    {
        Task<ApiResponse<UserDTO>?> CreateUserAsync(UserCreateUpdateDTO userDto);
        Task<ApiResponse<UserDTO>?> UpdateUserAsync(int id, UserCreateUpdateDTO userDto);
        Task<ApiResponse<object>?> DeleteUserAsync(int id);
        Task<ApiResponse<List<UserDTO>>?> GetAllUsersAsync();
        Task<ApiResponse<int>?> GetUserCountAsync();
        Task<ApiResponse<List<UserDTO>>?> GetPendingApprovalsAsync();
        Task<ApiResponse<UserDTO>?> ApproveUserAsync(int userId);
        Task<ApiResponse<object>?> RejectUserAsync(int userId);
    }
}

