using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Domain.Models;

namespace StudyRoomBooking.Infrastructure.Repositories;

public class InMemoryBookingRepository : IBookingRepository
{
    private readonly List<Booking> _bookings = new();

    public IEnumerable<Booking> GetAll() => _bookings.ToList();

    public Booking? GetById(Guid bookingId) => _bookings.FirstOrDefault(b => b.Id == bookingId);

    public IEnumerable<Booking> GetByRoomId(Guid roomId) =>
        _bookings.Where(b => b.RoomId == roomId).ToList();

    public IEnumerable<Booking> GetByUserId(Guid userId) =>
        _bookings.Where(b => b.UserId == userId).ToList();

    public void Add(Booking booking) => _bookings.Add(booking);

    public void Update(Booking booking)
    {
        var index = _bookings.FindIndex(b => b.Id == booking.Id);
        if (index >= 0)
        {
            _bookings[index] = booking;
        }
    }
}
