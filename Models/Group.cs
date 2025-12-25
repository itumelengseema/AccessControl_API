using System.ComponentModel.DataAnnotations;

namespace AccessControl_API.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        // Navigation properties
        public List<UserGroup> UserGroups { get; set; } = new();
        public List<GroupPermission> GroupPermissions { get; set; } = new();
    }
}
