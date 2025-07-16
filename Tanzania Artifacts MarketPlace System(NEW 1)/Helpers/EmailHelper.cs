using System.Net;
using System.Net.Mail;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Helpers
{
    public static class EmailHelper
    {
        public static async Task SendEmailAsync(string toEmail, string subject, string htmlBody, IConfiguration config)
        {
            var smtpPortString = config["EmailSettings:SmtpPort"];
            if (string.IsNullOrEmpty(smtpPortString))
            {
                throw new ArgumentNullException(nameof(smtpPortString), "SMTP port configuration is missing or null.");
            }

            var smtp = new SmtpClient(config["EmailSettings:SmtpServer"])
            {
                Port = int.Parse(smtpPortString),
                Credentials = new NetworkCredential(
                    config["EmailSettings:Username"] ?? throw new ArgumentNullException("EmailSettings:Username", "Username is missing or null."),
                    config["EmailSettings:Password"] ?? throw new ArgumentNullException("EmailSettings:Password", "Password is missing or null.")
                ),
                EnableSsl = true
            };

            var senderEmail = config["EmailSettings:SenderEmail"];
            var senderName = config["EmailSettings:SenderName"];
            if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderName))
            {
                throw new ArgumentNullException("EmailSettings:SenderEmail or EmailSettings:SenderName", "Sender email or name is missing or null.");
            }

            var message = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await smtp.SendMailAsync(message);
        }
    }
}