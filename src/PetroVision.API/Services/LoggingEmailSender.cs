using PetroVision.BLL.Interfaces;

namespace PetroVision.API.Services;

// Development sender: writes the email to the log instead of sending it.
// Replace with an SMTP / SendGrid implementation for production.
public class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger) => _logger = logger;

    public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        _logger.LogInformation("EMAIL to {To} | {Subject}\n{Body}", to, subject, body);
        return Task.CompletedTask;
    }
}
