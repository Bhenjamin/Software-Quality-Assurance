using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Infrastructure.Shared.Constants;

namespace StudyRoomBooking.Web.Pages.Bookings;

public class IndexModel : PageModel
{
    private readonly IBookingService _bookingService;

    public List<BookingViewModel> Bookings { get; set; } = new();

    public IndexModel(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    public IActionResult OnGet()
    {
        if (!IsAuthenticated())
            return RedirectToPage("/Auth/Login");

        var userId = int.Parse(HttpContext.Session.GetString(AppConstants.UserSessionKey) ?? "0");
        Bookings = _bookingService.GetUserBookings(userId);
        return Page();
    }

    private bool IsAuthenticated()
    {
        var userId = HttpContext.Session.GetString(AppConstants.UserSessionKey);
        return !string.IsNullOrEmpty(userId);
    }
}
