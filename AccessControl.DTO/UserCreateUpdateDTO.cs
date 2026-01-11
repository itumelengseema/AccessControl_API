using System.ComponentModel.DataAnnotations;

namespace AccessControl_API.Models.DTO
{
    public class UserCreateUpdateDTO
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Identification number is required")]
        [StringLength(50, ErrorMessage = "ID number cannot exceed 50 characters")]
        public string IdentificationNumber { get; set; } = null!;

        [Required(ErrorMessage = "Group is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid group")]
        public int GroupId { get; set; }
    }
}
