using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Domain.Enums;
using StudyRoomBooking.Domain.Interfaces;

namespace StudyRoomBooking.Infrastructure.Repositories;

public class InMemoryRoomMajorRestrictionRepository : IRoomMajorRestrictionRepository
{
    private readonly List<RoomMajorRestriction> _restrictions = new();
    private int _nextId = 1;

    public Task<RoomMajorRestriction?> GetByIdAsync(int id)
    {
        return Task.FromResult(_restrictions.FirstOrDefault(r => r.Id == id));
    }

    public Task<List<RoomMajorRestriction>> GetAllAsync()
    {
        return Task.FromResult(_restrictions.ToList());
    }

    public Task<List<RoomMajorRestriction>> GetByRoomIdAsync(int roomId)
    {
        return Task.FromResult(_restrictions.Where(r => r.RoomId == roomId && r.IsActive).ToList());
    }

    public async Task<List<StudentMajor>> GetAllowedMajorsForRoomAsync(int roomId)
    {
        var restrictions = await GetByRoomIdAsync(roomId);
        return restrictions.Select(r => r.Major).Distinct().ToList();
    }

    public Task AddAsync(RoomMajorRestriction restriction)
    {
        restriction.Id = _nextId++;
        _restrictions.Add(restriction);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(RoomMajorRestriction restriction)
    {
        var existing = _restrictions.FirstOrDefault(r => r.Id == restriction.Id);
        if (existing != null)
        {
            _restrictions.Remove(existing);
            _restrictions.Add(restriction);
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        var restriction = _restrictions.FirstOrDefault(r => r.Id == id);
        if (restriction != null)
        {
            _restrictions.Remove(restriction);
        }
        return Task.CompletedTask;
    }
}
