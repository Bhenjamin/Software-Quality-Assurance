using System.Text.RegularExpressions;

namespace StudyRoomBooking.Web.Utilities;

public static class EnumExtensions
{
    /// <summary>
    /// Converts PascalCase enum names to human-readable format with spaces.
    /// Examples: "ComputerLab" -> "Computer Lab", "Boardroom" -> "Boardroom"
    /// </summary>
    public static string ToDisplayName(this Enum value)
    {
        if (value == null)
            return string.Empty;

        // Insert a space before each uppercase letter that follows a lowercase letter
        var name = value.ToString();
        var displayName = Regex.Replace(name, "(?<!^)(?=[A-Z])", " ");
        return displayName;
    }
}
