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
    /// </summary>
    /// <returns>Një objekt me shifrat kryesore të sistemit.</returns>
    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetStats()
    {
        var today = DateTime.UtcNow.Date;
        
        // Summing logic needs to handle potential nulls
        var income = await _db.ShitjetProduktet
            .Where(s => s.DataShitjes.Month == DateTime.UtcNow.Month 
                     && s.DataShitjes.Year == DateTime.UtcNow.Year)
            .Select(s => s.CmimiTotal)
            .ToListAsync();
        
        var ratings = await _db.Vlereisimet
            .Select(v => (double)v.Nota)
            .ToListAsync();

        return Ok(new DashboardStatsDto(
            TotalKlientet: await _db.Klientet.CountAsync(),
            TotalTerminet: await _db.Terminet.CountAsync(),
            TerminetSot: await _db.Terminet.CountAsync(t => t.DataTerminit.Date == today),
            AnetaresimiAktiv: await _db.Anetaresimet.CountAsync(a => a.Statusi == "Aktiv"),
            TeDheratMujore: income.Sum(),
            TerapistetAktiv: await _db.Terapistet.CountAsync(t => t.Aktiv),
            ProductetNeStok: await _db.Produktet.CountAsync(p => p.SasiaStok > 0),
            NotaMesatare: ratings.Any() ? ratings.Average() : 0
        ));
    }

    /// <summary>
    /// Merr të dhënat analitike për grafikë (tendencat mujore dhe shërbimet popullore).
    /// </summary>
    /// <returns>Të dhëna të strukturuara për grafikët e frontend-it.</returns>
    [HttpGet("analytics")]
    public async Task<ActionResult> GetAnalytics()
    {
        // 1. Monthly Revenue Trend (Last 6 Months)
        var last6Months = Enumerable.Range(0, 6).Select(i => DateTime.UtcNow.AddMonths(-i)).Reverse();
        var trends = new List<object>();

        foreach (var month in last6Months)
        {
            var sales = await _db.ShitjetProduktet
                .Where(s => s.DataShitjes.Month == month.Month && s.DataShitjes.Year == month.Year)
                .SumAsync(s => (double)s.CmimiTotal);
            
            trends.Add(new { month = month.ToString("MMM"), revenue = sales });
        }

        // 2. Service Popularity (Top 5)
        var services = await _db.Terminet
            .Include(t => t.Sherbimi)
            .GroupBy(t => t.Sherbimi.EmriSherbimit)
            .Select(g => new { name = g.Key, value = g.Count() })
            .OrderByDescending(x => x.value)
            .Take(5)
            .ToListAsync();

        return Ok(new { trends, services });
    }
}
