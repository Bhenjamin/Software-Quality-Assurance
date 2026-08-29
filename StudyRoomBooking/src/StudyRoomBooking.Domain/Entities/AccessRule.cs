namespace StudyRoomBooking.Domain.Entities;

public class AccessRule
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TimeSpan? StartTime { get; set; } // Null means no time restriction
    public TimeSpan? EndTime { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
