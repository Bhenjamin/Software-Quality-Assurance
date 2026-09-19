namespace StudyRoomBooking.Domain.Interfaces;

public interface IUnitOfWork
{
    IBookingRepository Bookings { get; }
    IRoomRepository Rooms { get; }
    IUserRepository Users { get; }
    IRoomMajorRestrictionRepository RoomMajorRestrictions { get; }
    IAccessRuleRepository AccessRules { get; }
    IBookingOverrideRepository BookingOverrides { get; }
    Task SaveChangesAsync();
}
