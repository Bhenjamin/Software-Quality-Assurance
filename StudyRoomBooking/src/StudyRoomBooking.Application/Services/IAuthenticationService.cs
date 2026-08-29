namespace StudyRoomBooking.Application.Services;

public interface IAuthenticationService
{
    Task<(bool Success, int UserId, string UserName, string Role, string Message)> LoginAsync(string email, string password);
    Task LogoutAsync();
}
