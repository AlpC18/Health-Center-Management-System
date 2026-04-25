using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WellnessAPI.Data;
using WellnessAPI.DTOs;
using WellnessAPI.Models.Domain;
using WellnessAPI.Services;
using WellnessAPI.Hubs;

namespace WellnessAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class KlientetController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    private readonly IHubContext<NotificationHub> _hub;
    private readonly FileUploadService _fileService;
    private readonly EmailService _email;

    public KlientetController(ApplicationDbContext db, AuditService audit, IHubContext<NotificationHub> hub, FileUploadService fileService, EmailService email)
    {
        _db = db;
        _audit = audit;
        _hub = hub;
        _fileService = fileService;
        _email = email;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<KlientResponseDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        var q = _db.Klientet.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower();
            q = q.Where(k =>
                k.Emri.ToLower().Contains(s) ||
                k.Mbiemri.ToLower().Contains(s) ||
                k.Email.ToLower().Contains(s));
        }
        var total = await q.CountAsync();
        var data = await q
            .OrderByDescending(k => k.DataRegjistrimit)
            .Skip((page - 1) * limit).Take(limit)
            .Select(k => ToDto(k)).ToListAsync();
        return Ok(new { data, total, page, limit });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<KlientResponseDto>> GetById(int id)
    {
        var k = await _db.Klientet.FindAsync(id);
        if (k is null) return NotFound(new { message = $"Klienti #{id} nuk u gjet." });
        return Ok(ToDto(k));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Therapist")]
    public async Task<ActionResult<KlientResponseDto>> Create([FromBody] KlientCreateDto dto)
    {
        if (await _db.Klientet.AnyAsync(k => k.Email == dto.Email))
            return Conflict(new { message = "Email tashmë ekziston." });

        var k = new Klient
        {
            Emri = dto.Emri, Mbiemri = dto.Mbiemri, Email = dto.Email,
            Telefoni = dto.Telefoni, DataLindjes = dto.DataLindjes,
            Gjinia = dto.Gjinia, KushtetShendetesore = dto.KushtetShendetesore,
            DataRegjistrimit = DateTime.UtcNow
        };
        _db.Klientet.Add(k);
        await _db.SaveChangesAsync();
        
        await _audit.LogAsync("CREATE", "Klient", k.KlientId.ToString(), null, dto);
        
        // SignalR: Notify all connected clients about new client
        await _hub.Clients.All.SendAsync("ReceiveNotification", $"Klienti i ri u shtua: {k.Emri} {k.Mbiemri}");

        // Email: Send welcome email (async - don't wait for it if you want faster response, but better to await or fire & forget carefully)
        try {
            await _email.SendWelcomeEmailAsync(k.Email, k.Emri);
        } catch {
            // Log email failure but don't crash the request
        }

        return CreatedAtAction(nameof(GetById), new { id = k.KlientId }, ToDto(k));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Therapist")]
    public async Task<ActionResult<KlientResponseDto>> Update(
        int id, [FromBody] KlientUpdateDto dto)
    {
        var k = await _db.Klientet.FindAsync(id);
        if (k is null) return NotFound();
        if (await _db.Klientet.AnyAsync(x => x.Email == dto.Email && x.KlientId != id))
            return Conflict(new { message = "Email tashmë përdoret." });

        var old = ToDto(k);
        k.Emri = dto.Emri; k.Mbiemri = dto.Mbiemri; k.Email = dto.Email;
        k.Telefoni = dto.Telefoni; k.DataLindjes = dto.DataLindjes;
        k.Gjinia = dto.Gjinia; k.KushtetShendetesore = dto.KushtetShendetesore;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("UPDATE", "Klient", id.ToString(), old, dto);
        return Ok(ToDto(k));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Therapist")]
    public async Task<IActionResult> Delete(int id)
    {
        var k = await _db.Klientet.FindAsync(id);
        if (k is null) return NotFound();
        var old = ToDto(k);
        try
        {
            _fileService.DeleteFile(k.FotoPath);
            _db.Klientet.Remove(k);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "Klienti nuk mund të fshihet sepse ka të dhëna të lidhura (termine/anëtarësime/shitje)." });
        }
        await _audit.LogAsync("DELETE", "Klient", id.ToString(), old, null);
        return NoContent();
    }

    [HttpPost("{id:int}/foto")]
    [Authorize(Roles = "Admin,Therapist")]
    public async Task<ActionResult<KlientResponseDto>> UploadFoto(int id, IFormFile file)
    {
        var k = await _db.Klientet.FindAsync(id);
        if (k is null) return NotFound();

        var path = await _fileService.UploadFileAsync(file, "klientet");
        if (path != null)
        {
            _fileService.DeleteFile(k.FotoPath);
            k.FotoPath = path;
            await _db.SaveChangesAsync();
            await _audit.LogAsync("UPDATE_FOTO", "Klient", id.ToString(), null, new { path });
        }
        return Ok(ToDto(k));
    }

    private static KlientResponseDto ToDto(Klient k) => new(
        k.KlientId, k.Emri, k.Mbiemri, k.Email, k.Telefoni,
        k.DataLindjes, k.Gjinia, k.KushtetShendetesore, k.FotoPath, k.DataRegjistrimit);
}
