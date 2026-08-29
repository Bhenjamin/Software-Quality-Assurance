using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Domain.Models;

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RoomId { get; set; }
    public Guid UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    /// <summary>
    /// True if this booking's time range overlaps with the given range.
    /// Adjacent bookings (one ends exactly when the other starts) do not
    /// count as overlapping.
    /// </summary>
    public bool OverlapsWith(DateTime start, DateTime end) =>
        StartTime < end && start < EndTime;
}
