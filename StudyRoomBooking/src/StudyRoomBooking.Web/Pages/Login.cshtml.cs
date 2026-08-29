using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Services;

namespace StudyRoomBooking.Web.Pages;

public class LoginModel : PageModel
{
    private readonly IAuthenticationService _authenticationService;

    [BindProperty]
    public string Email { get; set; } = "";

    [BindProperty]
    public string Password { get; set; } = "";

    public string ErrorMessage { get; set; } = "";

    public LoginModel(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var (success, userId, userName, role, message) = await _authenticationService.LoginAsync(Email, Password);

        if (success)
        {
            // Store user info in session
            HttpContext.Session.SetString("UserId", userId.ToString());
            HttpContext.Session.SetString("CurrentUser", userName);
            HttpContext.Session.SetString("CurrentUserRole", role);
            HttpContext.Session.SetString("CurrentUserEmail", Email);

            return RedirectToPage("/Index");
        }

        ErrorMessage = message;
        Password = ""; // Clear password for security
        return Page();
    }
}
