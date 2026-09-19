namespace StudyRoomBooking.Application.Services;

public interface IAccessRuleService
{
    Task<bool> ValidateAccessAsync(int userId, int roomId);
    Task<List<Domain.Entities.AccessRule>> GetAllAsync();
    Task<List<Domain.Entities.AccessRule>> GetRulesByRoomIdAsync(int roomId);
    Task<Domain.Entities.AccessRule> CreateAsync(Domain.Entities.AccessRule rule);
    Task UpdateAsync(Domain.Entities.AccessRule rule);
    Task DeleteAsync(int id);
}
