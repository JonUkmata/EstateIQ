using EstateIQ.Interfaces;

namespace EstateIQ.Tests;

internal sealed class FakeEmailSender(bool verificationEmailSent = false) : IEmailSender
{
    public Task<bool> SendEmailVerificationAsync(string recipientEmail, string recipientName, string verificationToken)
    {
        return Task.FromResult(verificationEmailSent);
    }
}
