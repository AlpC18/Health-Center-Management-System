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

    private object NewKlientDto(string? email = null) => new
    {
        Emri = "Test",
        Mbiemri = "Klient",
        Email = email ?? $"klient_{Guid.NewGuid()}@test.com",
        Telefoni = "0691234567",
        Adresa = "Rruga e Testit 1"
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
        Assert.True(doc.GetProperty("success").GetBoolean());
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
            Adresa = "Adresa"
        });

        var response = await _client.GetAsync("/api/klientet?search=UniqueSearchName");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());
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
        Assert.True(doc.GetProperty("success").GetBoolean());
        Assert.Equal(1, doc.GetProperty("page").GetInt32());
        Assert.Equal(5, doc.GetProperty("limit").GetInt32());
    }

    // ── GetById ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingId_Returns200WithData()
    {
        await SetAdminAuthHeader();

        var (_, createDoc) = await Post("/api/klientet", NewKlientDto());
        var id = createDoc.GetProperty("data").GetProperty("Id").GetInt32();

        var response = await _client.GetAsync($"/api/klientet/{id}");
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
        Assert.True(doc.GetProperty("success").GetBoolean());
        Assert.True(doc.GetProperty("data").GetProperty("Id").GetInt32() > 0);
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
        var id = createDoc.GetProperty("data").GetProperty("Id").GetInt32();
        var newEmail = $"updated_{Guid.NewGuid()}@test.com";

        var response = await _client.PutAsJsonAsync($"/api/klientet/{id}", new
        {
            Emri = "Updated",
            Mbiemri = "Name",
            Email = newEmail,
            Telefoni = "0699999999",
            Adresa = "New Address",
            IsActive = true
        });
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());
        Assert.Equal("Updated", doc.GetProperty("data").GetProperty("Emri").GetString());
        Assert.Equal(newEmail, doc.GetProperty("data").GetProperty("Email").GetString());
    }

    [Fact]
    public async Task Update_NonExistentId_Returns404()
    {
        await SetAdminAuthHeader();
        var response = await _client.PutAsJsonAsync("/api/klientet/999999", new
        {
            Emri = "X", Mbiemri = "Y", Email = "x@y.com", Telefoni = "0", Adresa = "A", IsActive = true
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
        var id2 = doc2.GetProperty("data").GetProperty("Id").GetInt32();

        // Try to update klient2 with klient1's email
        var response = await _client.PutAsJsonAsync($"/api/klientet/{id2}", new
        {
            Emri = "X", Mbiemri = "Y", Email = email1, Telefoni = "0", Adresa = "A", IsActive = true
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ExistingId_Returns200()
    {
        await SetAdminAuthHeader();

        var (_, createDoc) = await Post("/api/klientet", NewKlientDto());
        var id = createDoc.GetProperty("data").GetProperty("Id").GetInt32();

        var response = await _client.DeleteAsync($"/api/klientet/{id}");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());
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
        var id = createDoc.GetProperty("data").GetProperty("Id").GetInt32();

        await _client.DeleteAsync($"/api/klientet/{id}");

        var response = await _client.GetAsync($"/api/klientet/{id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
