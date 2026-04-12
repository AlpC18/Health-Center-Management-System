using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using WellnessAPI.Tests.Infrastructure;

namespace WellnessAPI.Tests.Controllers;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static JsonElement ParseBody(string body)
        => JsonSerializer.Deserialize<JsonElement>(body);

    private async Task<(HttpResponseMessage Response, JsonElement Doc)> Post(string url, object body)
    {
        var response = await _client.PostAsJsonAsync(url, body);
        var content = await response.Content.ReadAsStringAsync();
        return (response, ParseBody(content));
    }

    private async Task<string> LoginAsAdmin()
    {
        var (resp, doc) = await Post("/api/auth/login", new { Email = "admin@wellness.al", Password = "Admin@12345!" });
        return doc.GetProperty("data").GetProperty("AccessToken").GetString()!;
    }

    // ── Register ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_ValidInput_Returns200WithSuccess()
    {
        var (resp, doc) = await Post("/api/auth/register", new
        {
            FirstName = "Test",
            LastName = "User",
            Email = $"new_{Guid.NewGuid()}@test.com",
            Password = "Test@1234!"
        });

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        var email = $"dup_{Guid.NewGuid()}@test.com";
        await Post("/api/auth/register", new { FirstName = "A", LastName = "B", Email = email, Password = "Test@1234!" });
        var (resp, doc) = await Post("/api/auth/register", new { FirstName = "A", LastName = "B", Email = email, Password = "Test@1234!" });

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        Assert.False(doc.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task Register_WeakPassword_Returns400()
    {
        var (resp, doc) = await Post("/api/auth/register", new
        {
            FirstName = "Test",
            LastName = "User",
            Email = $"weak_{Guid.NewGuid()}@test.com",
            Password = "weak"
        });

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        Assert.False(doc.GetProperty("success").GetBoolean());
    }

    // ── Login ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithTokens()
    {
        var email = $"login_{Guid.NewGuid()}@test.com";
        await Post("/api/auth/register", new { FirstName = "A", LastName = "B", Email = email, Password = "Test@1234!" });

        var (resp, doc) = await Post("/api/auth/login", new { Email = email, Password = "Test@1234!" });

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());
        Assert.NotEmpty(doc.GetProperty("data").GetProperty("AccessToken").GetString()!);
        Assert.NotEmpty(doc.GetProperty("data").GetProperty("RefreshToken").GetString()!);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var email = $"badpw_{Guid.NewGuid()}@test.com";
        await Post("/api/auth/register", new { FirstName = "A", LastName = "B", Email = email, Password = "Test@1234!" });

        var (resp, _) = await Post("/api/auth/login", new { Email = email, Password = "Wrong@1234!" });

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Login_NonExistentEmail_Returns401()
    {
        var (resp, _) = await Post("/api/auth/login", new { Email = "nobody@nowhere.com", Password = "Test@1234!" });
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Login_AdminSeededUser_Returns200()
    {
        var (resp, doc) = await Post("/api/auth/login", new { Email = "admin@wellness.al", Password = "Admin@12345!" });

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());
    }

    // ── Refresh ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Refresh_ValidToken_Returns200WithNewTokens()
    {
        var email = $"refresh_{Guid.NewGuid()}@test.com";
        await Post("/api/auth/register", new { FirstName = "A", LastName = "B", Email = email, Password = "Test@1234!" });
        var (_, loginDoc) = await Post("/api/auth/login", new { Email = email, Password = "Test@1234!" });
        var refreshToken = loginDoc.GetProperty("data").GetProperty("RefreshToken").GetString()!;

        var (resp, doc) = await Post("/api/auth/refresh", new { RefreshToken = refreshToken });

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());
        Assert.NotEmpty(doc.GetProperty("data").GetProperty("AccessToken").GetString()!);
    }

    [Fact]
    public async Task Refresh_InvalidToken_Returns401()
    {
        var (resp, _) = await Post("/api/auth/refresh", new { RefreshToken = "invalid-token" });
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Refresh_UsedToken_Returns401()
    {
        var email = $"usedtoken_{Guid.NewGuid()}@test.com";
        await Post("/api/auth/register", new { FirstName = "A", LastName = "B", Email = email, Password = "Test@1234!" });
        var (_, loginDoc) = await Post("/api/auth/login", new { Email = email, Password = "Test@1234!" });
        var refreshToken = loginDoc.GetProperty("data").GetProperty("RefreshToken").GetString()!;

        await Post("/api/auth/refresh", new { RefreshToken = refreshToken });
        var (resp, _) = await Post("/api/auth/refresh", new { RefreshToken = refreshToken });

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── Me ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Me_WithValidToken_Returns200WithUserData()
    {
        var token = await LoginAsAdmin();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/auth/me");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());
        Assert.Equal("admin@wellness.al", doc.GetProperty("data").GetProperty("Email").GetString());

        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── Logout ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Logout_WithValidToken_Returns200()
    {
        var email = $"logout_{Guid.NewGuid()}@test.com";
        await Post("/api/auth/register", new { FirstName = "A", LastName = "B", Email = email, Password = "Test@1234!" });
        var (_, loginDoc) = await Post("/api/auth/login", new { Email = email, Password = "Test@1234!" });
        var accessToken = loginDoc.GetProperty("data").GetProperty("AccessToken").GetString()!;
        var refreshToken = loginDoc.GetProperty("data").GetProperty("RefreshToken").GetString()!;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var (resp, doc) = await Post("/api/auth/logout", new { RefreshToken = refreshToken });

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());

        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task Logout_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var (resp, _) = await Post("/api/auth/logout", new { RefreshToken = "any" });
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── ChangePassword ───────────────────────────────────────────────────────

    [Fact]
    public async Task ChangePassword_ValidRequest_Returns200()
    {
        var email = $"chpw_{Guid.NewGuid()}@test.com";
        await Post("/api/auth/register", new { FirstName = "A", LastName = "B", Email = email, Password = "Test@1234!" });
        var (_, loginDoc) = await Post("/api/auth/login", new { Email = email, Password = "Test@1234!" });
        var accessToken = loginDoc.GetProperty("data").GetProperty("AccessToken").GetString()!;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _client.PutAsJsonAsync("/api/auth/change-password", new
        {
            CurrentPassword = "Test@1234!",
            NewPassword = "NewTest@5678!"
        });
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());

        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task ChangePassword_WrongCurrentPassword_Returns400()
    {
        var email = $"chpw_bad_{Guid.NewGuid()}@test.com";
        await Post("/api/auth/register", new { FirstName = "A", LastName = "B", Email = email, Password = "Test@1234!" });
        var (_, loginDoc) = await Post("/api/auth/login", new { Email = email, Password = "Test@1234!" });
        var accessToken = loginDoc.GetProperty("data").GetProperty("AccessToken").GetString()!;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _client.PutAsJsonAsync("/api/auth/change-password", new
        {
            CurrentPassword = "WrongOld@1234!",
            NewPassword = "NewTest@5678!"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task ChangePassword_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PutAsJsonAsync("/api/auth/change-password", new
        {
            CurrentPassword = "Test@1234!",
            NewPassword = "NewTest@5678!"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
