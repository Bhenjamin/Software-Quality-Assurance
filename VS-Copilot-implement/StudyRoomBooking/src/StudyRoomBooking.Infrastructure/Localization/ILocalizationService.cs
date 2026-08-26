namespace StudyRoomBooking.Infrastructure.Localization;

public interface ILocalizationService
{
    string GetString(string key);
    void SetLanguage(string languageCode);
    string GetCurrentLanguage();
}
