using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Domain.Models;

namespace StudyRoomBooking.Web.Pages.Bookings;

public class ConfirmationModel : PageModel
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;

    public ConfirmationModel(IBookingRepository bookingRepository, IRoomRepository roomRepository)
    {
        _bookingRepository = bookingRepository;
        _roomRepository = roomRepository;
    }

    public Booking? Booking { get; private set; }
    public Room? Room { get; private set; }

    public IActionResult OnGet(Guid bookingId)
    {
        Booking = _bookingRepository.GetById(bookingId);
        if (Booking is null)
        {
            TempData["Error"] = "That booking could not be found.";
            return RedirectToPage("/Rooms/Search");
        }

        Room = _roomRepository.GetById(Booking.RoomId);
        return Page();
    }
}
