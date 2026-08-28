using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudyRoomBooking.Web.Pages;

public class LogoutModel : PageModel
{
    public IActionResult OnGet()
    {
        // Clear session
        HttpContext.Session.Clear();
        return RedirectToPage("Login");
    }
}
