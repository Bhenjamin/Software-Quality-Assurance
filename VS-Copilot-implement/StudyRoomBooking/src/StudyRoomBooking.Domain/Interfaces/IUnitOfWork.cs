namespace StudyRoomBooking.Domain.Interfaces;

public interface IUnitOfWork
{
    IBookingRepository Bookings { get; }
    IRoomRepository Rooms { get; }
    IUserRepository Users { get; }
    Task SaveChangesAsync();
}
