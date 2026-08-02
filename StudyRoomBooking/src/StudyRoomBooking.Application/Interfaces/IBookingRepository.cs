using StudyRoomBooking.Domain.Models;

namespace StudyRoomBooking.Application.Interfaces;

public interface IBookingRepository
{
    IEnumerable<Booking> GetAll();
    Booking? GetById(Guid bookingId);
    IEnumerable<Booking> GetByRoomId(Guid roomId);
    IEnumerable<Booking> GetByUserId(Guid userId);
    void Add(Booking booking);
    void Update(Booking booking);
}
