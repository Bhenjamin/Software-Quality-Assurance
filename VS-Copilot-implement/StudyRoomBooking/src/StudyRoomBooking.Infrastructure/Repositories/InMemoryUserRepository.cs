using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Domain.Interfaces;

namespace StudyRoomBooking.Infrastructure.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();
    private int _nextId = 1;

    public Task<User?> GetByIdAsync(int id)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
    }

    public Task<List<User>> GetAllAsync()
    {
        return Task.FromResult(_users.ToList());
    }

    public Task<User?> GetByUserIdAsync(string userId)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.UserId == userId));
    }

    public Task AddAsync(User user)
    {
        user.Id = _nextId++;
        _users.Add(user);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user)
    {
        var existing = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existing != null)
        {
            _users.Remove(existing);
            _users.Add(user);
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            _users.Remove(user);
        }
        return Task.CompletedTask;
    }
}
