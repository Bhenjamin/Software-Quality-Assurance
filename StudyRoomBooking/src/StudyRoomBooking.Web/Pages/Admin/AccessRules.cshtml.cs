using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Domain.Entities;

namespace StudyRoomBooking.Web.Pages.Admin;

public class AccessRulesModel : AdminPageModel
{
    private readonly IRoomService _roomService;
    private readonly IAccessRuleService _accessRuleService;

    public List<Room> AvailableRooms { get; set; } = new();
    public List<AccessRule> AccessRules { get; set; } = new();
    public Dictionary<int, string> RoomNames { get; set; } = new();

    public AccessRulesModel(IRoomService roomService, IAccessRuleService accessRuleService)
    {
        _roomService = roomService;
        _accessRuleService = accessRuleService;
    }

    public async Task OnGetAsync()
    {
        AvailableRooms = await _roomService.GetAllRoomsAsync();
        AccessRules = await _accessRuleService.GetAllAsync();
        RoomNames = AvailableRooms.ToDictionary(room => room.Id, room => room.Name);
    }

    public async Task<IActionResult> OnPostAsync(int roomId, string ruleName, string description, 
        string? startTime = null, string? endTime = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ruleName) || !await RoomExists(roomId))
            {
                ModelState.AddModelError(string.Empty, "Select a valid room and enter a rule name.");
                await LoadData();
                return Page();
            }

            TimeSpan? start = ParseTime(startTime);
            TimeSpan? end = ParseTime(endTime);
            if (start.HasValue != end.HasValue || (start.HasValue && start >= end))
            {
                ModelState.AddModelError(string.Empty, "Provide both times, with the start before the end.");
                await LoadData();
                return Page();
            }

            var rule = new AccessRule
            {
                RoomId = roomId,
                RuleName = ruleName,
                Description = description,
                StartTime = start,
                EndTime = end,
                IsActive = true
            };

            await _accessRuleService.CreateAsync(rule);

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error creating rule: {ex.Message}");
            AvailableRooms = await _roomService.GetAllRoomsAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _accessRuleService.DeleteAsync(id);
        return RedirectToPage();
    }

    private async Task<bool> RoomExists(int id) => id > 0 && await _roomService.GetRoomByIdAsync(id) is not null;
    private static TimeSpan? ParseTime(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : TimeSpan.TryParse(value, out var time) ? time : null;
    private async Task LoadData()
    {
        AvailableRooms = await _roomService.GetAllRoomsAsync();
        AccessRules = await _accessRuleService.GetAllAsync();
        RoomNames = AvailableRooms.ToDictionary(room => room.Id, room => room.Name);
    }
}
