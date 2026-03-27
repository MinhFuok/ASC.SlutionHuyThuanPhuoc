using System.Security.Cryptography.Xml;

namespace ASC.WebHuyThuanPhuoc.Services
{
    public interface IEmailSender
    {
    Task SendEmailAsync(string email, string subject, string message);
    }
}
