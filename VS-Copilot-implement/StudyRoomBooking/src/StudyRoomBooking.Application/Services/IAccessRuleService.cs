namespace StudyRoomBooking.Application.Services;

public interface IAccessRuleService
{
    Task<bool> ValidateAccessAsync(int userId, int roomId);
    Task<List<Domain.Entities.AccessRule>> GetRulesByRoomIdAsync(int roomId);
}
