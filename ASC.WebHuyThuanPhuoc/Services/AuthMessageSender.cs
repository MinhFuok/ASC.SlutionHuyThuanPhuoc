using ASC.WebHuyThuanPhuoc.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

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

        public async Task SendSmsAsync(string number, string message)
        {
            if (string.IsNullOrWhiteSpace(number) ||
                string.IsNullOrWhiteSpace(message) ||
                string.IsNullOrWhiteSpace(_settings.Value.TwilioAccountSID) ||
                string.IsNullOrWhiteSpace(_settings.Value.TwilioAuthToken) ||
                string.IsNullOrWhiteSpace(_settings.Value.TwilioPhoneNumber))
            {
                return;
            }

            try
            {
                TwilioClient.Init(
                    _settings.Value.TwilioAccountSID,
                    _settings.Value.TwilioAuthToken);

                await MessageResource.CreateAsync(
                    to: new PhoneNumber(number),
                    from: new PhoneNumber(_settings.Value.TwilioPhoneNumber),
                    body: message);
            }
            catch (Twilio.Exceptions.ApiException ex)
            {
                Console.WriteLine($"Twilio SMS error: {ex.Code} - {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SMS error: {ex.Message}");
            }
        }
    }
}