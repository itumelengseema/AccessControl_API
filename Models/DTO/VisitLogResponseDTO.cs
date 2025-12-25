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
    }
}