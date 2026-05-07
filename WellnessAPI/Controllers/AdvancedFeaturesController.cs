using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WellnessAPI.Data;
using WellnessAPI.DTOs;
using WellnessAPI.Models.Domain;
using WellnessAPI.Services;

namespace WellnessAPI.Controllers;

[ApiController]
[Route("api/advanced")]
[Authorize]
public class AdvancedFeaturesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;

    public AdvancedFeaturesController(ApplicationDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet("appointments/availability")]
    public async Task<IActionResult> GetAvailability([FromQuery] int therapistId, [FromQuery] int serviceId, [FromQuery] DateTime date)
    {
        var service = await _db.Sherbimet.FindAsync(serviceId);
        var therapist = await _db.Terapistet.FindAsync(therapistId);
        if (service == null || therapist == null) return NotFound();

        var workStart = therapist.OrariFillimit;
        var workEnd = therapist.OrariMbarimit;
        var availability = await _db.TerapistAvailability
            .Where(a => a.TerapistId == therapistId && a.DitaJaves == date.DayOfWeek && a.Aktiv)
            .FirstOrDefaultAsync();

        if (availability != null)
        {
            workStart = availability.OraFillimit;
            workEnd = availability.OraMbarimit;
        }

        var appointments = await _db.Terminet
            .Where(t => t.TerapistId == therapistId && t.DataTerminit.Date == date.Date && t.Statusi != "Anuluar")
            .ToListAsync();

        var slots = new List<TimeSlotDto>();
        for (var start = workStart; start.Add(TimeSpan.FromMinutes(service.KohezgjatjaMin)) <= workEnd; start = start.Add(TimeSpan.FromMinutes(30)))
        {
            var end = start.Add(TimeSpan.FromMinutes(service.KohezgjatjaMin));
            var conflict = appointments.Any(t => start < t.OraMbarimit && end > t.OraFillimit);
            slots.Add(new TimeSlotDto(date.Date, start, end, !conflict));
        }

        return Ok(slots);
    }

    [HttpPost("appointments/{id:int}/cancel")]
    public async Task<IActionResult> CancelAppointment(int id, [FromBody] CancelTerminDto dto)
    {
        var appointment = await _db.Terminet.FindAsync(id);
        if (appointment == null) return NotFound();

        var oldStatus = appointment.Statusi;
        appointment.Statusi = "Anuluar";
        appointment.ArsyeAnulimi = dto.Reason;
        _db.TerminStatusHistory.Add(new TerminStatusHistory
        {
            TerminId = id,
            StatusiNga = oldStatus,
            StatusiNe = "Anuluar",
            Arsyeja = dto.Reason,
            CreatedBy = User.FindFirstValue(ClaimTypes.Email)
        });

        await _db.SaveChangesAsync();
        await _audit.LogAsync("Termin", "CANCEL", id.ToString(), oldValues: oldStatus, newValues: dto);
        return Ok(new { appointment.TerminId, appointment.Statusi, appointment.ArsyeAnulimi });
    }

    [HttpPost("waiting-list")]
    public async Task<IActionResult> AddWaitingListEntry([FromBody] WaitingListCreateDto dto)
    {
        var entry = new WaitingListEntry
        {
            KlientId = dto.KlientId,
            SherbimId = dto.SherbimId,
            TerapistId = dto.TerapistId,
            PreferredDate = dto.PreferredDate
        };
        _db.WaitingList.Add(entry);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("WaitingList", "CREATE", entry.WaitingListEntryId.ToString(), newValues: dto);
        return CreatedAtAction(nameof(GetWaitingList), new { id = entry.WaitingListEntryId }, entry);
    }

    [HttpGet("waiting-list")]
    public async Task<IActionResult> GetWaitingList() =>
        Ok(await _db.WaitingList.AsNoTracking().OrderByDescending(w => w.CreatedAt).ToListAsync());

    [HttpPost("packages/{id:int}/services")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdatePackageServices(int id, [FromBody] PackageServicesUpdateDto dto)
    {
        if (!await _db.PaketaWellness.AnyAsync(p => p.PaketId == id)) return NotFound();

        var current = await _db.PaketaSherbimet.Where(ps => ps.PaketId == id).ToListAsync();
        _db.PaketaSherbimet.RemoveRange(current);
        _db.PaketaSherbimet.AddRange(dto.Services.Select(s => new PaketaSherbim
        {
            PaketId = id,
            SherbimId = s.SherbimId,
            SeancaPerfshire = s.SeancaPerfshire
        }));

        await _db.SaveChangesAsync();
        await _audit.LogAsync("PaketaWellness", "UPDATE_SERVICES", id.ToString(), oldValues: current, newValues: dto);
        return NoContent();
    }

    [HttpPost("sales")]
    public async Task<IActionResult> CreateSaleWithStock([FromBody] ShitjeCreateDto dto)
    {
        var product = await _db.Produktet.FindAsync(dto.ProduktId);
        if (product == null) return NotFound("Product not found.");
        if (product.SasiaStok < dto.Sasia) return BadRequest("Insufficient stock.");

        product.SasiaStok -= dto.Sasia;
        var sale = new ShitjeProdukteve
        {
            KlientId = dto.KlientId,
            ProduktId = dto.ProduktId,
            Sasia = dto.Sasia,
            CmimiTotal = dto.CmimiTotal,
            DataShitjes = dto.DataShitjes,
            PaymentStatus = "Paid"
        };
        _db.ShitjetProduktet.Add(sale);
        await _db.SaveChangesAsync();

        _db.Invoices.Add(new Invoice
        {
            ShitjeId = sale.ShitjeId,
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{sale.ShitjeId:D5}",
            Amount = sale.CmimiTotal
        });

        if (product.SasiaStok <= product.MinimumStock)
        {
            _db.Notifications.Add(new Notification
            {
                Type = "LowStock",
                Title = "Low stock alert",
                Message = $"{product.EmriProduktit} has {product.SasiaStok} items left."
            });
        }

        await _db.SaveChangesAsync();
        await _audit.LogAsync("Shitje", "CREATE_WITH_STOCK", sale.ShitjeId.ToString(), newValues: dto);
        return Ok(new { sale.ShitjeId, product.SasiaStok });
    }

    [HttpPatch("sales/{id:int}/payment-status")]
    public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] PaymentStatusUpdateDto dto)
    {
        var sale = await _db.ShitjetProduktet.FindAsync(id);
        if (sale == null) return NotFound();
        sale.PaymentStatus = dto.PaymentStatus;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("Shitje", "PAYMENT_STATUS", id.ToString(), newValues: dto);
        return Ok(new { sale.ShitjeId, sale.PaymentStatus });
    }

    [HttpPost("payments/mock")]
    public async Task<IActionResult> MockPayment([FromBody] PaymentSimulationDto dto)
    {
        var sale = await _db.ShitjetProduktet.FindAsync(dto.ShitjeId);
        if (sale == null) return NotFound();
        sale.PaymentMethod = dto.PaymentMethod;
        sale.PaymentStatus = dto.Amount >= sale.CmimiTotal ? "Paid" : "Pending";
        await _db.SaveChangesAsync();
        return Ok(new { sale.ShitjeId, sale.PaymentMethod, sale.PaymentStatus });
    }

    [HttpGet("inventory/low-stock")]
    public async Task<IActionResult> GetLowStock() =>
        Ok(await _db.Produktet.AsNoTracking().Where(p => p.SasiaStok <= p.MinimumStock && !p.IsDeleted).ToListAsync());

    [HttpGet("memberships/expiring")]
    public async Task<IActionResult> GetExpiringMemberships([FromQuery] int days = 14)
    {
        var today = DateTime.UtcNow.Date;
        var limit = today.AddDays(days);
        return Ok(await _db.Anetaresimet
            .AsNoTracking()
            .Include(a => a.Klienti)
            .Include(a => a.Paketa)
            .Where(a => a.DataMbarimit >= today && a.DataMbarimit <= limit)
            .ToListAsync());
    }

    [HttpPost("notifications")]
    public async Task<IActionResult> CreateNotification([FromBody] NotificationCreateDto dto)
    {
        var notification = new Notification
        {
            Type = dto.Type,
            Title = dto.Title,
            Message = dto.Message,
            UserId = dto.UserId
        };
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();
        return Created("", notification);
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications() =>
        Ok(await _db.Notifications.AsNoTracking().OrderByDescending(n => n.CreatedAt).Take(50).ToListAsync());

    [HttpPatch("reviews/{id:int}/moderation")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> ModerateReview(int id, [FromBody] ReviewModerationDto dto)
    {
        var review = await _db.Vlereisimet.FindAsync(id);
        if (review == null) return NotFound();
        review.ModerationStatus = dto.ModerationStatus;
        await _db.SaveChangesAsync();
        return Ok(new { review.VleresimId, review.ModerationStatus });
    }

    [HttpGet("reports")]
    public async Task<IActionResult> GetReports()
    {
        var revenue = await _db.ShitjetProduktet.SumAsync(s => (decimal?)s.CmimiTotal) ?? 0m;
        var topProduct = await _db.ShitjetProduktet
            .Include(s => s.Produkti)
            .GroupBy(s => s.Produkti.EmriProduktit)
            .OrderByDescending(g => g.Sum(x => x.Sasia))
            .Select(g => g.Key)
            .FirstOrDefaultAsync() ?? "N/A";
        var mostPopularService = await _db.Terminet
            .Include(t => t.Sherbimi)
            .GroupBy(t => t.Sherbimi.EmriSherbimit)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefaultAsync() ?? "N/A";
        var topTherapist = await _db.Terminet
            .Include(t => t.Terapisti)
            .GroupBy(t => t.Terapisti.Emri + " " + t.Terapisti.Mbiemri)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefaultAsync() ?? "N/A";
        var lowStock = await _db.Produktet.CountAsync(p => p.SasiaStok <= p.MinimumStock);
        var expiring = await _db.Anetaresimet.CountAsync(a => a.DataMbarimit <= DateTime.UtcNow.Date.AddDays(14));
        return Ok(new AdvancedReportsDto(revenue, topProduct, mostPopularService, topTherapist, lowStock, expiring));
    }

    [HttpGet("reports/therapists")]
    public async Task<IActionResult> GetTherapistPerformance()
    {
        var data = await _db.Terapistet
            .AsNoTracking()
            .Select(t => new TherapistPerformanceDto(
                t.TerapistId,
                t.Emri + " " + t.Mbiemri,
                t.Terminet.Count,
                t.Terminet.Sum(x => (decimal?)x.Sherbimi.Cmimi) ?? 0m,
                t.Vlereisimet.Average(v => (double?)v.Nota) ?? 0.0))
            .ToListAsync();
        return Ok(data);
    }

    [HttpGet("reports/client-retention")]
    public async Task<IActionResult> GetClientRetention()
    {
        var total = await _db.Klientet.CountAsync();
        var returning = await _db.Terminet.GroupBy(t => t.KlientId).CountAsync(g => g.Count() > 1);
        var inactive = await _db.Klientet.CountAsync(k => !k.Terminet.Any(t => t.DataTerminit >= DateTime.UtcNow.AddMonths(-6)));
        var renewed = await _db.Anetaresimet.GroupBy(a => a.KlientId).CountAsync(g => g.Count() > 1);
        var rate = total == 0 ? 0 : Math.Round((double)renewed / total * 100, 2);
        return Ok(new ClientRetentionDto(total, returning, inactive, rate));
    }

    [HttpGet("recommendations/{clientId:int}")]
    public async Task<IActionResult> GetRecommendations(int clientId)
    {
        var usedServiceIds = await _db.Terminet.Where(t => t.KlientId == clientId).Select(t => t.SherbimId).Distinct().ToListAsync();
        var recommendations = await _db.Sherbimet
            .AsNoTracking()
            .Where(s => s.Aktiv && !usedServiceIds.Contains(s.SherbimId))
            .OrderByDescending(s => s.Vlereisimet.Average(v => (double?)v.Nota) ?? 0)
            .Take(5)
            .Select(s => new RecommendationDto(s.SherbimId, s.EmriSherbimit, "Recommended from highly rated services the client has not tried yet."))
            .ToListAsync();
        return Ok(recommendations);
    }

    [HttpPost("backup")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateBackupRecord()
    {
        var record = new BackupRecord
        {
            FileName = $"wellness-backup-{DateTime.UtcNow:yyyyMMddHHmmss}.db",
            CreatedBy = User.FindFirstValue(ClaimTypes.Email)
        };
        _db.BackupRecords.Add(record);
        await _db.SaveChangesAsync();
        return Ok(record);
    }
}
