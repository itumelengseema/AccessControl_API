using System.ComponentModel.DataAnnotations;

namespace AccessControl_API.Models.DTO
{
    public class CheckInDTO
    {
        [Required(ErrorMessage = "User ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid user ID")]
        public int UserId { get; set; }
        
        [StringLength(20, ErrorMessage = "Car registration cannot exceed 20 characters")]
        public string? CarRegistrationNumber { get; set; }
    }
}
