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
    private readonly IUserService _userService;

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

    /// <summary>
    /// Cached bookings by room for the current search: Dictionary[RoomId] = List of bookings
    /// Populated once per search to avoid N+1 database queries
    /// </summary>
    private Dictionary<int, List<Booking>> _roomBookingsCache = new();

    public RecurringBookingModel(IRoomService roomService, IBookingService bookingService, IUserService userService)
    {
        _roomService = roomService;
        _bookingService = bookingService;
        _userService = userService;
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
    /// API handler for creating a recurring booking via AJAX from the modal.
    /// Validates inputs, generates recurrence dates, checks availability, and creates bookings.
    /// Returns JSON response with success/error status.
    /// </summary>
    public async Task<IActionResult> OnPostCreateRecurringBooking(int roomId, string startDate, string startTime, 
        string endTime, string recurrencePattern, string recurrenceEndDate, string? notes)
    {
        try
        {
            // Get current user ID
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdStr, out int userId))
            {
                return new JsonResult(new { success = false, error = "User session expired. Please login again." })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }

            // Verify user is staff
            var currentUserRole = HttpContext.Session.GetString("CurrentUserRole");
            if (currentUserRole != "Staff")
            {
                return new JsonResult(new { success = false, error = "Only staff members can create recurring bookings." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            // Get room
            var room = await _roomService.GetRoomByIdAsync(roomId);
            if (room == null)
            {
                return new JsonResult(new { success = false, error = "Room not found." })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Get user
            var userIdObj = HttpContext.Session.GetString("CurrentUser");
            if (string.IsNullOrEmpty(userIdObj))
            {
                return new JsonResult(new { success = false, error = "User session expired. Please login again." })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }

            var user = await _userService.GetUserByUserIdAsync(userIdObj);
            if (user == null)
            {
                return new JsonResult(new { success = false, error = "User not found." })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Parse dates and times
            if (!DateTime.TryParseExact(startDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var date))
            {
                return new JsonResult(new { success = false, error = "Invalid start date format." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            TimeSpan start, end;
            try
            {
                start = TimeSpan.ParseExact(startTime, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                start = TimeSpan.ParseExact(startTime, @"h\:mm", System.Globalization.CultureInfo.InvariantCulture);
            }

            try
            {
                end = TimeSpan.ParseExact(endTime, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                end = TimeSpan.ParseExact(endTime, @"h\:mm", System.Globalization.CultureInfo.InvariantCulture);
            }

            // Validate time range (start must be before end)
            if (start >= end)
            {
                return new JsonResult(new { success = false, error = "Start time must be before end time." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Validate time is within allowed range (8:00 to 22:00)
            var minTime = new TimeSpan(8, 0, 0);
            var maxTime = new TimeSpan(22, 0, 0);
            if (start < minTime || end > maxTime)
            {
                return new JsonResult(new { success = false, error = "Booking time must be between 8:00 AM and 10:00 PM." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Validate booking date is not in the past
            var today = DateTime.Today;
            if (date.Date < today)
            {
                return new JsonResult(new { success = false, error = "Cannot book for dates in the past. Please select today or later." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Parse recurrence end date
            DateTime? recurrenceEnd = null;
            if (!string.IsNullOrEmpty(recurrenceEndDate) && recurrenceEndDate != "")
            {
                if (!DateTime.TryParseExact(recurrenceEndDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedEndDate))
                {
                    return new JsonResult(new { success = false, error = "Invalid recurrence end date format." })
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
                recurrenceEnd = parsedEndDate;

                // Validate recurrence end date is not before start date
                if (recurrenceEnd.Value.Date < date.Date)
                {
                    return new JsonResult(new { success = false, error = "Recurrence end date must be on or after the start date." })
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
            }

            // Parse recurrence pattern
            if (!Enum.TryParse<RecurrencePattern>(recurrencePattern, out var pattern))
            {
                return new JsonResult(new { success = false, error = "Invalid recurrence pattern." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Validate booking date and time constraints for the start date
            var (isValid, errorMessage) = await _bookingService.ValidateBookingAsync(roomId, date, start, end, skipAdvanceDaysCheck: true);
            if (!isValid)
            {
                return new JsonResult(new { success = false, error = errorMessage })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Check availability for start date
            var isAvailable = await _roomService.IsRoomAvailableAsync(roomId, date, start, end);
            if (!isAvailable)
            {
                return new JsonResult(new { success = false, error = "Selected time slot is not available." })
                {
                    StatusCode = StatusCodes.Status409Conflict
                };
            }

            // Generate all recurrence dates
            DateTime endDateForRecurrence = recurrenceEnd ?? date;
            var recurrenceDates = _bookingService.GenerateRecurrenceDates(date, endDateForRecurrence, pattern);

            // Validate availability and constraints for all recurrence dates
            foreach (var occurrenceDate in recurrenceDates)
            {
                var (dateIsValid, dateErrorMessage) = await _bookingService.ValidateBookingAsync(roomId, occurrenceDate, start, end, skipAdvanceDaysCheck: true);
                if (!dateIsValid)
                {
                    return new JsonResult(new { success = false, error = $"Cannot create recurring booking: {dateErrorMessage} (Date: {occurrenceDate:yyyy-MM-dd})" })
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }

                var dateIsAvailable = await _roomService.IsRoomAvailableAsync(roomId, occurrenceDate, start, end);
                if (!dateIsAvailable)
                {
                    return new JsonResult(new { success = false, error = $"Selected time slot is not available on {occurrenceDate:yyyy-MM-dd}." })
                    {
                        StatusCode = StatusCodes.Status409Conflict
                    };
                }
            }

            // Create individual bookings for each recurrence date
            foreach (var occurrenceDate in recurrenceDates)
            {
                var booking = new Booking
                {
                    RoomId = roomId,
                    UserId = user.Id,
                    BookingDate = occurrenceDate,
                    StartTime = start,
                    EndTime = end,
                    RecurrencePattern = RecurrencePattern.None,  // Each booking is standalone
                    RecurrenceEndDate = null,
                    Notes = notes,
                    Status = BookingStatus.Confirmed
                };

                await _bookingService.CreateBookingAsync(booking);
            }

            // Build success message
            var message = $"Recurring booking created successfully with {recurrenceDates.Count} bookings!";

            return new JsonResult(new
            {
                success = true,
                message = message,
                roomName = room.Name,
                startDate = date.ToString("yyyy-MM-dd"),
                recurrenceEndDate = (recurrenceEnd ?? date).ToString("yyyy-MM-dd"),
                startTime = start.ToString(@"hh\:mm"),
                endTime = end.ToString(@"hh\:mm"),
                recurrencePattern = recurrencePattern,
                bookingCount = recurrenceDates.Count
            });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = $"Error creating booking: {ex.Message}" })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
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

        // Get cached bookings for this room (pre-loaded in PreCalculateSlotStatusesAsync)
        if (!_roomBookingsCache.TryGetValue(roomId, out var cachedBookings))
        {
            cachedBookings = new List<Booking>();
        }

        foreach (var date in recurrenceDates)
        {
            // Filter cached bookings to only those for this specific date
            var bookingsForDate = cachedBookings
                .Where(b => b.BookingDate.Date == date.Date)
                .ToList();

            var nextTime = timeSlot.Add(TimeSpan.FromHours(1));

            // Check if this time slot is booked on this date
            var bookedSlot = bookingsForDate.FirstOrDefault(b =>
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
        _roomBookingsCache.Clear();

        // Pre-load all bookings for each room across the recurrence date range
        // This avoids N+1 queries in GetSlotStatusAsync
        foreach (var room in SearchResults)
        {
            var bookings = await _bookingService.GetRoomBookingsByDateRangeAsync(
                room.Id,
                RecurrenceDates.Min(),
                RecurrenceDates.Max()
            );
            _roomBookingsCache[room.Id] = bookings;
        }

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
