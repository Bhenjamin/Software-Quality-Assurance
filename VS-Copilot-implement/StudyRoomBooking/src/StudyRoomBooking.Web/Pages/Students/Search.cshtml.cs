using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Domain.Entities;

namespace StudyRoomBooking.Web.Pages.Students;

public class SearchModel : PageModel
{
    private readonly IRoomService _roomService;
    private readonly IBookingService _bookingService;

    [BindProperty]
    public RoomSearchCriteria SearchCriteria { get; set; } = new();

    public List<Room> SearchResults { get; set; } = new();
    public bool HasSearched { get; set; } = false;

    public SearchModel(IRoomService roomService, IBookingService bookingService)
    {
        _roomService = roomService;
        _bookingService = bookingService;
    }

    public void OnGet()
    {
        SearchCriteria.BookingDate = DateTime.Today;
    }

    public async Task<List<Domain.Entities.Booking>> GetRoomBookingsAsync(int roomId, DateTime date)
    {
        return await _bookingService.SearchBookingsAsync(date, roomId);
    }

    public async Task OnPostAsync()
    {
        HasSearched = true;

        try
        {
            // Validate booking date is not in the past
            var today = DateTime.Today;
            if (SearchCriteria.BookingDate.Date < today)
            {
                ModelState.AddModelError(string.Empty, "Cannot search for bookings in the past. Please select a date from today onwards.");
                return;
            }

            // Validate booking date is not more than 60 days in advance
            var daysInAdvance = (SearchCriteria.BookingDate.Date - today).Days;
            if (daysInAdvance > 60)
            {
                ModelState.AddModelError(string.Empty, $"Bookings can only be made up to 60 days in advance. Your selected date is {daysInAdvance} days away.");
                return;
            }

            SearchResults = await _roomService.SearchRoomsAsync(
                SearchCriteria.BookingDate,
                SearchCriteria.StartTime,
                SearchCriteria.EndTime,
                SearchCriteria.Capacity,
                SearchCriteria.RoomType,
                SearchCriteria.Location
            );
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error searching rooms: {ex.Message}");
        }
    }
}
