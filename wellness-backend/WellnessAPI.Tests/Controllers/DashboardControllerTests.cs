using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using WellnessAPI.Tests.Infrastructure;

namespace WellnessAPI.Tests.Controllers;

public class DashboardControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DashboardControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static JsonElement ParseBody(string body)
        => JsonSerializer.Deserialize<JsonElement>(body);

    private static JsonElement Payload(JsonElement doc)
        => doc.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object ? data : doc;

    private static bool IsSuccess(JsonElement doc)
        => !doc.TryGetProperty("success", out var success) || success.GetBoolean();

    private async Task SetAdminAuthHeader()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { Email = "admin@wellness.com", Password = "Admin123!" });
        var content = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(content);
        var token = doc.GetProperty("data").GetProperty("AccessToken").GetString()!;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task GetStats_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/dashboard/stats");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetStats_WithToken_Returns200WithSuccessTrue()
    {
        await SetAdminAuthHeader();
        var response = await _client.GetAsync("/api/dashboard/stats");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(IsSuccess(doc));
        Assert.True(Payload(doc).TryGetProperty("totalKlientet", out _));
    }

    [Fact]
    public async Task GetStats_WithToken_DataHasExpectedFields()
    {
        await SetAdminAuthHeader();
        var response = await _client.GetAsync("/api/dashboard/stats");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);
        var data = Payload(doc);

        Assert.True(data.TryGetProperty("totalKlientet", out _));
        Assert.True(data.TryGetProperty("terapistetAktiv", out _));
        Assert.True(data.TryGetProperty("totalTerminet", out _));
        Assert.True(data.TryGetProperty("terminetSot", out _));
        Assert.True(data.TryGetProperty("anetaresimiAktiv", out _));
        Assert.True(data.TryGetProperty("teDheratMujore", out _));
        Assert.True(data.TryGetProperty("notaMesatare", out _));
        Assert.True(data.TryGetProperty("productetNeStok", out _));
    }

    [Fact]
    public async Task GetStats_WithToken_CountsAreNonNegative()
    {
        await SetAdminAuthHeader();
        var response = await _client.GetAsync("/api/dashboard/stats");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);
        var data = Payload(doc);

        Assert.True(data.GetProperty("totalKlientet").GetInt32() >= 0);
        Assert.True(data.GetProperty("terapistetAktiv").GetInt32() >= 0);
        Assert.True(data.GetProperty("totalTerminet").GetInt32() >= 0);
        Assert.True(data.GetProperty("terminetSot").GetInt32() >= 0);
        Assert.True(data.GetProperty("anetaresimiAktiv").GetInt32() >= 0);
        Assert.True(data.GetProperty("teDheratMujore").GetDecimal() >= 0);
        Assert.True(data.GetProperty("notaMesatare").GetDouble() >= 0);
        Assert.True(data.GetProperty("productetNeStok").GetInt32() >= 0);
    }
}
