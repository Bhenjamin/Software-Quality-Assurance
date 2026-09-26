using System.Net;
using System.Net.Mail;

namespace StudyRoomBooking.Application.Services;

public sealed class EmailOptions
{
    public bool Enabled { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
}

public class NotificationService : INotificationService
{
    private readonly EmailOptions _emailOptions;

    public NotificationService(EmailOptions emailOptions)
    {
        _emailOptions = emailOptions;
    }

    public async Task SendBookingConfirmationAsync(string email, string roomName, DateTime bookingDate, TimeSpan startTime, TimeSpan endTime, string confirmationNumber)
    {
        var subject = $"Study room booking confirmed: {confirmationNumber}";
        var body = $"Đặt phòng học của bạn đã được xác nhận.\n" +
               $"Phòng học: {roomName}\n" +
               $"Ngày: {bookingDate:dd/MM/yyyy}\n" +
               $"Thời gian: {startTime:hh\\:mm} - {endTime:hh\\:mm}\n" +
               $"Mã xác nhận: {confirmationNumber}\n\n" +
               "----------------------------------------\n\n" +
               $"Your study room booking is confirmed.\n" +
               $"Room: {roomName}\n" +
               $"Date: {bookingDate:dd/MM/yyyy}\n" +
               $"Time: {startTime:hh\\:mm} - {endTime:hh\\:mm}\n" +
               $"Confirmation number: {confirmationNumber}\n";

        if (!_emailOptions.Enabled)
        {
            Console.WriteLine($"[NOTIFICATION] Booking Confirmation for {email}\n{body}");
            return;
        }

        using var message = new MailMessage(_emailOptions.From, email, subject, body);
        using var client = new SmtpClient(_emailOptions.Host, _emailOptions.Port)
        {
            EnableSsl = _emailOptions.UseSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_emailOptions.Username, _emailOptions.Password)
        };

        await client.SendMailAsync(message);
    }

    public async Task SendBookingCancellationAsync(string email, string roomName, DateTime bookingDate)
    {
        var subject = "Study room booking cancelled";
        var body = $"Đặt phòng học của bạn đã được hủy.\n" +
                   $"Phòng học: {roomName}\n" +
                   $"Ngày: {bookingDate:dd/MM/yyyy}\n\n" +
                   "----------------------------------------\n\n" +
                   $"Your study room booking has been cancelled.\n" +
                   $"Room: {roomName}\n" +
                   $"Date: {bookingDate:dd/MM/yyyy}\n";

        if (!_emailOptions.Enabled)
        {
            Console.WriteLine($"[NOTIFICATION] Booking Cancellation for {email}\n{body}");
            return;
        }

        using var message = new MailMessage(_emailOptions.From, email, subject, body);
        using var client = new SmtpClient(_emailOptions.Host, _emailOptions.Port)
        {
            EnableSsl = _emailOptions.UseSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_emailOptions.Username, _emailOptions.Password)
        };

        await client.SendMailAsync(message);
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
