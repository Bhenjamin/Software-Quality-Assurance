using StudyRoomBooking.Domain.Interfaces;

namespace StudyRoomBooking.Infrastructure.Repositories;

public class InMemoryUnitOfWork : IUnitOfWork
{
    public IBookingRepository Bookings { get; }
    public IRoomRepository Rooms { get; }
    public IUserRepository Users { get; }

    public InMemoryUnitOfWork()
    {
        Bookings = new InMemoryBookingRepository();
        Rooms = new InMemoryRoomRepository();
        Users = new InMemoryUserRepository();
    }

    public Task SaveChangesAsync()
    {
        // For in-memory implementation, no actual save needed
        return Task.CompletedTask;
    }
}
