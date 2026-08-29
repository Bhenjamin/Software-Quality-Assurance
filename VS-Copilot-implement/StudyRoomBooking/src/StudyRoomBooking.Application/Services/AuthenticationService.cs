namespace StudyRoomBooking.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserService _userService;

    // Demo credentials - in a real app, these would be in a database with hashed passwords
    private static readonly Dictionary<string, (string Password, string Role)> DemoAccounts = new()
    {
        { "student1@university.edu", ("password123", "Student") },
        { "student2@university.edu", ("password123", "Student") },
        { "staff@university.edu", ("password123", "Staff") },
        { "admin@university.edu", ("password123", "Admin") }
    };

    public AuthenticationService(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<(bool Success, int UserId, string UserName, string Role, string Message)> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return (false, 0, "", "", "Email and password are required.");
        }

        // Check demo credentials
        if (!DemoAccounts.TryGetValue(email.ToLower(), out var account))
        {
            return (false, 0, "", "", "Invalid email or password.");
        }

        if (account.Password != password)
        {
            return (false, 0, "", "", "Invalid email or password.");
        }

        // Get user from database
        var user = await _userService.GetUserByUserIdAsync(email.ToLower());

        if (user == null)
        {
            return (false, 0, "", "", "User not found. Please contact administrator.");
        }

        return (true, user.Id, user.UserId, user.Role.ToString(), "Login successful.");
    }

    public async Task LogoutAsync()
    {
        await Task.CompletedTask;
    }
}
