using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Application.ViewModels;

namespace StudyRoomBooking.Web.Pages.Students;

public class BookingHistoryModel : PageModel
{
    private readonly IBookingService _bookingService;
    private readonly IRoomService _roomService;
    private readonly IUserService _userService;

    public List<BookingViewModel> BookingHistory { get; set; } = new();

    public BookingHistoryModel(IBookingService bookingService, IRoomService roomService, IUserService userService)
    {
        _bookingService = bookingService;
        _roomService = roomService;
        _userService = userService;
    }

    public async Task OnGetAsync()
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
                BookingHistory.Add(new BookingViewModel
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
                    ConfirmationNumber = booking.ConfirmationNumber,
                    CreatedAt = booking.CreatedAt
                });
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error loading booking history: {ex.Message}");
        }
    }
}
