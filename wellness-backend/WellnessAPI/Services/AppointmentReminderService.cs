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

            var tomorrow = DateTime.Today.AddDays(1);

            var upcoming = await db.Terminet
                .Include(t => t.Klienti)
                .Include(t => t.Sherbimi)
                .Where(t => t.DataTerminit.Date == tomorrow.Date && t.Statusi == "Konfirmuar")
                .ToListAsync(stoppingToken);

            foreach (var termin in upcoming)
            {
                if (!string.IsNullOrEmpty(termin.Klienti?.Email))
                {
                    var subject = $"Kujtese: Termini juaj per {termin.Sherbimi?.EmriSherbimit}";
                    var body = $"I nderuar {termin.Klienti.Emri}, ju kujtojme se neser keni terminin e konfirmuar per sherbimin {termin.Sherbimi?.EmriSherbimit} ne oren {termin.OraFillimit}.";

                    // Fake send for now (logged only)
                    _logger.LogInformation("[EMAIL SENT to {Email}]: {Subject}", termin.Klienti.Email, subject);
                    _ = body;
                    _ = emailService;
                }
            }

            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }
}
