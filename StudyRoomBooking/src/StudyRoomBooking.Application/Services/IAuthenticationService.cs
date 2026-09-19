namespace StudyRoomBooking.Application.Services;

public interface IAuthenticationService
{
    Task<(bool Success, int UserId, string UserName, string Role, string Message)> LoginAsync(string email, string password);
    Task<(bool Success, int UserId, string UserName, string Role, string Message)> SignUpAsync(string email, string password, string name, string role, string? major);
    Task LogoutAsync();
}
