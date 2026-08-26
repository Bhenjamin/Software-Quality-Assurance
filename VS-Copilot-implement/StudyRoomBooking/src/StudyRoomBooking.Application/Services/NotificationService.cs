using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Application.ViewModels;

namespace StudyRoomBooking.Application.Services;

public class NotificationService : INotificationService
{
    public void SendBookingConfirmation(BookingViewModel booking, string userEmail)
    {
        // TODO: Implement email sending
        // For now, this is a placeholder that logs the notification intent
        Console.WriteLine($"[NOTIFICATION] Booking confirmation sent to {userEmail} for booking {booking.Id}");
    }

    public void SendBookingModificationNotification(BookingViewModel booking, string userEmail)
    {
        // TODO: Implement email sending
        Console.WriteLine($"[NOTIFICATION] Booking modification notification sent to {userEmail} for booking {booking.Id}");
    }

    public void SendBookingCancellationNotification(BookingViewModel booking, string userEmail)
    {
        // TODO: Implement email sending
        Console.WriteLine($"[NOTIFICATION] Booking cancellation notification sent to {userEmail} for booking {booking.Id}");
    }
}
