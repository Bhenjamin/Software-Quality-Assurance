using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Domain.Enums;
using StudyRoomBooking.Domain.Models;
using StudyRoomBooking.Infrastructure.Repositories;
using StudyRoomBooking.Infrastructure.SeedData;

namespace StudyRoomBooking.Tests;

/// <summary>
/// Builds a fresh, isolated set of repositories and services for each
/// test so tests cannot leak state into one another. Also exposes
/// convenience accessors for commonly used seeded rooms/users.
/// </summary>
public class TestFixture
{
    public IRoomRepository RoomRepository { get; }
    public IUserRepository UserRepository { get; }
    public IBookingRepository BookingRepository { get; }
    public IAccessControlService AccessControlService { get; }
    public IRoomSearchService RoomSearchService { get; }
    public IBookingService BookingService { get; }

    public TestFixture()
    {
        RoomRepository = new InMemoryRoomRepository();
        UserRepository = new InMemoryUserRepository();
        BookingRepository = new InMemoryBookingRepository();
        SampleDataSeeder.Seed(RoomRepository, UserRepository);

        AccessControlService = new AccessControlService();
        RoomSearchService = new RoomSearchService(RoomRepository, BookingRepository);
        BookingService = new BookingService(RoomRepository, UserRepository, BookingRepository, AccessControlService);
    }

    public User StudentInProgramme(string programme) =>
        UserRepository.GetAll().First(u => u.Role == UserRole.Student && u.Programme == programme);

    public User Staff() => UserRepository.GetAll().First(u => u.Role == UserRole.AcademicStaff);

    public User Admin() => UserRepository.GetAll().First(u => u.Role == UserRole.Administrator);

    public Room RoomOfType(RoomType type) => RoomRepository.GetAll().First(r => r.Type == type);
}
