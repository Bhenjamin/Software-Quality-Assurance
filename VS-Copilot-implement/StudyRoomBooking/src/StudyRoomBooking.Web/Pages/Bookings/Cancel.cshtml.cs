using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Infrastructure.Shared.Constants;

namespace StudyRoomBooking.Web.Pages.Bookings;

public class CancelModel : PageModel
{
    private readonly IBookingService _bookingService;
    private readonly IRoomService _roomService;

    [BindProperty]
    public BookingViewModel? Booking { get; set; }

    [BindProperty]
    public RoomViewModel? Room { get; set; }

    public CancelModel(IBookingService bookingService, IRoomService roomService)
    {
        _bookingService = bookingService;
        _roomService = roomService;
    }

    public IActionResult OnGet(int? id)
    {
        // Support both route parameter and query string formats
        id = id ?? (int.TryParse(Request.Query["id"], out int queryId) ? queryId : null);

        if (!IsAuthenticated())
            return RedirectToPage("/Auth/Login");

        if (!id.HasValue)
            return NotFound();

        var userId = GetCurrentUserId();
        var booking = _bookingService.GetBookingById(id.Value);

        if (booking == null)
        {
            TempData["ErrorMessage"] = "Booking not found.";
            return RedirectToPage("Index");
        }

        // Authorization: user can only cancel their own bookings
        if (booking.UserId != userId)
        {
            TempData["ErrorMessage"] = "You cannot cancel another user's booking.";
            return RedirectToPage("Index");
        }

        // Check if booking can be cancelled
        if (!CanCancelBooking(booking))
        {
            TempData["ErrorMessage"] = "This booking cannot be cancelled. It may be in the past or already cancelled.";
            return RedirectToPage("Index");
        }

        Booking = booking;
        Room = _roomService.GetRoomById(booking.RoomId);

        return Page();
    }

    public IActionResult OnPost(int? id)
    {
        // Support both route parameter and query string formats
        id = id ?? (int.TryParse(Request.Query["id"], out int queryId) ? queryId : null);

        if (!IsAuthenticated())
            return RedirectToPage("/Auth/Login");

        if (!id.HasValue)
            return NotFound();

        var userId = GetCurrentUserId();
        var booking = _bookingService.GetBookingById(id.Value);

        if (booking == null)
        {
            TempData["ErrorMessage"] = "Booking not found.";
            return RedirectToPage("Index");
        }

        // Authorization: user can only cancel their own bookings
        if (booking.UserId != userId)
        {
            TempData["ErrorMessage"] = "You cannot cancel another user's booking.";
            return RedirectToPage("Index");
        }

        // Check if booking can be cancelled
        if (!CanCancelBooking(booking))
        {
            TempData["ErrorMessage"] = "This booking cannot be cancelled.";
            return RedirectToPage("Index");
        }

        try
        {
            _bookingService.CancelBooking(id.Value);
            TempData["SuccessMessage"] = "Your booking has been cancelled successfully.";
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error cancelling booking: {ex.Message}";
            return Page();
        }
    }

    private int GetCurrentUserId()
    {
        return int.Parse(HttpContext.Session.GetString(AppConstants.UserSessionKey) ?? "0");
    }

    private bool IsAuthenticated()
    {
        var userId = HttpContext.Session.GetString(AppConstants.UserSessionKey);
        return !string.IsNullOrEmpty(userId);
    }

    private bool CanCancelBooking(BookingViewModel booking)
    {
        // Cannot cancel bookings that are already cancelled
        if (booking.Status.ToString() == "Cancelled")
            return false;

        // Cannot cancel bookings in the past (same day or earlier)
        if (booking.BookingDate < DateTime.Now.Date)
            return false;

        // Cannot cancel bookings on the same day
        // Optional: You can remove this if you want to allow same-day cancellations
        // if (booking.BookingDate == DateTime.Now.Date)
        //     return false;

        return true;
    }
}
