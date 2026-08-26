using System.Text.Json;
using Microsoft.AspNetCore.Http;
using StudyRoomBooking.Infrastructure.Shared;

namespace StudyRoomBooking.Infrastructure.Localization;

public class LocalizationService : ILocalizationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Dictionary<string, Dictionary<string, string>> _translations = new();
    private const string LanguageSessionKey = "CurrentLanguage";
    private const string DefaultLanguage = "en-US";

    public LocalizationService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        LoadTranslations();
    }

    private void LoadTranslations()
    {
        var languages = new[] { "en-US", "vi-VN" };
        var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Localization", "Resources");

        foreach (var lang in languages)
        {
            var filePath = Path.Combine(basePath, $"{lang}.json");
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                var translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (translations != null)
                    _translations[lang] = translations;
            }
        }
    }

    public string GetString(string key)
    {
        var language = GetCurrentLanguage();
        if (_translations.ContainsKey(language) && _translations[language].ContainsKey(key))
            return _translations[language][key];

        // Fallback to English
        if (_translations.ContainsKey(DefaultLanguage) && _translations[DefaultLanguage].ContainsKey(key))
            return _translations[DefaultLanguage][key];

        return key;
    }

    public void SetLanguage(string languageCode)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
            httpContext.Session.SetString(LanguageSessionKey, languageCode);
    }

    public string GetCurrentLanguage()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var language = httpContext.Session.GetString(LanguageSessionKey);
            if (!string.IsNullOrEmpty(language))
                return language;

            // Check Accept-Language header
            var acceptLanguage = httpContext.Request.Headers["Accept-Language"].ToString();
            if (acceptLanguage.Contains("vi"))
                return "vi-VN";
        }

        return DefaultLanguage;
    }
}
