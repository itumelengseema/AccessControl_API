using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccessControl_API.Models
{
    public class UserGroup
    {
        [Key, Column(Order = 0)]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Key, Column(Order = 1)]
        [ForeignKey(nameof(Group))]
        public int GroupId { get; set; }
        public Group Group { get; set; } = null!;
    }
}
