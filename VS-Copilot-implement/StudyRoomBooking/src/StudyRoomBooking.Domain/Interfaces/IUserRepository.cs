using StudyRoomBooking.Domain.Entities;

namespace StudyRoomBooking.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<List<User>> GetAllAsync();
    Task<User?> GetByUserIdAsync(string userId);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
}
