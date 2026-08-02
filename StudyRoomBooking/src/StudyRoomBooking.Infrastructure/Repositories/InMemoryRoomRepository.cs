using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Domain.Models;

namespace StudyRoomBooking.Infrastructure.Repositories;

/// <summary>
/// Simple in-memory store used for the prototype. A real deployment
/// would replace this with an EF Core (or similar) implementation of
/// IRoomRepository — nothing outside this class would need to change.
/// </summary>
public class InMemoryRoomRepository : IRoomRepository
{
    private readonly List<Room> _rooms = new();

    public IEnumerable<Room> GetAll() => _rooms.ToList();

    public Room? GetById(Guid roomId) => _rooms.FirstOrDefault(r => r.Id == roomId);

    public void Add(Room room) => _rooms.Add(room);

    public void Update(Room room)
    {
        var index = _rooms.FindIndex(r => r.Id == room.Id);
        if (index >= 0)
        {
            _rooms[index] = room;
        }
    }
}
