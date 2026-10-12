using System.Net;
using System.Net.Mail;

namespace BlitzMall_Backend.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendPasswordResetCodeAsync(
            string email,
            string code)
        {
            var smtpHost = _config["Email:SmtpHost"]
                ?? throw new InvalidOperationException("SMTP host is not configured.");

            var smtpPort = int.Parse(
                _config["Email:SmtpPort"]
                ?? throw new InvalidOperationException("SMTP port is not configured."));

            var username = _config["Email:Username"]
                ?? throw new InvalidOperationException("Email username is not configured.");

            var password = _config["Email:Password"]
                ?? throw new InvalidOperationException("Email password is not configured.");

            var senderEmail = _config["Email:SenderEmail"]
                ?? username;

            var senderName = _config["Email:SenderName"]
                ?? "BlitzMall";

            using var message = new MailMessage();

            message.From = new MailAddress(
                senderEmail,
                senderName);

            message.To.Add(email);
            message.Subject = "BlitzMall - Password Reset Code";

            message.Body =
                $"Your BlitzMall password reset code is: {code}\n\n" +
                "This code is valid for 10 minutes.\n\n" +
                "If you did not request a password reset, you can ignore this email.";

            message.IsBodyHtml = false;

            using var smtp = new SmtpClient(
                smtpHost,
                smtpPort);

            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential(
                username,
                password);

            await smtp.SendMailAsync(message);
        }
    }
}