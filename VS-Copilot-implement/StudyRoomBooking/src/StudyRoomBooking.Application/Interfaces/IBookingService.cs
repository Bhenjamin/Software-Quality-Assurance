using StudyRoomBooking.Application.ViewModels;

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
    List<BookingViewModel> GetBookingHistory(int userId);
    bool IsRoomAvailable(int roomId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeBookingId = null);
}
