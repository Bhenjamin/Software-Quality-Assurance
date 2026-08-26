using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Infrastructure.Shared.Constants;

namespace StudyRoomBooking.Web.Pages.Rooms;

public class IndexModel : PageModel
{
    private readonly IRoomService _roomService;

    public List<RoomViewModel> Rooms { get; set; } = new();

    public IndexModel(IRoomService roomService)
    {
        _roomService = roomService;
    }

    public void OnGet()
    {
        if (!IsAdmin())
            RedirectToPage("/Index");

        Rooms = _roomService.GetAllRooms();
    }

    private bool IsAdmin()
    {
        var role = HttpContext.Session.GetString("UserRole");
        return role == "Admin";
    }
}
