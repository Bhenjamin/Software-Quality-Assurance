using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Domain.Enums;
using StudyRoomBooking.Web.Utilities;

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
    public int CurrentUserId { get; set; } = 0;
    public List<BuildingLocation> AvailableBuildings { get; set; } = new();

    /// <summary>
    /// Cached slot statuses for the current search: Dictionary[RoomId][TimeSlot] = SlotStatus
    /// </summary>
    public Dictionary<int, Dictionary<TimeSpan, SlotStatus>> SlotStatusCache { get; set; } = new();

    /// <summary>
    /// Cached recurrence dates for the current search
    /// </summary>
    public List<DateTime> RecurrenceDates { get; set; } = new();

    public RecurringBookingModel(IRoomService roomService, IBookingService bookingService)
    {
        _roomService = roomService;
        _bookingService = bookingService;
    }

    public async Task OnGetAsync()
    {
        SearchCriteria.StartDate = DateTime.Today;
        SearchCriteria.RecurrenceEndDate = DateTime.Today.AddDays(30);
        // StartTime and EndTime are nullable, so leave them as null (not set)

        // Get current user ID
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (int.TryParse(userIdStr, out int userId))
        {
            CurrentUserId = userId;
        }

        // Get the current user role from session
        CurrentUserRole = HttpContext.Session.GetString("CurrentUserRole");

        // Populate available buildings
        PopulateAvailableBuildings();

        // Run search on page load to show room-time matrix
        HasSearched = true;
        SearchResults = await _roomService.SearchRoomsAsync(
            SearchCriteria.StartDate,
            SearchCriteria.StartTime,
            SearchCriteria.EndTime,
            SearchCriteria.Capacity,
            SearchCriteria.RoomType,
            SearchCriteria.Location
        );

        // Generate recurrence dates and pre-calculate slot statuses for initial load
        RecurrenceDates = GetRecurrenceDates();
        await PreCalculateSlotStatusesAsync();
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

    /// <summary>
    /// Determines if a time slot is in the past for today's start date.
    /// Returns true only if the start date is today AND the time slot is before current time.
    /// For future dates, this always returns false (no slots are in the past).
    /// </summary>
    public bool IsTimeSlotInPast(TimeSpan timeSlot)
    {
        // Only check for past times if the start date is today
        if (SearchCriteria.StartDate.Date == DateTime.Today)
        {
            var currentTime = DateTime.Now.TimeOfDay;
            var roundedCurrentTime = TimeSpan.FromHours(Math.Floor(currentTime.TotalHours));
            // A slot is in the past if its start time is before rounded current time
            return timeSlot < roundedCurrentTime;
        }

        // For future dates, no slots are in the past
        return false;
    }

    public async Task OnPostAsync()
    {
        HasSearched = true;

        try
        {
            // Get current user ID
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (int.TryParse(userIdStr, out int userId))
            {
                CurrentUserId = userId;
            }

            // Get the current user role from session
            CurrentUserRole = HttpContext.Session.GetString("CurrentUserRole");

            // Only staff can access this page
            if (CurrentUserRole != "Staff")
            {
                ModelState.AddModelError(string.Empty, "Only staff members can search recurring bookings.");
                PopulateAvailableBuildings();
                return;
            }

            // Validate start date is not in the past
            var today = DateTime.Today;
            if (SearchCriteria.StartDate.Date < today)
            {
                ModelState.AddModelError(string.Empty, "Start date cannot be in the past. Please select a date from today onwards.");
                PopulateAvailableBuildings();
                return;
            }

            // Validate start date is not more than 180 days ahead
            var daysInAdvance = (SearchCriteria.StartDate.Date - today).Days;
            if (daysInAdvance > 180)
            {
                ModelState.AddModelError(string.Empty, $"Start date can only be up to 6 months (180 days) ahead. Your selected date is {daysInAdvance} days away.");
                PopulateAvailableBuildings();
                return;
            }

            // Validate recurrence end date is after start date
            if (SearchCriteria.RecurrenceEndDate.Date < SearchCriteria.StartDate.Date)
            {
                ModelState.AddModelError(string.Empty, "Recurrence End Date must be on or after the Start Date.");
                PopulateAvailableBuildings();
                return;
            }

            // Populate available buildings
            PopulateAvailableBuildings();

            // Generate recurrence dates once for the search
            RecurrenceDates = GetRecurrenceDates();

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

            // Pre-calculate slot statuses for all rooms and time slots
            await PreCalculateSlotStatusesAsync();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error searching rooms: {ex.Message}");
            PopulateAvailableBuildings();
        }
    }

    /// <summary>
    /// Calculates the combined slot status across all recurrence dates.
    /// Priority (high to low): Unavailable > Mixed > Booked > YourBooking > Available
    /// 
    /// - Unavailable: if the slot is in the past (StartDate is today and hour has passed)
    /// - Mixed: if both current user and other users have bookings across the recurrence dates
    /// - Booked: if only other users have bookings on one or more recurrence dates
    /// - YourBooking: if only the current user has bookings on one or more recurrence dates
    /// - Available: if the slot is free on every recurrence date
    /// </summary>
    public async Task<SlotStatus> GetSlotStatusAsync(int roomId, TimeSpan timeSlot, List<DateTime> recurrenceDates)
    {
        // Priority 1: Check if slot is in the past (only when StartDate is today)
        if (IsTimeSlotInPast(timeSlot))
        {
            return SlotStatus.Unavailable;
        }

        // Priority 2-5: Analyze bookings across all recurrence dates
        bool hasCurrentUserBooking = false;
        bool hasOtherUserBooking = false;

        foreach (var date in recurrenceDates)
        {
            var bookings = await GetRoomBookingsAsync(roomId, date);
            var nextTime = timeSlot.Add(TimeSpan.FromHours(1));

            // Check if this time slot is booked on this date
            var bookedSlot = bookings.FirstOrDefault(b =>
                !(b.EndTime <= timeSlot || b.StartTime >= nextTime) &&
                b.Status != BookingStatus.Cancelled);

            if (bookedSlot != null)
            {
                // This slot is booked on this date
                if (bookedSlot.UserId == CurrentUserId)
                {
                    hasCurrentUserBooking = true;
                }
                else
                {
                    hasOtherUserBooking = true;
                }
            }
        }

        // Determine final status based on priority
        if (hasCurrentUserBooking && hasOtherUserBooking)
        {
            return SlotStatus.Mixed;
        }
        else if (hasOtherUserBooking)
        {
            return SlotStatus.Booked;
        }
        else if (hasCurrentUserBooking)
        {
            return SlotStatus.YourBooking;
        }
        else
        {
            // No bookings found on any date, slot is available on all dates
            return SlotStatus.Available;
        }
    }

    /// <summary>
    /// Pre-calculates slot statuses for all rooms and time slots in the search results.
    /// This avoids recalculating statuses repeatedly in the view.
    /// </summary>
    private async Task PreCalculateSlotStatusesAsync()
    {
        SlotStatusCache.Clear();

        var startTime = new TimeSpan(8, 0, 0);
        var endTime = new TimeSpan(22, 0, 0);
        var interval = TimeSpan.FromHours(1);

        foreach (var room in SearchResults)
        {
            SlotStatusCache[room.Id] = new Dictionary<TimeSpan, SlotStatus>();

            var currentTime = startTime;
            while (currentTime < endTime)
            {
                var status = await GetSlotStatusAsync(room.Id, currentTime, RecurrenceDates);
                SlotStatusCache[room.Id][currentTime] = status;
                currentTime = currentTime.Add(interval);
            }
        }
    }

    /// <summary>
    /// Generates all recurrence dates based on the selected recurrence pattern.
    /// - None: returns only the StartDate
    /// - Daily: every date from StartDate to RecurrenceEndDate inclusive
    /// - Weekly: every 7 days starting from StartDate
    /// - BiWeekly: every 14 days starting from StartDate
    /// - FourWeeks: every 28 days starting from StartDate
    /// </summary>
    public List<DateTime> GetRecurrenceDates()
    {
        var dates = new List<DateTime>();
        var pattern = SearchCriteria.RecurrencePattern ?? RecurrencePattern.None;

        if (pattern == RecurrencePattern.None)
        {
            // Only the start date
            dates.Add(SearchCriteria.StartDate);
        }
        else
        {
            int interval = pattern switch
            {
                RecurrencePattern.Daily => 1,
                RecurrencePattern.Weekly => 7,
                RecurrencePattern.BiWeekly => 14,
                RecurrencePattern.FourWeeks => 28,
                _ => 1
            };

            var currentDate = SearchCriteria.StartDate;
            while (currentDate <= SearchCriteria.RecurrenceEndDate)
            {
                dates.Add(currentDate);
                currentDate = currentDate.AddDays(interval);
            }
        }

        return dates;
    }

    private void PopulateAvailableBuildings()
    {
        AvailableBuildings = Enum.GetValues(typeof(BuildingLocation))
            .Cast<BuildingLocation>()
            .ToList();
    }
}
