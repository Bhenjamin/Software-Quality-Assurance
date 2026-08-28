using StudyRoomBooking.Domain.Entities;

namespace StudyRoomBooking.Domain.Interfaces;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(int id);
    Task<List<Room>> GetAllAsync();
    Task<Room?> GetByCodeAsync(string code);
    Task AddAsync(Room room);
    Task UpdateAsync(Room room);
    Task DeleteAsync(int id);
}
