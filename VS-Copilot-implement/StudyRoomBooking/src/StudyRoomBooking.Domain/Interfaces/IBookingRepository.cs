using StudyRoomBooking.Domain.Entities;

namespace StudyRoomBooking.Domain.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(int id);
    Task<List<Booking>> GetAllAsync();
    Task<List<Booking>> GetByUserIdAsync(int userId);
    Task<List<Booking>> GetByRoomIdAsync(int roomId);
    Task<List<Booking>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task AddAsync(Booking booking);
    Task UpdateAsync(Booking booking);
    Task DeleteAsync(int id);
}
