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

    public async Task<(bool Success, int UserId, string UserName, string Role, string Message)> SignUpAsync(string email, string password, string name, string role, string? major)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(name))
        {
            return (false, 0, "", "", "Email, password, and name are required.");
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            return (false, 0, "", "", "Role is required.");
        }

        // Validate role is Student or Staff
        if (role != "Student" && role != "Staff")
        {
            return (false, 0, "", "", "Invalid role. Role must be either Student or Staff.");
        }

        // For students, major is required
        if (role == "Student" && string.IsNullOrWhiteSpace(major))
        {
            return (false, 0, "", "", "Major is required for student registration.");
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();

        // Check if user already exists
        var existingUser = await _userService.GetUserByUserIdAsync(normalizedEmail);
        if (existingUser != null)
        {
            return (false, 0, "", "", "An account with this email already exists.");
        }

        // Create Supabase auth account
        var authResult = await _supabaseAuthClient.SignUpAsync(normalizedEmail, password);
        if (!authResult.Success)
        {
            return (false, 0, "", "", authResult.Error ?? "Failed to create authentication account. Please try again.");
        }

        // Create user in database
        try
        {
            var userRole = role == "Student" ? Domain.Enums.UserRole.Student : Domain.Enums.UserRole.Staff;

            var newUser = new Domain.Entities.User
            {
                UserId = normalizedEmail,
                Name = name,
                Email = normalizedEmail,
                Role = userRole,
                Major = userRole == Domain.Enums.UserRole.Student ? ParseMajor(major) : null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _userService.CreateUserAsync(newUser);
            return (true, createdUser.Id, createdUser.UserId, createdUser.Role.ToString(), "Sign up successful. You can now log in with your credentials.");
        }
        catch (Exception ex)
        {
            return (false, 0, "", "", $"Failed to create user account: {ex.Message}");
        }
    }

    private Domain.Enums.StudentMajor? ParseMajor(string? majorString)
    {
        if (string.IsNullOrWhiteSpace(majorString))
            return null;

        return majorString switch
        {
            "Engineering" => Domain.Enums.StudentMajor.Engineering,
            "Business" => Domain.Enums.StudentMajor.Business,
            "Science" => Domain.Enums.StudentMajor.Science,
            "Arts" => Domain.Enums.StudentMajor.Arts,
            _ => null
        };
    }

    public async Task LogoutAsync()
    {
        await Task.CompletedTask;
    }
}
