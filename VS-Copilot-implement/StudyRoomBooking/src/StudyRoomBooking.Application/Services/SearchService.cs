using StudyRoomBooking.Infrastructure.Data;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Application.Services;

public class SearchService : ISearchService
{
    private readonly DataStore _dataStore;

    public SearchService(DataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public List<RoomViewModel> SearchRooms(SearchViewModel criteria)
    {
        var query = _dataStore.Rooms.AsQueryable();

        // Filter by location
        if (!string.IsNullOrEmpty(criteria.Location))
            query = query.Where(r => r.Location.Contains(criteria.Location, StringComparison.OrdinalIgnoreCase));

        // Filter by capacity
        if (criteria.Capacity.HasValue && criteria.Capacity > 0)
            query = query.Where(r => r.Capacity >= criteria.Capacity);

        // Filter by room type
        if (criteria.RoomType.HasValue)
            query = query.Where(r => r.RoomType == criteria.RoomType);

        // Filter by availability if time range is specified
        if (criteria.StartTime.HasValue && criteria.EndTime.HasValue)
        {
            var availableRoomIds = GetAvailableRooms(criteria.SearchDate, criteria.StartTime.Value, criteria.EndTime.Value);
            query = query.Where(r => availableRoomIds.Contains(r.Id));
        }

        // Filter by availability status
        query = query.Where(r => r.IsAvailable);

        return query
            .Select(r => new RoomViewModel
            {
                Id = r.Id,
                RoomCode = r.RoomCode,
                RoomName = r.RoomName,
                Location = r.Location,
                Capacity = r.Capacity,
                RoomType = r.RoomType,
                Description = r.Description,
                IsAvailable = r.IsAvailable
            })
            .ToList();
    }

    private List<int> GetAvailableRooms(DateTime date, TimeSpan startTime, TimeSpan endTime)
    {
        var bookedRoomIds = _dataStore.Bookings
            .Where(b => b.BookingDate == date
                && b.Status != BookingStatus.Cancelled
                && !(b.EndTime <= startTime || b.StartTime >= endTime))
            .Select(b => b.RoomId)
            .Distinct()
            .ToList();

        return _dataStore.Rooms
            .Where(r => r.IsAvailable && !bookedRoomIds.Contains(r.Id))
            .Select(r => r.Id)
            .ToList();
    }
}
