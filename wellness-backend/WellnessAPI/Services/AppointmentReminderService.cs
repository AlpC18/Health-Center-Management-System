using Microsoft.EntityFrameworkCore;
using WellnessAPI.Data;

namespace WellnessAPI.Services;

public class AppointmentReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AppointmentReminderService> _logger;

    public AppointmentReminderService(IServiceProvider serviceProvider, ILogger<AppointmentReminderService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("AppointmentReminderService is checking for upcoming appointments...");

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
            var templateService = scope.ServiceProvider.GetRequiredService<TemplateService>();
            var smsService = scope.ServiceProvider.GetRequiredService<SmsService>();

            var tomorrow = DateTime.Today.AddDays(1);
            var dayAfter = tomorrow.AddDays(1);

            var upcoming = await db.Terminet
                .Include(t => t.Klienti)
                .Include(t => t.Sherbimi)
                .Include(t => t.Terapisti)
                .Where(t => t.DataTerminit >= tomorrow
                    && t.DataTerminit < dayAfter
                    && t.Statusi == Models.Domain.AppointmentStatus.Konfirmuar)
                .ToListAsync(stoppingToken);

            foreach (var termin in upcoming)
            {
                var tokens = new Dictionary<string, string?>
                {
                    ["ClientName"] = termin.Klienti?.Emri,
                    ["ServiceName"] = termin.Sherbimi?.EmriSherbimit,
                    ["TherapistName"] = termin.Terapisti is null ? "" : $"{termin.Terapisti.Emri} {termin.Terapisti.Mbiemri}",
                    ["AppointmentDate"] = termin.DataTerminit.ToString("dd/MM/yyyy"),
                    ["StartTime"] = termin.OraFillimit.ToString(@"hh\:mm"),
                    ["EndTime"] = termin.OraMbarimit.ToString(@"hh\:mm")
                };

                if (!string.IsNullOrEmpty(termin.Klienti?.Email) && termin.ReminderEmailSentAt is null)
                {
                    try
                    {
                        var rendered = await templateService.RenderAsync(
                            "appointment-reminder",
                            Models.Domain.TemplateChannel.Email,
                            $"Kujtese: Termini juaj per {termin.Sherbimi?.EmriSherbimit}",
                            "Pershendetje {{ClientName}}, ju kujtojme se neser keni terminin per {{ServiceName}} ne oren {{StartTime}}.",
                            tokens,
                            stoppingToken);
                        await emailService.SendEmailAsync(termin.Klienti.Email, rendered.Subject ?? "Kujtese termini", rendered.Body);
                        termin.ReminderEmailSentAt ??= DateTime.UtcNow;
                        _logger.LogInformation("Reminder email sent to {Email} for appointment {TerminId}", termin.Klienti.Email, termin.TerminId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Reminder email failed for appointment {TerminId}", termin.TerminId);
                    }
                }

                if (!string.IsNullOrWhiteSpace(termin.Klienti?.Telefoni) && termin.ReminderSmsSentAt is null)
                {
                    try
                    {
                        var rendered = await templateService.RenderAsync(
                            "appointment-reminder",
                            Models.Domain.TemplateChannel.Sms,
                            null,
                            "Wellness House: neser keni termin per {{ServiceName}} ne {{StartTime}}.",
                            tokens,
                            stoppingToken);
                        await smsService.SendSmsAsync(termin.Klienti.Telefoni, rendered.Body, stoppingToken);
                        termin.ReminderSmsSentAt = DateTime.UtcNow;
                        _logger.LogInformation("Reminder SMS sent to {Phone} for appointment {TerminId}", termin.Klienti.Telefoni, termin.TerminId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Reminder SMS failed for appointment {TerminId}", termin.TerminId);
                    }
                }
            }

            if (upcoming.Any(t => t.ReminderEmailSentAt is not null || t.ReminderSmsSentAt is not null))
                await db.SaveChangesAsync(stoppingToken);

            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }
}
