using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Domain.Entities;

namespace StudyRoomBooking.Web.Pages.Students;

public class MyBookingsModel : PageModel
{
    private readonly IBookingService _bookingService;
    private readonly IRoomService _roomService;
    private readonly IUserService _userService;

    public List<BookingViewModel> Bookings { get; set; } = new();
    public string? Message { get; set; }

    public MyBookingsModel(IBookingService bookingService, IRoomService roomService, IUserService userService)
    {
        _bookingService = bookingService;
        _roomService = roomService;
        _userService = userService;
    }

    public async Task OnGetAsync(string? message = null)
    {
        Message = message;
        await LoadUserBookings();
    }

    public async Task<IActionResult> OnPostCancelBookingAsync(int id)
    {
        try
        {
            await _bookingService.CancelBookingAsync(id);
            return RedirectToPage(new { message = "Booking cancelled successfully!" });
        }
        catch (Exception ex)
        {
            Message = $"Error cancelling booking: {ex.Message}";
            await LoadUserBookings();
            return Page();
        }
    }

    private async Task LoadUserBookings()
    {
        try
        {
            // Get current user from session
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                ModelState.AddModelError(string.Empty, "User not found in session.");
                return;
            }

            var bookings = await _bookingService.GetBookingsByUserIdAsync(userId);

            foreach (var booking in bookings.OrderByDescending(b => b.BookingDate))
            {
                var room = await _roomService.GetRoomByIdAsync(booking.RoomId);
                Bookings.Add(new BookingViewModel
                {
                    Id = booking.Id,
                    RoomId = booking.RoomId,
                    RoomName = room?.Name ?? "Unknown",
                    RoomCode = room?.Code ?? "",
                    UserId = booking.UserId,
                    BookingDate = booking.BookingDate,
                    StartTime = booking.StartTime,
                    EndTime = booking.EndTime,
                    Status = booking.Status,
                    ConfirmationNumber = booking.ConfirmationNumber
                });
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error loading bookings: {ex.Message}");
        }
    }
}
