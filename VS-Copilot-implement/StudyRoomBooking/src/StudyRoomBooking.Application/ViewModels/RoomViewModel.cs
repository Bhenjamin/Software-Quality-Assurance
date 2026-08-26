using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Application.ViewModels;

public class RoomViewModel
{
    public int Id { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public RoomType RoomType { get; set; }
    public string? Description { get; set; }
    public bool IsAvailable { get; set; }
}
