using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WellnessAPI.Data;
using WellnessAPI.DTOs;
using WellnessAPI.Models.Domain;
using WellnessAPI.Models.Identity;
using WellnessAPI.Services;

namespace WellnessAPI.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public NotificationsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool unreadOnly = false, [FromQuery] string? userId = null, [FromQuery] int limit = 50)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        var targetUserId = User.IsInRole("Admin") && !string.IsNullOrWhiteSpace(userId) ? userId : currentUserId;
        if (string.IsNullOrWhiteSpace(targetUserId)) return Unauthorized();

        var q = _db.Notifications.AsNoTracking().Where(n => n.UserId == targetUserId);
        if (unreadOnly) q = q.Where(n => !n.IsRead);

        var data = await q.OrderByDescending(n => n.CreatedAt)
            .Take(Math.Clamp(limit, 1, 200))
            .Select(n => new NotificationResponseDto(n.NotificationId, n.Type, n.Title, n.Message, n.Link, n.IsRead, n.CreatedAt, n.ReadAt))
            .ToListAsync();
        return Ok(data);
    }

    [HttpPatch("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        var notification = await _db.Notifications.FirstOrDefaultAsync(n => n.NotificationId == id);
        if (notification is null) return NotFound();
        if (notification.UserId != currentUserId && !User.IsInRole("Admin")) return Forbid();

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { success = true });
    }
}

[ApiController]
[Route("api/consents")]
[Authorize]
public class ConsentsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public ConsentsController(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    [HttpGet("mine")]
    public async Task<IActionResult> Mine()
    {
        var user = await CurrentUserAsync();
        if (user is null) return Unauthorized();
        var klientId = int.TryParse(user.KlientId, out var parsed) ? parsed : (int?)null;

        var logs = await _db.ConsentLogs.AsNoTracking()
            .Where(c => c.UserId == user.Id || (klientId != null && c.KlientId == klientId))
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ConsentLogResponseDto(c.ConsentLogId, c.KlientId, c.UserId, c.ConsentType, c.Version, c.Accepted, c.CreatedAt))
            .ToListAsync();
        return Ok(logs);
    }

    [HttpPost("accept")]
    public async Task<IActionResult> Accept(ConsentAcceptDto dto)
    {
        var user = await CurrentUserAsync();
        if (user is null) return Unauthorized();
        var klientId = int.TryParse(user.KlientId, out var parsed) ? parsed : (int?)null;

        _db.ConsentLogs.Add(new ConsentLog
        {
            KlientId = klientId,
            UserId = user.Id,
            ConsentType = dto.ConsentType,
            Version = dto.Version,
            Accepted = dto.Accepted,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        if (dto.ConsentType.Equals("PrivacyPolicy", StringComparison.OrdinalIgnoreCase) && dto.Accepted)
        {
            user.PrivacyPolicyAccepted = true;
            user.PrivacyPolicyAcceptedAt = DateTime.UtcNow;
            await _users.UpdateAsync(user);
        }

        await _db.SaveChangesAsync();
        return Ok(new { success = true });
    }

    private async Task<ApplicationUser?> CurrentUserAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return string.IsNullOrWhiteSpace(userId) ? null : await _users.FindByIdAsync(userId);
    }
}

