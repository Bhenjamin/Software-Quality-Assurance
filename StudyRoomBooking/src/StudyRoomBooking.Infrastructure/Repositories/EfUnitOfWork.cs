using StudyRoomBooking.Domain.Interfaces;
using StudyRoomBooking.Infrastructure.Persistence;

namespace StudyRoomBooking.Infrastructure.Repositories;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly StudyRoomBookingDbContext _db;

    public EfUnitOfWork(StudyRoomBookingDbContext db)
    {
        _db = db;
        Bookings = new EfBookingRepository(db);
        Rooms = new EfRoomRepository(db);
        Users = new EfUserRepository(db);
        RoomMajorRestrictions = new EfRoomMajorRestrictionRepository(db);
    }

    public IBookingRepository Bookings { get; }
    public IRoomRepository Rooms { get; }
    public IUserRepository Users { get; }
    public IRoomMajorRestrictionRepository RoomMajorRestrictions { get; }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}