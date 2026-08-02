using StudyRoomBooking.Domain.Models;

namespace StudyRoomBooking.Application.Interfaces;

/// <summary>
/// Persistence contract for rooms. The prototype ships an in-memory
/// implementation (see StudyRoomBooking.Infrastructure); a database-backed
/// implementation can be swapped in later without touching the Application
/// or Domain layers.
/// </summary>
public interface IRoomRepository
{
    IEnumerable<Room> GetAll();
    Room? GetById(Guid roomId);
    void Add(Room room);
    void Update(Room room);
}
