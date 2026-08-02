using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Domain.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    /// <summary>Academic programme, relevant for Student access checks. Null for staff.</summary>
    public string? Programme { get; set; }

    public override string ToString() => $"{FullName} ({Role})";
}
