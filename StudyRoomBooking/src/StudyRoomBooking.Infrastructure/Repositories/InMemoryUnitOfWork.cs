using StudyRoomBooking.Domain.Interfaces;

namespace StudyRoomBooking.Infrastructure.Repositories;

public class InMemoryUnitOfWork : IUnitOfWork
{
    public IBookingRepository Bookings { get; }
    public IRoomRepository Rooms { get; }
    public IUserRepository Users { get; }
    public IRoomMajorRestrictionRepository RoomMajorRestrictions { get; }
    public IAccessRuleRepository AccessRules { get; }
    public IBookingOverrideRepository BookingOverrides { get; }

    public InMemoryUnitOfWork()
    {
        Bookings = new InMemoryBookingRepository();
        Rooms = new InMemoryRoomRepository();
        Users = new InMemoryUserRepository();
        RoomMajorRestrictions = new InMemoryRoomMajorRestrictionRepository();
        AccessRules = new InMemoryAccessRuleRepository();
        BookingOverrides = new InMemoryBookingOverrideRepository();
    }

    public Task SaveChangesAsync()
    {
        // For in-memory implementation, no actual save needed
        return Task.CompletedTask;
    }
}
