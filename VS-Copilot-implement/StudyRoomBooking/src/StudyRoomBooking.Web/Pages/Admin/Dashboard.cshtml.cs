using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Infrastructure.Shared.Constants;

namespace StudyRoomBooking.Web.Pages.Admin;

public class DashboardModel : PageModel
{
    private readonly IAdminService _adminService;
    private readonly IRoomService _roomService;
    private readonly IBookingService _bookingService;

    public List<UserViewModel> Users { get; set; } = new();
    public List<BookingViewModel> Bookings { get; set; } = new();
    public List<RoomViewModel> Rooms { get; set; } = new();

    public DashboardModel(IAdminService adminService, IRoomService roomService, IBookingService bookingService)
    {
        _adminService = adminService;
        _roomService = roomService;
        _bookingService = bookingService;
    }

    public IActionResult OnGet()
    {
        if (!IsAdmin())
            return RedirectToPage("/Index");

        Users = _adminService.GetAllUsers();
        Bookings = _adminService.GetAllBookings();
        Rooms = _roomService.GetAllRooms();

        return Page();
    }

    private bool IsAdmin()
    {
        var role = HttpContext.Session.GetString("UserRole");
        return role == "Admin";
    }
}
