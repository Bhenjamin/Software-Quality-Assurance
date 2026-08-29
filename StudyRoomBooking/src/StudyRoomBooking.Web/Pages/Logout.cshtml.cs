using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudyRoomBooking.Web.Pages;

public class LogoutModel : PageModel
{
    public IActionResult OnPost()
    {
        // Clear all session data
        HttpContext.Session.Clear();

        // Remove the session cookie
        Response.Cookies.Delete(".AspNetCore.Session");

        return RedirectToPage("Login");
    }

    public IActionResult OnGet()
    {
        // Handle direct GET requests (old bookmarks, etc.)
        // Clear all session data
        HttpContext.Session.Clear();

        // Remove the session cookie
        Response.Cookies.Delete(".AspNetCore.Session");

        return RedirectToPage("Login");
    }
}
