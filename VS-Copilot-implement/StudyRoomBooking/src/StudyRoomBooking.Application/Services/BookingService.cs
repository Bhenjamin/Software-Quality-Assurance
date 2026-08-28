using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Application.Services;

public class BookingService : IBookingService
{
    private readonly Domain.Interfaces.IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private const int MaxAdvanceDaysAllowed = 60;

    public BookingService(Domain.Interfaces.IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<Booking?> GetBookingByIdAsync(int id)
    {
        return await _unitOfWork.Bookings.GetByIdAsync(id);
    }

    public async Task<List<Booking>> GetAllBookingsAsync()
    {
        return await _unitOfWork.Bookings.GetAllAsync();
    }

    public async Task<List<Booking>> GetBookingsByUserIdAsync(int userId)
    {
        return await _unitOfWork.Bookings.GetByUserIdAsync(userId);
    }

    public async Task<List<Booking>> GetBookingsByRoomIdAsync(int roomId)
    {
        return await _unitOfWork.Bookings.GetByRoomIdAsync(roomId);
    }

    public async Task<List<Booking>> SearchBookingsAsync(DateTime date, int? roomId = null, int? userId = null)
    {
        var allBookings = await _unitOfWork.Bookings.GetAllAsync();

        var filtered = allBookings.Where(b => 
            b.BookingDate.Date == date.Date &&
            b.Status != BookingStatus.Cancelled &&
            (!roomId.HasValue || b.RoomId == roomId.Value) &&
            (!userId.HasValue || b.UserId == userId.Value)
        ).ToList();

        return filtered;
    }

    public async Task<Booking> CreateBookingAsync(Booking booking)
    {
        booking.ConfirmationNumber = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
        booking.Status = BookingStatus.Confirmed;
        booking.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Bookings.AddAsync(booking);
        await _unitOfWork.SaveChangesAsync();

        // Send confirmation notification
        var user = await _unitOfWork.Users.GetByIdAsync(booking.UserId);
        var room = await _unitOfWork.Rooms.GetByIdAsync(booking.RoomId);

        if (user != null && room != null)
        {
            await _notificationService.SendBookingConfirmationAsync(user.Email, room.Name, booking.BookingDate, booking.ConfirmationNumber);
        }

        return booking;
    }

    public async Task<Booking> UpdateBookingAsync(Booking booking)
    {
        booking.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Bookings.UpdateAsync(booking);
        await _unitOfWork.SaveChangesAsync();
        return booking;
    }

    public async Task CancelBookingAsync(int bookingId)
    {
        var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
        if (booking != null)
        {
            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Bookings.UpdateAsync(booking);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<(bool IsValid, string ErrorMessage)> ValidateBookingAsync(int roomId, DateTime bookingDate, TimeSpan startTime, TimeSpan endTime)
    {
        // Get today's date in local time zone
        var today = DateTime.Today;  // Midnight today in local timezone
        var selectedDate = bookingDate.Date;  // Ensure we're comparing just the date part

        // Validation 1: Check if booking date is in the past (strictly before today)
        if (selectedDate < today)
        {
            var daysInPast = (today - selectedDate).Days;
            return (false, $"Cannot book rooms in the past. The date you selected is {daysInPast} days ago. Please select today or a future date.");
        }

        // Validation 2: Check if booking is more than 60 days in advance
        var daysInAdvance = (selectedDate - today).Days;
        if (daysInAdvance > MaxAdvanceDaysAllowed)
        {
            return (false, $"Bookings can only be made up to {MaxAdvanceDaysAllowed} days in advance. Your selected date is {daysInAdvance} days away.");
        }

        // Validation 3: Check for double bookings (same room, overlapping time)
        var existingBookings = await _unitOfWork.Bookings.GetByRoomIdAsync(roomId);
        var conflictingBookings = existingBookings.Where(b =>
            b.BookingDate.Date == selectedDate &&
            b.Status != BookingStatus.Cancelled &&
            !(b.EndTime <= startTime || b.StartTime >= endTime) // Check for time overlap
        ).ToList();

        if (conflictingBookings.Any())
        {
            return (false, "This room is already booked for the selected time slot. Please choose a different time or room.");
        }

        return (true, string.Empty);
    }
}
