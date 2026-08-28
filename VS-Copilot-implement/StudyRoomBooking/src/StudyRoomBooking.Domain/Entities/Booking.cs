using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Domain.Entities;

public class Booking
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public int UserId { get; set; }
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
    public string? Notes { get; set; }
    public RecurrencePattern RecurrencePattern { get; set; } = RecurrencePattern.None;
    public DateTime? RecurrenceEndDate { get; set; }
    public string? ConfirmationNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
