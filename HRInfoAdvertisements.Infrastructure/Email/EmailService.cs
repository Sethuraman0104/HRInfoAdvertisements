using HRInfoAdvertisements.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace HRInfoAdvertisements.Infrastructure.Email;

public sealed class EmailOptions
{
    public string Host { get; set; } = "";
    public int Port { get; set; }
    public bool UseSsl { get; set; }
    public string? User { get; set; }
    public string? Password { get; set; }
    public string FromAddress { get; set; } = "";
    public string FromName { get; set; } = "";
}

public sealed class EmailService : IEmailService
{
    private readonly EmailOptions _o;

    public EmailService(IOptions<EmailOptions> options) => _o = options.Value;

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_o.FromName, _o.FromAddress));
        msg.To.Add(MailboxAddress.Parse(to));
        msg.Subject = subject;
        msg.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_o.Host, _o.Port,
            _o.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

        if (!string.IsNullOrEmpty(_o.User))
            await client.AuthenticateAsync(_o.User, _o.Password);

        await client.SendAsync(msg);
        await client.DisconnectAsync(true);
    }
}