using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Domain.Enums;
using StudyRoomBooking.Domain.Models;

namespace StudyRoomBooking.Web.Pages.Rooms;

/// <summary>
/// Shows a month at a time for one room, so a user can see which days
/// already have bookings before picking a date — instead of having to
/// guess and type an exact date/time up front.
/// </summary>
public class CalendarModel : PageModel
{
    private readonly IRoomRepository _roomRepository;
    private readonly IBookingRepository _bookingRepository;

    public CalendarModel(IRoomRepository roomRepository, IBookingRepository bookingRepository)
    {
        _roomRepository = roomRepository;
        _bookingRepository = bookingRepository;
    }

    [BindProperty(SupportsGet = true)]
    public Guid RoomId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Year { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Month { get; set; }

    public Room? Room { get; private set; }
    public DateTime MonthStart { get; private set; }
    public Dictionary<DateTime, int> BookingCountByDay { get; private set; } = new();

    public IActionResult OnGet()
    {
        Room = _roomRepository.GetById(RoomId);
        if (Room is null)
        {
            TempData["Error"] = "That room no longer exists.";
            return RedirectToPage("Search");
        }

        var today = DateTime.UtcNow.Date;
        var year = Year ?? today.Year;
        var month = Month ?? today.Month;
        MonthStart = new DateTime(year, month, 1);

        var monthEnd = MonthStart.AddMonths(1);
        var bookings = _bookingRepository.GetByRoomId(RoomId)
            .Where(b => b.Status != BookingStatus.Cancelled)
            .Where(b => b.StartTime < monthEnd && b.EndTime > MonthStart);

        BookingCountByDay = bookings
            .GroupBy(b => b.StartTime.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        return Page();
    }
}
