using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IRoomService _roomService;

    public string CurrentUserName { get; set; } = "Guest";
    public string CurrentUserRole { get; set; } = "N/A";
    public string RoomsJson { get; set; } = "[]";

    public IndexModel(ILogger<IndexModel> logger, IRoomService roomService)
    {
        _logger = logger;
        _roomService = roomService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Check if user is logged in
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            // Redirect to login if not authenticated
            return RedirectToPage("Login");
        }

        // Get user info from session
        CurrentUserName = HttpContext.Session.GetString("CurrentUser") ?? "Guest";
        CurrentUserRole = HttpContext.Session.GetString("CurrentUserRole") ?? "Student";

        // Load rooms
        try
        {
            var rooms = await _roomService.GetAllRoomsAsync();
            var roomList = rooms.Select(r => new
            {
                code = r.Code,
                name = r.Name,
                location = r.Location,
                capacity = r.Capacity,
                type = r.Type.ToString()
            }).ToList();

            RoomsJson = JsonSerializer.Serialize(roomList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading rooms");
        }

        return Page();
    }
}
