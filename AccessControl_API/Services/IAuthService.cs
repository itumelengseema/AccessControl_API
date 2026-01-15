using AccessControl_API.Models.DTO;

namespace AccessControl_API.Services
{
    public interface IAuthService
    {
        Task<UserDTO?> RegisterAsync(RegistrationRequestDTO registrationRequestDTO);
        Task<LoginResponseDTO?> LoginAsync(LoginRequestDTO loginRequestDTO);
        Task<bool> IsEmailExistAsync(string email);
    }
}
