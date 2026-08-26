using StudyRoomBooking.Domain.Entities;

namespace StudyRoomBooking.Infrastructure.Data;

public class DataStore
{
    public List<User> Users { get; set; } = new();
    public List<Room> Rooms { get; set; } = new();
    public List<Booking> Bookings { get; set; } = new();
    public List<BookingRecurrence> BookingRecurrences { get; set; } = new();
    public List<AccessRule> AccessRules { get; set; } = new();
}
