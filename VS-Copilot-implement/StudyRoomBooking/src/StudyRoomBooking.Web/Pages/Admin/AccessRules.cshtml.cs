using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Domain.Entities;

namespace StudyRoomBooking.Web.Pages.Admin;

public class AccessRulesModel : PageModel
{
    private readonly IRoomService _roomService;

    public List<Room> AvailableRooms { get; set; } = new();
    public List<AccessRule> AccessRules { get; set; } = new();

    public AccessRulesModel(IRoomService roomService)
    {
        _roomService = roomService;
    }

    public async Task OnGetAsync()
    {
        AvailableRooms = await _roomService.GetAllRoomsAsync();
        // In a real implementation, we'd load access rules from the database
    }

    public async Task<IActionResult> OnPostAsync(int roomId, string ruleName, string description, 
        string? startTime = null, string? endTime = null)
    {
        try
        {
            // Create access rule
            TimeSpan? start = null;
            TimeSpan? end = null;

            if (!string.IsNullOrEmpty(startTime))
            {
                start = TimeSpan.ParseExact(startTime, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(endTime))
            {
                end = TimeSpan.ParseExact(endTime, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture);
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

            AccessRules.Add(rule);

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error creating rule: {ex.Message}");
            AvailableRooms = await _roomService.GetAllRoomsAsync();
            return Page();
        }
    }
}
