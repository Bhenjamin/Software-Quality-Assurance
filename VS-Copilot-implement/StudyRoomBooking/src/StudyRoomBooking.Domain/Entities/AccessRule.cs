using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Domain.Entities;

public class AccessRule
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public UserRole AllowedRole { get; set; }
    public AccessLevel AccessLevel { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Room? Room { get; set; }
}
