namespace StudyRoomBooking.Infrastructure.Shared.Utilities;

public static class DateTimeHelper
{
    public static DateTime GetTodayStart()
    {
        return DateTime.Today;
    }

    public static DateTime GetTodayEnd()
    {
        return DateTime.Today.AddDays(1).AddSeconds(-1);
    }

    public static bool IsInPast(DateTime dateTime)
    {
        return dateTime < DateTime.Now;
    }

    public static string FormatBookingDateTime(DateTime date, TimeSpan startTime, TimeSpan endTime)
    {
        return $"{date:MMM dd, yyyy} from {startTime:hh\\:mm} to {endTime:hh\\:mm}";
    }

    public static int GetWeekNumber(DateTime date)
    {
        return System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
            date, 
            System.Globalization.CalendarWeekRule.FirstDay, 
            DayOfWeek.Monday);
    }
}
