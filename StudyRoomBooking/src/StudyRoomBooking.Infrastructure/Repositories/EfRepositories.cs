using Microsoft.EntityFrameworkCore;
using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Domain.Enums;
using StudyRoomBooking.Domain.Interfaces;
using StudyRoomBooking.Infrastructure.Persistence;

namespace StudyRoomBooking.Infrastructure.Repositories;

public sealed class EfBookingRepository(StudyRoomBookingDbContext db) : IBookingRepository
{
    public Task<Booking?> GetByIdAsync(int id) => db.Bookings.FindAsync(id).AsTask();
    public Task<List<Booking>> GetAllAsync() => db.Bookings.AsNoTracking().ToListAsync();
    public Task<List<Booking>> GetByUserIdAsync(int userId) => db.Bookings.AsNoTracking().Where(booking => booking.UserId == userId).ToListAsync();
    public Task<List<Booking>> GetByRoomIdAsync(int roomId) => db.Bookings.AsNoTracking().Where(booking => booking.RoomId == roomId).ToListAsync();
    public Task<List<Booking>> GetByDateRangeAsync(DateTime startDate, DateTime endDate) => db.Bookings.AsNoTracking().Where(booking => booking.BookingDate.Date >= startDate.Date && booking.BookingDate.Date <= endDate.Date).ToListAsync();
    public Task AddAsync(Booking booking) => db.Bookings.AddAsync(booking).AsTask();
    public Task UpdateAsync(Booking booking) { db.Bookings.Update(booking); return Task.CompletedTask; }
    public async Task DeleteAsync(int id) { var booking = await db.Bookings.FindAsync(id); if (booking is not null) db.Bookings.Remove(booking); }
}

public sealed class EfRoomRepository(StudyRoomBookingDbContext db) : IRoomRepository
{
    public Task<Room?> GetByIdAsync(int id) => db.Rooms.FindAsync(id).AsTask();
    public Task<List<Room>> GetAllAsync() => db.Rooms.AsNoTracking().ToListAsync();
    public Task<Room?> GetByCodeAsync(string code) => db.Rooms.AsNoTracking().FirstOrDefaultAsync(room => room.Code == code);
    public Task AddAsync(Room room) => db.Rooms.AddAsync(room).AsTask();
    public Task UpdateAsync(Room room) { db.Rooms.Update(room); return Task.CompletedTask; }
    public async Task DeleteAsync(int id) { var room = await db.Rooms.FindAsync(id); if (room is not null) db.Rooms.Remove(room); }
}

public sealed class EfUserRepository(StudyRoomBookingDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(int id) => db.Users.FindAsync(id).AsTask();
    public Task<List<User>> GetAllAsync() => db.Users.AsNoTracking().ToListAsync();
    public Task<User?> GetByUserIdAsync(string userId) => db.Users.AsNoTracking().FirstOrDefaultAsync(user => user.UserId == userId);
    public Task AddAsync(User user) => db.Users.AddAsync(user).AsTask();
    public Task UpdateAsync(User user) { db.Users.Update(user); return Task.CompletedTask; }
    public async Task DeleteAsync(int id) { var user = await db.Users.FindAsync(id); if (user is not null) db.Users.Remove(user); }
}

public sealed class EfRoomMajorRestrictionRepository(StudyRoomBookingDbContext db) : IRoomMajorRestrictionRepository
{
    public Task<RoomMajorRestriction?> GetByIdAsync(int id) => db.RoomMajorRestrictions.FindAsync(id).AsTask();
    public Task<List<RoomMajorRestriction>> GetAllAsync() => db.RoomMajorRestrictions.AsNoTracking().ToListAsync();
    public Task<List<RoomMajorRestriction>> GetByRoomIdAsync(int roomId) => db.RoomMajorRestrictions.AsNoTracking().Where(restriction => restriction.RoomId == roomId && restriction.IsActive).ToListAsync();
    public Task<List<StudentMajor>> GetAllowedMajorsForRoomAsync(int roomId) => db.RoomMajorRestrictions.AsNoTracking().Where(restriction => restriction.RoomId == roomId && restriction.IsActive).Select(restriction => restriction.Major).Distinct().ToListAsync();
    public Task AddAsync(RoomMajorRestriction restriction) => db.RoomMajorRestrictions.AddAsync(restriction).AsTask();
    public Task UpdateAsync(RoomMajorRestriction restriction) { db.RoomMajorRestrictions.Update(restriction); return Task.CompletedTask; }
    public async Task DeleteAsync(int id) { var restriction = await db.RoomMajorRestrictions.FindAsync(id); if (restriction is not null) db.RoomMajorRestrictions.Remove(restriction); }
}
