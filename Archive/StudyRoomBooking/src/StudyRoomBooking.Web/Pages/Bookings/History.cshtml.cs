using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Domain.Models;
using StudyRoomBooking.Web.Services;

namespace StudyRoomBooking.Web.Pages.Bookings;

public class HistoryModel : PageModel
{
    private readonly IBookingService _bookingService;
    private readonly IRoomRepository _roomRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public HistoryModel(IBookingService bookingService, IRoomRepository roomRepository, ICurrentUserAccessor currentUserAccessor)
    {
        _bookingService = bookingService;
        _roomRepository = roomRepository;
        _currentUserAccessor = currentUserAccessor;
    }

    public IReadOnlyList<Booking> Bookings { get; private set; } = Array.Empty<Booking>();
    public Dictionary<Guid, Room> RoomsById { get; private set; } = new();

    public IActionResult OnGet()
    {
        var currentUser = _currentUserAccessor.GetCurrentUser();
        if (currentUser is null)
        {
            return RedirectToPage("/Index");
        }

        Bookings = _bookingService.GetBookingHistory(currentUser.Id).ToList();
        RoomsById = _roomRepository.GetAll().ToDictionary(r => r.Id);
        return Page();
    }
}
