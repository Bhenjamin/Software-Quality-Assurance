using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Domain.Enums;
using StudyRoomBooking.Domain.Models;

namespace StudyRoomBooking.Infrastructure.SeedData;

/// <summary>
/// Populates the in-memory repositories with representative sample data
/// so the prototype can be searched, booked against, and demonstrated
/// without a real university database (per the assessment scope).
/// </summary>
public static class SampleDataSeeder
{
    public static void Seed(IRoomRepository roomRepository, IUserRepository userRepository)
    {
        var rooms = new List<Room>
        {
            new() { Name = "Study Pod 1", Location = "Library, Level 1", Capacity = 4, Type = RoomType.StudyPod },
            new() { Name = "Study Pod 2", Location = "Library, Level 1", Capacity = 4, Type = RoomType.StudyPod },
            new() { Name = "Meeting Room A", Location = "Admin Building", Capacity = 8, Type = RoomType.MeetingRoom },
            new() { Name = "Classroom 101", Location = "Block B", Capacity = 40, Type = RoomType.Classroom },
            new()
            {
                Name = "Software Lab 1", Location = "Block C, Level 2", Capacity = 30, Type = RoomType.Laboratory,
                RequiresRestrictedAccess = true,
                AllowedRoles = new List<UserRole> { UserRole.AcademicStaff, UserRole.Student },
                AllowedProgrammes = new List<string> { "Computer Science", "Software Engineering" }
            },
            new()
            {
                Name = "Design Studio 1", Location = "Block D, Level 1", Capacity = 20, Type = RoomType.DesignStudio,
                RequiresRestrictedAccess = true,
                AllowedRoles = new List<UserRole> { UserRole.AcademicStaff, UserRole.Student },
                AllowedProgrammes = new List<string> { "Graphic Design", "Architecture" }
            }
        };

        foreach (var room in rooms)
        {
            roomRepository.Add(room);
        }

        var users = new List<User>
        {
            new() { FullName = "An Nguyen", Email = "an.nguyen@student.edu.vn", Role = UserRole.Student, Programme = "Computer Science" },
            new() { FullName = "Binh Tran", Email = "binh.tran@student.edu.vn", Role = UserRole.Student, Programme = "Graphic Design" },
            new() { FullName = "Dr. Chi Le", Email = "chi.le@staff.edu.vn", Role = UserRole.AcademicStaff },
            new() { FullName = "Duc Pham", Email = "duc.pham@admin.edu.vn", Role = UserRole.Administrator },
            new() { FullName = "Ha Vo", Email = "ha.vo@management.edu.vn", Role = UserRole.ManagementUser }
        };

        foreach (var user in users)
        {
            userRepository.Add(user);
        }
    }
}
