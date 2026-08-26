using StudyRoomBooking.Application.ViewModels;

namespace StudyRoomBooking.Application.Interfaces;

public interface IReportService
{
    ReportViewModel GenerateOccupancyReport(DateTime startDate, DateTime endDate);
    ReportViewModel GenerateUserBookingReport(int userId);
    ReportViewModel GenerateRoomBookingReport(int roomId);
}
