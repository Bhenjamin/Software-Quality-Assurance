using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Domain.Models;
using StudyRoomBooking.Web.Services;

namespace StudyRoomBooking.Web.Pages.Bookings;

public class CancelModel : PageModel
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IBookingService _bookingService;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public CancelModel(
        IBookingRepository bookingRepository,
        IRoomRepository roomRepository,
        IBookingService bookingService,
        ICurrentUserAccessor currentUserAccessor)
    {
        _bookingRepository = bookingRepository;
        _roomRepository = roomRepository;
        _bookingService = bookingService;
        _currentUserAccessor = currentUserAccessor;
    }

    [BindProperty(SupportsGet = true)]
    public Guid BookingId { get; set; }

    public Booking? Booking { get; private set; }
    public Room? Room { get; private set; }

    public IActionResult OnGet()
    {
        if (_currentUserAccessor.GetCurrentUser() is null)
        {
            return RedirectToPage("/Index");
        }

        Booking = _bookingRepository.GetById(BookingId);
        if (Booking is null)
        {
            TempData["Error"] = "That booking could not be found.";
            return RedirectToPage("History");
        }

        Room = _roomRepository.GetById(Booking.RoomId);
        return Page();
    }

    public IActionResult OnPost()
    {
        var currentUser = _currentUserAccessor.GetCurrentUser();
        if (currentUser is null)
        {
            return RedirectToPage("/Index");
        }

        var result = _bookingService.CancelBooking(BookingId, currentUser.Id, "Cancelled via web UI");

        TempData[result.Success ? "Success" : "Error"] = result.Success
            ? "Booking cancelled."
            : result.ErrorMessage;

        return RedirectToPage("History");
    }
}
