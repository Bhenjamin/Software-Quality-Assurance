using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Domain.Enums;
using StudyRoomBooking.Domain.Models;

namespace StudyRoomBooking.Application.Services;

/// <summary>
/// Centralises role-based access control so that booking rules for
/// restricted rooms and administrative actions live in exactly one place.
/// </summary>
public class AccessControlService : IAccessControlService
{
    public bool CanAccessRoom(User user, Room room)
    {
        if (!room.RequiresRestrictedAccess)
        {
            return true;
        }

        // Administrators can always access restricted rooms (override capability).
        if (user.Role == UserRole.Administrator)
        {
            return true;
        }

        // Role must be on the room's allow-list, if one is defined.
        var roleAllowed = room.AllowedRoles.Count == 0 || room.AllowedRoles.Contains(user.Role);
        if (!roleAllowed)
        {
            return false;
        }

        // Students additionally need to belong to an allowed programme,
        // when the room restricts by programme.
        if (user.Role == UserRole.Student && room.AllowedProgrammes.Count > 0)
        {
            return user.Programme is not null && room.AllowedProgrammes.Contains(user.Programme);
        }

        return true;
    }

    public bool CanPerformAdminAction(User user) => user.Role == UserRole.Administrator;

    public bool CanManageBooking(User user, Booking booking)
    {
        if (user.Role == UserRole.Administrator)
        {
            return true;
        }

        return booking.UserId == user.Id;
    }
}
