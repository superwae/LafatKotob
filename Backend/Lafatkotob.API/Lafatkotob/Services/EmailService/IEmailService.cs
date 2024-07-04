namespace Lafatkotob.Services.EmailService
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendPasswordResetEmailAsync(string email, string resetLink, string userName);
    }
}
