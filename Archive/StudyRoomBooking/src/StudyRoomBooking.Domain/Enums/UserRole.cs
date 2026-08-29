namespace StudyRoomBooking.Domain.Enums;

/// <summary>
/// The four user roles described in the project brief. Ordered roughly
/// by increasing system privilege for readability only — privilege is
/// still checked explicitly wherever it matters.
/// </summary>
public enum UserRole
{
    Student,
    AcademicStaff,
    Administrator,
    ManagementUser
}
