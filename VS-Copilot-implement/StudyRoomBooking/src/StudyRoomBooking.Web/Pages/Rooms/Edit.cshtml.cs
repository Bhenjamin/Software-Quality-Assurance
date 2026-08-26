using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Infrastructure.Shared.Constants;

namespace StudyRoomBooking.Web.Pages.Rooms;

public class EditModel : PageModel
{
    private readonly IRoomService _roomService;

    [BindProperty]
    public RoomViewModel Room { get; set; } = new();

    public EditModel(IRoomService roomService)
    {
        _roomService = roomService;
    }

    public IActionResult OnGet(int? id)
    {
        if (!IsAdmin())
            return RedirectToPage("/Index");

        if (!id.HasValue)
            return NotFound();

        var room = _roomService.GetRoomById(id.Value);
        if (room == null)
            return NotFound();

        Room = room;
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        _roomService.UpdateRoom(Room);
        return RedirectToPage("Index");
    }

    private bool IsAdmin()
    {
        var role = HttpContext.Session.GetString("UserRole");
        return role == "Admin";
    }
}
