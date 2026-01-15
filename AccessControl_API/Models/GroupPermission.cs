using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccessControl_API.Models
{
    public class GroupPermission
    {
        [Key, Column(Order = 0)]
        [ForeignKey(nameof(Group))]
        public int GroupId { get; set; }
        public Group Group { get; set; } = null!;

        [Key, Column(Order = 1)]
        [ForeignKey(nameof(Permission))]
        public int PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;
    }
}
