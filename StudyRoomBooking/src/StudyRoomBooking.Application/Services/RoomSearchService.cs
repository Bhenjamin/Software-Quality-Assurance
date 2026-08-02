using StudyRoomBooking.Application.DTOs;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Domain.Enums;
using StudyRoomBooking.Domain.Models;

namespace StudyRoomBooking.Application.Services;

public class RoomSearchService : IRoomSearchService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IBookingRepository _bookingRepository;

    public RoomSearchService(IRoomRepository roomRepository, IBookingRepository bookingRepository)
    {
        _roomRepository = roomRepository;
        _bookingRepository = bookingRepository;
    }

    public IEnumerable<Room> SearchAvailableRooms(RoomSearchCriteria criteria)
    {
        if (criteria.EndTime <= criteria.StartTime)
        {
            throw new ArgumentException("Search end time must be after start time.", nameof(criteria));
        }

        var candidates = _roomRepository.GetAll().Where(r => r.IsActive);

        if (criteria.MinCapacity.HasValue)
        {
            candidates = candidates.Where(r => r.Capacity >= criteria.MinCapacity.Value);
        }

        if (!string.IsNullOrWhiteSpace(criteria.Location))
        {
            candidates = candidates.Where(r =>
                r.Location.Contains(criteria.Location, StringComparison.OrdinalIgnoreCase));
        }

        if (criteria.Type.HasValue)
        {
            candidates = candidates.Where(r => r.Type == criteria.Type.Value);
        }

        return candidates
            .Where(room => IsRoomFree(room.Id, criteria.StartTime, criteria.EndTime))
            .OrderBy(r => r.Name)
            .ToList();
    }

    private bool IsRoomFree(Guid roomId, DateTime start, DateTime end)
    {
        return !_bookingRepository.GetByRoomId(roomId)
            .Where(b => b.Status != BookingStatus.Cancelled)
            .Any(b => b.OverlapsWith(start, end));
    }
}
