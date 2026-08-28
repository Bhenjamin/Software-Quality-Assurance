using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Web.Pages.Students;

public class RoomDetailsModel : PageModel
{
    private readonly IRoomService _roomService;
    private readonly IBookingService _bookingService;
    private readonly IUserService _userService;

    public Room? Room { get; set; }

    public RoomDetailsModel(IRoomService roomService, IBookingService bookingService, IUserService userService)
    {
        _roomService = roomService;
        _bookingService = bookingService;
        _userService = userService;
    }

    public async Task OnGetAsync(int id)
    {
        Room = await _roomService.GetRoomByIdAsync(id);
    }

    public async Task<IActionResult> OnPostBookRoomAsync(int roomId, string bookingDate, string startTime, string endTime, string? notes)
    {
        try
        {
            var room = await _roomService.GetRoomByIdAsync(roomId);
            if (room == null)
            {
                ModelState.AddModelError(string.Empty, "Room not found.");
                await OnGetAsync(roomId);
                return Page();
            }

            // Get current user from session
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                ModelState.AddModelError(string.Empty, "User not found in session.");
                return RedirectToPage("/Login");
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return RedirectToPage("Search");
            }

            var date = DateTime.ParseExact(bookingDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            var start = TimeSpan.ParseExact(startTime, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture);
            var end = TimeSpan.ParseExact(endTime, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture);

            // Validate booking date and time constraints FIRST
            var (isValid, errorMessage) = await _bookingService.ValidateBookingAsync(roomId, date, start, end);
            if (!isValid)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                await OnGetAsync(roomId);
                return Page();
            }

            // Check availability
            var isAvailable = await _roomService.IsRoomAvailableAsync(roomId, date, start, end);
            if (!isAvailable)
            {
                ModelState.AddModelError(string.Empty, "Selected time slot is not available.");
                await OnGetAsync(roomId);
                return Page();
            }

            // Create booking
            var booking = new Booking
            {
                RoomId = roomId,
                UserId = user.Id,
                BookingDate = date,
                StartTime = start,
                EndTime = end,
                Notes = notes,
                Status = BookingStatus.Confirmed
            };

            await _bookingService.CreateBookingAsync(booking);

            return RedirectToPage("MyBookings", new { message = "Booking created successfully!" });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error creating booking: {ex.Message}");
            await OnGetAsync(roomId);
            return Page();
        }
    }
}
