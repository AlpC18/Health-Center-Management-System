using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WellnessAPI.Data;
using WellnessAPI.DTOs;
using WellnessAPI.Models.Domain;
using WellnessAPI.Models.Identity;

namespace WellnessAPI.Controllers;

[ApiController]
[Route("api/portal")]
[Authorize]
public class KlientPortalController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public KlientPortalController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    private async Task<int?> GetKlientIdAsync()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        var user = await _userManager.FindByIdAsync(userId!);
        if (user?.KlientId == null) return null;
        return int.TryParse(user.KlientId, out var id) ? id : null;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var klientId = await GetKlientIdAsync();
        if (klientId == null) return NotFound(new { message = "Profili nuk u gjet." });

        var terminet = await _db.Terminet
            .Where(t => t.KlientId == klientId).ToListAsync();

        var anetaresimi = await _db.Anetaresimet
            .Where(a => a.KlientId == klientId && a.Statusi == "Aktiv")
            .CountAsync();

        var totalShpenzuar = await _db.ShitjetProduktet
            .Where(s => s.KlientId == klientId)
            .SumAsync(s => (decimal?)s.CmimiTotal) ?? 0;

        var terminIArdhshem = await _db.Terminet
            .Include(t => t.Sherbimi)
            .Include(t => t.Terapisti)
            .Where(t => t.KlientId == klientId
                && t.DataTerminit >= DateTime.UtcNow
                && t.Statusi != "Anuluar")
            .OrderBy(t => t.DataTerminit)
            .FirstOrDefaultAsync();

        return Ok(new {
            totalTerminet = terminet.Count,
            terminetAktive = terminet.Count(t =>
                t.Statusi == "Planifikuar" || t.Statusi == "Konfirmuar"),
            anetaresimiAktiv = anetaresimi,
            totalShpenzuar,
            terminIArdhshem = terminIArdhshem == null ? null : new {
                terminId = terminIArdhshem.TerminId,
                sherbimEmri = terminIArdhshem.Sherbimi.EmriSherbimit,
                terapistEmri = $"{terminIArdhshem.Terapisti.Emri} {terminIArdhshem.Terapisti.Mbiemri}",
                dataTerminit = terminIArdhshem.DataTerminit,
                oraFillimit = terminIArdhshem.OraFillimit,
                oraMbarimit = terminIArdhshem.OraMbarimit,
                statusi = terminIArdhshem.Statusi,
            }
        });
    }

    [HttpGet("profili")]
    public async Task<IActionResult> GetProfili()
    {
        var klientId = await GetKlientIdAsync();
        if (klientId == null) return NotFound();
        var k = await _db.Klientet.FindAsync(klientId);
        if (k == null) return NotFound();
        return Ok(new KlientResponseDto(
            k.KlientId, k.Emri, k.Mbiemri, k.Email,
            k.Telefoni, k.DataLindjes, k.Gjinia,
            k.KushtetShendetesore, k.FotoPath, k.DataRegjistrimit));
    }

    [HttpPut("profili")]
    public async Task<IActionResult> UpdateProfili([FromBody] KlientUpdateDto dto)
    {
        var klientId = await GetKlientIdAsync();
        if (klientId == null) return NotFound();
        var k = await _db.Klientet.FindAsync(klientId);
        if (k == null) return NotFound();
        k.Emri = dto.Emri;
        k.Mbiemri = dto.Mbiemri;
        k.Telefoni = dto.Telefoni;
        k.DataLindjes = dto.DataLindjes;
        k.Gjinia = dto.Gjinia;
        k.KushtetShendetesore = dto.KushtetShendetesore;
        await _db.SaveChangesAsync();
        return Ok(new KlientResponseDto(
            k.KlientId, k.Emri, k.Mbiemri, k.Email,
            k.Telefoni, k.DataLindjes, k.Gjinia,
            k.KushtetShendetesore, k.FotoPath, k.DataRegjistrimit));
    }

    [HttpGet("terminet")]
    public async Task<IActionResult> GetTerminet([FromQuery] string? statusi)
    {
        var klientId = await GetKlientIdAsync();
        if (klientId == null) return NotFound();
        var q = _db.Terminet
            .Include(t => t.Sherbimi)
            .Include(t => t.Terapisti)
            .Where(t => t.KlientId == klientId)
            .AsQueryable();
        if (!string.IsNullOrEmpty(statusi))
            q = q.Where(t => t.Statusi == statusi);
        var terminet = await q.OrderByDescending(t => t.DataTerminit)
            .Select(t => new TerminResponseDto(
                t.TerminId, t.KlientId, "",
                t.SherbimId, t.Sherbimi.EmriSherbimit,
                t.TerapistId,
                $"{t.Terapisti.Emri} {t.Terapisti.Mbiemri}",
                t.DataTerminit, t.OraFillimit, t.OraMbarimit,
                t.Statusi, t.Shenimet))
            .ToListAsync();
        return Ok(terminet);
    }

    [HttpPost("terminet")]
    public async Task<IActionResult> CreateTermin([FromBody] PortalTerminCreateDto dto)
    {
        var klientId = await GetKlientIdAsync();
        if (klientId == null) return NotFound();

        var cakisma = await _db.Terminet.AnyAsync(t =>
            t.TerapistId == dto.TerapistId &&
            t.DataTerminit.Date == dto.DataTerminit.Date &&
            t.Statusi != "Anuluar" &&
            ((dto.OraFillimit >= t.OraFillimit && dto.OraFillimit < t.OraMbarimit) ||
             (dto.OraMbarimit > t.OraFillimit && dto.OraMbarimit <= t.OraMbarimit) ||
             (dto.OraFillimit <= t.OraFillimit && dto.OraMbarimit >= t.OraMbarimit)));

        if (cakisma)
            return Conflict(new {
                message = "Terapisti është i zënë në këtë orë. Zgjidhni orë tjetër."
            });

        var termin = new Termin
        {
            KlientId = klientId.Value,
            SherbimId = dto.SherbimId,
            TerapistId = dto.TerapistId,
            DataTerminit = dto.DataTerminit,
            OraFillimit = dto.OraFillimit,
            OraMbarimit = dto.OraMbarimit,
            Statusi = "Planifikuar",
            Shenimet = dto.Shenimet
        };
        _db.Terminet.Add(termin);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Termini u rezervua!", terminId = termin.TerminId });
    }

    [HttpDelete("terminet/{id:int}")]
    public async Task<IActionResult> AnnulTermin(int id)
    {
        var klientId = await GetKlientIdAsync();
        if (klientId == null) return NotFound();
        var termin = await _db.Terminet
            .FirstOrDefaultAsync(t => t.TerminId == id && t.KlientId == klientId);
        if (termin == null) return NotFound(new { message = "Termini nuk u gjet." });
        if (termin.DataTerminit <= DateTime.UtcNow.AddHours(24))
            return BadRequest(new {
                message = "Nuk mund të anulohet. Koha ka kaluar (24 orë para terminit)."
            });
        termin.Statusi = "Anuluar";
        await _db.SaveChangesAsync();
        return Ok(new { message = "Termini u anulua." });
    }

    [HttpGet("anetaresimi")]
    public async Task<IActionResult> GetAnetaresimi()
    {
        var klientId = await GetKlientIdAsync();
        if (klientId == null) return NotFound();
        var list = await _db.Anetaresimet
            .Include(a => a.Paketa)
            .Where(a => a.KlientId == klientId)
            .OrderByDescending(a => a.DataFillimit)
            .Select(a => new AnetaresimResponseDto(
                a.AnetaresimId, a.KlientId, "",
                a.PaketId, a.Paketa.EmriPaketes,
                a.DataFillimit, a.DataMbarimit,
                a.Statusi, a.CmimiPaguar))
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("sherbimet")]
    public async Task<IActionResult> GetSherbimet()
    {
        var list = await _db.Sherbimet.Where(s => s.Aktiv)
            .Select(s => new SherbimResponseDto(
                s.SherbimId, s.EmriSherbimit, s.Kategoria,
                s.Pershkrimi, s.KohezgjatjaMin, s.Cmimi, s.Aktiv))
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("terapistet")]
    public async Task<IActionResult> GetTerapistet()
    {
        var list = await _db.Terapistet.Where(t => t.Aktiv)
            .Select(t => new TerapistResponseDto(
                t.TerapistId, t.Emri, t.Mbiemri,
                t.Specializimi, t.Licenca,
                t.Email, t.Telefoni, t.Aktiv))
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("paketat")]
    public async Task<IActionResult> GetPaketat()
    {
        var list = await _db.PaketaWellness.Where(p => p.Aktive)
            .Select(p => new PaketaResponseDto(
                p.PaketId, p.EmriPaketes, p.Pershkrimi,
                p.SherbimiPerfshire, p.Cmimi,
                p.KohezgjatjaMuaj, p.Aktive))
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("produktet")]
    public async Task<IActionResult> GetProduktet()
    {
        var list = await _db.Produktet
            .Where(p => p.Aktiv && p.SasiaStok > 0)
            .Select(p => new ProduktResponseDto(
                p.ProduktId, p.EmriProduktit, p.Kategoria,
                p.Pershkrimi, p.Cmimi, p.SasiaStok, p.Aktiv))
            .ToListAsync();
        return Ok(list);
    }

    [HttpPost("produktet/blej")]
    public async Task<IActionResult> BlejProdukt([FromBody] ShitjeCreateDto dto)
    {
        var klientId = await GetKlientIdAsync();
        if (klientId == null) return NotFound();
        var produkt = await _db.Produktet.FindAsync(dto.ProduktId);
        if (produkt == null) return NotFound();
        if (produkt.SasiaStok < dto.Sasia)
            return BadRequest(new {
                message = $"Stoku i pamjaftueshëm. Disponueshëm: {produkt.SasiaStok}"
            });
        produkt.SasiaStok -= dto.Sasia;
        _db.ShitjetProduktet.Add(new ShitjeProdukteve {
            KlientId = klientId.Value,
            ProduktId = dto.ProduktId,
            Sasia = dto.Sasia,
            CmimiTotal = produkt.Cmimi * dto.Sasia,
            DataShitjes = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return Ok(new { message = "Produkti u ble!", cmimiTotal = produkt.Cmimi * dto.Sasia });
    }

    [HttpGet("shitjet")]
    public async Task<IActionResult> GetShitjet()
    {
        var klientId = await GetKlientIdAsync();
        if (klientId == null) return NotFound();
        var list = await _db.ShitjetProduktet
            .Include(s => s.Produkti)
            .Where(s => s.KlientId == klientId)
            .OrderByDescending(s => s.DataShitjes)
            .Select(s => new ShitjeResponseDto(
                s.ShitjeId, s.KlientId, "",
                s.ProduktId, s.Produkti.EmriProduktit,
                s.Sasia, s.CmimiTotal, s.DataShitjes))
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("vlereisimet")]
    public async Task<IActionResult> GetVlereisimet()
    {
        var klientId = await GetKlientIdAsync();
        if (klientId == null) return NotFound();
        var list = await _db.Vlereisimet
            .Include(v => v.Sherbimi)
            .Include(v => v.Terapisti)
            .Where(v => v.KlientId == klientId)
            .Select(v => new VleresimResponseDto(
                v.VleresimId, v.KlientId, "",
                v.SherbimId, v.Sherbimi.EmriSherbimit,
                v.TerapistId,
                $"{v.Terapisti.Emri} {v.Terapisti.Mbiemri}",
                v.Nota, v.Komenti, v.DataVleresimit))
            .ToListAsync();
        return Ok(list);
    }

    [HttpPost("vlereisimet")]
    public async Task<IActionResult> AddVleresim([FromBody] VleresimCreateDto dto)
    {
        var klientId = await GetKlientIdAsync();
        if (klientId == null) return NotFound();
        if (dto.Nota < 1 || dto.Nota > 5)
            return BadRequest(new { message = "Nota duhet të jetë 1-5." });
        var exists = await _db.Vlereisimet.AnyAsync(v =>
            v.KlientId == klientId &&
            v.SherbimId == dto.SherbimId &&
            v.TerapistId == dto.TerapistId);
        if (exists)
            return Conflict(new {
                message = "Tashmë keni vlerësuar këtë shërbim me këtë terapist."
            });
        _db.Vlereisimet.Add(new Vleresim {
            KlientId = klientId.Value,
            SherbimId = dto.SherbimId,
            TerapistId = dto.TerapistId,
            Nota = dto.Nota,
            Komenti = dto.Komenti,
            DataVleresimit = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return Ok(new { message = "Vlerësimi u shtua!" });
    }

    [HttpGet("programet")]
    public async Task<IActionResult> GetProgramet()
    {
        var klientId = await GetKlientIdAsync();
        if (klientId == null) return NotFound();
        var list = await _db.KlientProgramet
            .Include(kp => kp.Programi)
            .Where(kp => kp.KlientId == klientId)
            .Select(kp => new {
                kp.KpId,
                kp.DataFillimit,
                kp.DataMbarimit,
                kp.Progresi,
                kp.Statusi,
                program = new ProgramResponseDto(
                    kp.Programi.ProgramId,
                    kp.Programi.EmriProgramit,
                    kp.Programi.Pershkrimi,
                    kp.Programi.KohezgjatjaJave,
                    kp.Programi.Qellimi,
                    kp.Programi.Ushtrimet,
                    kp.Programi.Dieta)
            }).ToListAsync();
        return Ok(list);
    }
}
