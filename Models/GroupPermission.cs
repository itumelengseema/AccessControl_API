namespace AccessControl_API.Models
{
    public class GroupPermission
    {
        public Guid GroupId { get; set; }
        public Group Group { get; set; } = null!;
        public Guid PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;
    }
}
