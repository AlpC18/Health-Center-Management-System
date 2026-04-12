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

    private async Task SetAdminAuthHeader()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { Email = "admin@wellness.al", Password = "Admin@12345!" });
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
        Assert.True(doc.GetProperty("success").GetBoolean());
        Assert.True(doc.TryGetProperty("data", out _));
    }

    [Fact]
    public async Task GetStats_WithToken_DataHasExpectedFields()
    {
        await SetAdminAuthHeader();
        var response = await _client.GetAsync("/api/dashboard/stats");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);
        var data = doc.GetProperty("data");

        Assert.True(data.TryGetProperty("TotalKlientet", out _));
        Assert.True(data.TryGetProperty("TotalTerapistet", out _));
        Assert.True(data.TryGetProperty("TotalTerminet", out _));
        Assert.True(data.TryGetProperty("TerminetSot", out _));
        Assert.True(data.TryGetProperty("AnetaresimetAktive", out _));
        Assert.True(data.TryGetProperty("TeDhjetatShitjet", out _));
        Assert.True(data.TryGetProperty("NotaMesatare", out _));
        Assert.True(data.TryGetProperty("ProduktetMeStokUlet", out _));
    }

    [Fact]
    public async Task GetStats_WithToken_CountsAreNonNegative()
    {
        await SetAdminAuthHeader();
        var response = await _client.GetAsync("/api/dashboard/stats");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);
        var data = doc.GetProperty("data");

        Assert.True(data.GetProperty("TotalKlientet").GetInt32() >= 0);
        Assert.True(data.GetProperty("TotalTerapistet").GetInt32() >= 0);
        Assert.True(data.GetProperty("TotalTerminet").GetInt32() >= 0);
        Assert.True(data.GetProperty("TerminetSot").GetInt32() >= 0);
        Assert.True(data.GetProperty("AnetaresimetAktive").GetInt32() >= 0);
        Assert.True(data.GetProperty("TeDhjetatShitjet").GetDecimal() >= 0);
        Assert.True(data.GetProperty("NotaMesatare").GetDouble() >= 0);
        Assert.True(data.GetProperty("ProduktetMeStokUlet").GetInt32() >= 0);
    }
}
