using StudyRoomBooking.Domain.Entities;

namespace StudyRoomBooking.Application.Services;

public interface IUserService
{
    Task<User?> GetUserByIdAsync(int id);
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByUserIdAsync(string userId);
    Task<User> CreateUserAsync(User user);
    Task<User> UpdateUserAsync(User user);
    Task DeleteUserAsync(int userId);
}
