using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WellnessAPI.Data;
using WellnessAPI.DTOs;
using WellnessAPI.Models.Domain;
using WellnessAPI.Services;

namespace WellnessAPI.Controllers;

/// <summary>
/// Kontrolluesi për statistikat dhe analizat e dashboard-it.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public DashboardController(ApplicationDbContext db) => _db = db;

    /// <summary>
    /// Merr statistikat kryesore për kartat e dashboard-it.
    /// Pranon date-range opsional (from/to) për filtrim të të ardhurave dhe termineve.
    /// </summary>
    /// <returns>Një objekt me shifrat kryesore të sistemit.</returns>
    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetStats(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        // Use explicit date ranges + straight comparisons so filtering runs server-side
        // on any provider (MySQL/MariaDB). Aggregates (SUM/AVG/COUNT) also run in SQL.
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        // If caller supplied a range, use it for sales total + appointments-in-range.
        // Otherwise default to "current month" for sales and "today" for appointments
        // (preserves existing behaviour for clients that don't send the params).
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1);
        var rangeFrom = from?.Date ?? monthStart;
        var rangeToExclusive = to.HasValue ? to.Value.Date.AddDays(1) : monthEnd;

        var income = await _db.ShitjetProduktet
            .Where(s => s.DataShitjes >= rangeFrom && s.DataShitjes < rangeToExclusive)
            .SumAsync(s => s.CmimiTotal);

        var terminetInRange = (from.HasValue || to.HasValue)
            ? await _db.Terminet.CountAsync(t => t.DataTerminit >= rangeFrom && t.DataTerminit < rangeToExclusive)
            : await _db.Terminet.CountAsync(t => t.DataTerminit >= today && t.DataTerminit < tomorrow);

        return Ok(new DashboardStatsDto(
            TotalKlientet: await _db.Klientet.CountAsync(),
            TotalTerminet: await _db.Terminet.CountAsync(),
            TerminetSot: terminetInRange,
            AnetaresimiAktiv: await _db.Anetaresimet.CountAsync(a => a.Statusi == "Aktiv"),
            TeDheratMujore: income,
            TerapistetAktiv: await _db.Terapistet.CountAsync(t => t.Aktiv),
            ProductetNeStok: await _db.Produktet.CountAsync(p => p.SasiaStok > 0),
            NotaMesatare: await _db.Vlereisimet.AverageAsync(v => (double?)v.Nota) ?? 0
        ));
    }

    /// <summary>
    /// Merr të dhënat analitike për grafikë (tendencat mujore dhe shërbimet popullore).
    /// Date-range opsional: nëse jepet, trendi filtrohet brenda intervalit.
    /// </summary>
    /// <returns>Të dhëna të strukturuara për grafikët e frontend-it.</returns>
    [HttpGet("analytics")]
    public async Task<ActionResult> GetAnalytics(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var trends = new List<object>();

        if (from.HasValue && to.HasValue)
        {
            // Filtered monthly trend within explicit range
            var start = new DateTime(from.Value.Year, from.Value.Month, 1);
            var endExclusive = new DateTime(to.Value.Year, to.Value.Month, 1).AddMonths(1);
            for (var m = start; m < endExclusive; m = m.AddMonths(1))
            {
                var monthEnd = m.AddMonths(1);
                var sales = await _db.ShitjetProduktet
                    .Where(s => s.DataShitjes >= m && s.DataShitjes < monthEnd)
                    .SumAsync(s => (double)s.CmimiTotal);
                trends.Add(new { month = m.ToString("MMM yy"), revenue = sales });
            }
        }
        else
        {
            // Default: last 6 months
            var last6Months = Enumerable.Range(0, 6).Select(i => DateTime.UtcNow.AddMonths(-i)).Reverse();
            foreach (var month in last6Months)
            {
                var monthStart = new DateTime(month.Year, month.Month, 1);
                var monthEnd = monthStart.AddMonths(1);
                var sales = await _db.ShitjetProduktet
                    .Where(s => s.DataShitjes >= monthStart && s.DataShitjes < monthEnd)
                    .SumAsync(s => (double)s.CmimiTotal);
                trends.Add(new { month = month.ToString("MMM"), revenue = sales });
            }
        }

        // Service popularity, optionally restricted by date range on the termin date
        IQueryable<WellnessAPI.Models.Domain.Termin> terminetQuery = _db.Terminet.Include(t => t.Sherbimi);
        if (from.HasValue) terminetQuery = terminetQuery.Where(t => t.DataTerminit >= from.Value.Date);
        if (to.HasValue) terminetQuery = terminetQuery.Where(t => t.DataTerminit < to.Value.Date.AddDays(1));

        var services = await terminetQuery
            .GroupBy(t => t.Sherbimi.EmriSherbimit)
            .Select(g => new { name = g.Key, value = g.Count() })
            .OrderByDescending(x => x.value)
            .Take(5)
            .ToListAsync();

        return Ok(new { trends, services });
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult> GetLowStock([FromQuery] int threshold = 10)
    {
        var products = await _db.Produktet
            .Where(p => p.Aktiv && p.SasiaStok <= threshold)
            .OrderBy(p => p.SasiaStok)
            .Select(p => new { p.ProduktId, p.EmriProduktit, p.SasiaStok, p.Kategoria })
            .ToListAsync();
        return Ok(new { data = products, total = products.Count, threshold });
    }
}
