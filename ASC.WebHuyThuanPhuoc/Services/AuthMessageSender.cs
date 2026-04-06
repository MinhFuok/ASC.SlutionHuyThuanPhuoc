using ASC.WebHuyThuanPhuoc.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ASC.WebHuyThuanPhuoc.Services
{
    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        private readonly IOptions<ApplicationSettings> _settings;

        public AuthMessageSender(IOptions<ApplicationSettings> settings)
        {
            _settings = settings;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(
                _settings.Value.ApplicationTitle,
                _settings.Value.SMTPAccount));
            mimeMessage.To.Add(MailboxAddress.Parse(email));
            mimeMessage.Subject = subject;

            mimeMessage.Body = new BodyBuilder
            {
                HtmlBody = message
            }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _settings.Value.SMTPServer,
                _settings.Value.SMTPPort,
                SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(
                _settings.Value.SMTPAccount,
                _settings.Value.SMTPPassword);

            await client.SendAsync(mimeMessage);
            await client.DisconnectAsync(true);
        }

        public Task SendSmsAsync(string number, string message)
        {
            return Task.CompletedTask;
        }
    }
}