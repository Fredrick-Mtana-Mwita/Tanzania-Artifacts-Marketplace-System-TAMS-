using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Interfaces;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            _logger.LogInformation("Attempting to send email to {Email} with subject: {Subject}", toEmail, subject);
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            try
            {
                using var smtp = new SmtpClient();
                _logger.LogInformation("Connecting to SMTP server {Server}:{Port}", _settings.SmtpServer, _settings.SmtpPort);
                await smtp.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                
                _logger.LogInformation("Authenticating with username: {Username}", _settings.Username);
                await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
                
                _logger.LogInformation("Sending email...");
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
                _logger.LogInformation("Email sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw; // Re-throw to maintain the original behavior
            }
        }
    }

}
