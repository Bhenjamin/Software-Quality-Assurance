using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Domain.Models;

/// <summary>
/// A bookable physical space. Rooms flagged as RequiresRestrictedAccess
/// (typically laboratories and design studios) are only bookable by
/// users whose role or academic programme is explicitly allowed.
/// </summary>
public class Room
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public RoomType Type { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When true, only users allowed by AllowedRoles / AllowedProgrammes
    /// (or an Administrator) may book this room.
    /// </summary>
    public bool RequiresRestrictedAccess { get; set; }

    /// <summary>Roles permitted to book this room when access is restricted.</summary>
    public List<UserRole> AllowedRoles { get; set; } = new();

    /// <summary>
    /// Academic programmes permitted to book this room when access is
    /// restricted (used for Student bookings of e.g. a design studio
    /// reserved for the Architecture programme). Empty means "no
    /// programme restriction" — role restriction still applies.
    /// </summary>
    public List<string> AllowedProgrammes { get; set; } = new();

    public override string ToString() =>
        $"{Name} ({Location}), capacity {Capacity}, {Type}";
}
