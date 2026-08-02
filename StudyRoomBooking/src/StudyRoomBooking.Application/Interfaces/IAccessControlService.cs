using StudyRoomBooking.Domain.Models;

namespace StudyRoomBooking.Application.Interfaces;

public interface IAccessControlService
{
    /// <summary>
    /// Whether the given user is permitted to book the given room,
    /// accounting for restricted-access rooms (labs, design studios).
    /// </summary>
    bool CanAccessRoom(User user, Room room);

    /// <summary>Whether the given user may perform administrative actions.</summary>
    bool CanPerformAdminAction(User user);

    /// <summary>Whether the given user may modify or cancel someone else's booking.</summary>
    bool CanManageBooking(User user, Booking booking);
}
