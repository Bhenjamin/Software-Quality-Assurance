using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Infrastructure.Shared.Constants;

namespace StudyRoomBooking.Web.Pages.Bookings;

public class CreateModel : PageModel
{
    private readonly IBookingService _bookingService;
    private readonly IRoomService _roomService;

    [BindProperty]
    public BookingViewModel Booking { get; set; } = new();

    public RoomViewModel? Room { get; set; }
    public string? ErrorMessage { get; set; }

    public CreateModel(IBookingService bookingService, IRoomService roomService)
    {
        _bookingService = bookingService;
        _roomService = roomService;
    }

    public IActionResult OnGet(int? roomId)
    {
        if (!IsAuthenticated())
            return RedirectToPage("/Auth/Login");

        if (roomId.HasValue)
        {
            Room = _roomService.GetRoomById(roomId.Value);
            if (Room != null)
                Booking.RoomId = Room.Id;
        }

        Booking.BookingDate = DateTime.Today;
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!IsAuthenticated())
            return RedirectToPage("/Auth/Login");

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var userId = int.Parse(HttpContext.Session.GetString(AppConstants.UserSessionKey) ?? "0");
            Booking.UserId = userId;
            _bookingService.CreateBooking(Booking);
            return RedirectToPage("Index");
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
            Room = _roomService.GetRoomById(Booking.RoomId);
            return Page();
        }
    }

    private bool IsAuthenticated()
    {
        var userId = HttpContext.Session.GetString(AppConstants.UserSessionKey);
        return !string.IsNullOrEmpty(userId);
    }
}
