using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Infrastructure.Shared.Constants;

namespace StudyRoomBooking.Web.Pages.Bookings;

public class EditModel : PageModel
{
    private readonly IBookingService _bookingService;

    [BindProperty]
    public BookingViewModel Booking { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public EditModel(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    public IActionResult OnGet(int? id)
    {
        if (!IsAuthenticated())
            return RedirectToPage("/Auth/Login");

        if (!id.HasValue)
            return NotFound();

        var booking = _bookingService.GetBookingById(id.Value);
        if (booking == null)
            return NotFound();

        Booking = booking;
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
            _bookingService.UpdateBooking(Booking);
            return RedirectToPage("Index");
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
    }

    private bool IsAuthenticated()
    {
        var userId = HttpContext.Session.GetString(AppConstants.UserSessionKey);
        return !string.IsNullOrEmpty(userId);
    }
}
