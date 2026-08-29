using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Domain.Entities;

public class RoomMajorRestriction
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public StudentMajor Major { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
