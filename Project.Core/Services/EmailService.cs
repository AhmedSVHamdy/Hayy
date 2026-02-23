using Microsoft.Extensions.Options;
using Project.Core.DTO;
using Project.Core.ServiceContracts;
using System.Net;
using System.Net.Mail;

namespace Project.Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var client = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
            {
                Credentials = new NetworkCredential(_emailSettings.Email, _emailSettings.Password),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.Email, _emailSettings.DisplayName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
        public async Task SendEmailWaitList(string toEmail, string subject, string body)
        {
            // 💡 مستقبلاً: هنا هتحط كود الـ SMTP أو SendGrid أو Firebase Push Notifications

            // مؤقتاً هنطبعها في الكونسول عشان نتأكد إنها شغالة
            Console.WriteLine($"\n==========================================");
            Console.WriteLine($"🔔 [إشعار جديد] إلى: {toEmail}");
            Console.WriteLine($"موضوع: {subject}");
            Console.WriteLine($"الرسالة: {body}");
            Console.WriteLine($"==========================================\n");

            await Task.CompletedTask;
        }
    }
}