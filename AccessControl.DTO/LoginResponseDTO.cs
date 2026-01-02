namespace AccessControl_API.Models.DTO
{
    public class LoginResponseDTO
    {
        public UserDTO User { get; set; } = null!;
        public string Token { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new List<string>();
    }
}
