using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Application.DTOs;

/// <summary>
/// Search filters for rooms. StartTime/EndTime are optional — when both are
/// supplied, results are limited to rooms with no conflicting booking in
/// that window; when omitted, results are just the rooms matching the
/// other filters, regardless of availability (used for browsing by room
/// before picking a time, e.g. from a calendar view).
/// </summary>
public class RoomSearchCriteria
{
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? MinCapacity { get; set; }
    public string? Location { get; set; }
    public RoomType? Type { get; set; }
}
