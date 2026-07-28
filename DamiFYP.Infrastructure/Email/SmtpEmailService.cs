using System.Net;
using System.Net.Mail;

namespace DamiFYP.Infrastructure.Email;

// Minimal SMTP-based email sender. If Email:Host isn't configured, sends are
// skipped (and logged to the console) instead of throwing, so the app still
// runs fine without real credentials - fill in the "Email" section in
// appsettings (or user-secrets/environment variables) to actually deliver
// mail. Takes a plain options POCO (rather than IOptions<T>/ILogger<T>) since
// this project has no package references of its own - see registration in
// Program.cs.
public sealed class SmtpEmailService : IEmailService
{
    private readonly SmtpEmailOptions _options;

    public SmtpEmailService(SmtpEmailOptions options)
    {
        _options = options;
    }

    public async Task SendEmailAsync(string toEmail, string toName, string subject, string body,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            Console.WriteLine($"[Email] Skipped '{subject}' - recipient has no email address on file.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.Host))
        {
            Console.WriteLine(
                $"[Email] Email:Host is not configured - skipping send of '{subject}' to {toEmail}. " +
                "Configure the Email section in appsettings to enable real sending.");
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromEmail, _options.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(new MailAddress(toEmail, toName));

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            Credentials = string.IsNullOrWhiteSpace(_options.Username)
                ? null
                : new NetworkCredential(_options.Username, _options.Password)
        };

        try
        {
            await client.SendMailAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Email] Failed to send '{subject}' to {toEmail}: {ex.Message}");
        }
    }
}
