using StudyRoomBooking.Infrastructure.Data;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Domain.Entities;

namespace StudyRoomBooking.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly DataStore _dataStore;

    public AuthenticationService(DataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public User? Authenticate(string email, string password)
    {
        var user = _dataStore.Users.FirstOrDefault(u => u.Email == email && u.IsActive);

        if (user != null && ValidatePassword(password, user.PasswordHash))
            return user;

        return null;
    }

    public void Register(string email, string fullName, string password)
    {
        var existingUser = _dataStore.Users.FirstOrDefault(u => u.Email == email);
        if (existingUser != null)
            throw new InvalidOperationException("User with this email already exists.");

        var newUser = new User
        {
            Id = _dataStore.Users.Count > 0 ? _dataStore.Users.Max(u => u.Id) + 1 : 1,
            Email = email,
            FullName = fullName,
            PasswordHash = HashPassword(password),
            Role = Domain.Enums.UserRole.Student,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dataStore.Users.Add(newUser);
    }

    public bool ValidatePassword(string password, string hash)
    {
        // TODO: Implement proper password hashing (BCrypt, PBKDF2, etc.)
        // For prototype, simple comparison
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    private string HashPassword(string password)
    {
        // TODO: Implement proper password hashing (BCrypt, PBKDF2, etc.)
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
