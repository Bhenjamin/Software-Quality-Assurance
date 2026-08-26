using StudyRoomBooking.Infrastructure.Data;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Application.Services;

public class AdminService : IAdminService
{
    private readonly DataStore _dataStore;

    public AdminService(DataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public List<UserViewModel> GetAllUsers()
    {
        return _dataStore.Users
            .Select(u => MapToViewModel(u))
            .ToList();
    }

    public UserViewModel? GetUserById(int id)
    {
        var user = _dataStore.Users.FirstOrDefault(u => u.Id == id);
        return user != null ? MapToViewModel(user) : null;
    }

    public void CreateUser(UserViewModel user)
    {
        // Implementation would include password hashing
        var newUser = new Domain.Entities.User
        {
            Id = _dataStore.Users.Count > 0 ? _dataStore.Users.Max(u => u.Id) + 1 : 1,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = DateTime.UtcNow,
            PasswordHash = string.Empty // Set by auth service
        };

        _dataStore.Users.Add(newUser);
    }

    public void UpdateUserRole(int userId, int roleId)
    {
        var user = _dataStore.Users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            user.Role = (UserRole)roleId;
            user.UpdatedAt = DateTime.UtcNow;
        }
    }

    public void DisableUser(int userId)
    {
        var user = _dataStore.Users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
        }
    }

    public void EnableUser(int userId)
    {
        var user = _dataStore.Users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
        }
    }

    public List<BookingViewModel> GetAllBookings()
    {
        return _dataStore.Bookings
            .Select(b => new BookingViewModel
            {
                Id = b.Id,
                UserId = b.UserId,
                RoomId = b.RoomId,
                BookingDate = b.BookingDate,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                Status = b.Status,
                Notes = b.Notes
            })
            .ToList();
    }

    public void ApproveBooking(int bookingId)
    {
        var booking = _dataStore.Bookings.FirstOrDefault(b => b.Id == bookingId);
        if (booking != null)
        {
            booking.Status = BookingStatus.Confirmed;
            booking.UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RejectBooking(int bookingId)
    {
        var booking = _dataStore.Bookings.FirstOrDefault(b => b.Id == bookingId);
        if (booking != null)
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;
        }
    }

    private UserViewModel MapToViewModel(Domain.Entities.User user)
    {
        return new UserViewModel
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
