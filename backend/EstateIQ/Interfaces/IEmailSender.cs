namespace EstateIQ.Interfaces;

public interface IEmailSender
{
    Task<bool> SendEmailVerificationAsync(string recipientEmail, string recipientName, string verificationToken);
}
