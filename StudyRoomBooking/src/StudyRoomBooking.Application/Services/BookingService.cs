using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Application.Services;

public class BookingService : IBookingService
{
    private readonly Domain.Interfaces.IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private const int MaxAdvanceDaysAllowed = 60;

    public BookingService(
        Domain.Interfaces.IUnitOfWork unitOfWork,
        INotificationService notificationService
    )
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

    public async Task<List<Booking>> SearchBookingsAsync(
        DateTime date,
        int? roomId = null,
        int? userId = null
    )
    {
        var allBookings = await _unitOfWork.Bookings.GetAllAsync();

        var filtered = allBookings
            .Where(b =>
                b.BookingDate.Date == date.Date
                && b.Status != BookingStatus.Cancelled
                && (!roomId.HasValue || b.RoomId == roomId.Value)
                && (!userId.HasValue || b.UserId == userId.Value)
            )
            .ToList();

        return filtered;
    }

    /// <summary>
    /// Fetches all bookings for a specific room within a date range.
    /// Optimized for recurring booking searches to avoid N+1 query problem.
    /// </summary>
    public async Task<List<Booking>> GetRoomBookingsByDateRangeAsync(int roomId, DateTime startDate, DateTime endDate)
    {
        var allBookings = await _unitOfWork.Bookings.GetAllAsync();

        var filtered = allBookings
            .Where(b =>
                b.RoomId == roomId
                && b.BookingDate.Date >= startDate.Date
                && b.BookingDate.Date <= endDate.Date
                && b.Status != BookingStatus.Cancelled
            )
            .ToList();

        return filtered;
    }

    public async Task<Booking> CreateBookingAsync(Booking booking)
    {
        if (booking.UserId <= 0)
        {
            throw new ArgumentException(
                "A valid student must be attached to the booking.",
                nameof(booking)
            );
        }

        var init_user = await _unitOfWork.Users.GetByIdAsync(booking.UserId);
        if (init_user == null)
        {
            throw new ArgumentException("The specified student does not exist.", nameof(booking));
        }

        booking.ConfirmationNumber = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
        booking.Status = BookingStatus.Confirmed;
        booking.BookingDate = DateTime.SpecifyKind(booking.BookingDate.Date, DateTimeKind.Utc);
        booking.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Bookings.AddAsync(booking);
        await _unitOfWork.SaveChangesAsync();

        // Send confirmation notification asynchronously WITHOUT blocking the response
        // This allows the user to see the confirmation immediately
        _ = SendBookingConfirmationNotificationAsync(booking);

        return booking;
    }

    // Send notification in the background without blocking
    private async Task SendBookingConfirmationNotificationAsync(Booking booking)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(booking.UserId);
            var room = await _unitOfWork.Rooms.GetByIdAsync(booking.RoomId);

            if (user != null && room != null)
            {
                await _notificationService.SendBookingConfirmationAsync(
                    user.Email,
                    room.Name,
                    booking.BookingDate,
                    booking.StartTime,
                    booking.EndTime,
                    booking.ConfirmationNumber
                );
            }
        }
        catch (Exception ex)
        {
            // Log error but don't throw - failure to send email shouldn't fail the booking
            System.Diagnostics.Debug.WriteLine($"[ERROR] Failed to send booking confirmation: {ex.Message}");
            Console.WriteLine($"[ERROR] Failed to send booking confirmation: {ex.Message}");
        }
    }

    public async Task<Booking> UpdateBookingAsync(Booking booking)
    {
        booking.UpdatedAt = DateTime.UtcNow;
        booking.BookingDate = DateTime.SpecifyKind(booking.BookingDate.Date, DateTimeKind.Utc);
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

    public async Task<(bool IsValid, string ErrorMessage)> ValidateBookingAsync(
        int roomId,
        DateTime bookingDate,
        TimeSpan startTime,
        TimeSpan endTime,
        int? bookingIdToExclude = null,
        bool skipAdvanceDaysCheck = false
    )
    {
        // Validation 0: Check if start time is before end time
        if (startTime >= endTime)
        {
            return (false, "Start hour must be before end hour. Please select a valid time range.");
        }

        // Get today's date in local time zone
        var today = DateTime.Today; // Midnight today in local timezone
        var selectedDate = bookingDate.Date; // Ensure we're comparing just the date part

        // Validation 1: Check if booking date is in the past (strictly before today)
        if (selectedDate < today)
        {
            var daysInPast = (today - selectedDate).Days;
            return (
                false,
                $"Cannot book rooms in the past. The date you selected is {daysInPast} days ago. Please select today or a future date."
            );
        }

        // Validation 1.5: Check if booking time is in the past for today's date
        if (selectedDate == today)
        {
            var currentTime = DateTime.Now.TimeOfDay;
            var roundedCurrentTime = TimeSpan.FromHours(Math.Floor(currentTime.TotalHours));
            if (startTime < roundedCurrentTime)
            {
                return (
                    false,
                    "Cannot book rooms for past times on today's date. The start time must be after the current time."
                );
            }
        }

        // Validation 2: Check if booking is more than 60 days ahead (skip for recurring bookings)
        if (!skipAdvanceDaysCheck)
        {
            var daysInAdvance = (selectedDate - today).Days;
            if (daysInAdvance > MaxAdvanceDaysAllowed)
            {
                return (
                    false,
                    $"Bookings can only be made up to {MaxAdvanceDaysAllowed} days ahead. Your selected date is {daysInAdvance} days away."
                );
            }
        }

        // Validation 3: Check for double bookings (same room, overlapping time)
        var existingBookings = await _unitOfWork.Bookings.GetByRoomIdAsync(roomId);
        var conflictingBookings = existingBookings
            .Where(b =>
                b.BookingDate.Date == selectedDate
                && b.Status != BookingStatus.Cancelled
                && b.Id != bookingIdToExclude
                && // Exclude the booking being modified
                !(b.EndTime <= startTime || b.StartTime >= endTime) // Check for time overlap
            )
            .ToList();
        if (conflictingBookings.Any())
        {
            // Build a detailed error message with conflicting booking times
            var conflictTimes = conflictingBookings
                .OrderBy(b => b.StartTime)
                .Select(b => $"- {b.StartTime:hh\\:mm} - {b.EndTime:hh\\:mm}");
            var errorMessage =
                "This room is already booked during the selected time. Please choose a different time or room.\n\nConflicting bookings:\n"
                + string.Join("\n", conflictTimes);
            return (false, errorMessage);
        }
        return (true, string.Empty);
    }

    /// <summary>
    /// Generates a list of dates based on the recurrence pattern between the start date and end date.
    /// </summary>
    public List<DateTime> GenerateRecurrenceDates(
        DateTime startDate,
        DateTime endDate,
        RecurrencePattern pattern
    )
    {
        var dates = new List<DateTime>();

        if (pattern == RecurrencePattern.None)
        {
            dates.Add(startDate);
            return dates;
        }

        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            dates.Add(currentDate);

            switch (pattern)
            {
                case RecurrencePattern.Daily:
                    currentDate = currentDate.AddDays(1);
                    break;
                case RecurrencePattern.Weekly:
                    currentDate = currentDate.AddDays(7);
                    break;
                case RecurrencePattern.BiWeekly:
                    currentDate = currentDate.AddDays(14);
                    break;
                case RecurrencePattern.FourWeeks:
                    currentDate = currentDate.AddDays(28);
                    break;
                default:
                    throw new ArgumentException($"Unknown recurrence pattern: {pattern}");
            }
        }

        return dates;
    }
}
