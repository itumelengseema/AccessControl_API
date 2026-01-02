using AccessControl_API.Models.DTO;

namespace AccessControl_Web.Services.IServices
{
    public interface IAuthService
    {
        Task<ApiResponse<UserDTO>?> RegisterAsync(RegistrationRequestDTO registrationRequest);
        Task<ApiResponse<LoginResponseDTO>?> LoginAsync(LoginRequestDTO loginRequest);
    }
}
