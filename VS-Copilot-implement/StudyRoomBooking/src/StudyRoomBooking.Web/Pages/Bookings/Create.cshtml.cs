using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Infrastructure.Data;
using StudyRoomBooking.Infrastructure.Shared.Constants;

namespace StudyRoomBooking.Web.Pages.Bookings;

public class CreateModel : PageModel
{
    private readonly IBookingService _bookingService;
    private readonly IRoomService _roomService;
    private readonly DataStore _dataStore;

    [BindProperty]
    public BookingViewModel Booking { get; set; } = new();

    public RoomViewModel? Room { get; set; }
    public string? ErrorMessage { get; set; }

    public CreateModel(IBookingService bookingService, IRoomService roomService, DataStore dataStore)
    {
        _bookingService = bookingService;
        _roomService = roomService;
        _dataStore = dataStore;
    }

    public IActionResult OnGet(int? roomId)
    {
        if (!IsAuthenticated())
            return RedirectToPage("/Auth/Login");

        if (roomId.HasValue)
        {
            Room = _roomService.GetRoomById(roomId.Value);
            if (Room != null)
            {
                // Check if user has access to this room
                var userId = GetCurrentUserId();
                var user = GetCurrentUser();
                if (user != null && !_bookingService.HasAccessToRoom(roomId.Value, user.Role))
                {
                    ErrorMessage = "You do not have permission to book this room.";
                    return RedirectToPage("/Bookings/Index");
                }
                Booking.RoomId = Room.Id;
            }
        }

        // Set date constraints: today at earliest, 60 days from now at latest
        Booking.BookingDate = DateTime.Today;
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!IsAuthenticated())
            return RedirectToPage("/Auth/Login");

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var userId = int.Parse(HttpContext.Session.GetString(AppConstants.UserSessionKey) ?? "0");
            Booking.UserId = userId;
            _bookingService.CreateBooking(Booking);
            return RedirectToPage("Index");
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
            Room = _roomService.GetRoomById(Booking.RoomId);
            return Page();
        }
    }

    private bool IsAuthenticated()
    {
        var userId = HttpContext.Session.GetString(AppConstants.UserSessionKey);
        return !string.IsNullOrEmpty(userId);
    }

    private int GetCurrentUserId()
    {
        return int.Parse(HttpContext.Session.GetString(AppConstants.UserSessionKey) ?? "0");
    }

    private User? GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        return _dataStore.Users.FirstOrDefault(u => u.Id == userId);
    }
}
