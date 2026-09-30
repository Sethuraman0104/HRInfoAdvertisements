using HRInfoAdvertisements.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace HRInfoAdvertisements.Infrastructure.Email;

public sealed class SmtpEmailService : IEmailService
{
    private readonly EmailOptions _o;
    public SmtpEmailService(IOptions<EmailOptions> options) => _o = options.Value;

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_o.FromName, _o.FromAddress));
        msg.To.Add(MailboxAddress.Parse(to));
        msg.Subject = subject;
        msg.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        var s = _o.Smtp;
        var mode = s.Port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;

        using var client = new SmtpClient();
        await client.ConnectAsync(s.Host, s.Port, mode);
        if (!string.IsNullOrEmpty(s.User))
            await client.AuthenticateAsync(s.User, s.Password);
        await client.SendAsync(msg);
        await client.DisconnectAsync(true);
    }
}