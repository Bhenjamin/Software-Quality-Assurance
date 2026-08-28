using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Application.Services;

public class RoomService : IRoomService
{
    private readonly Domain.Interfaces.IUnitOfWork _unitOfWork;

    public RoomService(Domain.Interfaces.IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Room?> GetRoomByIdAsync(int id)
    {
        return await _unitOfWork.Rooms.GetByIdAsync(id);
    }

    public async Task<List<Room>> GetAllRoomsAsync()
    {
        return await _unitOfWork.Rooms.GetAllAsync();
    }

    public async Task<List<Room>> SearchRoomsAsync(DateTime date, TimeSpan startTime, TimeSpan endTime, int? capacity = null, RoomType? type = null, string? location = null)
    {
        var allRooms = await _unitOfWork.Rooms.GetAllAsync();

        var filtered = allRooms.Where(r =>
            r.IsAvailable &&
            (!capacity.HasValue || r.Capacity >= capacity.Value) &&
            (!type.HasValue || r.Type == type.Value) &&
            (string.IsNullOrEmpty(location) || r.Location.Contains(location, StringComparison.OrdinalIgnoreCase))
        ).ToList();

        // Filter by availability on the requested date/time
        var availableRooms = new List<Room>();
        foreach (var room in filtered)
        {
            if (await IsRoomAvailableAsync(room.Id, date, startTime, endTime))
            {
                availableRooms.Add(room);
            }
        }

        return availableRooms;
    }

    public async Task<Room> CreateRoomAsync(Room room)
    {
        room.CreatedAt = DateTime.UtcNow;
        room.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Rooms.AddAsync(room);
        await _unitOfWork.SaveChangesAsync();
        return room;
    }

    public async Task<Room> UpdateRoomAsync(Room room)
    {
        room.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Rooms.UpdateAsync(room);
        await _unitOfWork.SaveChangesAsync();
        return room;
    }

    public async Task DeleteRoomAsync(int roomId)
    {
        await _unitOfWork.Rooms.DeleteAsync(roomId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime date, TimeSpan startTime, TimeSpan endTime)
    {
        var bookings = await _unitOfWork.Bookings.GetByRoomIdAsync(roomId);
        var conflictingBookings = bookings.Where(b =>
            b.BookingDate.Date == date.Date &&
            b.Status != BookingStatus.Cancelled &&
            // Check for time overlap
            !(b.EndTime <= startTime || b.StartTime >= endTime)
        ).ToList();

        return conflictingBookings.Count == 0;
    }
}
