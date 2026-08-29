using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Application.ViewModels;

public class RecurringBookingSearchCriteria
{
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime RecurrenceEndDate { get; set; } = DateTime.Today.AddDays(30);
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public int? Capacity { get; set; }
    public RoomType? RoomType { get; set; }
    public string? Location { get; set; }
}
