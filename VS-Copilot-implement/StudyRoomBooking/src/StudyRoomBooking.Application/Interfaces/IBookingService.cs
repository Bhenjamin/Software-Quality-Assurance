using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Application.Interfaces;

public interface IBookingService
{
    List<BookingViewModel> GetAllBookings();
    List<BookingViewModel> GetUserBookings(int userId);
    BookingViewModel? GetBookingById(int id);
    void CreateBooking(BookingViewModel booking);
    void UpdateBooking(BookingViewModel booking);
    void CancelBooking(int bookingId);
    bool CanCancelBooking(int bookingId);
    bool HasAccessToRoom(int roomId, UserRole userRole);
    List<BookingViewModel> GetBookingHistory(int userId);
    bool IsRoomAvailable(int roomId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeBookingId = null);
}
