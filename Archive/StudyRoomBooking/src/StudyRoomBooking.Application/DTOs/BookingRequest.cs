namespace StudyRoomBooking.Application.DTOs;

public class BookingRequest
{
    public Guid RoomId { get; set; }
    public Guid UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Purpose { get; set; } = string.Empty;

    /// <summary>
    /// Administrators only: bypass the double-booking check to force a
    /// booking through (e.g. resolving a conflict manually). Ignored,
    /// and rejected with an access-denied result, for non-administrators.
    /// </summary>
    public bool OverrideConflict { get; set; }
}
