using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WellnessAPI.Data;
using WellnessAPI.DTOs;

namespace WellnessAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public DashboardController(ApplicationDbContext db) => _db = db;

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var today = DateTime.UtcNow.Date;
        var firstOfMonth = new DateTime(today.Year, today.Month, 1);

        var totalKlientet = await _db.Klientet.CountAsync();
        var totalTerminet = await _db.Terminet.CountAsync();
        var terminetSot = await _db.Terminet.CountAsync(t => t.DataTerminit.Date == today);
        var anetaresimiAktiv = await _db.Anetaresimet.CountAsync(a => a.DataMbarimit >= today);
        var teDheratMujore = await _db.Anetaresimet
            .Where(a => a.DataFillimit >= firstOfMonth)
            .SumAsync(a => (decimal?)a.CmimiPaguar) ?? 0m;
        var terapistetAktiv = await _db.Terapistet.CountAsync(t => t.Aktiv);
        var produktetNeStok = await _db.Produktet.CountAsync(p => p.SasiaStok > 0);
        var notaMesatare = await _db.Vlereisimet.AverageAsync(v => (double?)v.Nota) ?? 0.0;

        return Ok(new DashboardStatsDto(
            totalKlientet,
            totalTerminet,
            terminetSot,
            anetaresimiAktiv,
            teDheratMujore,
            terapistetAktiv,
            produktetNeStok,
            notaMesatare
        ));
    }
}
