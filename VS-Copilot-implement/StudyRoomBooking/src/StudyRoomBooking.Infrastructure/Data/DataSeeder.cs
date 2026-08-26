using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Infrastructure.Data;

public class DataSeeder
{
    private readonly DataStore _dataStore;

    public DataSeeder(DataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public void Seed()
    {
        if (_dataStore.Users.Count == 0)
            SeedUsers();

        if (_dataStore.Rooms.Count == 0)
            SeedRooms();

        if (_dataStore.AccessRules.Count == 0)
            SeedAccessRules();
    }

    private void SeedUsers()
    {
        // Password: student123 (hashed with BCrypt)
        var studentHash = BCrypt.Net.BCrypt.HashPassword("student123");
        // Password: staff123 (hashed with BCrypt)
        var staffHash = BCrypt.Net.BCrypt.HashPassword("staff123");
        // Password: admin123 (hashed with BCrypt)
        var adminHash = BCrypt.Net.BCrypt.HashPassword("admin123");

        _dataStore.Users.AddRange(new List<User>
        {
            new User
            {
                Id = 1,
                Email = "student1@university.edu",
                FullName = "John Doe",
                PasswordHash = studentHash,
                Role = UserRole.Student,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = 2,
                Email = "student2@university.edu",
                FullName = "Jane Smith",
                PasswordHash = studentHash,
                Role = UserRole.Student,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = 3,
                Email = "staff1@university.edu",
                FullName = "Dr. Robert Johnson",
                PasswordHash = staffHash,
                Role = UserRole.Staff,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = 4,
                Email = "admin@university.edu",
                FullName = "Admin User",
                PasswordHash = adminHash,
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        });
    }

    private void SeedRooms()
    {
        _dataStore.Rooms.AddRange(new List<Room>
        {
            new Room
            {
                Id = 1,
                RoomCode = "A101",
                RoomName = "Study Room A101",
                Location = "Building A, Floor 1",
                Capacity = 4,
                RoomType = RoomType.Standard,
                Description = "Small study room for 4 people",
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            },
            new Room
            {
                Id = 2,
                RoomCode = "A102",
                RoomName = "Study Room A102",
                Location = "Building A, Floor 1",
                Capacity = 6,
                RoomType = RoomType.Standard,
                Description = "Medium study room for 6 people",
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            },
            new Room
            {
                Id = 3,
                RoomCode = "B201",
                RoomName = "Lab Room B201",
                Location = "Building B, Floor 2",
                Capacity = 20,
                RoomType = RoomType.Lab,
                Description = "Computer lab with 20 workstations",
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            },
            new Room
            {
                Id = 4,
                RoomCode = "B202",
                RoomName = "Seminar Room B202",
                Location = "Building B, Floor 2",
                Capacity = 30,
                RoomType = RoomType.Seminar,
                Description = "Seminar room with projector and whiteboard",
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            },
            new Room
            {
                Id = 5,
                RoomCode = "C301",
                RoomName = "Specialized Room C301",
                Location = "Building C, Floor 3",
                Capacity = 15,
                RoomType = RoomType.Specialized,
                Description = "Specialized room for research",
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            }
        });
    }

    private void SeedAccessRules()
    {
        _dataStore.AccessRules.AddRange(new List<AccessRule>
        {
            new AccessRule
            {
                Id = 1,
                RoomId = 1,
                AllowedRole = UserRole.Student,
                AccessLevel = AccessLevel.StudentsOnly,
                Description = "Study Room A101 - Students only",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new AccessRule
            {
                Id = 2,
                RoomId = 3,
                AllowedRole = UserRole.Staff,
                AccessLevel = AccessLevel.StaffOnly,
                Description = "Lab Room B201 - Staff supervision required",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new AccessRule
            {
                Id = 3,
                RoomId = 5,
                AllowedRole = UserRole.Staff,
                AccessLevel = AccessLevel.StaffOnly,
                Description = "Specialized Room C301 - Staff and Admin only",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        });
    }
}
