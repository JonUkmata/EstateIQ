using EstateIQ.Models;

namespace EstateIQ.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);

    string GenerateRefreshToken();

    string HashToken(string token);

    string GenerateVerificationToken();

    DateTime GetAccessTokenExpirationUtc();

    DateTime GetRefreshTokenExpirationUtc();
}
