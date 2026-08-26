using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Infrastructure.Localization;
using StudyRoomBooking.Infrastructure.Shared.Constants;

namespace StudyRoomBooking.Web.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly IAuthenticationService _authService;
    private readonly ILocalizationService _localizationService;

    [BindProperty]
    public LoginViewModel LoginInput { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public LoginModel(IAuthenticationService authService, ILocalizationService localizationService)
    {
        _authService = authService;
        _localizationService = localizationService;
    }

    public void OnGet()
    {
        if (User.Identity?.IsAuthenticated ?? false)
            RedirectToPage("/Index");
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = _authService.Authenticate(LoginInput.Email, LoginInput.Password);
        if (user == null)
        {
            ErrorMessage = _localizationService.GetString("invalid_credentials");
            return Page();
        }

        // Store user in session
        HttpContext.Session.SetString(AppConstants.UserSessionKey, user.Id.ToString());
        HttpContext.Session.SetString("UserEmail", user.Email);
        HttpContext.Session.SetString("UserRole", user.Role.ToString());

        return RedirectToPage("/Index");
    }
}
