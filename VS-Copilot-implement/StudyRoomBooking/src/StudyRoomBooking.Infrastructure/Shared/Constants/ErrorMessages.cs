namespace StudyRoomBooking.Infrastructure.Shared.Constants;

public static class ErrorMessages
{
    public const string RoomNotFound = "Room not found";
    public const string BookingNotFound = "Booking not found";
    public const string UserNotFound = "User not found";
    public const string RoomUnavailable = "Room is not available for the selected time";
    public const string InvalidTimeSlot = "Start time must be before end time";
    public const string BookingInPast = "Cannot book a room in the past";
    public const string AccessDenied = "Access denied";
    public const string UserAlreadyExists = "User with this email already exists";
    public const string InvalidCredentials = "Invalid email or password";
}
