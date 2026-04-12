using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using WellnessAPI.Tests.Infrastructure;

namespace WellnessAPI.Tests.Controllers;

public class SherbimetControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SherbimetControllerTests(CustomWebApplicationFactory factory)
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

    private async Task SetAdminAuthHeader()
    {
        var (_, doc) = await Post("/api/auth/login", new { Email = "admin@wellness.al", Password = "Admin@12345!" });
        var token = doc.GetProperty("data").GetProperty("AccessToken").GetString()!;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static object NewSherbimDto(string? emri = null) => new
    {
        Emri = emri ?? $"Sherbim_{Guid.NewGuid():N}",
        Pershkrimi = "Pershkrim testues",
        Cmimi = 2500m,
        Kohezgjatja = 60
    };

    // ── Authorization ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/sherbimet");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/sherbimet/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var (resp, _) = await Post("/api/sherbimet", NewSherbimDto());
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── GetAll ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_WithToken_Returns200WithSuccessTrue()
    {
        await SetAdminAuthHeader();
        var response = await _client.GetAsync("/api/sherbimet");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());
        Assert.True(doc.TryGetProperty("data", out _));
        Assert.True(doc.TryGetProperty("total", out _));
    }

    [Fact]
    public async Task GetAll_WithSearch_Returns200()
    {
        await SetAdminAuthHeader();

        var uniqueName = $"UniqueSherbim_{Guid.NewGuid():N}";
        await Post("/api/sherbimet", new
        {
            Emri = uniqueName,
            Pershkrimi = "Pershkrim",
            Cmimi = 1000m,
            Kohezgjatja = 30
        });

        var response = await _client.GetAsync($"/api/sherbimet?search={uniqueName}");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());
        Assert.True(doc.GetProperty("total").GetInt32() >= 1);
    }

    [Fact]
    public async Task GetAll_WithPagination_Returns200()
    {
        await SetAdminAuthHeader();
        var response = await _client.GetAsync("/api/sherbimet?page=1&limit=5");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());
        Assert.Equal(1, doc.GetProperty("page").GetInt32());
        Assert.Equal(5, doc.GetProperty("limit").GetInt32());
    }

    // ── GetById ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingId_Returns200WithData()
    {
        await SetAdminAuthHeader();

        var (_, createDoc) = await Post("/api/sherbimet", NewSherbimDto());
        var id = createDoc.GetProperty("data").GetProperty("Id").GetInt32();

        var response = await _client.GetAsync($"/api/sherbimet/{id}");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());
        Assert.Equal(id, doc.GetProperty("data").GetProperty("Id").GetInt32());
    }

    [Fact]
    public async Task GetById_NonExistentId_Returns404()
    {
        await SetAdminAuthHeader();
        var response = await _client.GetAsync("/api/sherbimet/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidInput_Returns201WithData()
    {
        await SetAdminAuthHeader();
        var (resp, doc) = await Post("/api/sherbimet", NewSherbimDto());

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());
        Assert.True(doc.GetProperty("data").GetProperty("Id").GetInt32() > 0);
    }

    [Fact]
    public async Task Create_ValidInput_DataHasExpectedFields()
    {
        await SetAdminAuthHeader();
        var emri = $"Sherbim_{Guid.NewGuid():N}";
        var (_, doc) = await Post("/api/sherbimet", new
        {
            Emri = emri,
            Pershkrimi = "Pershkrim testues",
            Cmimi = 3000m,
            Kohezgjatja = 45
        });
        var data = doc.GetProperty("data");

        Assert.Equal(emri, data.GetProperty("Emri").GetString());
        Assert.Equal(3000m, data.GetProperty("Cmimi").GetDecimal());
        Assert.Equal(45, data.GetProperty("Kohezgjatja").GetInt32());
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_ExistingId_Returns200WithUpdatedData()
    {
        await SetAdminAuthHeader();

        var (_, createDoc) = await Post("/api/sherbimet", NewSherbimDto());
        var id = createDoc.GetProperty("data").GetProperty("Id").GetInt32();

        var response = await _client.PutAsJsonAsync($"/api/sherbimet/{id}", new
        {
            Emri = "UpdatedSherbim",
            Pershkrimi = "Pershkrim i ri",
            Cmimi = 5000m,
            Kohezgjatja = 90,
            IsActive = true
        });
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());
        Assert.Equal("UpdatedSherbim", doc.GetProperty("data").GetProperty("Emri").GetString());
        Assert.Equal(5000m, doc.GetProperty("data").GetProperty("Cmimi").GetDecimal());
    }

    [Fact]
    public async Task Update_NonExistentId_Returns404()
    {
        await SetAdminAuthHeader();
        var response = await _client.PutAsJsonAsync("/api/sherbimet/999999", new
        {
            Emri = "X", Pershkrimi = "Y", Cmimi = 100m, Kohezgjatja = 30, IsActive = true
        });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ExistingId_Returns200()
    {
        await SetAdminAuthHeader();

        var (_, createDoc) = await Post("/api/sherbimet", NewSherbimDto());
        var id = createDoc.GetProperty("data").GetProperty("Id").GetInt32();

        var response = await _client.DeleteAsync($"/api/sherbimet/{id}");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task Delete_NonExistentId_Returns404()
    {
        await SetAdminAuthHeader();
        var response = await _client.DeleteAsync("/api/sherbimet/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ThenGetById_Returns404()
    {
        await SetAdminAuthHeader();

        var (_, createDoc) = await Post("/api/sherbimet", NewSherbimDto());
        var id = createDoc.GetProperty("data").GetProperty("Id").GetInt32();

        await _client.DeleteAsync($"/api/sherbimet/{id}");

        var response = await _client.GetAsync($"/api/sherbimet/{id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
