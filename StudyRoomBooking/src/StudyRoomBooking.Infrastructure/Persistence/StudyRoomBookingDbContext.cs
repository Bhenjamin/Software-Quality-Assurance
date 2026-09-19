using Microsoft.EntityFrameworkCore;
using StudyRoomBooking.Domain.Entities;

namespace StudyRoomBooking.Infrastructure.Persistence;

public class StudyRoomBookingDbContext : DbContext
{
    public StudyRoomBookingDbContext(DbContextOptions<StudyRoomBookingDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<RoomMajorRestriction> RoomMajorRestrictions => Set<RoomMajorRestriction>();
    public DbSet<AccessRule> AccessRules => Set<AccessRule>();
    public DbSet<BookingOverride> BookingOverrides => Set<BookingOverride>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(user => user.UserId).IsUnique();
        modelBuilder.Entity<Room>().HasIndex(room => room.Code).IsUnique();
        modelBuilder.Entity<Booking>().HasIndex(booking => new { booking.RoomId, booking.BookingDate });
        modelBuilder.Entity<RoomMajorRestriction>().HasIndex(restriction => new { restriction.RoomId, restriction.Major }).IsUnique();

        modelBuilder.Entity<User>().Property(user => user.Role).HasConversion<int>();
        modelBuilder.Entity<User>().Property(user => user.Major).HasConversion<int?>();
        modelBuilder.Entity<Room>().Property(room => room.Type).HasConversion<int>();
        modelBuilder.Entity<Booking>().Property(booking => booking.Status).HasConversion<int>();
        modelBuilder.Entity<Booking>().Property(booking => booking.RecurrencePattern).HasConversion<int>();
        modelBuilder.Entity<RoomMajorRestriction>().Property(restriction => restriction.Major).HasConversion<int>();
        modelBuilder.Entity<AccessRule>().HasOne<Room>().WithMany().HasForeignKey(rule => rule.RoomId);
        modelBuilder.Entity<BookingOverride>().HasOne<Booking>().WithMany().HasForeignKey(bookingOverride => bookingOverride.BookingId);
        modelBuilder.Entity<BookingOverride>().HasOne<User>().WithMany().HasForeignKey(bookingOverride => bookingOverride.AdminId);

        modelBuilder.Entity<Booking>().HasOne<Room>().WithMany().HasForeignKey(booking => booking.RoomId);
        modelBuilder.Entity<Booking>().HasOne<User>().WithMany().HasForeignKey(booking => booking.UserId);
        modelBuilder.Entity<RoomMajorRestriction>().HasOne<Room>().WithMany().HasForeignKey(restriction => restriction.RoomId);
    }
}