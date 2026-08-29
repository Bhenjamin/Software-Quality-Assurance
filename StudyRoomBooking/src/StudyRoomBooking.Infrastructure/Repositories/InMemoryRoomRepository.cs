using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Domain.Interfaces;

namespace StudyRoomBooking.Infrastructure.Repositories;

public class InMemoryRoomRepository : IRoomRepository
{
    private readonly List<Room> _rooms = new();
    private int _nextId = 1;

    public Task<Room?> GetByIdAsync(int id)
    {
        return Task.FromResult(_rooms.FirstOrDefault(r => r.Id == id));
    }

    public Task<List<Room>> GetAllAsync()
    {
        return Task.FromResult(_rooms.ToList());
    }

    public Task<Room?> GetByCodeAsync(string code)
    {
        return Task.FromResult(_rooms.FirstOrDefault(r => r.Code == code));
    }

    public Task AddAsync(Room room)
    {
        room.Id = _nextId++;
        _rooms.Add(room);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Room room)
    {
        var existing = _rooms.FirstOrDefault(r => r.Id == room.Id);
        if (existing != null)
        {
            _rooms.Remove(existing);
            _rooms.Add(room);
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        var room = _rooms.FirstOrDefault(r => r.Id == id);
        if (room != null)
        {
            _rooms.Remove(room);
        }
        return Task.CompletedTask;
    }
}
