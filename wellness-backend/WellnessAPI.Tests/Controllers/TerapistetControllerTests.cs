using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using WellnessAPI.Tests.Infrastructure;

namespace WellnessAPI.Tests.Controllers;

public class TerapistetControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TerapistetControllerTests(CustomWebApplicationFactory factory)
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
        if (payload.TryGetProperty("terapistId", out var camelId)) return camelId.GetInt32();
        if (payload.TryGetProperty("TerapistId", out var pascalId)) return pascalId.GetInt32();
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

    private static object NewTerapistDto(string? email = null) => new
    {
        Emri = "Test",
        Mbiemri = "Terapist",
        Email = email ?? $"terapist_{Guid.NewGuid()}@test.com",
        Telefoni = "0691234567",
        Specializimi = "Masazh",
        Licenca = $"LIC-{Guid.NewGuid():N}"[..12],
        Aktiv = true
    };

    // ── Authorization ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/terapistet");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/terapistet/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var (resp, _) = await Post("/api/terapistet", NewTerapistDto());
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── GetAll ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_WithToken_Returns200WithSuccessTrue()
    {
        await SetAdminAuthHeader();
        var response = await _client.GetAsync("/api/terapistet");
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

        await Post("/api/terapistet", new
        {
            Emri = "UniqueTerapist",
            Mbiemri = "Testues",
            Email = $"unique_{Guid.NewGuid()}@test.com",
            Telefoni = "0691234567",
            Specializimi = "Fizioterapi",
            Licenca = $"LIC-{Guid.NewGuid():N}"[..12],
            Aktiv = true
        });

        var response = await _client.GetAsync("/api/terapistet?search=UniqueTerapist");
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
        var response = await _client.GetAsync("/api/terapistet?page=1&limit=5");
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

        var (_, createDoc) = await Post("/api/terapistet", NewTerapistDto());
        var id = GetId(createDoc);

        var response = await _client.GetAsync($"/api/terapistet/{id}");
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
        var response = await _client.GetAsync("/api/terapistet/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidInput_Returns201WithData()
    {
        await SetAdminAuthHeader();
        var (resp, doc) = await Post("/api/terapistet", NewTerapistDto());

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        Assert.True(IsSuccess(doc));
        Assert.True(GetId(doc) > 0);
    }

    [Fact]
    public async Task Create_DuplicateEmail_Returns400()
    {
        await SetAdminAuthHeader();
        var email = $"dup_{Guid.NewGuid()}@test.com";
        await Post("/api/terapistet", NewTerapistDto(email));
        var (resp, doc) = await Post("/api/terapistet", NewTerapistDto(email));

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        Assert.False(doc.GetProperty("success").GetBoolean());
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_ExistingId_Returns200WithUpdatedData()
    {
        await SetAdminAuthHeader();

        var (_, createDoc) = await Post("/api/terapistet", NewTerapistDto());
        var id = GetId(createDoc);
        var newEmail = $"updated_{Guid.NewGuid()}@test.com";

        var response = await _client.PutAsJsonAsync($"/api/terapistet/{id}", new
        {
            Emri = "UpdatedEmri",
            Mbiemri = "UpdatedMbiemri",
            Email = newEmail,
            Telefoni = "0699999999",
            Specializimi = "Akupunkture",
            Licenca = $"LIC-{Guid.NewGuid():N}"[..12],
            Aktiv = true
        });
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(IsSuccess(doc));
        var data = Payload(doc);
        Assert.Equal("UpdatedEmri", data.GetProperty("emri").GetString());
        Assert.Equal(newEmail, data.GetProperty("email").GetString());
    }

    [Fact]
    public async Task Update_NonExistentId_Returns404()
    {
        await SetAdminAuthHeader();
        var response = await _client.PutAsJsonAsync("/api/terapistet/999999", new
        {
            Emri = "X",
            Mbiemri = "Y",
            Email = "x@example.com",
            Telefoni = "0691234567",
            Specializimi = "Z",
            Licenca = $"LIC-{Guid.NewGuid():N}"[..12],
            Aktiv = true
        });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_DuplicateEmail_Returns400()
    {
        await SetAdminAuthHeader();

        var email1 = $"t1_{Guid.NewGuid()}@test.com";
        var email2 = $"t2_{Guid.NewGuid()}@test.com";
        await Post("/api/terapistet", NewTerapistDto(email1));
        var (_, doc2) = await Post("/api/terapistet", NewTerapistDto(email2));
        var id2 = GetId(doc2);

        var response = await _client.PutAsJsonAsync($"/api/terapistet/{id2}", new
        {
            Emri = "X",
            Mbiemri = "Y",
            Email = email1,
            Telefoni = "0691234567",
            Specializimi = "Z",
            Licenca = $"LIC-{Guid.NewGuid():N}"[..12],
            Aktiv = true
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ExistingId_Returns200()
    {
        await SetAdminAuthHeader();

        var (_, createDoc) = await Post("/api/terapistet", NewTerapistDto());
        var id = GetId(createDoc);

        var response = await _client.DeleteAsync($"/api/terapistet/{id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExistentId_Returns404()
    {
        await SetAdminAuthHeader();
        var response = await _client.DeleteAsync("/api/terapistet/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ThenGetById_Returns404()
    {
        await SetAdminAuthHeader();

        var (_, createDoc) = await Post("/api/terapistet", NewTerapistDto());
        var id = GetId(createDoc);

        await _client.DeleteAsync($"/api/terapistet/{id}");

        var response = await _client.GetAsync($"/api/terapistet/{id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
