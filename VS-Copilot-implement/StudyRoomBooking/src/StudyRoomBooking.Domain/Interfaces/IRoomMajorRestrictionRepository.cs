using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Domain.Interfaces;

public interface IRoomMajorRestrictionRepository
{
    Task<RoomMajorRestriction?> GetByIdAsync(int id);
    Task<List<RoomMajorRestriction>> GetAllAsync();
    Task<List<RoomMajorRestriction>> GetByRoomIdAsync(int roomId);
    Task<List<StudentMajor>> GetAllowedMajorsForRoomAsync(int roomId);
    Task AddAsync(RoomMajorRestriction restriction);
    Task UpdateAsync(RoomMajorRestriction restriction);
    Task DeleteAsync(int id);
}
