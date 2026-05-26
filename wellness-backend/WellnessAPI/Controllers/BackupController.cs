using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WellnessAPI.Data;

namespace WellnessAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class BackupController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public BackupController(ApplicationDbContext db) => _db = db;

    /// <summary>
    /// Exports a logical, portable backup of all business data as JSON.
    /// Provider-agnostic (works on MySQL, SQLite, etc.). Authentication and
    /// token tables are intentionally excluded for security.
    /// </summary>
    [HttpGet("database")]
    public async Task<IActionResult> ExportDatabase()
    {
        var backup = new
        {
            generatedAt = DateTime.UtcNow,
            klientet = await _db.Klientet.AsNoTracking().ToListAsync(),
            sherbimet = await _db.Sherbimet.AsNoTracking().ToListAsync(),
            terapistet = await _db.Terapistet.AsNoTracking().ToListAsync(),
            terminet = await _db.Terminet.AsNoTracking().ToListAsync(),
            paketaWellness = await _db.PaketaWellness.AsNoTracking().ToListAsync(),
            anetaresimet = await _db.Anetaresimet.AsNoTracking().ToListAsync(),
            programet = await _db.Programet.AsNoTracking().ToListAsync(),
            klientProgramet = await _db.KlientProgramet.AsNoTracking().ToListAsync(),
            produktet = await _db.Produktet.AsNoTracking().ToListAsync(),
            shitjetProduktet = await _db.ShitjetProduktet.AsNoTracking().ToListAsync(),
            vlereisimet = await _db.Vlereisimet.AsNoTracking().ToListAsync(),
            sallat = await _db.Sallat.AsNoTracking().ToListAsync(),
            furnizuesit = await _db.Furnizuesit.AsNoTracking().ToListAsync(),
            lajmerimet = await _db.Lajmerimet.AsNoTracking().ToListAsync(),
            zbritjet = await _db.Zbritjet.AsNoTracking().ToListAsync(),
            pushimet = await _db.Pushimet.AsNoTracking().ToListAsync(),
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        var json = JsonSerializer.Serialize(backup, options);
        var bytes = Encoding.UTF8.GetBytes(json);
        var fileName = $"WellnessBackup_{DateTime.Now:yyyyMMdd_HHmm}.json";
        return File(bytes, "application/json", fileName);
    }
}
