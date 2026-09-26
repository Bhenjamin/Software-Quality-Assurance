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
    /// Finds the user's booking for a specific room and date, then fetches details and calculates per-date statuses.
    /// Called when user clicks on a blue (YourBooking) slot.
    /// </summary>
    public async Task<IActionResult> OnPostGetRecurringBookingDetailsAsync(int roomId, string startDate, string recurrenceEndDate, string recurrencePattern, string newStartTime, string newEndTime)
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

            // Parse startDate
            if (!DateTime.TryParse(startDate, out DateTime startDateObj))
            {
                return new JsonResult(new { success = false, error = "Invalid start date format." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Parse recurrence end date
            if (!DateTime.TryParse(recurrenceEndDate, out DateTime endDateObj))
            {
                return new JsonResult(new { success = false, error = "Invalid end date format." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Parse recurrence pattern
            if (!Enum.TryParse<RecurrencePattern>(recurrencePattern, out var requestedPattern))
            {
                return new JsonResult(new { success = false, error = "Invalid recurrence pattern." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Parse new times first so we can use them for validation
            TimeSpan newStart, newEnd;
            try
            {
                newStart = TimeSpan.ParseExact(newStartTime, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture);
                newEnd = TimeSpan.ParseExact(newEndTime, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return new JsonResult(new { success = false, error = "Invalid time format." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Validate time range
            if (newStart >= newEnd)
            {
                return new JsonResult(new { success = false, error = "Start time must be before end time." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Search for user's recurring booking in a wide date range
            var searchStartDate = startDateObj.AddDays(-60);
            var searchEndDate = endDateObj.AddDays(60);

            var allRoomBookings = await _bookingService.GetRoomBookingsByDateRangeAsync(
                roomId,
                searchStartDate,
                searchEndDate
            );

            // Find the user's recurring booking that matches:
            // Case 1: A single recurring booking (RecurrencePattern != None) that spans the date range
            var userBooking = allRoomBookings
                .Where(b => 
                    b.UserId == userId && 
                    b.Status != BookingStatus.Cancelled && 
                    b.RecurrencePattern == requestedPattern && 
                    b.RecurrenceEndDate.HasValue && 
                    b.RecurrenceEndDate.Value.Date >= endDateObj.Date.AddDays(-5) && // End date roughly matches
                    b.BookingDate.Date <= startDateObj.Date) // Booking starts before or on the search start date
                .OrderBy(b => b.BookingDate)
                .FirstOrDefault();

            if (userBooking == null)
            {
                // Try a more lenient search - just find ANY recurring booking by this user in this room
                userBooking = allRoomBookings
                    .Where(b => 
                        b.UserId == userId && 
                        b.Status != BookingStatus.Cancelled && 
                        b.RecurrencePattern != RecurrencePattern.None && 
                        b.RecurrenceEndDate.HasValue)
                    .OrderByDescending(b => b.BookingDate)
                    .FirstOrDefault();
            }

            // Case 2: If no recurring booking found, look for individual bookings from CreateRecurring
            // These are bookings with RecurrencePattern = None that form a series
            if (userBooking == null)
            {
                // Find all individual bookings for this user in this room within the requested date range
                // (Don't filter by exact time - user might be trying different time slots with this series)
                var userBookingsInRange = allRoomBookings
                    .Where(b =>
                        b.UserId == userId &&
                        b.Status != BookingStatus.Cancelled &&
                        b.RecurrencePattern == RecurrencePattern.None &&
                        b.RoomId == roomId &&
                        b.BookingDate.Date >= startDateObj.Date.AddDays(-7) &&
                        b.BookingDate.Date <= endDateObj.Date.AddDays(7))
                    .OrderBy(b => b.BookingDate)
                    .ToList();

                // If we found bookings, check if they form a pattern
                if (userBookingsInRange.Count >= 1)
                {
                    // Use the first booking as a template
                    var firstBooking = userBookingsInRange.First();

                    // Check if these bookings follow a recurrence pattern
                    // by checking the date differences
                    bool isValidSeries = false;
                    int interval = 1; // Default to daily

                    if (userBookingsInRange.Count >= 2)
                    {
                        // Calculate the interval from the first two bookings
                        var firstInterval = (userBookingsInRange[1].BookingDate.Date - userBookingsInRange[0].BookingDate.Date).Days;

                        // Verify this interval is consistent for requested pattern
                        if (Enum.TryParse<RecurrencePattern>(recurrencePattern, out var parsedPattern))
                        {
                            var expectedInterval = parsedPattern switch
                            {
                                RecurrencePattern.Daily => 1,
                                RecurrencePattern.Weekly => 7,
                                RecurrencePattern.BiWeekly => 14,
                                RecurrencePattern.FourWeeks => 28,
                                _ => 1
                            };

                            // If the interval matches or is multiples (accounting for gaps), this is likely a valid series
                            isValidSeries = (firstInterval == expectedInterval || firstInterval % expectedInterval == 0);
                        }
                        else
                        {
                            // If can't parse pattern, just accept any consistent interval
                            isValidSeries = true;
                            interval = firstInterval;
                        }
                    }
                    else
                    {
                        // Single booking - still treat as potential series start
                        isValidSeries = true;
                    }

                    if (isValidSeries)
                    {
                        // This is a series of individual bookings. Create a virtual recurring booking using REQUESTED dates
                        userBooking = new Booking
                        {
                            Id = firstBooking.Id,
                            RoomId = roomId,
                            UserId = userId,
                            BookingDate = startDateObj,  // Use REQUESTED start date
                            StartTime = firstBooking.StartTime,  // Use first booking's time as reference
                            EndTime = firstBooking.EndTime,
                            RecurrenceEndDate = endDateObj,  // Use REQUESTED end date
                            Status = BookingStatus.Confirmed
                        };

                        // Set the pattern from the request parameter
                        if (Enum.TryParse<RecurrencePattern>(recurrencePattern, out var pattern))
                        {
                            userBooking.RecurrencePattern = pattern;
                        }
                        else if (userBookingsInRange.Count >= 2)
                        {
                            // Calculate pattern from booking dates
                            var daysDiff = (userBookingsInRange[1].BookingDate.Date - userBookingsInRange[0].BookingDate.Date).Days;
                            userBooking.RecurrencePattern = daysDiff switch
                            {
                                1 => RecurrencePattern.Daily,
                                7 => RecurrencePattern.Weekly,
                                14 => RecurrencePattern.BiWeekly,
                                28 => RecurrencePattern.FourWeeks,
                                _ => RecurrencePattern.Daily
                            };
                        }
                    }
                }
            }

            if (userBooking == null)
            {
                // Additional debug: count individual bookings
                var individualBookings = allRoomBookings
                    .Where(b => b.UserId == userId && b.RecurrencePattern == RecurrencePattern.None)
                    .Count();

                return new JsonResult(new { 
                    success = false, 
                    error = "Booking not found. Your recurring booking must have a recurrence pattern (Daily, Weekly, etc.) and an end date.",
                    debug = new {
                        searchedRoomId = roomId,
                        searchedUserId = userId,
                        searchStartDate = searchStartDate,
                        searchEndDate = searchEndDate,
                        bookingsFoundCount = allRoomBookings.Count(),
                        userBookingsCount = allRoomBookings.Count(b => b.UserId == userId),
                        isRecurringCount = allRoomBookings.Count(b => b.UserId == userId && b.RecurrencePattern != RecurrencePattern.None),
                        individualBookingsCount = individualBookings
                    }
                })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Get room details
            var room = await _roomService.GetRoomByIdAsync(userBooking.RoomId);
            if (room == null)
            {
                return new JsonResult(new { success = false, error = "Room not found." })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Generate recurrence dates based on pattern and end date
            var recurrenceDates = GenerateRecurrenceDatesForBooking(userBooking);

            // Debug: Log recurrence dates for troubleshooting
            System.Diagnostics.Debug.WriteLine($"Generated recurrence dates: {string.Join(", ", recurrenceDates.Select(d => d.ToString("yyyy-MM-dd")))}");
            System.Diagnostics.Debug.WriteLine($"Virtual booking StartTime: {userBooking.StartTime:hh\\:mm}, EndTime: {userBooking.EndTime:hh\\:mm}");
            System.Diagnostics.Debug.WriteLine($"Requested newStart: {newStart:hh\\:mm}, newEnd: {newEnd:hh\\:mm}");

            // Use the already-loaded allRoomBookings instead of querying again
            var allBookings = allRoomBookings
                .Where(b => 
                    b.BookingDate.Date >= recurrenceDates.Min().Date && 
                    b.BookingDate.Date <= recurrenceDates.Max().Date)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"Total bookings in recurrence range: {allBookings.Count}");
            System.Diagnostics.Debug.WriteLine($"User's bookings in recurrence range: {allBookings.Count(b => b.UserId == userId)}");
            foreach (var booking in allBookings.Where(b => b.UserId == userId))
            {
                System.Diagnostics.Debug.WriteLine($"  User booking: {booking.BookingDate:yyyy-MM-dd} {booking.StartTime:hh\\:mm}-{booking.EndTime:hh\\:mm}");
            }

            // Determine status for each recurrence date
            var dateStatuses = new List<object>();
            foreach (var date in recurrenceDates)
            {
                // For each recurrence date, check all bookings that could apply to it:
                // 1. Bookings with the exact booking date matching this date
                // 2. Recurring bookings that span this date
                var bookingsOnDate = allBookings.Where(b => 
                {
                    if (b.Status == BookingStatus.Cancelled)
                        return false;

                    // Check if this is a non-recurring booking on this exact date
                    if (b.RecurrencePattern == RecurrencePattern.None)
                        return b.BookingDate.Date == date.Date;

                    // For recurring bookings, check if this date falls within the recurrence range
                    if (b.BookingDate.Date <= date.Date && b.RecurrenceEndDate.HasValue && b.RecurrenceEndDate.Value.Date >= date.Date)
                    {
                        // Additional check: verify the recurrence pattern includes this date
                        var interval = b.RecurrencePattern switch
                        {
                            RecurrencePattern.Daily => 1,
                            RecurrencePattern.Weekly => 7,
                            RecurrencePattern.BiWeekly => 14,
                            RecurrencePattern.FourWeeks => 28,
                            _ => 1
                        };

                        var daysDiff = (date.Date - b.BookingDate.Date).Days;
                        if (daysDiff % interval == 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }).ToList();

                System.Diagnostics.Debug.WriteLine($"Date {date:yyyy-MM-dd}: found {bookingsOnDate.Count} bookings in range");
                foreach (var b in bookingsOnDate)
                {
                    System.Diagnostics.Debug.WriteLine($"  Booking: User={b.UserId}, {b.StartTime:hh\\:mm}-{b.EndTime:hh\\:mm}");
                }

                // Check if current user has booking(s) for this date/time slot
                // Find ALL user bookings that overlap with the requested time (not just FirstOrDefault)
                var userBookingsOnDate = bookingsOnDate.Where(b => 
                    b.UserId == userId && 
                    !(b.EndTime <= newStart || b.StartTime >= newEnd)).ToList();

                // Calculate the merged time range and the overlap with requested time
                TimeSpan? mergedStartTime = null;
                TimeSpan? mergedEndTime = null;
                TimeSpan? overlapStartTime = null;
                TimeSpan? overlapEndTime = null;
                string timeDisplay = ""; // Empty for "missing"

                if (userBookingsOnDate.Count > 0)
                {
                    // Find the earliest start and latest end time across all overlapping bookings
                    mergedStartTime = userBookingsOnDate.Min(b => b.StartTime);
                    mergedEndTime = userBookingsOnDate.Max(b => b.EndTime);

                    // Calculate the OVERLAP of merged bookings with the requested time
                    // overlap start = max(booking_start, requested_start)
                    // overlap end = min(booking_end, requested_end)
                    overlapStartTime = mergedStartTime > newStart ? mergedStartTime : newStart;
                    overlapEndTime = mergedEndTime < newEnd ? mergedEndTime : newEnd;

                    System.Diagnostics.Debug.WriteLine($"Date {date:yyyy-MM-dd}: Found {userBookingsOnDate.Count} user bookings");
                    System.Diagnostics.Debug.WriteLine($"  Merged booking range: {mergedStartTime:hh\\:mm}-{mergedEndTime:hh\\:mm}");
                    System.Diagnostics.Debug.WriteLine($"  Requested range: {newStart:hh\\:mm}-{newEnd:hh\\:mm}");
                    System.Diagnostics.Debug.WriteLine($"  Overlap range: {overlapStartTime:hh\\:mm}-{overlapEndTime:hh\\:mm}");
                    foreach (var ub in userBookingsOnDate)
                    {
                        System.Diagnostics.Debug.WriteLine($"  Individual booking: {ub.StartTime:hh\\:mm}-{ub.EndTime:hh\\:mm}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Date {date:yyyy-MM-dd}: No user bookings found");
                }

                // Check if there are conflicting bookings (by any user) with the new time
                var conflicts = bookingsOnDate.Where(b => 
                    b.UserId != userId && 
                    !(b.EndTime <= newStart || b.StartTime >= newEnd)).ToList();

                string status = "missing";
                string dateDisplay = date.ToString("dd/MM");

                System.Diagnostics.Debug.WriteLine($"Checking date {date:yyyy-MM-dd}: userBookingsOnDate.Count={userBookingsOnDate.Count}, conflicts={conflicts.Count}");

                if (mergedStartTime.HasValue && mergedEndTime.HasValue && overlapStartTime.HasValue && overlapEndTime.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine($"  merged times: {mergedStartTime:hh\\:mm}-{mergedEndTime:hh\\:mm}, requested: {newStart:hh\\:mm}-{newEnd:hh\\:mm}, overlap: {overlapStartTime:hh\\:mm}-{overlapEndTime:hh\\:mm}");

                    // Check if the merged booking covers the FULL requested time slot
                    if (mergedStartTime <= newStart && mergedEndTime >= newEnd)
                    {
                        // Full booking exists for requested time - display only the requested range overlap
                        status = "existing";
                        timeDisplay = $"{overlapStartTime:hh\\:mm}-{overlapEndTime:hh\\:mm}";
                        System.Diagnostics.Debug.WriteLine($"  -> Status: EXISTING");
                    }
                    else
                    {
                        // Partial booking(s) - not covering full requested time, label as "missing hour"
                        // Display only the overlapping portion
                        status = "missing hour";
                        timeDisplay = $"{overlapStartTime:hh\\:mm}-{overlapEndTime:hh\\:mm}";
                        System.Diagnostics.Debug.WriteLine($"  -> Status: MISSING HOUR");
                    }
                }
                else if (conflicts.Count > 0)
                {
                    // There's a conflicting booking but user doesn't have one
                    status = "missing";
                    timeDisplay = ""; // No time for "missing"
                    System.Diagnostics.Debug.WriteLine($"  -> Status: MISSING (conflict)");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"  -> Status: NOT EXIST");
                }

                dateStatuses.Add(new
                {
                    date = date.ToString("yyyy-MM-dd"),
                    dateDisplay = dateDisplay,
                    timeDisplay = timeDisplay,
                    status = status
                });
            }

            return new JsonResult(new
            {
                success = true,
                booking = new
                {
                    id = userBooking.Id,
                    roomId = userBooking.RoomId,
                    roomName = room.Name,
                    roomCode = room.Code,
                    roomLocation = room.Location,
                    roomCapacity = room.Capacity,
                    bookingDate = userBooking.BookingDate.ToString("yyyy-MM-dd"),
                    startTime = userBooking.StartTime.ToString(@"hh\:mm"),
                    endTime = userBooking.EndTime.ToString(@"hh\:mm"),
                    recurrencePattern = userBooking.RecurrencePattern.ToString(),
                    recurrenceEndDate = userBooking.RecurrenceEndDate?.ToString("yyyy-MM-dd") ?? "",
                    notes = userBooking.Notes ?? ""
                },
                recurrenceDates = recurrenceDates.Select(d => d.ToString("yyyy-MM-dd")).ToList(),
                dateStatuses = dateStatuses,
                newTimeStart = newStartTime,
                newTimeEnd = newEndTime
            });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = $"Error fetching booking details: {ex.Message}" })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    /// <summary>
    /// OLD HANDLER - Kept for reference if direct booking ID is passed
    /// Fetches details of a recurring booking (single booking instance) clicked by the user.
    /// Returns all expected recurrence dates with their current booking status.
    /// </summary>
    public async Task<IActionResult> OnPostGetRecurringBookingDetailsWithIdAsync(int bookingId, string newStartTime, string newEndTime)

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

            // Fetch the booking
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null)
            {
                return new JsonResult(new { success = false, error = "Booking not found." })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Verify ownership
            if (booking.UserId != userId)
            {
                return new JsonResult(new { success = false, error = "You don't have permission to modify this booking." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            // Get room details
            var room = await _roomService.GetRoomByIdAsync(booking.RoomId);
            if (room == null)
            {
                return new JsonResult(new { success = false, error = "Room not found." })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Generate recurrence dates based on pattern and end date
            var recurrenceDates = GenerateRecurrenceDatesForBooking(booking);

            // Parse new times
            TimeSpan newStart, newEnd;
            try
            {
                newStart = TimeSpan.ParseExact(newStartTime, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture);
                newEnd = TimeSpan.ParseExact(newEndTime, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return new JsonResult(new { success = false, error = "Invalid time format." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Validate time range
            if (newStart >= newEnd)
            {
                return new JsonResult(new { success = false, error = "Start time must be before end time." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Get all bookings for the room within the recurrence date range
            var allBookings = await _bookingService.GetRoomBookingsByDateRangeAsync(
                booking.RoomId,
                recurrenceDates.Min(),
                recurrenceDates.Max()
            );

            // Determine status for each recurrence date
            var dateStatuses = new List<object>();
            foreach (var date in recurrenceDates)
            {
                // For each recurrence date, check all bookings that could apply to it:
                // 1. Bookings with the exact booking date matching this date
                // 2. Recurring bookings that span this date
                var bookingsOnDate = allBookings.Where(b => 
                {
                    if (b.Status == BookingStatus.Cancelled)
                        return false;

                    // Check if this is a non-recurring booking on this exact date
                    if (b.RecurrencePattern == RecurrencePattern.None)
                        return b.BookingDate.Date == date.Date;

                    // For recurring bookings, check if this date falls within the recurrence range
                    if (b.BookingDate.Date <= date.Date && b.RecurrenceEndDate.HasValue && b.RecurrenceEndDate.Value.Date >= date.Date)
                    {
                        // Additional check: verify the recurrence pattern includes this date
                        var interval = b.RecurrencePattern switch
                        {
                            RecurrencePattern.Daily => 1,
                            RecurrencePattern.Weekly => 7,
                            RecurrencePattern.BiWeekly => 14,
                            RecurrencePattern.FourWeeks => 28,
                            _ => 1
                        };

                        var daysDiff = (date.Date - b.BookingDate.Date).Days;
                        if (daysDiff % interval == 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }).ToList();

                // Check if current user has a booking for this date/time slot
                var userBooking = bookingsOnDate.FirstOrDefault(b => 
                    b.UserId == userId && 
                    !(b.EndTime <= newStart || b.StartTime >= newEnd));

                // Check if there are conflicting bookings (by any user) with the new time
                var conflicts = bookingsOnDate.Where(b => 
                    b.UserId != userId && 
                    !(b.EndTime <= newStart || b.StartTime >= newEnd)).ToList();

                string status = "missing";
                string detail = "";

                if (userBooking != null)
                {
                    status = "exist";
                    detail = $"Exists: {userBooking.StartTime:hh\\:mm}-{userBooking.EndTime:hh\\:mm}";
                }
                else if (conflicts.Count > 0)
                {
                    status = "conflict";
                    detail = $"Conflict: booked {conflicts[0].StartTime:hh\\:mm}-{conflicts[0].EndTime:hh\\:mm}";
                }
                else
                {
                    detail = "Available for booking";
                }

                dateStatuses.Add(new
                {
                    date = date.ToString("yyyy-MM-dd"),
                    dateDisplay = date.ToString("dddd, MMM dd"),
                    status = status,
                    detail = detail
                });
            }

            return new JsonResult(new
            {
                success = true,
                booking = new
                {
                    id = booking.Id,
                    roomId = booking.RoomId,
                    roomName = room.Name,
                    roomCode = room.Code,
                    roomLocation = room.Location,
                    roomCapacity = room.Capacity,
                    bookingDate = booking.BookingDate.ToString("yyyy-MM-dd"),
                    startTime = booking.StartTime.ToString(@"hh\:mm"),
                    endTime = booking.EndTime.ToString(@"hh\:mm"),
                    recurrencePattern = booking.RecurrencePattern.ToString(),
                    recurrenceEndDate = booking.RecurrenceEndDate?.ToString("yyyy-MM-dd") ?? "",
                    notes = booking.Notes ?? ""
                },
                recurrenceDates = recurrenceDates.Select(d => d.ToString("yyyy-MM-dd")).ToList(),
                dateStatuses = dateStatuses,
                newTimeStart = newStartTime,
                newTimeEnd = newEndTime
            });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = $"Error fetching booking details: {ex.Message}" })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    /// <summary>
    /// Syncs all bookings in a recurring booking series. Creates missing bookings and updates existing ones to match the new time.
    /// </summary>
    public async Task<IActionResult> OnPostSyncAllBookingsAsync(int bookingId, string startTime, string endTime)
    {
        try
        {
            // Get current user ID
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdStr, out int userId))
            {
                return new JsonResult(new { success = false, error = "User session expired." })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }

            // Fetch the original booking
            var originalBooking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (originalBooking == null)
            {
                return new JsonResult(new { success = false, error = "Booking not found." })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Verify ownership
            if (originalBooking.UserId != userId)
            {
                return new JsonResult(new { success = false, error = "Permission denied." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            // Parse new times
            TimeSpan newStart, newEnd;
            try
            {
                newStart = TimeSpan.ParseExact(startTime, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture);
                newEnd = TimeSpan.ParseExact(endTime, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return new JsonResult(new { success = false, error = "Invalid time format." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            if (newStart >= newEnd)
            {
                return new JsonResult(new { success = false, error = "Start time must be before end time." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Generate recurrence dates
            var recurrenceDates = GenerateRecurrenceDatesForBooking(originalBooking);

            // Get room
            var room = await _roomService.GetRoomByIdAsync(originalBooking.RoomId);
            if (room == null)
            {
                return new JsonResult(new { success = false, error = "Room not found." })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Get all user bookings for this recurrence series
            var allBookings = await _bookingService.GetRoomBookingsByDateRangeAsync(
                originalBooking.RoomId,
                recurrenceDates.Min(),
                recurrenceDates.Max()
            );

            var userBookingsInSeries = allBookings.Where(b => b.UserId == userId).ToList();

            var results = new List<object>();
            int successCount = 0;
            int errorCount = 0;

            foreach (var date in recurrenceDates)
            {
                try
                {
                    // Check if user already has booking for this date
                    var existingBooking = userBookingsInSeries.FirstOrDefault(b => b.BookingDate.Date == date.Date);

                    if (existingBooking != null)
                    {
                        // Update existing booking
                        // First validate new time doesn't conflict with other users' bookings
                        var conflictingBookings = allBookings.Where(b =>
                            b.UserId != userId &&
                            b.BookingDate.Date == date.Date &&
                            b.Status != BookingStatus.Cancelled &&
                            !(b.EndTime <= newStart || b.StartTime >= newEnd)
                        ).ToList();

                        if (conflictingBookings.Count > 0)
                        {
                            results.Add(new
                            {
                                date = date.ToString("yyyy-MM-dd"),
                                status = "error",
                                message = $"Cannot update: Time slot conflicts with another booking on {date:MMM dd}"
                            });
                            errorCount++;
                            continue;
                        }

                        // Update the booking
                        existingBooking.StartTime = newStart;
                        existingBooking.EndTime = newEnd;
                        existingBooking.UpdatedAt = DateTime.UtcNow;
                        await _bookingService.UpdateBookingAsync(existingBooking);

                        results.Add(new
                        {
                            date = date.ToString("yyyy-MM-dd"),
                            status = "updated",
                            message = $"Updated booking to {newStart:hh\\:mm}-{newEnd:hh\\:mm}"
                        });
                        successCount++;
                    }
                    else
                    {
                        // Create new booking for this date
                        // First validate availability
                        var isAvailable = await _roomService.IsRoomAvailableAsync(originalBooking.RoomId, date, newStart, newEnd);
                        if (!isAvailable)
                        {
                            results.Add(new
                            {
                                date = date.ToString("yyyy-MM-dd"),
                                status = "error",
                                message = $"Cannot create: Room not available on {date:MMM dd} at {newStart:hh\\:mm}-{newEnd:hh\\:mm}"
                            });
                            errorCount++;
                            continue;
                        }

                        // Validate booking constraints
                        var (isValid, errorMsg) = await _bookingService.ValidateBookingAsync(
                            originalBooking.RoomId,
                            date,
                            newStart,
                            newEnd,
                            skipAdvanceDaysCheck: true
                        );

                        if (!isValid)
                        {
                            results.Add(new
                            {
                                date = date.ToString("yyyy-MM-dd"),
                                status = "error",
                                message = $"Cannot create: {errorMsg}"
                            });
                            errorCount++;
                            continue;
                        }

                        // Create the booking
                        var user = await _userService.GetUserByUserIdAsync(HttpContext.Session.GetString("CurrentUser"));
                        var newBooking = new Booking
                        {
                            RoomId = originalBooking.RoomId,
                            UserId = user.Id,
                            BookingDate = date,
                            StartTime = newStart,
                            EndTime = newEnd,
                            RecurrencePattern = RecurrencePattern.None,
                            RecurrenceEndDate = null,
                            Notes = originalBooking.Notes,
                            Status = BookingStatus.Confirmed
                        };

                        await _bookingService.CreateBookingAsync(newBooking);

                        results.Add(new
                        {
                            date = date.ToString("yyyy-MM-dd"),
                            status = "created",
                            message = $"Created booking for {newStart:hh\\:mm}-{newEnd:hh\\:mm}"
                        });
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    results.Add(new
                    {
                        date = date.ToString("yyyy-MM-dd"),
                        status = "error",
                        message = $"Error: {ex.Message}"
                    });
                    errorCount++;
                }
            }

            return new JsonResult(new
            {
                success = errorCount == 0,
                message = $"Completed: {successCount} succeeded, {errorCount} failed",
                results = results,
                successCount = successCount,
                errorCount = errorCount
            });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = $"Error syncing bookings: {ex.Message}" })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    /// <summary>
    /// Creates bookings for all missing dates in a recurring booking series.
    /// For "missing" status: creates full booking from newStartTime to newEndTime
    /// For "missing hour" status: creates partial booking to fill the gap
    /// </summary>
    public async Task<IActionResult> OnPostBookMissingBookingsAsync(int roomId, string startDate, string recurrenceEndDate, string recurrencePattern, string newStartTime, string newEndTime)
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

            // Parse dates
            if (!DateTime.TryParse(startDate, out DateTime startDateObj))
            {
                return new JsonResult(new { success = false, error = "Invalid start date format." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            if (!DateTime.TryParse(recurrenceEndDate, out DateTime endDateObj))
            {
                return new JsonResult(new { success = false, error = "Invalid end date format." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Parse pattern
            if (!Enum.TryParse<RecurrencePattern>(recurrencePattern, out var requestedPattern))
            {
                return new JsonResult(new { success = false, error = "Invalid recurrence pattern." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Parse new times
            TimeSpan newStart, newEnd;
            try
            {
                newStart = TimeSpan.ParseExact(newStartTime, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture);
                newEnd = TimeSpan.ParseExact(newEndTime, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return new JsonResult(new { success = false, error = "Invalid time format." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            if (newStart >= newEnd)
            {
                return new JsonResult(new { success = false, error = "Start time must be before end time." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Generate recurrence dates
            var startDateOnly = startDateObj.Date;
            var endDateOnly = endDateObj.Date;
            var recurrenceDates = new List<DateTime>();

            int interval = requestedPattern switch
            {
                RecurrencePattern.Daily => 1,
                RecurrencePattern.Weekly => 7,
                RecurrencePattern.BiWeekly => 14,
                RecurrencePattern.FourWeeks => 28,
                _ => 1
            };

            var currentDate = startDateOnly;
            while (currentDate <= endDateOnly)
            {
                recurrenceDates.Add(currentDate);
                currentDate = currentDate.AddDays(interval);
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

            // Get all bookings in the date range
            var allBookings = await _bookingService.GetRoomBookingsByDateRangeAsync(roomId, startDateOnly, endDateOnly);

            var results = new List<object>();
            int successCount = 0;
            int errorCount = 0;

            // Process each date in the recurrence
            foreach (var date in recurrenceDates)
            {
                try
                {
                    // Find user's booking for this date that overlaps with new time
                    var userBookingOnDate = allBookings.FirstOrDefault(b =>
                        b.UserId == userId &&
                        b.BookingDate.Date == date.Date &&
                        b.Status != BookingStatus.Cancelled &&
                        !(b.EndTime <= newStart || b.StartTime >= newEnd));

                    TimeSpan bookStartTime = newStart;
                    TimeSpan bookEndTime = newEnd;

                    // If user has a partial booking, only book the missing hours
                    if (userBookingOnDate != null)
                    {
                        // Check if the existing booking covers the FULL requested time slot
                        if (userBookingOnDate.StartTime <= newStart && userBookingOnDate.EndTime >= newEnd)
                        {
                            // Full booking exists, no need to create anything
                            results.Add(new
                            {
                                date = date.ToString("yyyy-MM-dd"),
                                dateDisplay = date.ToString("dd/MM"),
                                status = "skipped",
                                message = "Booking already exists for full time period"
                            });
                            continue;
                        }

                        // Partial booking exists - calculate what's missing
                        // Only book the missing part to complete the requested time
                        if (userBookingOnDate.StartTime > newStart)
                        {
                            // Missing time at the start
                            bookStartTime = newStart;
                            bookEndTime = userBookingOnDate.StartTime;
                        }
                        else if (userBookingOnDate.EndTime < newEnd)
                        {
                            // Missing time at the end
                            bookStartTime = userBookingOnDate.EndTime;
                            bookEndTime = newEnd;
                        }
                        else
                        {
                            // Booking is in the middle, shouldn't happen but skip
                            results.Add(new
                            {
                                date = date.ToString("yyyy-MM-dd"),
                                dateDisplay = date.ToString("dd/MM"),
                                status = "skipped",
                                message = "Existing booking covers requested time"
                            });
                            continue;
                        }
                    }
                    else
                    {
                        // No booking exists for this date, check if there's a conflicting booking
                        var conflicts = allBookings.Where(b =>
                            b.UserId != userId &&
                            b.BookingDate.Date == date.Date &&
                            b.Status != BookingStatus.Cancelled &&
                            !(b.EndTime <= newStart || b.StartTime >= newEnd)).ToList();

                        if (conflicts.Count > 0)
                        {
                            results.Add(new
                            {
                                date = date.ToString("yyyy-MM-dd"),
                                dateDisplay = date.ToString("dd/MM"),
                                status = "error",
                                message = "Cannot book: Time slot conflicts with another booking"
                            });
                            errorCount++;
                            continue;
                        }
                    }

                    // Validate availability
                    var isAvailable = await _roomService.IsRoomAvailableAsync(roomId, date, bookStartTime, bookEndTime);
                    if (!isAvailable)
                    {
                        results.Add(new
                        {
                            date = date.ToString("yyyy-MM-dd"),
                            dateDisplay = date.ToString("dd/MM"),
                            status = "error",
                            message = $"Room not available at {bookStartTime:hh\\:mm}-{bookEndTime:hh\\:mm}"
                        });
                        errorCount++;
                        continue;
                    }

                    // Validate booking constraints
                    var (isValid, errorMsg) = await _bookingService.ValidateBookingAsync(
                        roomId,
                        date,
                        bookStartTime,
                        bookEndTime,
                        skipAdvanceDaysCheck: true
                    );

                    if (!isValid)
                    {
                        results.Add(new
                        {
                            date = date.ToString("yyyy-MM-dd"),
                            dateDisplay = date.ToString("dd/MM"),
                            status = "error",
                            message = $"Cannot book: {errorMsg}"
                        });
                        errorCount++;
                        continue;
                    }

                    // Create the booking
                    var user = await _userService.GetUserByUserIdAsync(HttpContext.Session.GetString("CurrentUser"));
                    var newBooking = new Booking
                    {
                        RoomId = roomId,
                        UserId = user.Id,
                        BookingDate = date,
                        StartTime = bookStartTime,
                        EndTime = bookEndTime,
                        Status = BookingStatus.Confirmed,
                        RecurrencePattern = RecurrencePattern.None, // Individual booking, not recurring
                        Notes = "Auto-created to fill missing booking slot",
                        CreatedAt = DateTime.UtcNow
                    };

                    await _bookingService.CreateBookingAsync(newBooking);

                    results.Add(new
                    {
                        date = date.ToString("yyyy-MM-dd"),
                        dateDisplay = date.ToString("dd/MM"),
                        status = "created",
                        message = $"Created booking {bookStartTime:hh\\:mm}-{bookEndTime:hh\\:mm}"
                    });
                    successCount++;
                }
                catch (Exception ex)
                {
                    results.Add(new
                    {
                        date = date.ToString("yyyy-MM-dd"),
                        dateDisplay = date.ToString("dd/MM"),
                        status = "error",
                        message = $"Error creating booking: {ex.Message}"
                    });
                    errorCount++;
                }
            }

            return new JsonResult(new
            {
                success = true,
                message = $"Booking process completed. Created: {successCount}, Failed: {errorCount}",
                results = results,
                stats = new
                {
                    created = successCount,
                    failed = errorCount,
                    skipped = results.Count(r => ((dynamic)r).status == "skipped")
                }
            });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = $"Error booking missing slots: {ex.Message}" })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    /// <summary>
    /// Deletes all bookings in a recurring booking series.
    /// </summary>
    /// <summary>
    /// Deletes or modifies all bookings in a recurring booking series.
    /// For bookings completely within the requested time range, they are deleted.
    /// For bookings that partially overlap, they are modified to remove only the overlapping part.
    /// </summary>
    public async Task<IActionResult> OnPostDeleteAllBookingsAsync(int bookingId, string? recurrenceDatesJson = null, string? startTime = null, string? endTime = null)
    {
        try
        {
            // Get current user ID
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdStr, out int userId))
            {
                return new JsonResult(new { success = false, error = "User session expired." })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }

            // Fetch the original booking
            var originalBooking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (originalBooking == null)
            {
                return new JsonResult(new { success = false, error = "Booking not found." })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Verify ownership
            if (originalBooking.UserId != userId)
            {
                return new JsonResult(new { success = false, error = "Permission denied." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            // Parse the time range for deletion
            TimeSpan? deleteStartTime = null;
            TimeSpan? deleteEndTime = null;
            if (!string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
            {
                try
                {
                    deleteStartTime = TimeSpan.ParseExact(startTime, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture);
                    deleteEndTime = TimeSpan.ParseExact(endTime, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture);
                    System.Diagnostics.Debug.WriteLine($"Delete time range: {deleteStartTime:hh\\:mm} - {deleteEndTime:hh\\:mm}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to parse time range: {ex.Message}");
                    deleteStartTime = null;
                    deleteEndTime = null;
                }
            }

            // Use provided recurrence dates or generate them from booking
            List<DateTime> recurrenceDates;
            if (!string.IsNullOrEmpty(recurrenceDatesJson))
            {
                try
                {
                    // Parse the JSON array of dates
                    var dateStrings = System.Text.Json.JsonSerializer.Deserialize<List<string>>(recurrenceDatesJson);
                    recurrenceDates = dateStrings?
                        .Select(d => DateTime.Parse(d).Date)
                        .ToList() ?? new List<DateTime>();

                    System.Diagnostics.Debug.WriteLine($"Using provided recurrence dates: {recurrenceDates.Count} dates");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to parse recurrence dates JSON: {ex.Message}");
                    recurrenceDates = GenerateRecurrenceDatesForBooking(originalBooking);
                }
            }
            else
            {
                recurrenceDates = GenerateRecurrenceDatesForBooking(originalBooking);
            }

            // Get all bookings for this room within the date range
            if (recurrenceDates.Count == 0)
            {
                return new JsonResult(new { success = false, error = "No recurrence dates found." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            var allBookings = await _bookingService.GetRoomBookingsByDateRangeAsync(
                originalBooking.RoomId,
                recurrenceDates.Min(),
                recurrenceDates.Max()
            );

            // Find and delete/modify all user's bookings for these dates
            var results = new List<object>();
            int deletedCount = 0;
            int modifiedCount = 0;

            foreach (var date in recurrenceDates)
            {
                // For each recurrence date, check all bookings that could apply to it:
                // 1. Bookings with the exact booking date matching this date
                // 2. Recurring bookings that span this date
                var userBookingsOnDate = allBookings.Where(b => 
                {
                    if (b.UserId != userId || b.Status == BookingStatus.Cancelled)
                        return false;

                    // Check if this is a non-recurring booking on this exact date
                    if (b.RecurrencePattern == RecurrencePattern.None)
                        return b.BookingDate.Date == date.Date;

                    // For recurring bookings, check if this date falls within the recurrence range
                    if (b.BookingDate.Date <= date.Date && b.RecurrenceEndDate.HasValue && b.RecurrenceEndDate.Value.Date >= date.Date)
                    {
                        // Additional check: verify the recurrence pattern includes this date
                        var interval = b.RecurrencePattern switch
                        {
                            RecurrencePattern.Daily => 1,
                            RecurrencePattern.Weekly => 7,
                            RecurrencePattern.BiWeekly => 14,
                            RecurrencePattern.FourWeeks => 28,
                            _ => 1
                        };

                        var daysDiff = (date.Date - b.BookingDate.Date).Days;
                        if (daysDiff % interval == 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }).ToList();

                System.Diagnostics.Debug.WriteLine($"Date {date:yyyy-MM-dd}: Found {userBookingsOnDate.Count} user bookings");

                foreach (var booking in userBookingsOnDate)
                {
                    try
                    {
                        // If time range is specified, check for partial overlaps
                        if (deleteStartTime.HasValue && deleteEndTime.HasValue)
                        {
                            // Determine if booking overlaps with the delete time range
                            bool bookingEntirelyCovered = booking.StartTime >= deleteStartTime && booking.EndTime <= deleteEndTime;
                            bool bookingPartiallyOverlaps = !(booking.EndTime <= deleteStartTime || booking.StartTime >= deleteEndTime);

                            if (bookingEntirelyCovered)
                            {
                                // Booking is completely within the delete range - DELETE it
                                booking.Status = BookingStatus.Cancelled;
                                await _bookingService.UpdateBookingAsync(booking);

                                results.Add(new
                                {
                                    date = date.ToString("yyyy-MM-dd"),
                                    status = "deleted",
                                    message = $"Booking {booking.StartTime:hh\\:mm}-{booking.EndTime:hh\\:mm} deleted"
                                });
                                deletedCount++;

                                System.Diagnostics.Debug.WriteLine($"  Date {date:yyyy-MM-dd}: Booking {booking.StartTime:hh\\:mm}-{booking.EndTime:hh\\:mm} DELETED (entirely covered)");
                            }
                            else if (bookingPartiallyOverlaps)
                            {
                                // Booking partially overlaps - MODIFY it to keep non-overlapping parts
                                TimeSpan originalStart = booking.StartTime;
                                TimeSpan originalEnd = booking.EndTime;

                                // Keep the part BEFORE the delete range
                                if (booking.StartTime < deleteStartTime && booking.EndTime > deleteStartTime)
                                {
                                    // Booking extends before and into the delete range
                                    booking.EndTime = deleteStartTime.Value;
                                    await _bookingService.UpdateBookingAsync(booking);

                                    results.Add(new
                                    {
                                        date = date.ToString("yyyy-MM-dd"),
                                        status = "modified",
                                        message = $"Booking modified from {originalStart:hh\\:mm}-{originalEnd:hh\\:mm} to {booking.StartTime:hh\\:mm}-{booking.EndTime:hh\\:mm}"
                                    });
                                    modifiedCount++;

                                    System.Diagnostics.Debug.WriteLine($"  Date {date:yyyy-MM-dd}: Booking MODIFIED (kept part before): {booking.StartTime:hh\\:mm}-{booking.EndTime:hh\\:mm}");
                                }
                                else if (booking.StartTime < deleteEndTime && booking.EndTime > deleteEndTime)
                                {
                                    // Booking extends through and after the delete range
                                    booking.StartTime = deleteEndTime.Value;
                                    await _bookingService.UpdateBookingAsync(booking);

                                    results.Add(new
                                    {
                                        date = date.ToString("yyyy-MM-dd"),
                                        status = "modified",
                                        message = $"Booking modified from {originalStart:hh\\:mm}-{originalEnd:hh\\:mm} to {booking.StartTime:hh\\:mm}-{booking.EndTime:hh\\:mm}"
                                    });
                                    modifiedCount++;

                                    System.Diagnostics.Debug.WriteLine($"  Date {date:yyyy-MM-dd}: Booking MODIFIED (kept part after): {booking.StartTime:hh\\:mm}-{booking.EndTime:hh\\:mm}");
                                }
                            }
                            else
                            {
                                // No overlap - KEEP booking
                                System.Diagnostics.Debug.WriteLine($"  Date {date:yyyy-MM-dd}: Booking {booking.StartTime:hh\\:mm}-{booking.EndTime:hh\\:mm} kept (no overlap)");
                            }
                        }
                        else
                        {
                            // No time range specified - DELETE all user bookings for these dates
                            booking.Status = BookingStatus.Cancelled;
                            await _bookingService.UpdateBookingAsync(booking);

                            results.Add(new
                            {
                                date = date.ToString("yyyy-MM-dd"),
                                status = "deleted",
                                message = "Booking cancelled"
                            });
                            deletedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        results.Add(new
                        {
                            date = date.ToString("yyyy-MM-dd"),
                            status = "error",
                            message = $"Error processing: {ex.Message}"
                        });
                        System.Diagnostics.Debug.WriteLine($"  Error processing booking: {ex.Message}");
                    }
                }
            }

            return new JsonResult(new
            {
                success = true,
                message = $"Processed {deletedCount} deletion(s) and {modifiedCount} modification(s)",
                results = results,
                deletedCount = deletedCount,
                modifiedCount = modifiedCount
            });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = $"Error deleting bookings: {ex.Message}" })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    /// <summary>
    /// Helper method to generate recurrence dates for a booking based on its pattern and end date.
    /// </summary>
    private List<DateTime> GenerateRecurrenceDatesForBooking(Booking booking)
    {
        var dates = new List<DateTime>();
        var pattern = booking.RecurrencePattern;
        var endDate = booking.RecurrenceEndDate ?? booking.BookingDate;

        if (pattern == RecurrencePattern.None)
        {
            dates.Add(booking.BookingDate);
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

            var currentDate = booking.BookingDate;
            while (currentDate <= endDate)
            {
                dates.Add(currentDate);
                currentDate = currentDate.AddDays(interval);
            }
        }

        return dates;
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
            // Filter cached bookings to only those for this specific date and its recurring bookings
            var bookingsForDate = cachedBookings.Where(b =>
            {
                if (b.Status == BookingStatus.Cancelled)
                    return false;

                // Check if this is a non-recurring booking on this exact date
                if (b.RecurrencePattern == RecurrencePattern.None)
                    return b.BookingDate.Date == date.Date;

                // For recurring bookings, check if this date falls within the recurrence range
                if (b.BookingDate.Date <= date.Date && b.RecurrenceEndDate.HasValue && b.RecurrenceEndDate.Value.Date >= date.Date)
                {
                    // Additional check: verify the recurrence pattern includes this date
                    var interval = b.RecurrencePattern switch
                    {
                        RecurrencePattern.Daily => 1,
                        RecurrencePattern.Weekly => 7,
                        RecurrencePattern.BiWeekly => 14,
                        RecurrencePattern.FourWeeks => 28,
                        _ => 1
                    };

                    var daysDiff = (date.Date - b.BookingDate.Date).Days;
                    if (daysDiff % interval == 0)
                    {
                        return true;
                    }
                }

                return false;
            }).ToList();

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
