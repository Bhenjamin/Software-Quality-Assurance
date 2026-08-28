using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Application.ViewModels;

public class RoomSearchCriteria
{
    public DateTime BookingDate { get; set; } = DateTime.Today;
    public TimeSpan StartTime { get; set; } = new TimeSpan(8, 0, 0);
    public TimeSpan EndTime { get; set; } = new TimeSpan(17, 0, 0);
    public int? Capacity { get; set; }
    public RoomType? RoomType { get; set; }
    public string? Location { get; set; }
}
