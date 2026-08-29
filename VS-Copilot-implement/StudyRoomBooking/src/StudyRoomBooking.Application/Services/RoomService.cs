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

    public async Task<List<Room>> SearchRoomsAsync(DateTime date, TimeSpan? startTime, TimeSpan? endTime, int? capacity = null, RoomType? type = null, string? location = null)
    {
        return await SearchRoomsAsync(date, startTime, endTime, capacity, type, location, null);
    }

    public async Task<List<Room>> SearchRoomsAsync(DateTime date, TimeSpan? startTime, TimeSpan? endTime, int? capacity = null, RoomType? type = null, string? location = null, StudentMajor? studentMajor = null)
    {
        var allRooms = await _unitOfWork.Rooms.GetAllAsync();

        var filtered = allRooms.Where(r =>
            r.IsAvailable &&
            (!capacity.HasValue || r.Capacity >= capacity.Value) &&
            (!type.HasValue || r.Type == type.Value) &&
            (string.IsNullOrEmpty(location) || r.Location.Contains(location, StringComparison.OrdinalIgnoreCase))
        ).ToList();

        // Filter by student major if provided
        if (studentMajor.HasValue)
        {
            var filteredByMajor = new List<Room>();
            foreach (var room in filtered)
            {
                // When searching with "Any Type", exclude ComputerLab rooms for students
                if (!type.HasValue && room.Type == RoomType.ComputerLab)
                {
                    continue;
                }

                var allowedMajors = await _unitOfWork.RoomMajorRestrictions.GetAllowedMajorsForRoomAsync(room.Id);

                // Study rooms are always available to all students
                if (room.Type == RoomType.Study)
                {
                    filteredByMajor.Add(room);
                }
                // Specialised restricted rooms (DesignStudio, EngineeringLab) should be included
                // if the student has access, regardless of "Any Type" search
                else if (room.Type == RoomType.DesignStudio || room.Type == RoomType.EngineeringLab)
                {
                    if (allowedMajors.Contains(studentMajor.Value))
                    {
                        filteredByMajor.Add(room);
                    }
                }
                // If room type is not specified (Any Type search):
                // - Students should only see Study rooms and restricted Lab rooms for their major
                // - Do NOT show unrestricted rooms (Meeting, Seminar, etc.)
                else if (!type.HasValue && allowedMajors.Count == 0)
                {
                    // Skip unrestricted non-Study rooms when doing "Any Type" search
                    continue;
                }
                // If room has no restrictions and a specific type is selected, show it
                else if (type.HasValue && allowedMajors.Count == 0)
                {
                    filteredByMajor.Add(room);
                }
                // If room has restrictions, only allowed majors can book it
                else if (allowedMajors.Contains(studentMajor.Value))
                {
                    filteredByMajor.Add(room);
                }
            }
            filtered = filteredByMajor;
        }

        // If both times are not set, return all filtered rooms (no time-based filtering)
        if (!startTime.HasValue && !endTime.HasValue)
        {
            return filtered;
        }

        // If at least one time is set, use default values for missing times
        var actualStartTime = startTime ?? new TimeSpan(8, 0, 0);
        var actualEndTime = endTime ?? new TimeSpan(22, 0, 0);

        // Filter by availability on the requested date/time
        var availableRooms = new List<Room>();
        foreach (var room in filtered)
        {
            if (await IsRoomAvailableAsync(room.Id, date, actualStartTime, actualEndTime))
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

    public async Task<List<StudentMajor>> GetAllowedMajorsForRoomAsync(int roomId)
    {
        return await _unitOfWork.RoomMajorRestrictions.GetAllowedMajorsForRoomAsync(roomId);
    }
}

