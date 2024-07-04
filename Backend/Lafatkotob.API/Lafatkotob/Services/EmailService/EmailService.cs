
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Lafatkotob.Services.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using (var client = new SmtpClient("in-v3.mailjet.com"))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_configuration["Mailjet:ApiKey"], _configuration["Mailjet:ApiSecret"]);
                client.Port = 587;
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_configuration["Mailjet:Email"]),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
            }
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetLink, string userName)
        {
            string subject = "Reset Your Password";
            string body = $@"Hello {userName},<br><br>
                     You've requested to reset your password. Please click the link below to set a new password:<br>
                     <a href='{resetLink}'>Reset Password</a><br><br>
                     If you didn't request this, please ignore this email.<br><br>
                     Thank you.";


            await SendEmailAsync(email, subject, body);
        }
    }
}
