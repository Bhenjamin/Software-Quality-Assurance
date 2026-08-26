using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Application.ViewModels;

namespace StudyRoomBooking.Web.Pages.Admin;

public class ReportsModel : PageModel
{
    private readonly IReportService _reportService;

    [BindProperty]
    public DateTime StartDate { get; set; }

    [BindProperty]
    public DateTime EndDate { get; set; }

    public ReportViewModel? OccupancyReport { get; set; }
    public bool HasGenerated { get; set; } = false;

    public ReportsModel(IReportService reportService)
    {
        _reportService = reportService;
    }

    public void OnGet()
    {
        if (!IsAdmin())
            RedirectToPage("/Index");

        StartDate = DateTime.Today.AddDays(-30);
        EndDate = DateTime.Today;
    }

    public IActionResult OnPost()
    {
        if (!IsAdmin())
            return RedirectToPage("/Index");

        if (StartDate > EndDate)
            ModelState.AddModelError("EndDate", "End date must be after start date");

        if (!ModelState.IsValid)
            return Page();

        OccupancyReport = _reportService.GenerateOccupancyReport(StartDate, EndDate);
        HasGenerated = true;

        return Page();
    }

    private bool IsAdmin()
    {
        var role = HttpContext.Session.GetString("UserRole");
        return role == "Admin";
    }
}
