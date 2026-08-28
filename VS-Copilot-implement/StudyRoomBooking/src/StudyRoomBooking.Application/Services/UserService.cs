using StudyRoomBooking.Domain.Entities;

namespace StudyRoomBooking.Application.Services;

public class UserService : IUserService
{
    private readonly Domain.Interfaces.IUnitOfWork _unitOfWork;

    public UserService(Domain.Interfaces.IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _unitOfWork.Users.GetByIdAsync(id);
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _unitOfWork.Users.GetAllAsync();
    }

    public async Task<User?> GetUserByUserIdAsync(string userId)
    {
        return await _unitOfWork.Users.GetByUserIdAsync(userId);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        user.CreatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return user;
    }

    public async Task DeleteUserAsync(int userId)
    {
        await _unitOfWork.Users.DeleteAsync(userId);
        await _unitOfWork.SaveChangesAsync();
    }
}
