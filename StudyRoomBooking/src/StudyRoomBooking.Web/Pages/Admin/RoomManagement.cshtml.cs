using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Web.Pages.Admin;

public class RoomManagementModel : AdminPageModel
{
    private readonly IRoomService _roomService;

    public List<Room> Rooms { get; set; } = new();
    public Room? EditingRoom { get; set; }
    public bool ShowForm { get; set; } = false;
    public bool IsEdit { get; set; } = false;
    public Dictionary<int, List<StudentMajor>> RoomAllowedMajors { get; set; } = new();
    public List<StudentMajor> EditingAllowedMajors { get; set; } = new();
    public StudentMajor[] AvailableMajors { get; } = Enum.GetValues<StudentMajor>();
    public bool EditingAllMajors => EditingAllowedMajors.Count == 0;

    public RoomManagementModel(IRoomService roomService)
    {
        _roomService = roomService;
    }

    public async Task OnGetAsync()
    {
        await LoadRoomsAsync();
    }

    public async Task<IActionResult> OnGetCreateAsync()
    {
        await LoadRoomsAsync();
        ShowForm = true;
        IsEdit = false;
        EditingRoom = new Room { IsAvailable = true };
        return Page();
    }

    public async Task<IActionResult> OnGetEditAsync(int id)
    {
        await LoadRoomsAsync();
        EditingRoom = await _roomService.GetRoomByIdAsync(id);
        EditingAllowedMajors = await _roomService.GetAllowedMajorsForRoomAsync(id);
        ShowForm = true;
        IsEdit = true;
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(int? roomId, string code, string name, string location,
        int capacity, RoomType type, string description, string studentAccess = "all",
        StudentMajor[]? allowedMajors = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(location) || capacity < 1 ||
                !Enum.IsDefined(type) ||
                (studentAccess != "all" && studentAccess != "specific") ||
                (studentAccess == "specific" && (allowedMajors is null || allowedMajors.Length == 0 ||
                    allowedMajors.Any(major => !Enum.IsDefined(major)))))
            {
                ModelState.AddModelError(string.Empty, "Code, name, location, a valid type, and a positive capacity are required.");
                await LoadRoomsAsync();
                EditingRoom = roomId.HasValue ? await _roomService.GetRoomByIdAsync(roomId.Value) : new Room { IsAvailable = true };
                EditingAllowedMajors = allowedMajors?.ToList() ?? new();
                ShowForm = true;
                IsEdit = roomId.HasValue;
                return Page();
            }

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
                    await _roomService.SetAllowedMajorsForRoomAsync(room.Id, studentAccess == "specific" ? allowedMajors ?? Array.Empty<StudentMajor>() : Array.Empty<StudentMajor>());
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "The room could not be found.");
                    Rooms = await _roomService.GetAllRoomsAsync();
                    ShowForm = true;
                    IsEdit = true;
                    return Page();
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
                await _roomService.SetAllowedMajorsForRoomAsync(newRoom.Id, studentAccess == "specific" ? allowedMajors ?? Array.Empty<StudentMajor>() : Array.Empty<StudentMajor>());
            }

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error saving room: {ex.Message}");
            await LoadRoomsAsync();
            ShowForm = true;
            IsEdit = roomId.HasValue;
            EditingAllowedMajors = allowedMajors?.ToList() ?? new();
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
            await LoadRoomsAsync();
            return Page();
        }
    }

    private async Task LoadRoomsAsync()
    {
        Rooms = await _roomService.GetAllRoomsAsync();
        RoomAllowedMajors = new();
        foreach (var room in Rooms)
        {
            RoomAllowedMajors[room.Id] = await _roomService.GetAllowedMajorsForRoomAsync(room.Id);
        }
    }
}
