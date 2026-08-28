using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Application.Services;

public class ReportService : IReportService
{
    private readonly Domain.Interfaces.IUnitOfWork _unitOfWork;

    public ReportService(Domain.Interfaces.IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Dictionary<string, int>> GetBookingStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        var bookings = await _unitOfWork.Bookings.GetByDateRangeAsync(startDate, endDate);

        var stats = new Dictionary<string, int>
        {
            { "Total Bookings", bookings.Count },
            { "Confirmed", bookings.Count(b => b.Status == BookingStatus.Confirmed) },
            { "Cancelled", bookings.Count(b => b.Status == BookingStatus.Cancelled) },
            { "Completed", bookings.Count(b => b.Status == BookingStatus.Completed) }
        };

        return stats;
    }

    public async Task<List<(string RoomName, int BookingCount)>> GetRoomUtilizationAsync(DateTime startDate, DateTime endDate)
    {
        var bookings = await _unitOfWork.Bookings.GetByDateRangeAsync(startDate, endDate);
        var rooms = await _unitOfWork.Rooms.GetAllAsync();

        var utilization = new List<(string, int)>();

        foreach (var room in rooms)
        {
            var roomBookingCount = bookings.Count(b => b.RoomId == room.Id && b.Status != BookingStatus.Cancelled);
            utilization.Add((room.Name, roomBookingCount));
        }

        return utilization.OrderByDescending(x => x.Item2).ToList();
    }
}
