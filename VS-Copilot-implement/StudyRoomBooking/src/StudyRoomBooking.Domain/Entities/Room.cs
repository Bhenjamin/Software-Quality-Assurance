using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Domain.Entities;

public class Room
{
    public int Id { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public RoomType RoomType { get; set; }
    public string? Description { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<AccessRule> AccessRules { get; set; } = new List<AccessRule>();
}
