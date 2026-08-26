using System.Text;
using Microsoft.AspNetCore.Http;

namespace StudyRoomBooking.Infrastructure.Shared;

public static class SessionExtensions
{
    public static void SetString(this ISession session, string key, string value)
    {
        session.Set(key, Encoding.UTF8.GetBytes(value));
    }

    public static string? GetString(this ISession session, string key)
    {
        if (session.TryGetValue(key, out byte[]? value))
        {
            return Encoding.UTF8.GetString(value);
        }
        return null;
    }
}
