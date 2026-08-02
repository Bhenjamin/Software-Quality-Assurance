using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Application.DTOs;

/// <summary>Search filters for room availability. All fields except the time window are optional.</summary>
public class RoomSearchCriteria
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int? MinCapacity { get; set; }
    public string? Location { get; set; }
    public RoomType? Type { get; set; }
}
