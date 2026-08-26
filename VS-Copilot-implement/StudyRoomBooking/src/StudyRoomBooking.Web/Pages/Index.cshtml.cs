using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Infrastructure.Shared.Constants;

namespace StudyRoomBooking.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public string? UserName { get; set; }
    public string? UserRole { get; set; }
    public bool IsAuthenticated { get; set; }

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        var userId = HttpContext.Session.GetString(AppConstants.UserSessionKey);
        IsAuthenticated = !string.IsNullOrEmpty(userId);

        if (IsAuthenticated)
        {
            UserName = HttpContext.Session.GetString("UserEmail");
            UserRole = HttpContext.Session.GetString("UserRole");
        }
        else
        {
            Response.Redirect("/Auth/Login");
        }
    }
}
