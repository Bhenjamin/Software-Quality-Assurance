namespace StudyRoomBooking.Infrastructure.Shared.Utilities;

public static class ValidationHelper
{
    public static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidTimeSlot(TimeSpan startTime, TimeSpan endTime)
    {
        return startTime < endTime;
    }

    public static bool IsValidCapacity(int capacity)
    {
        return capacity > 0 && capacity <= 100;
    }

    public static bool IsValidRoomCode(string roomCode)
    {
        return !string.IsNullOrWhiteSpace(roomCode) && roomCode.Length <= 20;
    }
}
