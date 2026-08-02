namespace StudyRoomBooking.Application.DTOs;

public class BookingModificationRequest
{
    public Guid BookingId { get; set; }
    public Guid RequestingUserId { get; set; }
    public DateTime NewStartTime { get; set; }
    public DateTime NewEndTime { get; set; }
}
