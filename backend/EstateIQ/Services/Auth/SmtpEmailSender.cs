using System.Net;
using System.Net.Mail;
using EstateIQ.Interfaces;
using EstateIQ.Models;
using Microsoft.Extensions.Options;

namespace EstateIQ.Services.Auth;

public class SmtpEmailSender(
    IOptions<SmtpSettings> smtpOptions,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly SmtpSettings _settings = smtpOptions.Value;
    private readonly ILogger<SmtpEmailSender> _logger = logger;

    public async Task<bool> SendEmailVerificationAsync(string recipientEmail, string recipientName, string verificationToken)
    {
        if (!_settings.IsConfigured)
        {
            _logger.LogWarning("SMTP is not configured. Email verification will use the local demo token fallback.");
            return false;
        }

        var verificationUrl = BuildVerificationUrl(verificationToken);
        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = "Verify your EstateIQ email",
            Body = BuildBody(recipientName, verificationUrl),
            IsBodyHtml = true
        };
        message.To.Add(new MailAddress(recipientEmail, recipientName));

        using var smtpClient = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl
        };

        if (!string.IsNullOrWhiteSpace(_settings.Username))
        {
            smtpClient.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
        }

        await smtpClient.SendMailAsync(message);
        _logger.LogInformation("Sent verification email to {RecipientEmail}.", recipientEmail);
        return true;
    }

    private string BuildVerificationUrl(string verificationToken)
    {
        var baseUrl = _settings.FrontendBaseUrl.TrimEnd('/');
        return $"{baseUrl}/verify-email?token={Uri.EscapeDataString(verificationToken)}";
    }

    private static string BuildBody(string recipientName, string verificationUrl)
    {
        var safeName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(recipientName) ? "there" : recipientName);
        var safeUrl = WebUtility.HtmlEncode(verificationUrl);

        return $"""
            <p>Hello {safeName},</p>
            <p>Thanks for creating an EstateIQ account. Confirm your email address by opening this link:</p>
            <p><a href="{safeUrl}">Verify email</a></p>
            <p>If the button does not work, copy and paste this URL into your browser:</p>
            <p>{safeUrl}</p>
            <p>This verification link expires in 24 hours.</p>
            """;
    }
}
