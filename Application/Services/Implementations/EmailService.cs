using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Application.Services.Interfaces;

namespace Application.Services.Implementations
{
    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
        {
            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            var senderEmail = Environment.GetEnvironmentVariable("SMTP_SENDER_EMAIL");

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(senderEmail))
            {
                throw new Exception("SendGrid configuration is missing.");
            }

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(senderEmail);
            var to = new EmailAddress(toEmail);
            var plainTextBody = isHtml ? null : body; // Use body as plain text if not HTML
            var htmlBody = isHtml ? body : null; // Use body as HTML if specified
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextBody, htmlBody);

            var response = await client.SendEmailAsync(msg);

            if (response.StatusCode != System.Net.HttpStatusCode.OK &&
                response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                var errorBody = await response.Body.ReadAsStringAsync();
                throw new Exception($"Failed to send email via SendGrid: {errorBody}");
            }
        }
    }
}