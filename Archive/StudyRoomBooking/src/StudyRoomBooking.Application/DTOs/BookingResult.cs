using StudyRoomBooking.Domain.Models;

namespace StudyRoomBooking.Application.DTOs;

/// <summary>
/// Outcome of a booking operation. Using a result object (rather than
/// throwing for expected failures like "room already booked") keeps
/// control flow simple for callers such as a UI or API controller.
/// </summary>
public class BookingResult
{
    public bool Success { get; init; }
    public Booking? Booking { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }

    public static BookingResult Ok(Booking booking) =>
        new() { Success = true, Booking = booking };

    public static BookingResult Fail(string errorCode, string errorMessage) =>
        new() { Success = false, ErrorCode = errorCode, ErrorMessage = errorMessage };
}
