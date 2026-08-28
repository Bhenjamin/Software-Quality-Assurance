using StudyRoomBooking.Domain.Entities;

namespace StudyRoomBooking.Application.Services;

public interface IBookingService
{
    Task<Booking?> GetBookingByIdAsync(int id);
    Task<List<Booking>> GetAllBookingsAsync();
    Task<List<Booking>> GetBookingsByUserIdAsync(int userId);
    Task<List<Booking>> GetBookingsByRoomIdAsync(int roomId);
    Task<List<Booking>> SearchBookingsAsync(DateTime date, int? roomId = null, int? userId = null);
    Task<Booking> CreateBookingAsync(Booking booking);
    Task<Booking> UpdateBookingAsync(Booking booking);
    Task CancelBookingAsync(int bookingId);
    Task<(bool IsValid, string ErrorMessage)> ValidateBookingAsync(int roomId, DateTime bookingDate, TimeSpan startTime, TimeSpan endTime);
}
