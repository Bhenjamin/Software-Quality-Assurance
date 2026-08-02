using StudyRoomBooking.Domain.Models;

namespace StudyRoomBooking.Application.Interfaces;

public interface IUserRepository
{
    IEnumerable<User> GetAll();
    User? GetById(Guid userId);
    void Add(User user);
}
