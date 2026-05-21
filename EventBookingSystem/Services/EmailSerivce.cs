using EventBookingSystem.Configuration;
using EventBookingSystem.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EventBookingSystem.Services
{
    public class EmailSerivce : IEmailService
    {
        private readonly MailSettings _mailSettings;
        public EmailSerivce(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }
        public async Task SendEmailAsync(string mailTo, string subject, string body, IList<IFormFile>? attachments = null)
        {
            var email = new MimeMessage();
            email.Subject = subject;
            var from = new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Email);
            var to = new MailboxAddress(null, mailTo);
            email.From.Add(from);
            email.To.Add(to);

            var bb = new BodyBuilder();

            if (attachments is not null)
            {
                byte[] fileBytes;
                foreach(var file in attachments)
                {
                    if (file.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        await file.CopyToAsync(ms);
                        fileBytes = ms.ToArray();

                        bb.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                    }
                }
            }

            bb.HtmlBody = body;
            email.Body = bb.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_mailSettings.Email, _mailSettings.Password);
            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);
        }
    }
}
