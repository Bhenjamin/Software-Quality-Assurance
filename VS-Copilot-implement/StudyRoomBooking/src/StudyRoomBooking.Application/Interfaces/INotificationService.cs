using StudyRoomBooking.Application.ViewModels;

namespace StudyRoomBooking.Application.Interfaces;

public interface INotificationService
{
    void SendBookingConfirmation(BookingViewModel booking, string userEmail);
    void SendBookingModificationNotification(BookingViewModel booking, string userEmail);
    void SendBookingCancellationNotification(BookingViewModel booking, string userEmail);
}
