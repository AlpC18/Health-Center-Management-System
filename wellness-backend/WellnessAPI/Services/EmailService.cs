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
        var fromAddress = _config["Email:From"] ?? throw new InvalidOperationException("Email:From is not configured.");
        var host = _config["Email:Host"] ?? throw new InvalidOperationException("Email:Host is not configured.");
        var username = _config["Email:Username"] ?? throw new InvalidOperationException("Email:Username is not configured.");
        var password = _config["Email:Password"] ?? throw new InvalidOperationException("Email:Password is not configured.");

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("Wellness House", fromAddress));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = body };
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(host, int.Parse(_config["Email:Port"] ?? "587"), MailKit.Security.SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(username, password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string userName)
    {
        string subject = "Mire se vini ne Wellness House!";
        string body = $@"
            <div style='font-family: sans-serif; color: #333;'>
                <h2 style='color: #16a34a;'>Pershendetje {userName}!</h2>
                <p>Jemi shume te lumtur qe jeni bere pjese e <b>Wellness House</b>.</p>
                <p>Tani mund t'i shijoni te gjitha sherbimet tona profesionale per shendetin dhe mireqenien tuaj.</p>
                <hr style='border: 0; border-top: 1px solid #eee;' />
                <p style='font-size: 0.8em; color: #888;'>Kjo eshte nje email automatike, ju lutem mos u pergjigjni.</p>
            </div>";

        await SendEmailAsync(toEmail, subject, body);
    }
}
