using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WellnessAPI.Models.Domain;

namespace WellnessAPI.Services;

public record StripeCheckoutSession(string Id, string Url);

public class StripeCheckoutService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _http;

    public StripeCheckoutService(IConfiguration config, HttpClient http)
    {
        _config = config;
        _http = http;
    }

    public async Task<StripeCheckoutSession> CreateMembershipCheckoutSessionAsync(
        Anetaresim membership,
        PaketaWellness package,
        decimal amount,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default)
    {
        var secretKey = _config["Stripe:SecretKey"] ?? throw new InvalidOperationException("Stripe:SecretKey is not configured.");
        var currency = _config["Stripe:Currency"] ?? "eur";
        var cents = (int)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.stripe.com/v1/checkout/sessions");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", secretKey);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["mode"] = "payment",
            ["success_url"] = successUrl,
            ["cancel_url"] = cancelUrl,
            ["payment_method_types[0]"] = "card",
            ["line_items[0][quantity]"] = "1",
            ["line_items[0][price_data][currency]"] = currency,
            ["line_items[0][price_data][unit_amount]"] = cents.ToString(),
            ["line_items[0][price_data][product_data][name]"] = package.EmriPaketes,
            ["metadata[anetaresimId]"] = membership.AnetaresimId.ToString(),
            ["metadata[klientId]"] = membership.KlientId.ToString(),
            ["metadata[paketId]"] = membership.PaketId.ToString()
        });

        using var response = await _http.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Stripe Checkout deshtoi: {content}");

        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;
        return new StripeCheckoutSession(
            root.GetProperty("id").GetString() ?? "",
            root.GetProperty("url").GetString() ?? "");
    }

    public bool VerifyWebhookSignature(string payload, string? signatureHeader)
    {
        var secret = _config["Stripe:WebhookSecret"];
        if (string.IsNullOrWhiteSpace(secret)) return true;
        if (string.IsNullOrWhiteSpace(signatureHeader)) return false;

        var parts = signatureHeader.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Split('=', 2))
            .Where(p => p.Length == 2)
            .GroupBy(p => p[0])
            .ToDictionary(g => g.Key, g => g.Select(x => x[1]).ToList());

        if (!parts.TryGetValue("t", out var ts) || !parts.TryGetValue("v1", out var signatures))
            return false;

        var signedPayload = $"{ts[0]}.{payload}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var expected = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload))).ToLowerInvariant();
        return signatures.Any(sig => CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(expected),
            Encoding.ASCII.GetBytes(sig)));
    }
}
