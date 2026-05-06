using EstateIQ.Interfaces;
using EstateIQ.Models;
using Microsoft.AspNetCore.Identity;

namespace EstateIQ.Services.Auth;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string HashPassword(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(User user, string passwordHash, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, passwordHash, password);

        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