[ApiController]
[Route("api/privacy")]
[Authorize]
public class PrivacyController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public PrivacyController(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportMine()
    {
        var user = await CurrentUserAsync();
        if (user is null) return Unauthorized();
        if (!int.TryParse(user.KlientId, out var klientId)) return NotFound(new { message = "Profili i klientit nuk u gjet." });
        return Ok(await BuildExportAsync(klientId, user));
    }

    [HttpPost("erase")]
    public async Task<IActionResult> EraseMine()
    {
        var user = await CurrentUserAsync();
        if (user is null) return Unauthorized();
        if (!int.TryParse(user.KlientId, out var klientId)) return NotFound(new { message = "Profili i klientit nuk u gjet." });
        await SoftEraseAsync(klientId, user);
        return Ok(new { success = true, message = "Te dhenat personale u anonimizuan dhe llogaria u caktivizua." });
    }

    [HttpGet("admin/klient/{klientId:int}/export")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExportClient(int klientId)
    {
        var user = await _users.Users.FirstOrDefaultAsync(u => u.KlientId == klientId.ToString());
        return Ok(await BuildExportAsync(klientId, user));
    }

    [HttpPost("admin/klient/{klientId:int}/erase")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EraseClient(int klientId)
    {
        var user = await _users.Users.FirstOrDefaultAsync(u => u.KlientId == klientId.ToString());
        await SoftEraseAsync(klientId, user);
        return Ok(new { success = true });
    }

    private async Task<object> BuildExportAsync(int klientId, ApplicationUser? user)
    {
        var klient = await _db.Klientet.AsNoTracking().FirstOrDefaultAsync(k => k.KlientId == klientId);
        if (klient is null) throw new InvalidOperationException("Klienti nuk u gjet.");
        var userId = user?.Id;

        return new
        {
            exportedAt = DateTime.UtcNow,
            user = user is null ? null : new
            {
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.PhoneNumber,
                user.PrivacyPolicyAccepted,
                user.PrivacyPolicyAcceptedAt,
                user.SmsOptIn
            },
            klient,
            consents = await _db.ConsentLogs.AsNoTracking().Where(c => c.KlientId == klientId || (userId != null && c.UserId == userId)).ToListAsync(),
            terminet = await _db.Terminet.AsNoTracking().Where(t => t.KlientId == klientId).ToListAsync(),
            anetaresimet = await _db.Anetaresimet.AsNoTracking().Where(a => a.KlientId == klientId).ToListAsync(),
            shitjet = await _db.ShitjetProduktet.AsNoTracking().Where(s => s.KlientId == klientId).ToListAsync(),
            vleresimet = await _db.Vlereisimet.AsNoTracking().Where(v => v.KlientId == klientId).ToListAsync(),
            pikat = await _db.KlientPikat.AsNoTracking().Where(p => p.KlientId == klientId).ToListAsync()
        };
    }

    private async Task SoftEraseAsync(int klientId, ApplicationUser? user)
    {
        var klient = await _db.Klientet.FirstOrDefaultAsync(k => k.KlientId == klientId);
        if (klient is not null)
        {
            var stamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            klient.Emri = "Deleted";
            klient.Mbiemri = $"Client-{klient.KlientId}";
            klient.Email = $"deleted-{klient.KlientId}-{stamp}@example.invalid";
            klient.Telefoni = null;
            klient.DataLindjes = null;
            klient.Gjinia = null;
            klient.KushtetShendetesore = null;
            klient.FotoPath = null;
            klient.IsDeleted = true;
            klient.DeletedAt = DateTime.UtcNow;
        }

        if (user is not null)
        {
            user.IsActive = false;
            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.GdprErasureRequested = true;
            user.GdprErasureRequestedAt = DateTime.UtcNow;
            user.Email = $"deleted-{user.Id}@example.invalid";
            user.UserName = user.Email;
            user.NormalizedEmail = user.Email.ToUpperInvariant();
            user.NormalizedUserName = user.NormalizedEmail;
            user.PhoneNumber = null;
            user.Adresa = null;
            user.TotpSecret = null;
            user.TwoFactorEnabled = false;
            await _users.UpdateAsync(user);
        }

        await _db.SaveChangesAsync();
    }

    private async Task<ApplicationUser?> CurrentUserAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return string.IsNullOrWhiteSpace(userId) ? null : await _users.FindByIdAsync(userId);
    }
}

[ApiController]
[Route("api/2fa")]
[Authorize]
public class TwoFactorController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly TotpService _totp;

    public TwoFactorController(UserManager<ApplicationUser> users, TotpService totp)
    {
        _users = users;
        _totp = totp;
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status()
    {
        var user = await CurrentUserAsync();
        if (user is null) return Unauthorized();
        return Ok(new { enabled = user.TwoFactorEnabled, enrolled = !string.IsNullOrWhiteSpace(user.TotpSecret) });
    }

    [HttpPost("enroll")]
    public async Task<IActionResult> Enroll()
    {
        var user = await CurrentUserAsync();
        if (user is null) return Unauthorized();
        user.TotpSecret = _totp.GenerateSecret();
        user.TwoFactorEnabled = false;
        user.TotpEnabledAt = null;
        await _users.UpdateAsync(user);

        var issuer = "Wellness House";
        return Ok(new
        {
            secret = user.TotpSecret,
            otpauthUri = _totp.BuildOtpAuthUri(issuer, user.Email ?? user.Id, user.TotpSecret)
        });
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify(TwoFactorVerifyDto dto)
    {
        var user = await CurrentUserAsync();
        if (user is null) return Unauthorized();
        if (string.IsNullOrWhiteSpace(user.TotpSecret)) return BadRequest(new { message = "Filloni regjistrimin e 2FA me pare." });
        if (!_totp.VerifyCode(user.TotpSecret, dto.Code)) return BadRequest(new { message = "Kodi eshte i pavlefshem." });

        user.TwoFactorEnabled = true;
        user.TotpEnabledAt = DateTime.UtcNow;
        await _users.UpdateAsync(user);
        return Ok(new { success = true, enabled = true });
    }

    [HttpPost("disable")]
    public async Task<IActionResult> Disable(TwoFactorVerifyDto dto)
    {
        var user = await CurrentUserAsync();
        if (user is null) return Unauthorized();
        if (user.TwoFactorEnabled && !string.IsNullOrWhiteSpace(user.TotpSecret) && !_totp.VerifyCode(user.TotpSecret, dto.Code))
            return BadRequest(new { message = "Kodi eshte i pavlefshem." });

        user.TwoFactorEnabled = false;
        user.TotpSecret = null;
        user.TotpEnabledAt = null;
        await _users.UpdateAsync(user);
        return Ok(new { success = true, enabled = false });
    }

    private async Task<ApplicationUser?> CurrentUserAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return string.IsNullOrWhiteSpace(userId) ? null : await _users.FindByIdAsync(userId);
    }
}

[ApiController]
[Route("api/templates")]
[Authorize(Roles = "Admin")]
public class TemplatesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public TemplatesController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _db.Templates.AsNoTracking()
            .OrderBy(t => t.Key).ThenBy(t => t.Channel)
            .Select(t => new TemplateResponseDto(t.TemplateId, t.Key, t.Name, t.Channel.ToString(), t.Subject, t.Body, t.Active, t.UpdatedAt))
            .ToListAsync();
        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create(TemplateUpsertDto dto)
    {
        var channel = Enum.Parse<TemplateChannel>(dto.Channel, ignoreCase: true);
        if (await _db.Templates.AnyAsync(t => t.Key == dto.Key && t.Channel == channel))
            return Conflict(new { message = "Template me kete key/channel ekziston." });

        var template = new Template
        {
            Key = dto.Key,
            Name = dto.Name,
            Channel = channel,
            Subject = dto.Subject,
            Body = dto.Body,
            Active = dto.Active,
            UpdatedAt = DateTime.UtcNow,
            UpdatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")
        };
        _db.Templates.Add(template);
        await _db.SaveChangesAsync();
        return Ok(new TemplateResponseDto(template.TemplateId, template.Key, template.Name, template.Channel.ToString(), template.Subject, template.Body, template.Active, template.UpdatedAt));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, TemplateUpsertDto dto)
    {
        var template = await _db.Templates.FindAsync(id);
        if (template is null) return NotFound();
        var channel = Enum.Parse<TemplateChannel>(dto.Channel, ignoreCase: true);
        template.Key = dto.Key;
        template.Name = dto.Name;
        template.Channel = channel;
        template.Subject = dto.Subject;
        template.Body = dto.Body;
        template.Active = dto.Active;
        template.UpdatedAt = DateTime.UtcNow;
        template.UpdatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        await _db.SaveChangesAsync();
        return Ok(new TemplateResponseDto(template.TemplateId, template.Key, template.Name, template.Channel.ToString(), template.Subject, template.Body, template.Active, template.UpdatedAt));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var template = await _db.Templates.FindAsync(id);
        if (template is null) return NotFound();
        _db.Templates.Remove(template);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

[ApiController]
[Route("api/lokacionet")]
[Authorize(Roles = "Admin")]
public class LokacionetController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public LokacionetController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _db.Lokacionet.AsNoTracking()
            .OrderBy(l => l.Emri)
            .Select(l => new LokacioniResponseDto(l.LokacioniId, l.Emri, l.Adresa, l.Telefoni, l.Aktiv, l.CreatedAt))
            .ToListAsync();
        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create(LokacioniDto dto)
    {
        var lokacioni = new Lokacioni { Emri = dto.Emri, Adresa = dto.Adresa, Telefoni = dto.Telefoni, Aktiv = dto.Aktiv, CreatedAt = DateTime.UtcNow };
        _db.Lokacionet.Add(lokacioni);
        await _db.SaveChangesAsync();
        return Ok(new LokacioniResponseDto(lokacioni.LokacioniId, lokacioni.Emri, lokacioni.Adresa, lokacioni.Telefoni, lokacioni.Aktiv, lokacioni.CreatedAt));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, LokacioniDto dto)
    {
        var lokacioni = await _db.Lokacionet.FindAsync(id);
        if (lokacioni is null) return NotFound();
        lokacioni.Emri = dto.Emri;
        lokacioni.Adresa = dto.Adresa;
        lokacioni.Telefoni = dto.Telefoni;
        lokacioni.Aktiv = dto.Aktiv;
        await _db.SaveChangesAsync();
        return Ok(new LokacioniResponseDto(lokacioni.LokacioniId, lokacioni.Emri, lokacioni.Adresa, lokacioni.Telefoni, lokacioni.Aktiv, lokacioni.CreatedAt));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var lokacioni = await _db.Lokacionet.FindAsync(id);
        if (lokacioni is null) return NotFound();
        lokacioni.Aktiv = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

[ApiController]
[Route("api/payments/stripe")]
public class StripePaymentsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly StripeCheckoutService _stripe;
    private readonly IConfiguration _config;

    public StripePaymentsController(ApplicationDbContext db, UserManager<ApplicationUser> users, StripeCheckoutService stripe, IConfiguration config)
    {
        _db = db;
        _users = users;
        _stripe = stripe;
        _config = config;
    }

    [HttpPost("memberships/checkout")]
    [Authorize]
    public async Task<IActionResult> CreateMembershipCheckout(StripeMembershipCheckoutDto dto)
    {
        var user = await CurrentUserAsync();
        if (user is null) return Unauthorized();
        if (!int.TryParse(user.KlientId, out var klientId)) return NotFound(new { message = "Profili i klientit nuk u gjet." });

        var klient = await _db.Klientet.FirstOrDefaultAsync(k => k.KlientId == klientId);
        var paketa = await _db.PaketaWellness.FirstOrDefaultAsync(p => p.PaketId == dto.PaketId && p.Aktive);
        if (klient is null || paketa is null) return NotFound();

        var discountAmount = Math.Round(paketa.Cmimi * klient.DiscountPercent / 100m, 2);
        var finalPrice = Math.Max(0, paketa.Cmimi - discountAmount);
        var start = DateTime.UtcNow.Date;
        var membership = new Anetaresim
        {
            KlientId = klient.KlientId,
            PaketId = paketa.PaketId,
            DataFillimit = start,
            DataMbarimit = start.AddMonths(paketa.KohezgjatjaMuaj),
            Statusi = "NePritje",
            CmimiPaguar = finalPrice,
            DiscountPercent = klient.DiscountPercent,
            PaymentProvider = "Stripe",
            PaymentStatus = "Pending"
        };

        _db.Anetaresimet.Add(membership);
        await _db.SaveChangesAsync();

        try
        {
            var frontendBase = (_config["App:FrontendBaseUrl"] ?? "http://localhost:5173").TrimEnd('/');
            var session = await _stripe.CreateMembershipCheckoutSessionAsync(
                membership,
                paketa,
                finalPrice,
                $"{frontendBase}/portal/anetaresimi?checkout=success",
                $"{frontendBase}/portal/anetaresimi?checkout=cancel");

            membership.StripeSessionId = session.Id;
            membership.PaymentReference = session.Id;
            await _db.SaveChangesAsync();
            return Ok(new { sessionId = session.Id, url = session.Url, amount = finalPrice, discountPercent = klient.DiscountPercent });
        }
        catch (Exception ex)
        {
            _db.Anetaresimet.Remove(membership);
            await _db.SaveChangesAsync();
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook()
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();
        if (!_stripe.VerifyWebhookSignature(payload, Request.Headers["Stripe-Signature"].FirstOrDefault()))
            return Unauthorized();

        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;
        var type = root.GetProperty("type").GetString();
        if (type is "checkout.session.completed" or "checkout.session.expired")
        {
            var session = root.GetProperty("data").GetProperty("object");
            var sessionId = session.GetProperty("id").GetString();
            var membership = await _db.Anetaresimet.FirstOrDefaultAsync(a => a.StripeSessionId == sessionId);
            if (membership is not null)
            {
                if (type == "checkout.session.completed")
                {
                    membership.PaymentStatus = "Paid";
                    membership.Statusi = "Aktiv";
                    membership.PaymentReference = sessionId;
                }
                else
                {
                    membership.PaymentStatus = "Expired";
                    membership.Statusi = "Anuluar";
                }
                await _db.SaveChangesAsync();
            }
        }

        return Ok();
    }

    private async Task<ApplicationUser?> CurrentUserAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return string.IsNullOrWhiteSpace(userId) ? null : await _users.FindByIdAsync(userId);
    }
}
