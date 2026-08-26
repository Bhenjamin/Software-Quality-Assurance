using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Infrastructure.Shared.Constants;

namespace StudyRoomBooking.Web.Pages.Admin;

public class AccessRulesModel : PageModel
{
    public void OnGet()
    {
        if (!IsAdmin())
            RedirectToPage("/Index");
    }

    private bool IsAdmin()
    {
        var role = HttpContext.Session.GetString("UserRole");
        return role == "Admin";
    }
}
