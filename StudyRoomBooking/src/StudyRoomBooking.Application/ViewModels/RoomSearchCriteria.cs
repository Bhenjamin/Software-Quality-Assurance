using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Application.ViewModels;

public class RoomSearchCriteria
{
    public DateTime BookingDate { get; set; } = DateTime.Today;
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public int? Capacity { get; set; }
    public RoomType? RoomType { get; set; }
    public string? Location { get; set; }
}
