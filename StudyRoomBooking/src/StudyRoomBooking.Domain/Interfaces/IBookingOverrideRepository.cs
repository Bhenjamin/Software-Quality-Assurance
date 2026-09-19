using StudyRoomBooking.Domain.Entities;

namespace StudyRoomBooking.Domain.Interfaces;

public interface IBookingOverrideRepository
{
    Task<BookingOverride?> GetByIdAsync(int id);
    Task<List<BookingOverride>> GetAllAsync();
    Task AddAsync(BookingOverride bookingOverride);
    Task UpdateAsync(BookingOverride bookingOverride);
    Task DeleteAsync(int id);
}
