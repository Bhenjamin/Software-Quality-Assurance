using StudyRoomBooking.Domain.Entities;

namespace StudyRoomBooking.Application.Services;

public class AccessRuleService : IAccessRuleService
{
    public async Task<bool> ValidateAccessAsync(int userId, int roomId)
    {
        // Placeholder for access validation logic
        // In the initial prototype, all users have access unless restricted
        await Task.CompletedTask;
        return true;
    }

    public async Task<List<AccessRule>> GetRulesByRoomIdAsync(int roomId)
    {
        // Placeholder for retrieving rules by room
        await Task.CompletedTask;
        return new List<AccessRule>();
    }
}
