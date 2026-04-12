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

            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

                var tomorrow = DateTime.Today.AddDays(1);
                
                // Get appointments scheduled for tomorrow
                var upcoming = await db.Terminet
                    .Include(t => t.Klient)
                    .Include(t => t.Sherbim)
                    .Where(t => t.DataTerminit.Date == tomorrow.Date && t.Statusi == "Konfirmuar")
                    .ToListAsync(stoppingToken);

                foreach (var termin in upcoming)
                {
                    if (!string.IsNullOrEmpty(termin.Klient?.Email))
                    {
                        var subject = $"Kujtesë: Termini juaj për {termin.Sherbim?.Emri}";
                        var body = $"Të nderuar {termin.Klient.Emri}, ju kujtojmë se nesër keni terminin e konfirmuar për shërbimin {termin.Sherbim?.Emri} në orën {termin.OraFillimit}.";
                        
                        // Fake Send
                        _logger.LogInformation($"[EMAIL SENT to {termin.Klient.Email}]: {subject}");
                    }
                }
            }

            // Check every 12 hours
            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }
}
