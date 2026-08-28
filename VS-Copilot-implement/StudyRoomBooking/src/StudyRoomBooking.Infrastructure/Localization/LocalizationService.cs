using System.Text.Json;

namespace StudyRoomBooking.Infrastructure.Localization;

public class LocalizationService : ILocalizationService
{
    private Dictionary<string, object> _resources = new();
    private string _currentLanguage = "en";
    private readonly string _localizationPath;

    public string CurrentLanguage => _currentLanguage;
    public List<string> AvailableLanguages => new() { "en", "vi" };

    public LocalizationService()
    {
        _localizationPath = Path.Combine(AppContext.BaseDirectory, "Localization", "Resources");
        LoadResources("en");
    }

    public void SetLanguage(string languageCode)
    {
        if (AvailableLanguages.Contains(languageCode))
        {
            _currentLanguage = languageCode;
            LoadResources(languageCode);
        }
    }

    public string GetString(string key)
    {
        return GetString("", key);
    }

    public string GetString(string section, string key)
    {
        try
        {
            if (string.IsNullOrEmpty(section))
            {
                // Try to find the key in any section
                foreach (var section_item in _resources)
                {
                    if (section_item.Value is JsonElement jsonElement)
                    {
                        if (jsonElement.TryGetProperty(key, out var value))
                        {
                            return value.GetString() ?? key;
                        }
                    }
                }
            }
            else
            {
                if (_resources.TryGetValue(section, out var sectionObj))
                {
                    if (sectionObj is JsonElement jsonElement)
                    {
                        if (jsonElement.TryGetProperty(key, out var value))
                        {
                            return value.GetString() ?? key;
                        }
                    }
                }
            }
        }
        catch
        {
            // If anything goes wrong, return the key
        }

        return key;
    }

    private void LoadResources(string languageCode)
    {
        _resources.Clear();

        var filePath = Path.Combine(_localizationPath, $"{languageCode}.json");

        if (File.Exists(filePath))
        {
            try
            {
                var json = File.ReadAllText(filePath);
                using (var doc = JsonDocument.Parse(json))
                {
                    foreach (var property in doc.RootElement.EnumerateObject())
                    {
                        _resources[property.Name] = property.Value.Clone();
                    }
                }
            }
            catch
            {
                // If loading fails, resources will be empty
            }
        }
    }
}
