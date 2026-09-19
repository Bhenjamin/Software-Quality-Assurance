using StudyRoomBooking.Domain.Entities;

namespace StudyRoomBooking.Domain.Interfaces;

public interface IAccessRuleRepository
{
    Task<AccessRule?> GetByIdAsync(int id);
    Task<List<AccessRule>> GetAllAsync();
    Task AddAsync(AccessRule rule);
    Task UpdateAsync(AccessRule rule);
    Task DeleteAsync(int id);
}
