using StudyRoomBooking.Application.ViewModels;

namespace StudyRoomBooking.Application.Interfaces;

public interface IRoomService
{
    List<RoomViewModel> GetAllRooms();
    RoomViewModel? GetRoomById(int id);
    void CreateRoom(RoomViewModel room);
    void UpdateRoom(RoomViewModel room);
    void DeleteRoom(int id);
}
