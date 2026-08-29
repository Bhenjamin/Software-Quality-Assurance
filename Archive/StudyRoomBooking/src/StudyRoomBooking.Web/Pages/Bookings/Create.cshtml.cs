using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.DTOs;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Domain.Models;
using StudyRoomBooking.Web.Services;

namespace StudyRoomBooking.Web.Pages.Bookings;

public class CreateModel : PageModel
{
    private readonly IRoomRepository _roomRepository;
    private readonly IBookingService _bookingService;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public CreateModel(IRoomRepository roomRepository, IBookingService bookingService, ICurrentUserAccessor currentUserAccessor)
    {
        _roomRepository = roomRepository;
        _bookingService = bookingService;
        _currentUserAccessor = currentUserAccessor;
    }

    [BindProperty(SupportsGet = true)]
    public Guid RoomId { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime Start { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime End { get; set; }

    [BindProperty]
    public string Purpose { get; set; } = string.Empty;

    public Room? Room { get; private set; }

    public IActionResult OnGet()
    {
        if (_currentUserAccessor.GetCurrentUser() is null)
        {
            return RedirectToPage("/Index");
        }

        Room = _roomRepository.GetById(RoomId);
        if (Room is null)
        {
            TempData["Error"] = "That room no longer exists.";
            return RedirectToPage("/Rooms/Search");
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        var currentUser = _currentUserAccessor.GetCurrentUser();
        if (currentUser is null)
        {
            return RedirectToPage("/Index");
        }

        var result = _bookingService.CreateBooking(new BookingRequest
        {
            RoomId = RoomId,
            UserId = currentUser.Id,
            StartTime = Start,
            EndTime = End,
            Purpose = Purpose
        });

        if (!result.Success)
        {
            TempData["Error"] = result.ErrorMessage;
            Room = _roomRepository.GetById(RoomId);
            return Page();
        }

        return RedirectToPage("Confirmation", new { bookingId = result.Booking!.Id });
    }
}
