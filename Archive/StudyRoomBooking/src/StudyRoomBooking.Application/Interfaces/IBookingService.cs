using StudyRoomBooking.Application.DTOs;

namespace StudyRoomBooking.Application.Interfaces;

public interface IBookingService
{
    BookingResult CreateBooking(BookingRequest request);
    BookingResult ModifyBooking(BookingModificationRequest request);
    BookingResult CancelBooking(Guid bookingId, Guid requestingUserId, string? reason = null);
    IEnumerable<Domain.Models.Booking> GetBookingHistory(Guid userId);
}
