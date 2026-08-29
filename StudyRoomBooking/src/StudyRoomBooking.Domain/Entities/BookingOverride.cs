namespace StudyRoomBooking.Domain.Entities;

public class BookingOverride
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int AdminId { get; set; } // User who created the override
    public string Reason { get; set; } = string.Empty;
    public bool AllowsExceptionToRule { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
