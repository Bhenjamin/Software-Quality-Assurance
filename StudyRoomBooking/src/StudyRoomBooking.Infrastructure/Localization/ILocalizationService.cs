using System.Text.Json;

namespace StudyRoomBooking.Infrastructure.Localization;

public interface ILocalizationService
{
    string GetString(string key);
    string GetString(string section, string key);
    void SetLanguage(string languageCode);
    string CurrentLanguage { get; }
    List<string> AvailableLanguages { get; }
}
