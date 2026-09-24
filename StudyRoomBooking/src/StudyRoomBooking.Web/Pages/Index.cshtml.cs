using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IRoomService _roomService;
    private readonly IBookingService _bookingService;
    private readonly IUserService _userService;

    [BindProperty]
    public RoomSearchCriteria SearchCriteria { get; set; } = new();

    public List<Room> SearchResults { get; set; } = new();
    public bool HasSearched { get; set; } = false;
    public string? CurrentUserRole { get; set; } = null;
    public StudentMajor? CurrentUserMajor { get; set; } = null;
    public string? CurrentUserName { get; set; } = null;
    public int CurrentUserId { get; set; } = 0;
    public List<RoomType> AvailableRoomTypes { get; set; } = new();

    public IndexModel(IRoomService roomService, IBookingService bookingService, IUserService userService)
    {
        _roomService = roomService;
        _bookingService = bookingService;
        _userService = userService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
        {
            return RedirectToPage("Login");
        }

        // Get current user ID
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (int.TryParse(userIdStr, out int userId))
        {
            CurrentUserId = userId;
        }

        SearchCriteria.BookingDate = DateTime.Today;
        // StartTime and EndTime are nullable, so leave them as null (not set)

        // Get the current user role from session
        CurrentUserRole = HttpContext.Session.GetString("CurrentUserRole");
        CurrentUserName = HttpContext.Session.GetString("CurrentUser");

        // Get user's major if student
        if (CurrentUserRole == "Student" && !string.IsNullOrEmpty(CurrentUserName))
        {
            var user = await _userService.GetUserByUserIdAsync(CurrentUserName);
            if (user != null)
            {
                CurrentUserMajor = user.Major;
            }
        }

        // Populate available room types based on user role
        await PopulateAvailableRoomTypesAsync();
        HasSearched = true;
        SearchResults = await _roomService.SearchRoomsAsync(
            SearchCriteria.BookingDate,
            SearchCriteria.StartTime,
            SearchCriteria.EndTime,
            SearchCriteria.Capacity,
            SearchCriteria.RoomType,
            SearchCriteria.Location,
            CurrentUserRole == "Student" ? CurrentUserMajor : null
        );
        return Page();
    }

    private async Task PopulateAvailableRoomTypesAsync()
    {
        if (CurrentUserRole == "Staff" || CurrentUserRole == "Admin")
        {
            // Staff can see all room types
            AvailableRoomTypes = Enum.GetValues(typeof(RoomType))
                .Cast<RoomType>()
                .ToList();
        }
        else if (CurrentUserRole == "Student" && CurrentUserMajor.HasValue)
        {
            // Students can only see Study rooms and restricted rooms available to their major
            var allRooms = await _roomService.GetAllRoomsAsync();
            var availableTypes = new HashSet<RoomType>();

            // Add Study room type (always available to all students)
            availableTypes.Add(RoomType.Study);

            // Check which restricted rooms are available to this student's major
            foreach (var room in allRooms)
            {
                if (room.Type == RoomType.Study)
                {
                    // Already added above
                    continue;
                }

                var allowedMajors = await _roomService.GetAllowedMajorsForRoomAsync(room.Id);

                // If room has restrictions and student's major is in the list, add this room type
                if (allowedMajors.Count > 0 && allowedMajors.Contains(CurrentUserMajor.Value))
                {
                    availableTypes.Add(room.Type);
                }
            }

            // Exclude ComputerLab for Engineering students (they should only see their restricted Lab rooms)
            if (CurrentUserMajor.Value == StudentMajor.Engineering)
            {
                availableTypes.Remove(RoomType.ComputerLab);
            }

            AvailableRoomTypes = availableTypes.ToList();
        }
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

    /// <summary>
    /// Determines if a time slot is in the past for today's date.
    /// Returns true only if the booking date is today AND the time slot is before current time.
    /// For future dates, this always returns false (no slots are in the past).
    /// </summary>
    public bool IsTimeSlotInPast(TimeSpan timeSlot)
    {
        // Only check for past times if the booking date is today
        if (SearchCriteria.BookingDate.Date == DateTime.Today)
        {
            var currentTime = DateTime.Now.TimeOfDay;
            var roundedCurrentTime = TimeSpan.FromHours(Math.Floor(currentTime.TotalHours));
            // A slot is in the past if its start time is before rounded current time
            return timeSlot < roundedCurrentTime;
        }

        // For future dates, no slots are in the past
        return false;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
        {
            return RedirectToPage("Login");
        }

        // Get current user ID
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (int.TryParse(userIdStr, out int userId))
        {
            CurrentUserId = userId;
        }

        HasSearched = true;

        try
        {
            // Get the current user role and name from session
            CurrentUserRole = HttpContext.Session.GetString("CurrentUserRole");
            CurrentUserName = HttpContext.Session.GetString("CurrentUser");

            // Get user's major if student
            if (CurrentUserRole == "Student" && !string.IsNullOrEmpty(CurrentUserName))
            {
                var user = await _userService.GetUserByUserIdAsync(CurrentUserName);
                if (user != null)
                {
                    CurrentUserMajor = user.Major;
                }
            }

            // Validate booking date is not in the past
            var today = DateTime.Today;
            if (SearchCriteria.BookingDate.Date < today)
            {
                ModelState.AddModelError(string.Empty, "Cannot search for bookings in the past. Please select a date from today onwards.");
                return Page();
            }

            // Validate booking date is not more than 60 days ahead
            var daysInAdvance = (SearchCriteria.BookingDate.Date - today).Days;
            if (daysInAdvance > 60)
            {
                ModelState.AddModelError(string.Empty, $"Bookings can only be made up to 60 days ahead. Your selected date is {daysInAdvance} days away.");
                return Page();
            }

            // Populate available room types (in case major or role changed)
            await PopulateAvailableRoomTypesAsync();

            // No longer restrict students to Study rooms only - they can now access their major-restricted rooms
            // Room filtering will be applied based on major restrictions instead

            // Use provided times as-is (nullable) - no defaults
            // When both are null, all rooms will be shown; when one or both are set, filtering applies

            // Search with major filtering for students
            SearchResults = await _roomService.SearchRoomsAsync(
                SearchCriteria.BookingDate,
                SearchCriteria.StartTime,
                SearchCriteria.EndTime,
                SearchCriteria.Capacity,
                SearchCriteria.RoomType,
                SearchCriteria.Location,
                CurrentUserRole == "Student" ? CurrentUserMajor : null
            );
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error searching rooms: {ex.Message}");
        }

        return Page();
    }

    /// <summary>
    /// API handler for booking a room directly via AJAX from the calendar modal.
    /// This performs the same validation and booking logic as RoomDetails.OnPostBookRoomAsync.
    /// </summary>
    public async Task<IActionResult> OnPostBookRoomAsync(int roomId, string bookingDate, string startTime, string endTime, string? notes)
    {
        var startTimePerfomance = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // Verify user is authenticated
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                return new JsonResult(new { success = false, error = "User not found in session. Please log in again." })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }

            var sw1 = System.Diagnostics.Stopwatch.StartNew();
            var room = await _roomService.GetRoomByIdAsync(roomId);
            sw1.Stop();
            System.Diagnostics.Debug.WriteLine($"[PERF] GetRoomByIdAsync: {sw1.ElapsedMilliseconds}ms");

            if (room == null)
            {
                return new JsonResult(new { success = false, error = "Room not found." })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var sw2 = System.Diagnostics.Stopwatch.StartNew();
            var user = await _userService.GetUserByIdAsync(userId);
            sw2.Stop();
            System.Diagnostics.Debug.WriteLine($"[PERF] GetUserByIdAsync: {sw2.ElapsedMilliseconds}ms");

            if (user == null)
            {
                return new JsonResult(new { success = false, error = "User not found." })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Parse dates and times
            var date = DateTime.ParseExact(bookingDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

            // Parse time - handle both "8:00" and "08:00" formats
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

            // Validate booking date and time constraints
            var sw3 = System.Diagnostics.Stopwatch.StartNew();
            var (isValid, errorMessage) = await _bookingService.ValidateBookingAsync(roomId, date, start, end);
            sw3.Stop();
            System.Diagnostics.Debug.WriteLine($"[PERF] ValidateBookingAsync: {sw3.ElapsedMilliseconds}ms");

            if (!isValid)
            {
                return new JsonResult(new { success = false, error = errorMessage })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Check availability
            var sw4 = System.Diagnostics.Stopwatch.StartNew();
            var isAvailable = await _roomService.IsRoomAvailableAsync(roomId, date, start, end);
            sw4.Stop();
            System.Diagnostics.Debug.WriteLine($"[PERF] IsRoomAvailableAsync: {sw4.ElapsedMilliseconds}ms");

            if (!isAvailable)
            {
                return new JsonResult(new { success = false, error = "Selected time slot is not available." })
                {
                    StatusCode = StatusCodes.Status409Conflict
                };
            }

            // Check for overlapping bookings by current user
            var sw4b = System.Diagnostics.Stopwatch.StartNew();
            var userBookings = await _bookingService.GetBookingsByUserIdAsync(userId);
            sw4b.Stop();
            System.Diagnostics.Debug.WriteLine($"[PERF] GetBookingsByUserIdAsync: {sw4b.ElapsedMilliseconds}ms");

            // Filter for confirmed bookings on the same date and check for time overlap
            var overlappingBooking = userBookings.FirstOrDefault(b =>
                b.BookingDate == date &&
                b.Status != BookingStatus.Cancelled &&
                // Check if time slots overlap: new booking starts before existing ends AND new booking ends after existing starts
                !(start >= b.EndTime || end <= b.StartTime)
            );

            if (overlappingBooking != null)
            {
                return new JsonResult(new { 
                    success = false, 
                    error = $"You cannot book overlapping time slots. You already have a booking from {overlappingBooking.StartTime:hh\\:mm} to {overlappingBooking.EndTime:hh\\:mm} on this day." 
                })
                {
                    StatusCode = StatusCodes.Status409Conflict
                };
            }

            // Create booking
            var sw5 = System.Diagnostics.Stopwatch.StartNew();
            var booking = new Booking
            {
                RoomId = roomId,
                UserId = user.Id,
                BookingDate = date,
                StartTime = start,
                EndTime = end,
                Notes = notes,
                Status = BookingStatus.Confirmed
            };

            await _bookingService.CreateBookingAsync(booking);
            sw5.Stop();
            System.Diagnostics.Debug.WriteLine($"[PERF] CreateBookingAsync: {sw5.ElapsedMilliseconds}ms");

            startTimePerfomance.Stop();
            System.Diagnostics.Debug.WriteLine($"[PERF] Total OnPostBookRoomAsync: {startTimePerfomance.ElapsedMilliseconds}ms");

            return new JsonResult(new
            { 
                success = true, 
                message = "Booking created successfully!",
                bookingId = booking.Id,
                confirmationNumber = booking.ConfirmationNumber,
                roomName = room.Name,
                bookingDate = booking.BookingDate.ToString("yyyy-MM-dd"),
                startTime = booking.StartTime.ToString(@"hh\:mm"),
                endTime = booking.EndTime.ToString(@"hh\:mm")
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
    /// API endpoint to get available hours for a room on a specific date.
    /// Returns list of available start hours (8-21).
    /// </summary>
    public async Task<IActionResult> OnGetAvailableHoursAsync(int roomId, string bookingDate)
    {
        try
        {
            if (!DateTime.TryParseExact(bookingDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var date))
            {
                return new JsonResult(new { success = false, error = "Invalid date format" })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            var bookings = await _bookingService.SearchBookingsAsync(date, roomId);

            // Generate all possible hours (8-22)
            var availableHours = new List<string>();
            for (int hour = 8; hour <= 21; hour++)
            {
                var startTime = new TimeSpan(hour, 0, 0);
                var endTime = new TimeSpan(hour + 1, 0, 0);

                // Check if this slot is available
                bool isAvailable = true;
                foreach (var booking in bookings)
                {
                    // Check if our proposed slot conflicts with existing booking
                    if (!(endTime <= booking.StartTime || startTime >= booking.EndTime))
                    {
                        isAvailable = false;
                        break;
                    }
                }

                if (isAvailable)
                {
                    availableHours.Add($"{hour:D2}:00");
                }
            }

            return new JsonResult(new { success = true, hours = availableHours });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = $"Error fetching available hours: {ex.Message}" })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    /// <summary>
    /// AJAX endpoint to get booking details for modification popup
    /// </summary>
    public async Task<IActionResult> OnPostGetBookingDetailsAsync(int bookingId)
    {
        try
        {
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null)
            {
                return new JsonResult(new { success = false, error = "Booking not found" })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Check if booking is cancelled
            if (booking.Status == BookingStatus.Cancelled)
            {
                return new JsonResult(new { success = false, error = "Cannot modify a cancelled booking" })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            var room = await _roomService.GetRoomByIdAsync(booking.RoomId);
            if (room == null)
            {
                return new JsonResult(new { success = false, error = "Room not found" })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
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
                    notes = booking.Notes ?? ""
                }
            });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = $"Error fetching booking: {ex.Message}" })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    /// <summary>
    /// AJAX endpoint to modify an existing booking
    /// </summary>
    public async Task<IActionResult> OnPostModifyBookingAsync(int bookingId, string bookingDate, string startHour, string endHour, string? notes)
    {
        try
        {
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null)
            {
                return new JsonResult(new { success = false, error = "Booking not found" })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Check if booking is cancelled
            if (booking.Status == BookingStatus.Cancelled)
            {
                return new JsonResult(new { success = false, error = "Cannot modify a cancelled booking" })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Parse date
            var date = DateTime.ParseExact(bookingDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

            // Parse time - handle both "8:00" and "08:00" formats
            TimeSpan start, end;
            try
            {
                start = TimeSpan.ParseExact(startHour, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                start = TimeSpan.ParseExact(startHour, @"h\:mm", System.Globalization.CultureInfo.InvariantCulture);
            }

            try
            {
                end = TimeSpan.ParseExact(endHour, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                end = TimeSpan.ParseExact(endHour, @"h\:mm", System.Globalization.CultureInfo.InvariantCulture);
            }

            // Validate time constraints
            if (start >= end)
            {
                return new JsonResult(new { success = false, error = "Start time must be before end time" })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Validate booking date and time constraints only if date or time has changed
            bool dateOrTimeChanged = date.Date != booking.BookingDate.Date || start != booking.StartTime || end != booking.EndTime;

            if (dateOrTimeChanged)
            {
                var (isValid, errorMessage) = await _bookingService.ValidateBookingAsync(booking.RoomId, date, start, end, bookingId);
                if (!isValid)
                {
                    return new JsonResult(new { success = false, error = errorMessage })
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }

                // Check availability (excluding current booking)
                var isAvailable = await _roomService.IsRoomAvailableAsync(booking.RoomId, date, start, end, bookingId);
                if (!isAvailable)
                {
                    return new JsonResult(new { success = false, error = "Selected time slot is not available" })
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }

                // Check for overlapping bookings by current user (excluding current booking)
                var userBookings = await _bookingService.GetBookingsByUserIdAsync(booking.UserId);

                var overlappingBooking = userBookings.FirstOrDefault(b =>
                    b.Id != bookingId &&
                    b.BookingDate == date &&
                    b.Status != BookingStatus.Cancelled &&
                    !(start >= b.EndTime || end <= b.StartTime)
                );

                if (overlappingBooking != null)
                {
                    return new JsonResult(new { success = false, error = $"You cannot book overlapping time slots. You already have a booking from {overlappingBooking.StartTime:hh\\:mm} to {overlappingBooking.EndTime:hh\\:mm} on this day." })
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
            }

            // Update booking
            booking.BookingDate = date;
            booking.StartTime = start;
            booking.EndTime = end;
            booking.Notes = notes;

            await _bookingService.UpdateBookingAsync(booking);

            return new JsonResult(new
            {
                success = true,
                message = "Booking updated successfully"
            });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = $"Error updating booking: {ex.Message}" })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    /// <summary>
    /// AJAX endpoint to delete/cancel a booking
    /// </summary>
    public async Task<IActionResult> OnPostDeleteBookingAsync(int bookingId)
    {
        try
        {
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null)
            {
                return new JsonResult(new { success = false, error = "Booking not found" })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Check if booking is already cancelled
            if (booking.Status == BookingStatus.Cancelled)
            {
                return new JsonResult(new { success = false, error = "Booking is already cancelled" })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            await _bookingService.CancelBookingAsync(bookingId);

            return new JsonResult(new
            {
                success = true,
                message = "Booking cancelled successfully"
            });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = $"Error cancelling booking: {ex.Message}" })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
