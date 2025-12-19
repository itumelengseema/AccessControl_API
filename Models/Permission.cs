namespace AccessControl_API.Models
{
    public class Permission
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<GroupPermission> GroupPermissions { get; set; } = new List<GroupPermission>();
    }
}
