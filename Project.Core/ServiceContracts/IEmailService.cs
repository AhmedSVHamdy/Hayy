namespace Project.Core.ServiceContracts
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendEmailWaitList(string toEmail, string subject, string body);
    }
}
