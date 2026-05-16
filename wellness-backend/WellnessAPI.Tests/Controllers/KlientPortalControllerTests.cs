using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using WellnessAPI.Tests.Infrastructure;

namespace WellnessAPI.Tests.Controllers;

public class KlientPortalControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public KlientPortalControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static JsonElement ParseBody(string body) =>
        JsonSerializer.Deserialize<JsonElement>(body);

    private async Task<(HttpResponseMessage Response, JsonElement Doc)> Post(string url, object body)
    {
        var response = await _client.PostAsJsonAsync(url, body);
        var content = await response.Content.ReadAsStringAsync();
        return (response, ParseBody(content));
    }

    private async Task SetAdminAuthHeader()
    {
        var (_, doc) = await Post("/api/auth/login",
            new { Email = "admin@wellness.com", Password = "Admin123!" });
        var token = doc.GetProperty("data").GetProperty("AccessToken").GetString()!;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task SetClientAuthHeader()
    {
        // Register a fresh klient user for isolation
        var email = $"portal_test_{Guid.NewGuid()}@test.com";
        await Post("/api/auth/register", new
        {
            FirstName = "Portal",
            LastName = "Test",
            Email = email,
            Password = "Portal@12345!",
            Role = "Klient"
        });
        var (_, loginDoc) = await Post("/api/auth/login",
            new { Email = email, Password = "Portal@12345!" });
        var token = loginDoc.GetProperty("data").GetProperty("AccessToken").GetString()!;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    // ── 401 Without Token ────────────────────────────────────────────────────

    [Fact]
    public async Task Dashboard_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/portal/dashboard");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Terminet_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/portal/terminet");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Produktet_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/portal/produktet");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Vlereisimet_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/portal/vlereisimet");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Anetaresimi_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/portal/anetaresimi");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── Admin cannot access portal dashboard (no KlientId) ──────────────────

    [Fact]
    public async Task Dashboard_AdminUser_Returns404BecauseNoKlientProfile()
    {
        await SetAdminAuthHeader();
        var response = await _client.GetAsync("/api/portal/dashboard");
        // Admin user has no KlientId, so portal returns 404
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Klient can access portal ─────────────────────────────────────────────

    [Fact]
    public async Task Terminet_WithKlientToken_Returns200()
    {
        await SetClientAuthHeader();
        var response = await _client.GetAsync("/api/portal/terminet");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Produktet_WithKlientToken_Returns200()
    {
        await SetClientAuthHeader();
        var response = await _client.GetAsync("/api/portal/produktet");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Sherbimet_WithKlientToken_Returns200()
    {
        await SetClientAuthHeader();
        var response = await _client.GetAsync("/api/portal/sherbimet");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Terapistet_WithKlientToken_Returns200()
    {
        await SetClientAuthHeader();
        var response = await _client.GetAsync("/api/portal/terapistet");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── Klient user can access their own portal dashboard ────────────────────

    [Fact]
    public async Task Dashboard_KlientUser_Returns200()
    {
        // Registration creates a Klient record automatically, so dashboard returns 200
        await SetClientAuthHeader();
        var response = await _client.GetAsync("/api/portal/dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── Create appointment via portal requires valid klientId ────────────────

    [Fact]
    public async Task CreateTermin_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PostAsJsonAsync("/api/portal/terminet", new
        {
            SherbimId = 1,
            TerapistId = 1,
            DataTerminit = DateTime.UtcNow.AddDays(3),
            OraFillimit = "09:00:00",
            OraMbarimit = "10:00:00"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
