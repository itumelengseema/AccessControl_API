using AccessControl_API.Models.DTO;
using AccessControl_Web.Services.IServices;

namespace AccessControl_Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly IBaseServices _baseService;

        public AuthService(IBaseServices baseService)
        {
            _baseService = baseService;
        }

        public async Task<ApiResponse<LoginResponseDTO>?> LoginAsync(LoginRequestDTO loginRequest)
        {
            return await _baseService.SendAsync<ApiResponse<LoginResponseDTO>>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Url = SD.AccessControlAPIBase + SD.AuthLogin,
                Data = loginRequest
            });
        }

        public async Task<ApiResponse<UserDTO>?> RegisterAsync(RegistrationRequestDTO registrationRequest)
        {
            return await _baseService.SendAsync<ApiResponse<UserDTO>>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Url = SD.AccessControlAPIBase + SD.AuthRegister,
                Data = registrationRequest
            });
        }
    }
}
