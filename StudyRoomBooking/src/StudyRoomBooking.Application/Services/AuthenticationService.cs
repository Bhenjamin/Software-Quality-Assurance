namespace StudyRoomBooking.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserService _userService;
    private readonly SupabaseAuthClient _supabaseAuthClient;

    public AuthenticationService(IUserService userService, SupabaseAuthClient supabaseAuthClient)
    {
        _userService = userService;
        _supabaseAuthClient = supabaseAuthClient;
    }

    public async Task<(bool Success, int UserId, string UserName, string Role, string Message)> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return (false, 0, "", "", "Email and password are required.");
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var authResult = await _supabaseAuthClient.SignInAsync(normalizedEmail, password);
        if (!authResult.Success)
        {
            return (false, 0, "", "", authResult.Error ?? "Invalid email or password.");
        }

        var user = await _userService.GetUserByUserIdAsync(normalizedEmail);

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
