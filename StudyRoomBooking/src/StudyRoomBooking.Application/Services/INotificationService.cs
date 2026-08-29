namespace StudyRoomBooking.Application.Services;

public interface INotificationService
{
    Task SendBookingConfirmationAsync(string email, string roomName, DateTime bookingDate, string confirmationNumber);
    Task SendBookingCancellationAsync(string email, string roomName, DateTime bookingDate);
    Task SendBookingModificationAsync(string email, string roomName, DateTime oldDate, DateTime newDate);
}
