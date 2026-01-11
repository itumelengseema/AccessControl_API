using System.ComponentModel.DataAnnotations;

namespace AccessControl_API.Models.DTO
{
    public class GroupDTO
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Group name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Group name must be between 2 and 100 characters")]
        public string Name { get; set; } = null!;
    }
}