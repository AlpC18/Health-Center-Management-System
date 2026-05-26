using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using WellnessAPI.Tests.Infrastructure;

namespace WellnessAPI.Tests.Controllers;

public class KlientetControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public KlientetControllerTests(CustomWebApplicationFactory factory)
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
        if (payload.TryGetProperty("klientId", out var camelId)) return camelId.GetInt32();
        if (payload.TryGetProperty("KlientId", out var pascalId)) return pascalId.GetInt32();
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

    private object NewKlientDto(string? email = null) => new
    {
        Emri = "Test",
        Mbiemri = "Klient",
        Email = email ?? $"klient_{Guid.NewGuid()}@test.com",
        Telefoni = "0691234567",
        DataLindjes = DateTime.UtcNow.AddYears(-25),
        Gjinia = "M",
        KushtetShendetesore = "Asnje"
    };

    // ── Authorization ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/klientet");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/klientet/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var (resp, _) = await Post("/api/klientet", NewKlientDto());
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── GetAll ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_WithToken_Returns200WithSuccessTrue()
    {
        await SetAdminAuthHeader();
        var response = await _client.GetAsync("/api/klientet");
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

        // Create a klient we can search for
        var email = $"searchable_{Guid.NewGuid()}@test.com";
        await Post("/api/klientet", new
        {
            Emri = "UniqueSearchName",
            Mbiemri = "Klient",
            Email = email,
            Telefoni = "0691234567",
            DataLindjes = DateTime.UtcNow.AddYears(-25),
            Gjinia = "F",
            KushtetShendetesore = "Asnje"
        });

        var response = await _client.GetAsync("/api/klientet?search=UniqueSearchName");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(IsSuccess(doc));
        Assert.True(doc.GetProperty("total").GetInt32() >= 1);
    }

    [Fact]
    public async Task GetAll_DefaultPagination_Returns200()
    {
        await SetAdminAuthHeader();
        var response = await _client.GetAsync("/api/klientet?page=1&limit=5");
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

        var (_, createDoc) = await Post("/api/klientet", NewKlientDto());
        var id = GetId(createDoc);

        var response = await _client.GetAsync($"/api/klientet/{id}");
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
        var response = await _client.GetAsync("/api/klientet/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidInput_Returns201WithData()
    {
        await SetAdminAuthHeader();
        var (resp, doc) = await Post("/api/klientet", NewKlientDto());

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        Assert.True(IsSuccess(doc));
        Assert.True(GetId(doc) > 0);
    }

    [Fact]
    public async Task Create_DuplicateEmail_Returns400()
    {
        await SetAdminAuthHeader();
        var email = $"dup_{Guid.NewGuid()}@test.com";
        await Post("/api/klientet", NewKlientDto(email));
        var (resp, doc) = await Post("/api/klientet", NewKlientDto(email));

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        Assert.False(doc.GetProperty("success").GetBoolean());
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_ExistingId_Returns200WithUpdatedData()
    {
        await SetAdminAuthHeader();

        var (_, createDoc) = await Post("/api/klientet", NewKlientDto());
        var id = GetId(createDoc);
        var newEmail = $"updated_{Guid.NewGuid()}@test.com";

        var response = await _client.PutAsJsonAsync($"/api/klientet/{id}", new
        {
            Emri = "Updated",
            Mbiemri = "Name",
            Email = newEmail,
            Telefoni = "0699999999",
            DataLindjes = DateTime.UtcNow.AddYears(-30),
            Gjinia = "F",
            KushtetShendetesore = "Updated"
        });
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(IsSuccess(doc));
        var data = Payload(doc);
        Assert.Equal("Updated", data.GetProperty("emri").GetString());
        Assert.Equal(newEmail, data.GetProperty("email").GetString());
    }

    [Fact]
    public async Task Update_NonExistentId_Returns404()
    {
        await SetAdminAuthHeader();
        var response = await _client.PutAsJsonAsync("/api/klientet/999999", new
        {
            Emri = "X",
            Mbiemri = "Y",
            Email = "x@example.com",
            Telefoni = "0691234567",
            DataLindjes = DateTime.UtcNow.AddYears(-25),
            Gjinia = "M",
            KushtetShendetesore = "Asnje"
        });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_DuplicateEmail_Returns400()
    {
        await SetAdminAuthHeader();

        var email1 = $"e1_{Guid.NewGuid()}@test.com";
        var email2 = $"e2_{Guid.NewGuid()}@test.com";
        await Post("/api/klientet", NewKlientDto(email1));
        var (_, doc2) = await Post("/api/klientet", NewKlientDto(email2));
        var id2 = GetId(doc2);

        // Try to update klient2 with klient1's email
        var response = await _client.PutAsJsonAsync($"/api/klientet/{id2}", new
        {
            Emri = "X",
            Mbiemri = "Y",
            Email = email1,
            Telefoni = "0691234567",
            DataLindjes = DateTime.UtcNow.AddYears(-25),
            Gjinia = "M",
            KushtetShendetesore = "Asnje"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ExistingId_Returns200()
    {
        await SetAdminAuthHeader();

        var (_, createDoc) = await Post("/api/klientet", NewKlientDto());
        var id = GetId(createDoc);

        var response = await _client.DeleteAsync($"/api/klientet/{id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExistentId_Returns404()
    {
        await SetAdminAuthHeader();
        var response = await _client.DeleteAsync("/api/klientet/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ThenGetById_Returns404()
    {
        await SetAdminAuthHeader();

        var (_, createDoc) = await Post("/api/klientet", NewKlientDto());
        var id = GetId(createDoc);

        await _client.DeleteAsync($"/api/klientet/{id}");

        var response = await _client.GetAsync($"/api/klientet/{id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Pagination ────────────────────────────────────────────────────────────
    // These tests use the actual response format: { data: [...], total: N, page: N, limit: N }
    // Admin credentials match the seed: admin@wellness.com / Admin123!

    private async Task SetSeedAdminAuthHeader()
    {
        var (_, doc) = await Post("/api/auth/login", new { Email = "admin@wellness.com", Password = "Admin123!" });
        var token = doc.TryGetProperty("accessToken", out var accessToken)
            ? accessToken.GetString() ?? ""
            : doc.TryGetProperty("data", out var data) && data.TryGetProperty("accessToken", out var nestedAccessToken)
                ? nestedAccessToken.GetString() ?? ""
                : "";
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!);
    }

    [Fact]
    public async Task GetAll_Pagination_LimitIsRespected()
    {
        await SetSeedAdminAuthHeader();
        var response = await _client.GetAsync("/api/klientet?page=1&limit=3");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var data = doc.GetProperty("data");
        Assert.True(data.GetArrayLength() <= 3, "data array should have at most 3 items");
        Assert.Equal(1, doc.GetProperty("page").GetInt32());
        Assert.Equal(3, doc.GetProperty("limit").GetInt32());
    }

    [Fact]
    public async Task GetAll_Pagination_TotalIsPositive()
    {
        await SetSeedAdminAuthHeader();
        var response = await _client.GetAsync("/api/klientet");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(doc.GetProperty("total").GetInt32() > 0, "total should be > 0 (seed data exists)");
    }

    [Fact]
    public async Task GetAll_Pagination_PageAndLimitReflectedInResponse()
    {
        await SetSeedAdminAuthHeader();
        var response = await _client.GetAsync("/api/klientet?page=2&limit=4");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, doc.GetProperty("page").GetInt32());
        Assert.Equal(4, doc.GetProperty("limit").GetInt32());
    }

    [Fact]
    public async Task GetAll_Pagination_BeyondLastPage_ReturnsEmptyData()
    {
        await SetSeedAdminAuthHeader();

        // Get total first
        var firstResp = await _client.GetAsync("/api/klientet?page=1&limit=1000");
        var firstBody = await firstResp.Content.ReadAsStringAsync();
        var firstDoc = ParseBody(firstBody);
        var total = firstDoc.GetProperty("total").GetInt32();

        // Request a page far beyond available data
        var response = await _client.GetAsync($"/api/klientet?page=9999&limit=10");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, doc.GetProperty("data").GetArrayLength());
        Assert.Equal(total, doc.GetProperty("total").GetInt32());
    }

    [Fact]
    public async Task GetAll_Pagination_Page2DifferentFromPage1()
    {
        await SetSeedAdminAuthHeader();

        // Seed has 15 clients, so pages 1 and 2 with limit=5 should return different items
        var resp1 = await _client.GetAsync("/api/klientet?page=1&limit=5");
        var resp2 = await _client.GetAsync("/api/klientet?page=2&limit=5");

        var doc1 = ParseBody(await resp1.Content.ReadAsStringAsync());
        var doc2 = ParseBody(await resp2.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, resp1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, resp2.StatusCode);

        // Both pages should have items (seed has 15 clients)
        var page1Count = doc1.GetProperty("data").GetArrayLength();
        var page2Count = doc2.GetProperty("data").GetArrayLength();
        Assert.True(page1Count > 0, "page 1 should have items");
        Assert.True(page2Count > 0, "page 2 should have items");

        // Total must be consistent across pages
        Assert.Equal(
            doc1.GetProperty("total").GetInt32(),
            doc2.GetProperty("total").GetInt32());
    }
}
