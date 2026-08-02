using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Domain.Models;

namespace StudyRoomBooking.Infrastructure.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();

    public IEnumerable<User> GetAll() => _users.ToList();

    public User? GetById(Guid userId) => _users.FirstOrDefault(u => u.Id == userId);

    public void Add(User user) => _users.Add(user);
}
