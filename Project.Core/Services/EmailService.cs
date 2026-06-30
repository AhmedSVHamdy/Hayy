using Microsoft.Extensions.Options;
using Project.Core.DTO;
using Project.Core.ServiceContracts;
using MimeKit; // 💡 تابعة لـ MailKit
using MailKit.Net.Smtp; // 💡 تأكد إن الـ SmtpClient جاي من MailKit مش System.Net
using MailKit.Security;

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
            var email = new MimeMessage();

            // 1. إعداد بيانات المرسل والمستقبل
            email.From.Add(new MailboxAddress(_emailSettings.DisplayName, _emailSettings.Email));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            // 2. تجهيز محتوى الـ HTML
            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            // 3. الاتصال والإرسال الآمن عبر MailKit
            using var smtp = new SmtpClient();

            // 💡 إنشاء عداد زمني صارم مدته 7 ثواني
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(7));

            try
            {
                // تمرير الـ Token لكل العمليات عشان لو الوقت خلص يفصل فوراً
                await smtp.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.Auto, cts.Token);

                var cleanPassword = _emailSettings.Password.Replace(" ", "");
                await smtp.AuthenticateAsync(_emailSettings.Email, cleanPassword, cts.Token);

                await smtp.SendAsync(email, cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("❌ [Email Error]: انتهى وقت الاتصال (Timeout)! السيرفر لا يستجيب أو البورت مغلق.");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [Email Error]: {ex.Message}");
                throw;
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }

        public async Task SendEmailWaitList(string toEmail, string subject, string body)
        {
            Console.WriteLine($"\n==========================================");
            Console.WriteLine($"🔔 [إشعار جديد] إلى: {toEmail}");
            Console.WriteLine($"موضوع: {subject}");
            Console.WriteLine($"الرسالة: {body}");
            Console.WriteLine($"==========================================\n");

            await Task.CompletedTask;
        }
    }
}