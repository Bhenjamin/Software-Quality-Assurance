using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Web.Pages.Admin;

public class BookingManagementModel : PageModel
{
    private readonly IBookingService _bookingService;
    private readonly IRoomService _roomService;
    private readonly IUserService _userService;

    public List<BookingViewModel> Bookings { get; set; } = new();
    public DateTime? FilterDate { get; set; }
    public BookingStatus? FilterStatus { get; set; }

    public BookingManagementModel(IBookingService bookingService, IRoomService roomService, IUserService userService)
    {
        _bookingService = bookingService;
        _roomService = roomService;
        _userService = userService;
    }

    public async Task OnGetAsync(DateTime? filterDate = null, BookingStatus? filterStatus = null)
    {
        FilterDate = filterDate;
        FilterStatus = filterStatus;
        await LoadBookings();
    }

    public async Task OnPostAsync(DateTime? filterDate = null, BookingStatus? filterStatus = null)
    {
        FilterDate = filterDate;
        FilterStatus = filterStatus;
        await LoadBookings();
    }

    public async Task<IActionResult> OnPostCancelBookingAsync(int id)
    {
        try
        {
            await _bookingService.CancelBookingAsync(id);
            return RedirectToPage(new { filterDate = FilterDate?.ToString("yyyy-MM-dd"), filterStatus = FilterStatus });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error cancelling booking: {ex.Message}");
            await LoadBookings();
            return Page();
        }
    }

    private async Task LoadBookings()
    {
        try
        {
            var allBookings = await _bookingService.GetAllBookingsAsync();

            var filtered = allBookings.AsEnumerable();

            if (FilterDate.HasValue)
            {
                filtered = filtered.Where(b => b.BookingDate.Date == FilterDate.Value.Date);
            }

            if (FilterStatus.HasValue)
            {
                filtered = filtered.Where(b => b.Status == FilterStatus.Value);
            }

            foreach (var booking in filtered.OrderByDescending(b => b.BookingDate))
            {
                var room = await _roomService.GetRoomByIdAsync(booking.RoomId);
                var user = await _userService.GetUserByIdAsync(booking.UserId);

                Bookings.Add(new BookingViewModel
                {
                    Id = booking.Id,
                    RoomId = booking.RoomId,
                    RoomName = room?.Name ?? "Unknown",
                    RoomCode = room?.Code ?? "",
                    UserId = booking.UserId,
                    UserName = user?.Name ?? "Unknown",
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
