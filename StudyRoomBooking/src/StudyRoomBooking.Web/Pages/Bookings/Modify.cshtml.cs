using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.DTOs;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Domain.Models;
using StudyRoomBooking.Web.Services;

namespace StudyRoomBooking.Web.Pages.Bookings;

public class ModifyModel : PageModel
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IBookingService _bookingService;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public ModifyModel(
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

    [BindProperty]
    public DateTime NewStart { get; set; }

    [BindProperty]
    public DateTime NewEnd { get; set; }

    public Booking? Booking { get; private set; }
    public Room? Room { get; private set; }

    public IActionResult OnGet()
    {
        var currentUser = _currentUserAccessor.GetCurrentUser();
        if (currentUser is null)
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
        NewStart = Booking.StartTime;
        NewEnd = Booking.EndTime;
        return Page();
    }

    public IActionResult OnPost()
    {
        var currentUser = _currentUserAccessor.GetCurrentUser();
        if (currentUser is null)
        {
            return RedirectToPage("/Index");
        }

        var result = _bookingService.ModifyBooking(new BookingModificationRequest
        {
            BookingId = BookingId,
            RequestingUserId = currentUser.Id,
            NewStartTime = NewStart,
            NewEndTime = NewEnd
        });

        if (!result.Success)
        {
            TempData["Error"] = result.ErrorMessage;
            Booking = _bookingRepository.GetById(BookingId);
            Room = Booking is not null ? _roomRepository.GetById(Booking.RoomId) : null;
            return Page();
        }

        TempData["Success"] = "Booking updated.";
        return RedirectToPage("History");
    }
}
