namespace DamiFYP.Infrastructure.Email;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string toName, string subject, string body,
        CancellationToken cancellationToken = default);
}
