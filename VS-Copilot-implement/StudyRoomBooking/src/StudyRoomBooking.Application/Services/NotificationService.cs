namespace StudyRoomBooking.Application.Services;

public class NotificationService : INotificationService
{
    public async Task SendBookingConfirmationAsync(string email, string roomName, DateTime bookingDate, string confirmationNumber)
    {
        // Mock implementation - console output
        Console.WriteLine($"[NOTIFICATION] Booking Confirmation sent to {email}");
        Console.WriteLine($"  Room: {roomName}");
        Console.WriteLine($"  Date: {bookingDate:dd/MM/yyyy}");
        Console.WriteLine($"  Confirmation #: {confirmationNumber}");
        await Task.CompletedTask;
    }

    public async Task SendBookingCancellationAsync(string email, string roomName, DateTime bookingDate)
    {
        // Mock implementation - console output
        Console.WriteLine($"[NOTIFICATION] Booking Cancellation sent to {email}");
        Console.WriteLine($"  Room: {roomName}");
        Console.WriteLine($"  Date: {bookingDate:dd/MM/yyyy}");
        await Task.CompletedTask;
    }

    public async Task SendBookingModificationAsync(string email, string roomName, DateTime oldDate, DateTime newDate)
    {
        // Mock implementation - console output
        Console.WriteLine($"[NOTIFICATION] Booking Modification sent to {email}");
        Console.WriteLine($"  Room: {roomName}");
        Console.WriteLine($"  Old Date: {oldDate:dd/MM/yyyy}");
        Console.WriteLine($"  New Date: {newDate:dd/MM/yyyy}");
        await Task.CompletedTask;
    }
}
