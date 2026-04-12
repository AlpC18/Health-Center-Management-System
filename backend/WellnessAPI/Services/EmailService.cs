using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace WellnessAPI.Services;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config) => _config = config;

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("Wellness House", _config["Email:From"]));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = body };
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        // Not: Gerçek gönderim için appsettings.json'da geçerli SMTP bilgileri olmalı.
        // Geliştirme aşamasında "Mailtrap" gibi servisler önerilir.
        await smtp.ConnectAsync(_config["Email:Host"], int.Parse(_config["Email:Port"] ?? "587"), MailKit.Security.SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string userName)
    {
        string subject = "Mirë se vini në Wellness House!";
        string body = $@"
            <div style='font-family: sans-serif; color: #333;'>
                <h2 style='color: #16a34a;'>Përshëndetje {userName}!</h2>
                <p>Jemi shumë të lumtur që jeni bërë pjesë e <b>Wellness House</b>.</p>
                <p>Tani mund t'i shijoni të gjitha shërbimet tona profesionale për shëndetin dhe mirëqenien tuaj.</p>
                <hr style='border: 0; border-top: 1px solid #eee;' />
                <p style='font-size: 0.8em; color: #888;'>Kjo është një email automatike, ju lutem mos u përgjigjni.</p>
            </div>";
        
        await SendEmailAsync(toEmail, subject, body);
    }
}
