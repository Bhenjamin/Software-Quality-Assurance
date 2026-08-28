using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty; // Student ID or Staff ID
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
