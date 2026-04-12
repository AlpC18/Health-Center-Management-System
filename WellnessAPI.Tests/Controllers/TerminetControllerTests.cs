using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using WellnessAPI.Tests.Infrastructure;

namespace WellnessAPI.Tests.Controllers;

public class TerminetControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TerminetControllerTests(CustomWebApplicationFactory factory)
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

    private async Task<int> CreateKlient()
    {
        var (_, doc) = await Post("/api/klientet", new
        {
            Emri = "Test",
            Mbiemri = "Klient",
            Email = $"klient_{Guid.NewGuid()}@test.com",
            Telefoni = "0691234567",
            Adresa = "Rruga e Testit 1"
        });
        return doc.GetProperty("data").GetProperty("Id").GetInt32();
    }

    private async Task<int> CreateTerapist()
    {
        var (_, doc) = await Post("/api/terapistet", new
        {
            Emri = "Test",
            Mbiemri = "Terapist",
            Email = $"terapist_{Guid.NewGuid()}@test.com",
            Telefoni = "0691234567",
            Specializimi = "Masazh"
        });
        return doc.GetProperty("data").GetProperty("Id").GetInt32();
    }

    private async Task<int> CreateSherbim()
    {
        var (_, doc) = await Post("/api/sherbimet", new
        {
            Emri = $"Sherbim_{Guid.NewGuid():N}",
            Pershkrimi = "Pershkrim testues",
            Cmimi = 2500m,
            Kohezgjatja = 60
        });
        return doc.GetProperty("data").GetProperty("Id").GetInt32();
    }

    private async Task<(int KlientId, int TerapistId, int SherbimId)> CreatePrerequisites()
    {
        var klientId = await CreateKlient();
        var terapistId = await CreateTerapist();
        var sherbimId = await CreateSherbim();
        return (klientId, terapistId, sherbimId);
    }

    private static object NewTerminDto(int klientId, int terapistId, int sherbimId) => new
    {
        KlientId = klientId,
        TerapistId = terapistId,
        SherbimId = sherbimId,
        DataOra = DateTime.UtcNow.AddDays(1).ToString("o"),
        Shenime = "Shenim testues"
    };

    // ── Authorization ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/terminet");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/terminet/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var (resp, _) = await Post("/api/terminet", new { KlientId = 1, TerapistId = 1, SherbimId = 1, DataOra = DateTime.UtcNow, Shenime = "" });
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── GetAll ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_WithToken_Returns200WithSuccessTrue()
    {
        await SetAdminAuthHeader();
        var response = await _client.GetAsync("/api/terminet");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());
        Assert.True(doc.TryGetProperty("data", out _));
        Assert.True(doc.TryGetProperty("total", out _));
    }

    [Fact]
    public async Task GetAll_WithPagination_Returns200()
    {
        await SetAdminAuthHeader();
        var response = await _client.GetAsync("/api/terminet?page=1&limit=5");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());
        Assert.Equal(1, doc.GetProperty("page").GetInt32());
        Assert.Equal(5, doc.GetProperty("limit").GetInt32());
    }

    [Fact]
    public async Task GetAll_WithKlientIdFilter_Returns200()
    {
        await SetAdminAuthHeader();
        var (klientId, terapistId, sherbimId) = await CreatePrerequisites();
        await Post("/api/terminet", NewTerminDto(klientId, terapistId, sherbimId));

        var response = await _client.GetAsync($"/api/terminet?klientId={klientId}");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());
        Assert.True(doc.GetProperty("total").GetInt32() >= 1);
    }

    [Fact]
    public async Task GetAll_WithTerapistIdFilter_Returns200()
    {
        await SetAdminAuthHeader();
        var (klientId, terapistId, sherbimId) = await CreatePrerequisites();
        await Post("/api/terminet", NewTerminDto(klientId, terapistId, sherbimId));

        var response = await _client.GetAsync($"/api/terminet?terapistId={terapistId}");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());
        Assert.True(doc.GetProperty("total").GetInt32() >= 1);
    }

    [Fact]
    public async Task GetAll_WithStatusiFilter_Returns200()
    {
        await SetAdminAuthHeader();
        var response = await _client.GetAsync("/api/terminet?statusi=Planifikuar");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());
    }

    // ── GetById ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingId_Returns200WithData()
    {
        await SetAdminAuthHeader();
        var (klientId, terapistId, sherbimId) = await CreatePrerequisites();

        var (_, createDoc) = await Post("/api/terminet", NewTerminDto(klientId, terapistId, sherbimId));
        var id = createDoc.GetProperty("data").GetProperty("Id").GetInt32();

        var response = await _client.GetAsync($"/api/terminet/{id}");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());
        Assert.Equal(id, doc.GetProperty("data").GetProperty("Id").GetInt32());
    }

    [Fact]
    public async Task GetById_ExistingId_HasNavigationFields()
    {
        await SetAdminAuthHeader();
        var (klientId, terapistId, sherbimId) = await CreatePrerequisites();

        var (_, createDoc) = await Post("/api/terminet", NewTerminDto(klientId, terapistId, sherbimId));
        var id = createDoc.GetProperty("data").GetProperty("Id").GetInt32();

        var response = await _client.GetAsync($"/api/terminet/{id}");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);
        var data = doc.GetProperty("data");

        Assert.True(data.TryGetProperty("KlientId", out _));
        Assert.True(data.TryGetProperty("TerapistId", out _));
        Assert.True(data.TryGetProperty("SherbimId", out _));
        Assert.True(data.TryGetProperty("DataOra", out _));
        Assert.True(data.TryGetProperty("Statusi", out _));
    }

    [Fact]
    public async Task GetById_NonExistentId_Returns404()
    {
        await SetAdminAuthHeader();
        var response = await _client.GetAsync("/api/terminet/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidInput_Returns201WithData()
    {
        await SetAdminAuthHeader();
        var (klientId, terapistId, sherbimId) = await CreatePrerequisites();

        var (resp, doc) = await Post("/api/terminet", NewTerminDto(klientId, terapistId, sherbimId));

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());
        Assert.True(doc.GetProperty("data").GetProperty("Id").GetInt32() > 0);
    }

    [Fact]
    public async Task Create_ValidInput_HasCorrectKlientAndTerapist()
    {
        await SetAdminAuthHeader();
        var (klientId, terapistId, sherbimId) = await CreatePrerequisites();

        var (_, doc) = await Post("/api/terminet", NewTerminDto(klientId, terapistId, sherbimId));
        var data = doc.GetProperty("data");

        Assert.Equal(klientId, data.GetProperty("KlientId").GetInt32());
        Assert.Equal(terapistId, data.GetProperty("TerapistId").GetInt32());
        Assert.Equal(sherbimId, data.GetProperty("SherbimId").GetInt32());
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_ExistingId_Returns200WithUpdatedData()
    {
        await SetAdminAuthHeader();
        var (klientId, terapistId, sherbimId) = await CreatePrerequisites();

        var (_, createDoc) = await Post("/api/terminet", NewTerminDto(klientId, terapistId, sherbimId));
        var id = createDoc.GetProperty("data").GetProperty("Id").GetInt32();

        var newDataOra = DateTime.UtcNow.AddDays(7).ToString("o");
        var response = await _client.PutAsJsonAsync($"/api/terminet/{id}", new
        {
            KlientId = klientId,
            TerapistId = terapistId,
            SherbimId = sherbimId,
            DataOra = newDataOra,
            Statusi = "Konfirmuar",
            Shenime = "Shenim i ri"
        });
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());
        Assert.Equal("Konfirmuar", doc.GetProperty("data").GetProperty("Statusi").GetString());
    }

    [Fact]
    public async Task Update_NonExistentId_Returns404()
    {
        await SetAdminAuthHeader();
        var (klientId, terapistId, sherbimId) = await CreatePrerequisites();

        var response = await _client.PutAsJsonAsync("/api/terminet/999999", new
        {
            KlientId = klientId,
            TerapistId = terapistId,
            SherbimId = sherbimId,
            DataOra = DateTime.UtcNow.AddDays(1).ToString("o"),
            Statusi = "Planifikuar",
            Shenime = ""
        });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ExistingId_Returns200()
    {
        await SetAdminAuthHeader();
        var (klientId, terapistId, sherbimId) = await CreatePrerequisites();

        var (_, createDoc) = await Post("/api/terminet", NewTerminDto(klientId, terapistId, sherbimId));
        var id = createDoc.GetProperty("data").GetProperty("Id").GetInt32();

        var response = await _client.DeleteAsync($"/api/terminet/{id}");
        var body = await response.Content.ReadAsStringAsync();
        var doc = ParseBody(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(doc.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task Delete_NonExistentId_Returns404()
    {
        await SetAdminAuthHeader();
        var response = await _client.DeleteAsync("/api/terminet/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ThenGetById_Returns404()
    {
        await SetAdminAuthHeader();
        var (klientId, terapistId, sherbimId) = await CreatePrerequisites();

        var (_, createDoc) = await Post("/api/terminet", NewTerminDto(klientId, terapistId, sherbimId));
        var id = createDoc.GetProperty("data").GetProperty("Id").GetInt32();

        await _client.DeleteAsync($"/api/terminet/{id}");

        var response = await _client.GetAsync($"/api/terminet/{id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
