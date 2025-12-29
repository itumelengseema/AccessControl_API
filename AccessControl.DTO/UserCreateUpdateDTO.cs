namespace AccessControl_API.Models.DTO
{
    public class UserCreateUpdateDTO
    {

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string IdentificationNumber { get; set; } = null!;

        public int GroupId { get; set; }
    }
}
