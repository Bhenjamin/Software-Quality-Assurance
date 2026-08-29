using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Web.Pages.Staff;

public class RecurringBookingModel : PageModel
{
    private readonly IRoomService _roomService;
    private readonly IBookingService _bookingService;

    [BindProperty]
    public RecurringBookingSearchCriteria SearchCriteria { get; set; } = new();

    public List<Room> SearchResults { get; set; } = new();
    public bool HasSearched { get; set; } = false;
    public string? CurrentUserRole { get; set; } = null;

    public RecurringBookingModel(IRoomService roomService, IBookingService bookingService)
    {
        _roomService = roomService;
        _bookingService = bookingService;
    }

    public void OnGet()
    {
        SearchCriteria.StartDate = DateTime.Today;
        SearchCriteria.RecurrenceEndDate = DateTime.Today.AddDays(30);
        // StartTime and EndTime are nullable, so leave them as null (not set)

        // Get the current user role from session
        CurrentUserRole = HttpContext.Session.GetString("CurrentUserRole");

        // Staff can search all room types - no restriction here
    }

    public async Task<List<Domain.Entities.Booking>> GetRoomBookingsAsync(int roomId, DateTime date)
    {
        return await _bookingService.SearchBookingsAsync(date, roomId);
    }

    /// <summary>
    /// Filters rooms to show only those that are free during the specified time slot(s)
    /// across the ENTIRE recurrence date range (StartDate to RecurrenceEndDate).
    /// Returns only rooms that are consistently available during the search hours on every day
    /// in the recurrence period.
    /// </summary>
    public async Task<List<Room>> FilterRoomsByDateRangeAvailabilityAsync(List<Room> rooms)
    {
        // If no time criteria are set, return all rooms (availability check not needed)
        if (!SearchCriteria.StartTime.HasValue && !SearchCriteria.EndTime.HasValue)
        {
            return rooms;
        }

        var filteredRooms = new List<Room>();

        foreach (var room in rooms)
        {
            bool isAvailableAcrossRange = true;

            // Check availability for each day in the recurrence range
            for (var date = SearchCriteria.StartDate; date <= SearchCriteria.RecurrenceEndDate; date = date.AddDays(1))
            {
                var bookingsForRoom = await GetRoomBookingsAsync(room.Id, date);

                // Define the time window to check
                TimeSpan searchStartTime = SearchCriteria.StartTime ?? new TimeSpan(8, 0, 0);
                TimeSpan searchEndTime = SearchCriteria.EndTime ?? new TimeSpan(22, 0, 0);

                // Check if the room is booked during the search time window on this date
                bool isBookedInTimeWindow = bookingsForRoom.Any(b =>
                    !(b.EndTime <= searchStartTime || b.StartTime >= searchEndTime) &&
                    b.Status != BookingStatus.Cancelled);

                if (isBookedInTimeWindow)
                {
                    isAvailableAcrossRange = false;
                    break; // This room is not available on at least one day, exclude it
                }
            }

            // Add room only if it's available across all days in the range
            if (isAvailableAcrossRange)
            {
                filteredRooms.Add(room);
            }
        }

        return filteredRooms;
    }

    /// <summary>
    /// Determines if a time slot should be displayed based on search criteria.
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

            // Only staff can access this page
            if (CurrentUserRole != "Staff")
            {
                ModelState.AddModelError(string.Empty, "Only staff members can search recurring bookings.");
                return;
            }

            // Validate start date is not in the past
            var today = DateTime.Today;
            if (SearchCriteria.StartDate.Date < today)
            {
                ModelState.AddModelError(string.Empty, "Start date cannot be in the past. Please select a date from today onwards.");
                return;
            }

            // Validate start date is not more than 60 days in advance
            var daysInAdvance = (SearchCriteria.StartDate.Date - today).Days;
            if (daysInAdvance > 60)
            {
                ModelState.AddModelError(string.Empty, $"Start date can only be up to 60 days in advance. Your selected date is {daysInAdvance} days away.");
                return;
            }

            // Validate recurrence end date is after start date
            if (SearchCriteria.RecurrenceEndDate.Date < SearchCriteria.StartDate.Date)
            {
                ModelState.AddModelError(string.Empty, "Recurrence End Date must be on or after the Start Date.");
                return;
            }

            // Use the start date for room availability search (showing first day of recurrence pattern)
            SearchResults = await _roomService.SearchRoomsAsync(
                SearchCriteria.StartDate,
                SearchCriteria.StartTime,
                SearchCriteria.EndTime,
                SearchCriteria.Capacity,
                SearchCriteria.RoomType,
                SearchCriteria.Location
            );

            // If time criteria are set, filter to show only rooms available across the entire date range
            if (SearchCriteria.StartTime.HasValue || SearchCriteria.EndTime.HasValue)
            {
                SearchResults = await FilterRoomsByDateRangeAvailabilityAsync(SearchResults);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error searching rooms: {ex.Message}");
        }
    }
}
