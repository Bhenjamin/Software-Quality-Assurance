using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Domain.Enums;
using StudyRoomBooking.Infrastructure.Shared.Constants;

namespace StudyRoomBooking.Web.Pages.Rooms;

public class CreateModel : PageModel
{
    private readonly IRoomService _roomService;

    [BindProperty]
    public RoomViewModel Room { get; set; } = new();

    public CreateModel(IRoomService roomService)
    {
        _roomService = roomService;
    }

    public void OnGet()
    {
        if (!IsAdmin())
            RedirectToPage("/Index");
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        _roomService.CreateRoom(Room);
        return RedirectToPage("Index");
    }

    private bool IsAdmin()
    {
        var role = HttpContext.Session.GetString("UserRole");
        return role == "Admin";
    }
}
