using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Domain.Entities;

public class BookingRecurrence
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int RoomId { get; set; }
    public RecurrencePattern Pattern { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int OccurrenceCount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public User? User { get; set; }
    public Room? Room { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
