using System.Net.Http.Json;
using System.Text.Json;

namespace StudyRoomBooking.Application.Services;

public sealed class SupabaseAuthClient
{
    private readonly HttpClient _httpClient;
    private readonly string _supabaseUrl;
    private readonly string _anonKey;

    public SupabaseAuthClient(HttpClient httpClient, string supabaseUrl, string anonKey)
    {
        _httpClient = httpClient;
        _supabaseUrl = supabaseUrl.TrimEnd('/');
        _anonKey = anonKey;
    }

    public async Task<(bool Success, string? Error)> SignInAsync(string email, string password)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_supabaseUrl}/auth/v1/token?grant_type=password")
        {
            Content = JsonContent.Create(new { email, password })
        };

        request.Headers.Add("apikey", _anonKey);

        using var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var body = await response.Content.ReadAsStringAsync();
        try
        {
            using var document = JsonDocument.Parse(body);
            var error = document.RootElement.TryGetProperty("message", out var message)
                ? message.GetString()
                : document.RootElement.TryGetProperty("msg", out var msg)
                    ? msg.GetString()
                    : document.RootElement.TryGetProperty("error_description", out var description)
                        ? description.GetString()
                        : null;
            return (false, error);
        }
        catch (JsonException)
        {
            return (false, null);
        }
    }
}