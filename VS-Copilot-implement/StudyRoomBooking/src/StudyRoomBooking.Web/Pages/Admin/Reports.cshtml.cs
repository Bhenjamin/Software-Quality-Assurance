using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Services;

namespace StudyRoomBooking.Web.Pages.Admin;

public class ReportsModel : PageModel
{
    private readonly IReportService _reportService;

    public Dictionary<string, int> BookingStatistics { get; set; } = new();
    public List<(string RoomName, int BookingCount)> RoomUtilization { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public ReportsModel(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task OnGetAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        StartDate = startDate ?? DateTime.Today.AddMonths(-1);
        EndDate = endDate ?? DateTime.Today;

        await LoadReports();
    }

    public async Task OnPostAsync(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;

        await LoadReports();
    }

    private async Task LoadReports()
    {
        try
        {
            BookingStatistics = await _reportService.GetBookingStatisticsAsync(StartDate, EndDate);
            RoomUtilization = await _reportService.GetRoomUtilizationAsync(StartDate, EndDate);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error loading report: {ex.Message}");
        }
    }
}
