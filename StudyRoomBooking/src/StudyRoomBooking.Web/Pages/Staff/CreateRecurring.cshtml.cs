using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Web.Pages.Staff;

public class CreateRecurringModel : PageModel
{
    private readonly IRoomService _roomService;
    private readonly IBookingService _bookingService;
    private readonly IUserService _userService;

    public List<Room> AvailableRooms { get; set; } = new();

    // Pre-populated fields from recurring booking link
    public int? PreSelectedRoomId { get; set; }
    public string? PreSelectedStartDate { get; set; }
    public string? PreSelectedStartTime { get; set; }
    public string? PreSelectedEndTime { get; set; }
    public string? PreSelectedRecurrenceEndDate { get; set; }

    public CreateRecurringModel(IRoomService roomService, IBookingService bookingService, IUserService userService)
    {
        _roomService = roomService;
        _bookingService = bookingService;
        _userService = userService;
    }

    public async Task OnGetAsync(int? roomId = null, string? startDate = null, string? startTime = null, 
        string? endTime = null, string? recurrenceEndDate = null)
    {
        AvailableRooms = await _roomService.GetAllRoomsAsync();

        // Store pre-populated values from recurring booking search
        PreSelectedRoomId = roomId;
        PreSelectedStartDate = startDate;
        PreSelectedStartTime = startTime;
        PreSelectedEndTime = endTime;
        PreSelectedRecurrenceEndDate = recurrenceEndDate;
    }

    public async Task<IActionResult> OnPostAsync(int roomId, string startDate, string startTime, string endTime, 
        RecurrencePattern recurrencePattern, string? recurrenceEndDate, string? notes)
    {
        try
        {
            var room = await _roomService.GetRoomByIdAsync(roomId);
            if (room == null)
            {
                ModelState.AddModelError(string.Empty, "Room not found.");
                await OnGetAsync();
                return Page();
            }

            // Get current user (staff member) from session
            var currentUserName = HttpContext.Session.GetString("CurrentUser");
            if (string.IsNullOrEmpty(currentUserName))
            {
                ModelState.AddModelError(string.Empty, "User session expired. Please login again.");
                await OnGetAsync();
                return Page();
            }

            var user = await _userService.GetUserByUserIdAsync(currentUserName);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                await OnGetAsync();
                return Page();
            }

            var date = DateTime.ParseExact(startDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            var start = TimeSpan.ParseExact(startTime, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture);
            var end = TimeSpan.ParseExact(endTime, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture);

            // Validate time range (start must be before end)
            if (start >= end)
            {
                ModelState.AddModelError(string.Empty, "Start time must be before end time.");
                await OnGetAsync();
                return Page();
            }

            // Validate time is within allowed range (8:00 to 22:00)
            var minTime = new TimeSpan(8, 0, 0);
            var maxTime = new TimeSpan(22, 0, 0);
            if (start < minTime || end > maxTime)
            {
                ModelState.AddModelError(string.Empty, "Booking time must be between 8:00 AM and 10:00 PM.");
                await OnGetAsync();
                return Page();
            }

            // Validate booking date is not in the past
            var today = DateTime.Today;
            if (date.Date < today)
            {
                ModelState.AddModelError(string.Empty, "Cannot book for dates in the past. Please select today or later.");
                await OnGetAsync();
                return Page();
            }

            // Parse recurrence end date if provided
            DateTime? recurrenceEnd = null;
            if (!string.IsNullOrEmpty(recurrenceEndDate))
            {
                recurrenceEnd = DateTime.ParseExact(recurrenceEndDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

                // Validate recurrence end date is not before start date
                if (recurrenceEnd.Value.Date < date.Date)
                {
                    ModelState.AddModelError(string.Empty, "Recurrence end date must be on or after the start date.");
                    await OnGetAsync();
                    return Page();
                }
            }

            // Validate booking date and time constraints for the start date
            var (isValid, errorMessage) = await _bookingService.ValidateBookingAsync(roomId, date, start, end, skipAdvanceDaysCheck: true);
            if (!isValid)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                await OnGetAsync();
                return Page();
            }

            // Check availability
            var isAvailable = await _roomService.IsRoomAvailableAsync(roomId, date, start, end);
            if (!isAvailable)
            {
                ModelState.AddModelError(string.Empty, "Selected time slot is not available.");
                await OnGetAsync();
                return Page();
            }

            // Generate all recurrence dates
            DateTime endDateForRecurrence = recurrenceEnd ?? date;
            var recurrenceDates = _bookingService.GenerateRecurrenceDates(date, endDateForRecurrence, recurrencePattern);

            // Validate availability and constraints for all recurrence dates (skip 60-day limit for recurring bookings)
            foreach (var occurrenceDate in recurrenceDates)
            {
                // Validate constraints for each date (skipAdvanceDaysCheck=true for recurring bookings)
                var (dateIsValid, dateErrorMessage) = await _bookingService.ValidateBookingAsync(roomId, occurrenceDate, start, end, skipAdvanceDaysCheck: true);
                if (!dateIsValid)
                {
                    ModelState.AddModelError(string.Empty, $"Cannot create recurring booking: {dateErrorMessage} (Date: {occurrenceDate:yyyy-MM-dd})");
                    await OnGetAsync();
                    return Page();
                }

                // Check availability for each date
                var dateIsAvailable = await _roomService.IsRoomAvailableAsync(roomId, occurrenceDate, start, end);
                if (!dateIsAvailable)
                {
                    ModelState.AddModelError(string.Empty, $"Selected time slot is not available on {occurrenceDate:yyyy-MM-dd}.");
                    await OnGetAsync();
                    return Page();
                }
            }

            // Create individual bookings for each recurrence date
            foreach (var occurrenceDate in recurrenceDates)
            {
                var booking = new Booking
                {
                    RoomId = roomId,
                    UserId = user.Id,
                    BookingDate = occurrenceDate,
                    StartTime = start,
                    EndTime = end,
                    RecurrencePattern = RecurrencePattern.None,  // Each booking is standalone, not recurring
                    RecurrenceEndDate = null,
                    Notes = notes,
                    Status = BookingStatus.Confirmed
                };

                await _bookingService.CreateBookingAsync(booking);
            }

            // Build success message
            var message = $"Recurring booking created successfully with {recurrenceDates.Count} bookings!";

            return RedirectToPage("/Students/MyBookings", new { message = message });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error creating booking: {ex.Message}");
            await OnGetAsync();
            return Page();
        }
    }
}
