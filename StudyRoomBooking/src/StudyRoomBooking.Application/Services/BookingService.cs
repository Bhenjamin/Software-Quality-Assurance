using StudyRoomBooking.Application.DTOs;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Domain.Enums;
using StudyRoomBooking.Domain.Models;

namespace StudyRoomBooking.Application.Services;

/// <summary>
/// Coordinates booking creation, modification and cancellation. All
/// validation (room/user existence, access control, time sanity,
/// double-booking prevention) happens here so that every caller — a
/// future web API, the console demo, or tests — gets the same rules.
/// </summary>
public class BookingService : IBookingService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IAccessControlService _accessControlService;

    public BookingService(
        IRoomRepository roomRepository,
        IUserRepository userRepository,
        IBookingRepository bookingRepository,
        IAccessControlService accessControlService)
    {
        _roomRepository = roomRepository;
        _userRepository = userRepository;
        _bookingRepository = bookingRepository;
        _accessControlService = accessControlService;
    }

    public BookingResult CreateBooking(BookingRequest request)
    {
        var user = _userRepository.GetById(request.UserId);
        if (user is null)
        {
            return BookingResult.Fail("USER_NOT_FOUND", "The requesting user does not exist.");
        }

        var room = _roomRepository.GetById(request.RoomId);
        if (room is null || !room.IsActive)
        {
            return BookingResult.Fail("ROOM_NOT_FOUND", "The requested room does not exist or is inactive.");
        }

        var timeValidation = ValidateTimes(request.StartTime, request.EndTime);
        if (timeValidation is not null)
        {
            return timeValidation;
        }

        if (!_accessControlService.CanAccessRoom(user, room))
        {
            return BookingResult.Fail("ACCESS_DENIED", $"{user.Role} users are not permitted to book {room.Name}.");
        }

        if (request.OverrideConflict && !_accessControlService.CanPerformAdminAction(user))
        {
            return BookingResult.Fail("ACCESS_DENIED", "Only administrators may override a booking conflict.");
        }

        var hasConflict = HasConflict(room.Id, request.StartTime, request.EndTime, excludingBookingId: null);
        if (hasConflict && !request.OverrideConflict)
        {
            return BookingResult.Fail("DOUBLE_BOOKING", "The room is already booked for an overlapping time slot.");
        }

        var booking = new Booking
        {
            RoomId = room.Id,
            UserId = user.Id,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Purpose = request.Purpose,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.UtcNow
        };

        _bookingRepository.Add(booking);
        return BookingResult.Ok(booking);
    }

    public BookingResult ModifyBooking(BookingModificationRequest request)
    {
        var booking = _bookingRepository.GetById(request.BookingId);
        if (booking is null)
        {
            return BookingResult.Fail("BOOKING_NOT_FOUND", "The booking does not exist.");
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return BookingResult.Fail("BOOKING_CANCELLED", "A cancelled booking cannot be modified.");
        }

        var requestingUser = _userRepository.GetById(request.RequestingUserId);
        if (requestingUser is null)
        {
            return BookingResult.Fail("USER_NOT_FOUND", "The requesting user does not exist.");
        }

        if (!_accessControlService.CanManageBooking(requestingUser, booking))
        {
            return BookingResult.Fail("ACCESS_DENIED", "You are not permitted to modify this booking.");
        }

        var timeValidation = ValidateTimes(request.NewStartTime, request.NewEndTime);
        if (timeValidation is not null)
        {
            return timeValidation;
        }

        if (HasConflict(booking.RoomId, request.NewStartTime, request.NewEndTime, excludingBookingId: booking.Id))
        {
            return BookingResult.Fail("DOUBLE_BOOKING", "The room is already booked for the requested new time slot.");
        }

        booking.StartTime = request.NewStartTime;
        booking.EndTime = request.NewEndTime;
        booking.Status = BookingStatus.Modified;
        booking.ModifiedAt = DateTime.UtcNow;

        _bookingRepository.Update(booking);
        return BookingResult.Ok(booking);
    }

    public BookingResult CancelBooking(Guid bookingId, Guid requestingUserId, string? reason = null)
    {
        var booking = _bookingRepository.GetById(bookingId);
        if (booking is null)
        {
            return BookingResult.Fail("BOOKING_NOT_FOUND", "The booking does not exist.");
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return BookingResult.Fail("ALREADY_CANCELLED", "This booking has already been cancelled.");
        }

        var requestingUser = _userRepository.GetById(requestingUserId);
        if (requestingUser is null)
        {
            return BookingResult.Fail("USER_NOT_FOUND", "The requesting user does not exist.");
        }

        if (!_accessControlService.CanManageBooking(requestingUser, booking))
        {
            return BookingResult.Fail("ACCESS_DENIED", "You are not permitted to cancel this booking.");
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAt = DateTime.UtcNow;
        booking.CancellationReason = reason;

        _bookingRepository.Update(booking);
        return BookingResult.Ok(booking);
    }

    public IEnumerable<Booking> GetBookingHistory(Guid userId) =>
        _bookingRepository.GetByUserId(userId).OrderByDescending(b => b.StartTime);

    private static BookingResult? ValidateTimes(DateTime start, DateTime end)
    {
        if (end <= start)
        {
            return BookingResult.Fail("INVALID_TIME_RANGE", "The end time must be after the start time.");
        }

        if (start < DateTime.UtcNow.AddMinutes(-1))
        {
            return BookingResult.Fail("INVALID_TIME_RANGE", "Bookings cannot be made in the past.");
        }

        return null;
    }

    private bool HasConflict(Guid roomId, DateTime start, DateTime end, Guid? excludingBookingId)
    {
        return _bookingRepository.GetByRoomId(roomId)
            .Where(b => b.Status != BookingStatus.Cancelled)
            .Where(b => excludingBookingId is null || b.Id != excludingBookingId.Value)
            .Any(b => b.OverlapsWith(start, end));
    }
}
