using System.ComponentModel.DataAnnotations;

namespace AccessControl_API.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = null!;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = null!;

        [Required, MaxLength(50)]
        public string IdentificationNumber { get; set; } = null!;
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string PasswordHash { get; set; } = null!;




        // Navigation properties
        public List<UserGroup> UserGroups { get; set; } = new();
        public List<VisitLog> VisitLogs { get; set; } = new();

    }


}
