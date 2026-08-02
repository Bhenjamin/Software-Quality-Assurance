using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Domain.Models;
using StudyRoomBooking.Web.Services;

namespace StudyRoomBooking.Web.Pages.Admin;

public class BookingsOverviewModel : PageModel
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAccessControlService _accessControlService;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public BookingsOverviewModel(
        IBookingRepository bookingRepository,
        IRoomRepository roomRepository,
        IUserRepository userRepository,
        IAccessControlService accessControlService,
        ICurrentUserAccessor currentUserAccessor)
    {
        _bookingRepository = bookingRepository;
        _roomRepository = roomRepository;
        _userRepository = userRepository;
        _accessControlService = accessControlService;
        _currentUserAccessor = currentUserAccessor;
    }

    public IReadOnlyList<Booking> Bookings { get; private set; } = Array.Empty<Booking>();
    public Dictionary<Guid, Room> RoomsById { get; private set; } = new();
    public Dictionary<Guid, User> UsersById { get; private set; } = new();

    public IActionResult OnGet()
    {
        var admin = _currentUserAccessor.GetCurrentUser();
        if (admin is null)
        {
            return RedirectToPage("/Index");
        }

        if (!_accessControlService.CanPerformAdminAction(admin))
        {
            TempData["Error"] = "Administrator access is required to view this page.";
            return RedirectToPage("/Rooms/Search");
        }

        // NOTE for Developer 3: this is the natural place to grow into the
        // room-utilisation reporting feature (counts, filters, date ranges)
        // once the traceability/quality-metrics work defines what to show.
        Bookings = _bookingRepository.GetAll().OrderByDescending(b => b.StartTime).ToList();
        RoomsById = _roomRepository.GetAll().ToDictionary(r => r.Id);
        UsersById = _userRepository.GetAll().ToDictionary(u => u.Id);
        return Page();
    }
}
