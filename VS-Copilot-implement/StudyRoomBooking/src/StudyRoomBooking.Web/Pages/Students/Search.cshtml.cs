using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Web.Pages.Students;

public class SearchModel : PageModel
{
    private readonly IRoomService _roomService;
    private readonly IBookingService _bookingService;

    [BindProperty]
    public RoomSearchCriteria SearchCriteria { get; set; } = new();

    public List<Room> SearchResults { get; set; } = new();
    public bool HasSearched { get; set; } = false;
    public string? CurrentUserRole { get; set; } = null;

    public SearchModel(IRoomService roomService, IBookingService bookingService)
    {
        _roomService = roomService;
        _bookingService = bookingService;
    }

    public void OnGet()
    {
        SearchCriteria.BookingDate = DateTime.Today;
        // StartTime and EndTime are nullable, so leave them as null (not set)

        // Get the current user role from session
        CurrentUserRole = HttpContext.Session.GetString("CurrentUserRole");

        // Apply role-based room type filtering
        if (CurrentUserRole == "Student")
        {
            // Students can only search for Study rooms
            SearchCriteria.RoomType = RoomType.Study;
        }
        // Staff can search all room types - no restriction here
    }

    public async Task<List<Domain.Entities.Booking>> GetRoomBookingsAsync(int roomId, DateTime date)
    {
        return await _bookingService.SearchBookingsAsync(date, roomId);
    }

    /// <summary>
    /// Determines if a time slot should be displayed as bookable based on search criteria.
    /// Returns true if the time slot matches the user's time range selection.
    /// - If neither start nor end is set: always return true
    /// - If only start is set: return true if timeSlot >= start
    /// - If only end is set: return true if timeSlot <= end
    /// - If both are set: return true if timeSlot >= start AND timeSlot < end
    /// </summary>
    public bool IsTimeSlotInSearchRange(TimeSpan timeSlot)
    {
        bool isStartSet = SearchCriteria.StartTime.HasValue;
        bool isEndSet = SearchCriteria.EndTime.HasValue;

        if (!isStartSet && !isEndSet)
        {
            // Neither set - show all time slots
            return true;
        }
        else if (isStartSet && !isEndSet)
        {
            // Only start is set - show from start to 22:00
            return timeSlot >= SearchCriteria.StartTime.Value;
        }
        else if (!isStartSet && isEndSet)
        {
            // Only end is set - show from 8:00 to end
            return timeSlot < SearchCriteria.EndTime.Value;
        }
        else
        {
            // Both set - show from start to end
            return timeSlot >= SearchCriteria.StartTime.Value && timeSlot < SearchCriteria.EndTime.Value;
        }
    }

    public async Task OnPostAsync()
    {
        HasSearched = true;

        try
        {
            // Get the current user role from session
            CurrentUserRole = HttpContext.Session.GetString("CurrentUserRole");

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

            // Apply role-based room type filtering
            if (CurrentUserRole == "Student")
            {
                // Students can only book Study rooms - override any filter selection
                SearchCriteria.RoomType = RoomType.Study;
            }
            // Staff can search all room types - use their selected filter as-is

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
