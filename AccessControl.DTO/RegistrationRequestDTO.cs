using System.ComponentModel.DataAnnotations;

namespace AccessControl_API.Models.DTO
{
    public class RegistrationRequestDTO
    {
        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string IdentificationNumber { get; set; } = null!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;

        [Required]
        public int GroupId { get; set; } // The group to which the user will be assigned
    }
}
