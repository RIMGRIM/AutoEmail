using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using AutoEmail.Models;

namespace AutoEmail.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    public SmtpEmailSender(IOptions<EmailSettings> options) => _settings = options.Value;

    public async Task SendAsync(string to, string subject, string htmlBody, string? textBody = null, CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.DisplayName, _settings.From));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var builder = new BodyBuilder
        {
            HtmlBody = htmlBody,
            TextBody = textBody ?? StripHtml(htmlBody)
        };
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.Smtp.Host, _settings.Smtp.Port, SecureSocketOptions.StartTls, ct);
        if (!string.IsNullOrWhiteSpace(_settings.Smtp.User))
        {
            await client.AuthenticateAsync(_settings.Smtp.User, _settings.Smtp.Password, ct);
        }
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
    }
}
