using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Web.Pages.Admin;

public class RoomManagementModel : PageModel
{
    private readonly IRoomService _roomService;

    public List<Room> Rooms { get; set; } = new();
    public Room? EditingRoom { get; set; }
    public bool ShowForm { get; set; } = false;
    public bool IsEdit { get; set; } = false;

    public RoomManagementModel(IRoomService roomService)
    {
        _roomService = roomService;
    }

    public async Task OnGetAsync()
    {
        Rooms = await _roomService.GetAllRoomsAsync();
    }

    public async Task<IActionResult> OnGetCreateAsync()
    {
        Rooms = await _roomService.GetAllRoomsAsync();
        ShowForm = true;
        IsEdit = false;
        EditingRoom = new Room { IsAvailable = true };
        return Page();
    }

    public async Task<IActionResult> OnGetEditAsync(int id)
    {
        Rooms = await _roomService.GetAllRoomsAsync();
        EditingRoom = await _roomService.GetRoomByIdAsync(id);
        ShowForm = true;
        IsEdit = true;
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(int? roomId, string code, string name, string location, 
        int capacity, RoomType type, string description)
    {
        try
        {
            // Read checkbox value directly from form (more reliable than model binding)
            bool isAvailable = Request.Form.ContainsKey("isAvailable") && 
                              Request.Form["isAvailable"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase);

            if (roomId.HasValue)
            {
                // Update existing room
                var room = await _roomService.GetRoomByIdAsync(roomId.Value);
                if (room != null)
                {
                    room.Code = code;
                    room.Name = name;
                    room.Location = location;
                    room.Capacity = capacity;
                    room.Type = type;
                    room.Description = description;
                    room.IsAvailable = isAvailable;

                    await _roomService.UpdateRoomAsync(room);
                }
            }
            else
            {
                // Create new room
                var newRoom = new Room
                {
                    Code = code,
                    Name = name,
                    Location = location,
                    Capacity = capacity,
                    Type = type,
                    Description = description,
                    IsAvailable = isAvailable
                };

                await _roomService.CreateRoomAsync(newRoom);
            }

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error saving room: {ex.Message}");
            Rooms = await _roomService.GetAllRoomsAsync();
            ShowForm = true;
            IsEdit = roomId.HasValue;
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            await _roomService.DeleteRoomAsync(id);
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error deleting room: {ex.Message}");
            Rooms = await _roomService.GetAllRoomsAsync();
            return Page();
        }
    }
}
