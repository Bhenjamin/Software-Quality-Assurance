using StudyRoomBooking.Domain.Entities;

namespace StudyRoomBooking.Application.Services;

public class AccessRuleService : IAccessRuleService
{
    private readonly Domain.Interfaces.IUnitOfWork _unitOfWork;

    public AccessRuleService(Domain.Interfaces.IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> ValidateAccessAsync(int userId, int roomId)
    {
        var rules = await GetRulesByRoomIdAsync(roomId);
        return !rules.Any(rule => rule.IsActive);
    }

    public async Task<List<AccessRule>> GetRulesByRoomIdAsync(int roomId)
    {
        return (await _unitOfWork.AccessRules.GetAllAsync())
            .Where(rule => rule.RoomId == roomId)
            .ToList();
    }

    public Task<List<AccessRule>> GetAllAsync() => _unitOfWork.AccessRules.GetAllAsync();

    public async Task<AccessRule> CreateAsync(AccessRule rule)
    {
        rule.CreatedAt = DateTime.UtcNow;
        await _unitOfWork.AccessRules.AddAsync(rule);
        await _unitOfWork.SaveChangesAsync();
        return rule;
    }

    public async Task UpdateAsync(AccessRule rule)
    {
        await _unitOfWork.AccessRules.UpdateAsync(rule);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await _unitOfWork.AccessRules.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }
}
