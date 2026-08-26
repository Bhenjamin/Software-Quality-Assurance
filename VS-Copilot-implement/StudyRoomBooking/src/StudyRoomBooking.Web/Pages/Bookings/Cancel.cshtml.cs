using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Infrastructure.Shared.Constants;

namespace StudyRoomBooking.Web.Pages.Bookings;

public class CancelModel : PageModel
{
    private readonly IBookingService _bookingService;

    public CancelModel(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    public IActionResult OnPost(int? id)
    {
        if (!IsAuthenticated())
            return RedirectToPage("/Auth/Login");

        if (!id.HasValue)
            return NotFound();

        _bookingService.CancelBooking(id.Value);
        return RedirectToPage("Index");
    }

    private bool IsAuthenticated()
    {
        var userId = HttpContext.Session.GetString(AppConstants.UserSessionKey);
        return !string.IsNullOrEmpty(userId);
    }
}
