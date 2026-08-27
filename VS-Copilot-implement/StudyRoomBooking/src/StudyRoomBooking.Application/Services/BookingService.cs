using StudyRoomBooking.Infrastructure.Data;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Application.Services;

public class BookingService : IBookingService
{
    private readonly DataStore _dataStore;
    private readonly INotificationService _notificationService;

    public BookingService(DataStore dataStore, INotificationService notificationService)
    {
        _dataStore = dataStore;
        _notificationService = notificationService;
    }

    public List<BookingViewModel> GetAllBookings()
    {
        return _dataStore.Bookings
            .Select(b => MapToViewModel(b))
            .ToList();
    }

    public List<BookingViewModel> GetUserBookings(int userId)
    {
        return _dataStore.Bookings
            .Where(b => b.UserId == userId && b.Status != BookingStatus.Cancelled)
            .Select(b => MapToViewModel(b))
            .ToList();
    }

    public BookingViewModel? GetBookingById(int id)
    {
        var booking = _dataStore.Bookings.FirstOrDefault(b => b.Id == id);
        return booking != null ? MapToViewModel(booking) : null;
    }

    public void CreateBooking(BookingViewModel booking)
    {
        if (!IsRoomAvailable(booking.RoomId, booking.BookingDate, booking.StartTime, booking.EndTime))
            throw new InvalidOperationException("Room is not available for the selected time slot.");

        var newBooking = new Booking
        {
            Id = _dataStore.Bookings.Count > 0 ? _dataStore.Bookings.Max(b => b.Id) + 1 : 1,
            UserId = booking.UserId,
            RoomId = booking.RoomId,
            BookingDate = booking.BookingDate,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            Status = BookingStatus.Confirmed,
            Notes = booking.Notes,
            ConfirmedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _dataStore.Bookings.Add(newBooking);

        // Send confirmation (placeholder - implement in NotificationService)
        var user = _dataStore.Users.FirstOrDefault(u => u.Id == booking.UserId);
        if (user != null)
            _notificationService.SendBookingConfirmation(booking, user.Email);
    }

    public void UpdateBooking(BookingViewModel booking)
    {
        var existingBooking = _dataStore.Bookings.FirstOrDefault(b => b.Id == booking.Id);
        if (existingBooking == null)
            return;

        if (!IsRoomAvailable(booking.RoomId, booking.BookingDate, booking.StartTime, booking.EndTime, booking.Id))
            throw new InvalidOperationException("Room is not available for the selected time slot.");

        existingBooking.RoomId = booking.RoomId;
        existingBooking.BookingDate = booking.BookingDate;
        existingBooking.StartTime = booking.StartTime;
        existingBooking.EndTime = booking.EndTime;
        existingBooking.Notes = booking.Notes;
        existingBooking.UpdatedAt = DateTime.UtcNow;

        var user = _dataStore.Users.FirstOrDefault(u => u.Id == booking.UserId);
        if (user != null)
            _notificationService.SendBookingModificationNotification(booking, user.Email);
    }

    public void CancelBooking(int bookingId)
    {
        var booking = _dataStore.Bookings.FirstOrDefault(b => b.Id == bookingId);
        if (booking == null)
            throw new InvalidOperationException($"Booking with ID {bookingId} not found.");

        if (booking.Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("This booking has already been cancelled.");

        if (booking.BookingDate < DateTime.Now.Date)
            throw new InvalidOperationException("Cannot cancel bookings from the past.");

        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;

        var user = _dataStore.Users.FirstOrDefault(u => u.Id == booking.UserId);
        if (user != null)
        {
            var bookingVm = MapToViewModel(booking);
            _notificationService.SendBookingCancellationNotification(bookingVm, user.Email);
        }
    }

    public bool CanCancelBooking(int bookingId)
    {
        var booking = _dataStore.Bookings.FirstOrDefault(b => b.Id == bookingId);
        if (booking == null)
            return false;

        if (booking.Status == BookingStatus.Cancelled)
            return false;

        if (booking.BookingDate < DateTime.Now.Date)
            return false;

        return true;
    }

    public List<BookingViewModel> GetBookingHistory(int userId)
    {
        return _dataStore.Bookings
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookingDate)
            .Select(b => MapToViewModel(b))
            .ToList();
    }

    public bool IsRoomAvailable(int roomId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeBookingId = null)
    {
        var conflictingBookings = _dataStore.Bookings
            .Where(b => b.RoomId == roomId
                && b.BookingDate == date
                && b.Status != BookingStatus.Cancelled
                && (excludeBookingId == null || b.Id != excludeBookingId)
                && !(b.EndTime <= startTime || b.StartTime >= endTime))
            .ToList();

        return conflictingBookings.Count == 0;
    }

    private BookingViewModel MapToViewModel(Booking booking)
    {
        var room = _dataStore.Rooms.FirstOrDefault(r => r.Id == booking.RoomId);
        return new BookingViewModel
        {
            Id = booking.Id,
            UserId = booking.UserId,
            RoomId = booking.RoomId,
            RoomName = room?.RoomName,
            BookingDate = booking.BookingDate,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            Status = booking.Status,
            Notes = booking.Notes
        };
    }
}
