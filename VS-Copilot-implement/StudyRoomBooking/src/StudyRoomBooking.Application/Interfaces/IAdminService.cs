using StudyRoomBooking.Application.ViewModels;

namespace StudyRoomBooking.Application.Interfaces;

public interface IAdminService
{
    List<UserViewModel> GetAllUsers();
    UserViewModel? GetUserById(int id);
    void CreateUser(UserViewModel user);
    void UpdateUserRole(int userId, int roleId);
    void DisableUser(int userId);
    void EnableUser(int userId);
    List<BookingViewModel> GetAllBookings();
    void ApproveBooking(int bookingId);
    void RejectBooking(int bookingId);
}
