using System.Net.Http.Headers;
using System.Text;

namespace WellnessAPI.Services;

public class SmsService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _http;

    public SmsService(IConfiguration config, HttpClient http)
    {
        _config = config;
        _http = http;
    }

    public async Task SendSmsAsync(string toPhone, string body, CancellationToken cancellationToken = default)
    {
        var sid = _config["Twilio:AccountSid"] ?? throw new InvalidOperationException("Twilio:AccountSid is not configured.");
        var token = _config["Twilio:AuthToken"] ?? throw new InvalidOperationException("Twilio:AuthToken is not configured.");
        var from = _config["Twilio:From"] ?? throw new InvalidOperationException("Twilio:From is not configured.");

        using var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.twilio.com/2010-04-01/Accounts/{sid}/Messages.json");
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($"{sid}:{token}")));
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["From"] = from,
            ["To"] = toPhone,
            ["Body"] = body
        });

        using var response = await _http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
