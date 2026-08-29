using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Domain.Enums;
using StudyRoomBooking.Domain.Models;

namespace StudyRoomBooking.Web.Pages.Rooms;

/// <summary>
/// Single-day view for a room: shows existing bookings and lets the user
/// pick a start/end time, then hands off to the booking creation page.
/// </summary>
public class DayModel : PageModel
{
    private readonly IRoomRepository _roomRepository;
    private readonly IBookingRepository _bookingRepository;

    public DayModel(IRoomRepository roomRepository, IBookingRepository bookingRepository)
    {
        _roomRepository = roomRepository;
        _bookingRepository = bookingRepository;
    }

    [BindProperty(SupportsGet = true)]
    public Guid RoomId { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime Date { get; set; }

    [BindProperty]
    public string StartTimeOfDay { get; set; } = "09:00";

    [BindProperty]
    public string EndTimeOfDay { get; set; } = "10:00";

    public Room? Room { get; private set; }
    public List<Booking> ExistingBookings { get; private set; } = new();

    public IActionResult OnGet()
    {
        Room = _roomRepository.GetById(RoomId);
        if (Room is null)
        {
            TempData["Error"] = "That room no longer exists.";
            return RedirectToPage("Search");
        }

        ExistingBookings = _bookingRepository.GetByRoomId(RoomId)
            .Where(b => b.Status != BookingStatus.Cancelled)
            .Where(b => b.StartTime.Date == Date.Date)
            .OrderBy(b => b.StartTime)
            .ToList();

        return Page();
    }

    public IActionResult OnPostPickSlot()
    {
        if (!TimeSpan.TryParse(StartTimeOfDay, out var start) || !TimeSpan.TryParse(EndTimeOfDay, out var end))
        {
            TempData["Error"] = "Please provide valid start and end times.";
            return RedirectToPage(new { roomId = RoomId, date = Date.ToString("yyyy-MM-dd") });
        }

        var startTime = Date.Date + start;
        var endTime = Date.Date + end;

        return RedirectToPage("/Bookings/Create", new
        {
            roomId = RoomId,
            start = startTime.ToString("o"),
            end = endTime.ToString("o")
        });
    }
}
