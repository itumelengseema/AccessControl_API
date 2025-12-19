namespace AccessControl_API.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public string? Email { get; set; }
        public required string PhoneNumber { get; set; }
        public ICollection<UserGroup> userGroups { get; set; } = new List<UserGroup>();

    }


}
