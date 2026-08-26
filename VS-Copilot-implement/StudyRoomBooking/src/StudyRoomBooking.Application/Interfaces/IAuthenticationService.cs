using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Domain.Entities;

namespace StudyRoomBooking.Application.Interfaces;

public interface IAuthenticationService
{
    User? Authenticate(string email, string password);
    void Register(string email, string fullName, string password);
    bool ValidatePassword(string password, string hash);
}
