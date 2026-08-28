namespace StudyRoomBooking.Application.Services;

public interface IReportService
{
    Task<Dictionary<string, int>> GetBookingStatisticsAsync(DateTime startDate, DateTime endDate);
    Task<List<(string RoomName, int BookingCount)>> GetRoomUtilizationAsync(DateTime startDate, DateTime endDate);
}
