namespace AccessControl_API.Models.DTO
{
    public class VisitLogResponseDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public bool IsActive { get; set; }
        public string? CarRegistrationNumber { get; set; }
        
        // User details for display
        public string UserFirstName { get; set; } = string.Empty;
        public string UserLastName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserIdentificationNumber { get; set; } = string.Empty;
    }
}