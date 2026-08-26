using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Application.ViewModels;

public class BookingViewModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int RoomId { get; set; }
    public string? RoomName { get; set; }
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public BookingStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime? RecurrenceStartDate { get; set; }
    public DateTime? RecurrenceEndDate { get; set; }
    public RecurrencePattern RecurrencePattern { get; set; }
    public int RecurrenceCount { get; set; }
}
