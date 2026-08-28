using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Domain.Entities;

namespace StudyRoomBooking.Web.Pages.Admin;

public class OverridesModel : PageModel
{
    private readonly IBookingService _bookingService;
    private readonly IRoomService _roomService;
    private readonly IUserService _userService;

    public List<BookingViewModel> AvailableBookings { get; set; } = new();
    public List<BookingOverride> Overrides { get; set; } = new();

    public OverridesModel(IBookingService bookingService, IRoomService roomService, IUserService userService)
    {
        _bookingService = bookingService;
        _roomService = roomService;
        _userService = userService;
    }

    public async Task OnGetAsync()
    {
        await LoadBookings();
    }

    public async Task<IActionResult> OnPostAsync(int bookingId, string reason, bool allowException)
    {
        try
        {
            var admin = await _userService.GetUserByUserIdAsync("ADM001");

            var @override = new BookingOverride
            {
                BookingId = bookingId,
                AdminId = admin?.Id ?? 1,
                Reason = reason,
                AllowsExceptionToRule = allowException
            };

            Overrides.Add(@override);

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error creating override: {ex.Message}");
            await LoadBookings();
            return Page();
        }
    }

    private async Task LoadBookings()
    {
        try
        {
            var bookings = await _bookingService.GetAllBookingsAsync();

            foreach (var booking in bookings)
            {
                var room = await _roomService.GetRoomByIdAsync(booking.RoomId);
                var user = await _userService.GetUserByIdAsync(booking.UserId);

                AvailableBookings.Add(new BookingViewModel
                {
                    Id = booking.Id,
                    RoomId = booking.RoomId,
                    RoomName = room?.Name ?? "Unknown",
                    UserId = booking.UserId,
                    UserName = user?.Name ?? "Unknown",
                    BookingDate = booking.BookingDate,
                    StartTime = booking.StartTime,
                    EndTime = booking.EndTime,
                    Status = booking.Status
                });
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error loading bookings: {ex.Message}");
        }
    }
}
