using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Application.ViewModels;

public class CreateRecurringBookingViewModel
{
    public int RoomId { get; set; }
    public DateTime StartDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public RecurrencePattern RecurrencePattern { get; set; } = RecurrencePattern.Weekly;
    public DateTime? RecurrenceEndDate { get; set; }
    public string? Notes { get; set; }
}
