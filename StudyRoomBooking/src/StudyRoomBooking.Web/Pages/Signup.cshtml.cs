using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Web.Pages;

public class SignupModel : PageModel
{
    private readonly IAuthenticationService _authenticationService;

    [BindProperty]
    public string Email { get; set; } = "";

    [BindProperty]
    public string Password { get; set; } = "";

    [BindProperty]
    public string ConfirmPassword { get; set; } = "";

    [BindProperty]
    public string Name { get; set; } = "";

    [BindProperty]
    public string Role { get; set; } = "Student";

    [BindProperty]
    public string Major { get; set; } = "";

    public List<string> Roles { get; set; } = new();
    public List<string> Majors { get; set; } = new();

    public string ErrorMessage { get; set; } = "";
    public string SuccessMessage { get; set; } = "";

    public SignupModel(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public void OnGet()
    {
        InitializeDropdowns();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        InitializeDropdowns();

        // Validate all required fields
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) || 
            string.IsNullOrWhiteSpace(ConfirmPassword) || string.IsNullOrWhiteSpace(Name) || 
            string.IsNullOrWhiteSpace(Role))
        {
            ErrorMessage = "All fields are required.";
            return Page();
        }

        // Validate email format
        if (!IsValidEmail(Email))
        {
            ErrorMessage = "Please enter a valid email address.";
            return Page();
        }

        // Validate password length
        if (Password.Length < 6)
        {
            ErrorMessage = "Password must be at least 6 characters long.";
            return Page();
        }

        // Validate password confirmation
        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            ConfirmPassword = "";
            Password = "";
            return Page();
        }

        // Validate role
        if (Role != "Student" && Role != "Staff")
        {
            ErrorMessage = "Invalid role selected.";
            return Page();
        }

        // For students, major is required
        if (Role == "Student" && string.IsNullOrWhiteSpace(Major))
        {
            ErrorMessage = "Please select a major.";
            return Page();
        }

        // Attempt signup
        var (success, userId, userName, role, message) = await _authenticationService.SignUpAsync(
            Email.Trim(), Password, Name.Trim(), Role, Role == "Student" ? Major : null);

        if (success)
        {
            SuccessMessage = message;
            // Clear form
            Email = "";
            Password = "";
            ConfirmPassword = "";
            Name = "";
            Role = "Student";
            Major = "";

            // Redirect to login page after a brief delay message display
            return RedirectToPage("/Login");
        }

        ErrorMessage = message;
        // Clear sensitive fields
        Password = "";
        ConfirmPassword = "";
        return Page();
    }

    private void InitializeDropdowns()
    {
        Roles = new List<string> { "Student", "Staff" };
        Majors = new List<string> 
        { 
            "Engineering", 
            "Business", 
            "Science", 
            "Arts"
        };
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
