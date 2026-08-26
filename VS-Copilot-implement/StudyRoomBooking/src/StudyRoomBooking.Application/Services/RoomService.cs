using StudyRoomBooking.Infrastructure.Data;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Domain.Entities;

namespace StudyRoomBooking.Application.Services;

public class RoomService : IRoomService
{
    private readonly DataStore _dataStore;

    public RoomService(DataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public List<RoomViewModel> GetAllRooms()
    {
        return _dataStore.Rooms
            .Select(r => MapToViewModel(r))
            .ToList();
    }

    public RoomViewModel? GetRoomById(int id)
    {
        var room = _dataStore.Rooms.FirstOrDefault(r => r.Id == id);
        return room != null ? MapToViewModel(room) : null;
    }

    public void CreateRoom(RoomViewModel room)
    {
        var newRoom = new Room
        {
            Id = _dataStore.Rooms.Count > 0 ? _dataStore.Rooms.Max(r => r.Id) + 1 : 1,
            RoomCode = room.RoomCode,
            RoomName = room.RoomName,
            Location = room.Location,
            Capacity = room.Capacity,
            RoomType = room.RoomType,
            Description = room.Description,
            IsAvailable = room.IsAvailable,
            CreatedAt = DateTime.UtcNow
        };

        _dataStore.Rooms.Add(newRoom);
    }

    public void UpdateRoom(RoomViewModel room)
    {
        var existingRoom = _dataStore.Rooms.FirstOrDefault(r => r.Id == room.Id);
        if (existingRoom == null)
            return;

        existingRoom.RoomCode = room.RoomCode;
        existingRoom.RoomName = room.RoomName;
        existingRoom.Location = room.Location;
        existingRoom.Capacity = room.Capacity;
        existingRoom.RoomType = room.RoomType;
        existingRoom.Description = room.Description;
        existingRoom.IsAvailable = room.IsAvailable;
        existingRoom.UpdatedAt = DateTime.UtcNow;
    }

    public void DeleteRoom(int id)
    {
        var room = _dataStore.Rooms.FirstOrDefault(r => r.Id == id);
        if (room != null)
            _dataStore.Rooms.Remove(room);
    }

    private RoomViewModel MapToViewModel(Room room)
    {
        return new RoomViewModel
        {
            Id = room.Id,
            RoomCode = room.RoomCode,
            RoomName = room.RoomName,
            Location = room.Location,
            Capacity = room.Capacity,
            RoomType = room.RoomType,
            Description = room.Description,
            IsAvailable = room.IsAvailable
        };
    }
}
