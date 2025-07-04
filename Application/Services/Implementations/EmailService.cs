using System;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Application.Services.Implementations
{
    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
        {
            var senderEmail = Environment.GetEnvironmentVariable("SMTP_SENDER_EMAIL");
            var appPassword = Environment.GetEnvironmentVariable("SMTP_APP_PASSWORD");
            var smtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER");
            var portStr = Environment.GetEnvironmentVariable("SMTP_PORT_NUMBER");

            if (string.IsNullOrEmpty(senderEmail) ||
                string.IsNullOrEmpty(appPassword) ||
                string.IsNullOrEmpty(smtpServer) ||
                string.IsNullOrEmpty(portStr))
            {
                throw new Exception("SMTP configuration is missing.");
            }

            if (!int.TryParse(portStr, out int port))
            {
                throw new Exception("Invalid SMTP port number.");
            }

            var smtpClient = new SmtpClient(smtpServer)
            {
                Port = port,
                Credentials = new NetworkCredential(senderEmail, appPassword),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml,
            };
            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
