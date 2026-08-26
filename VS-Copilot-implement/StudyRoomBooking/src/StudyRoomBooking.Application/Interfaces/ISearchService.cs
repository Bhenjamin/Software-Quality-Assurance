using StudyRoomBooking.Application.ViewModels;

namespace StudyRoomBooking.Application.Interfaces;

public interface ISearchService
{
    List<RoomViewModel> SearchRooms(SearchViewModel criteria);
}
