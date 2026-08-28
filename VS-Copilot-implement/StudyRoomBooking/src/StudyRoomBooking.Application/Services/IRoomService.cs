using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Application.Services;

public interface IRoomService
{
    Task<Room?> GetRoomByIdAsync(int id);
    Task<List<Room>> GetAllRoomsAsync();
    Task<List<Room>> SearchRoomsAsync(DateTime date, TimeSpan? startTime, TimeSpan? endTime, int? capacity = null, RoomType? type = null, string? location = null);
    Task<Room> CreateRoomAsync(Room room);
    Task<Room> UpdateRoomAsync(Room room);
    Task DeleteRoomAsync(int roomId);
    Task<bool> IsRoomAvailableAsync(int roomId, DateTime date, TimeSpan startTime, TimeSpan endTime);
}
