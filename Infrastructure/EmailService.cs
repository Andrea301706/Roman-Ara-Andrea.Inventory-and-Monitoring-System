using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Infrastructure
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Invite Email
        public async Task SendInviteEmailAsync(string toEmail, string inviteUrl)
        {
            await SendEmailAsync(
                toEmail,
                "You're Invited!",
                $@"
Hello,

You have been invited to create your account.

Click the link below to set your password:

<a href='{inviteUrl}'>{inviteUrl}</a>

If you did not expect this invitation, you may ignore this email.

Thank you.");
        }

        // Forgot Password Email
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var smtpPort = _configuration.GetValue<int>("EmailSettings:Port");
                var smtpUsername = _configuration["EmailSettings:Username"];
                var smtpPassword = _configuration["EmailSettings:Password"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"];

                if (string.IsNullOrWhiteSpace(smtpHost) ||
                    string.IsNullOrWhiteSpace(smtpUsername) ||
                    string.IsNullOrWhiteSpace(smtpPassword) ||
                    string.IsNullOrWhiteSpace(fromEmail) ||
                    string.IsNullOrWhiteSpace(fromName))
                {
                    throw new Exception("One or more EmailSettings values are missing in appsettings.json.");
                }

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                message.To.Add(toEmail);

                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                throw new Exception($"Email sending failed: {ex.Message}", ex);
            }
        }
    }
}