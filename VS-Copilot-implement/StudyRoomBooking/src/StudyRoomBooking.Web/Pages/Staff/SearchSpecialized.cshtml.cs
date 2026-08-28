using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Domain.Entities;

namespace StudyRoomBooking.Web.Pages.Staff;

public class SearchSpecializedModel : PageModel
{
    private readonly IRoomService _roomService;
    private readonly IBookingService _bookingService;

    public RoomSearchCriteria SearchCriteria { get; set; } = new();
    public List<Room> SearchResults { get; set; } = new();
    public bool HasSearched { get; set; } = false;

    public SearchSpecializedModel(IRoomService roomService, IBookingService bookingService)
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
            // Use provided times as-is (nullable) - no defaults
            // When both are null, all rooms will be shown; when one or both are set, filtering applies
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
