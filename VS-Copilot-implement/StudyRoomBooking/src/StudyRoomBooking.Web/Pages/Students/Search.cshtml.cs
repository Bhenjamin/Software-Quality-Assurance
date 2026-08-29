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
    private readonly IUserService _userService;

    [BindProperty]
    public RoomSearchCriteria SearchCriteria { get; set; } = new();

    public List<Room> SearchResults { get; set; } = new();
    public bool HasSearched { get; set; } = false;
    public string? CurrentUserRole { get; set; } = null;
    public StudentMajor? CurrentUserMajor { get; set; } = null;
    public string? CurrentUserName { get; set; } = null;
    public List<RoomType> AvailableRoomTypes { get; set; } = new();

    public SearchModel(IRoomService roomService, IBookingService bookingService, IUserService userService)
    {
        _roomService = roomService;
        _bookingService = bookingService;
        _userService = userService;
    }

    public async Task OnGetAsync()
    {
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
    }

    private async Task PopulateAvailableRoomTypesAsync()
    {
        if (CurrentUserRole == "Staff")
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

    public async Task OnPostAsync()
    {
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
                return;
            }

            // Validate booking date is not more than 60 days in advance
            var daysInAdvance = (SearchCriteria.BookingDate.Date - today).Days;
            if (daysInAdvance > 60)
            {
                ModelState.AddModelError(string.Empty, $"Bookings can only be made up to 60 days in advance. Your selected date is {daysInAdvance} days away.");
                return;
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
    }
}
