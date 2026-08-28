using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Domain.Interfaces;

namespace StudyRoomBooking.Infrastructure.Repositories;

public class InMemoryBookingRepository : IBookingRepository
{
    private readonly List<Booking> _bookings = new();
    private int _nextId = 1;

    public Task<Booking?> GetByIdAsync(int id)
    {
        return Task.FromResult(_bookings.FirstOrDefault(b => b.Id == id));
    }

    public Task<List<Booking>> GetAllAsync()
    {
        return Task.FromResult(_bookings.ToList());
    }

    public Task<List<Booking>> GetByUserIdAsync(int userId)
    {
        return Task.FromResult(_bookings.Where(b => b.UserId == userId).ToList());
    }

    public Task<List<Booking>> GetByRoomIdAsync(int roomId)
    {
        return Task.FromResult(_bookings.Where(b => b.RoomId == roomId).ToList());
    }

    public Task<List<Booking>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return Task.FromResult(_bookings.Where(b =>
            b.BookingDate.Date >= startDate.Date &&
            b.BookingDate.Date <= endDate.Date
        ).ToList());
    }

    public Task AddAsync(Booking booking)
    {
        booking.Id = _nextId++;
        _bookings.Add(booking);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Booking booking)
    {
        var existing = _bookings.FirstOrDefault(b => b.Id == booking.Id);
        if (existing != null)
        {
            _bookings.Remove(existing);
            _bookings.Add(booking);
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        var booking = _bookings.FirstOrDefault(b => b.Id == id);
        if (booking != null)
        {
            _bookings.Remove(booking);
        }
        return Task.CompletedTask;
    }
}
