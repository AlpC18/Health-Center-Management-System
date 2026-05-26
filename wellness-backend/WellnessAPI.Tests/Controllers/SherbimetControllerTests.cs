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

    private static JsonElement Payload(JsonElement doc)
        => doc.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object ? data : doc;

    private static bool IsSuccess(JsonElement doc)
        => !doc.TryGetProperty("success", out var success) || success.GetBoolean();

    private static int GetId(JsonElement doc)
    {
        var payload = Payload(doc);
        if (payload.TryGetProperty("sherbimId", out var camelId)) return camelId.GetInt32();
        if (payload.TryGetProperty("SherbimId", out var pascalId)) return pascalId.GetInt32();
        return payload.GetProperty("Id").GetInt32();
    }

    private async Task<(HttpResponseMessage Response, JsonElement Doc)> Post(string url, object body)
    {
        var response = await _client.PostAsJsonAsync(url, body);
        var content = await response.Content.ReadAsStringAsync();
        return (response, string.IsNullOrWhiteSpace(content) ? default : ParseBody(content));
    }

    private async Task SetAdminAuthHeader()
    {
        var (_, doc) = await Post("/api/auth/login", new { Email = "admin@wellness.com", Password = "Admin123!" });
        var token = doc.GetProperty("data").GetProperty("AccessToken").GetString()!;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static object NewSherbimDto(string? emri = null) => new
    {
        EmriSherbimit = emri ?? $"Sherbim_{Guid.NewGuid():N}",
        Kategoria = "Masazh",
        Pershkrimi = "Pershkrim testues",
        Cmimi = 2500m,
        KohezgjatjaMin = 60,
        Aktiv = true
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
        Assert.True(IsSuccess(doc));
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
            EmriSherbimit = uniqueName,
            Kategoria = "Spa",
            Pershkrimi = "Pershkrim",
            Cmimi = 1000m,
            KohezgjatjaMin = 30,
            Aktiv = true
        });

        var response = await _client.GetAsync($"/api/sherbimet?search={uniqueName}");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(IsSuccess(doc));
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
        Assert.True(IsSuccess(doc));
        Assert.Equal(1, doc.GetProperty("page").GetInt32());
        Assert.Equal(5, doc.GetProperty("limit").GetInt32());
    }

    // ── GetById ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingId_Returns200WithData()
    {
        await SetAdminAuthHeader();

        var (_, createDoc) = await Post("/api/sherbimet", NewSherbimDto());
        var id = GetId(createDoc);

        var response = await _client.GetAsync($"/api/sherbimet/{id}");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(IsSuccess(doc));
        Assert.Equal(id, GetId(doc));
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
        Assert.True(IsSuccess(doc));
        Assert.True(GetId(doc) > 0);
    }

    [Fact]
    public async Task Create_ValidInput_DataHasExpectedFields()
    {
        await SetAdminAuthHeader();
        var emri = $"Sherbim_{Guid.NewGuid():N}";
        var (_, doc) = await Post("/api/sherbimet", new
        {
            EmriSherbimit = emri,
            Kategoria = "Yoga",
            Pershkrimi = "Pershkrim testues",
            Cmimi = 3000m,
            KohezgjatjaMin = 45,
            Aktiv = true
        });
        var data = Payload(doc);

        Assert.Equal(emri, data.GetProperty("emriSherbimit").GetString());
        Assert.Equal(3000m, data.GetProperty("cmimi").GetDecimal());
        Assert.Equal(45, data.GetProperty("kohezgjatjaMin").GetInt32());
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_ExistingId_Returns200WithUpdatedData()
    {
        await SetAdminAuthHeader();

        var (_, createDoc) = await Post("/api/sherbimet", NewSherbimDto());
        var id = GetId(createDoc);

        var response = await _client.PutAsJsonAsync($"/api/sherbimet/{id}", new
        {
            EmriSherbimit = "UpdatedSherbim",
            Kategoria = "Fizioterapi",
            Pershkrimi = "Pershkrim i ri",
            Cmimi = 5000m,
            KohezgjatjaMin = 90,
            Aktiv = true
        });
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(IsSuccess(doc));
        var data = Payload(doc);
        Assert.Equal("UpdatedSherbim", data.GetProperty("emriSherbimit").GetString());
        Assert.Equal(5000m, data.GetProperty("cmimi").GetDecimal());
    }

    [Fact]
    public async Task Update_NonExistentId_Returns404()
    {
        await SetAdminAuthHeader();
        var response = await _client.PutAsJsonAsync("/api/sherbimet/999999", new
        {
            EmriSherbimit = "X",
            Kategoria = "Spa",
            Pershkrimi = "Y",
            Cmimi = 100m,
            KohezgjatjaMin = 30,
            Aktiv = true
        });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ExistingId_Returns200()
    {
        await SetAdminAuthHeader();

        var (_, createDoc) = await Post("/api/sherbimet", NewSherbimDto());
        var id = GetId(createDoc);

        var response = await _client.DeleteAsync($"/api/sherbimet/{id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
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
        var id = GetId(createDoc);

        await _client.DeleteAsync($"/api/sherbimet/{id}");

        var response = await _client.GetAsync($"/api/sherbimet/{id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
