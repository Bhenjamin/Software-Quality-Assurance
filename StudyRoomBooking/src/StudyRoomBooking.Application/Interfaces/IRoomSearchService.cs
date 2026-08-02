using StudyRoomBooking.Application.DTOs;
using StudyRoomBooking.Domain.Models;

namespace StudyRoomBooking.Application.Interfaces;

public interface IRoomSearchService
{
    /// <summary>
    /// Returns rooms matching the given criteria that have no confirmed
    /// booking overlapping the requested time window.
    /// </summary>
    IEnumerable<Room> SearchAvailableRooms(RoomSearchCriteria criteria);
}
