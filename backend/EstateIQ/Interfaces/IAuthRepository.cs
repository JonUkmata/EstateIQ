using EstateIQ.Models;

namespace EstateIQ.Interfaces;

public interface IAuthRepository
{
    Task<bool> EmailExistsAsync(string email);

    Task<Role?> GetRoleByNameAsync(string roleName);

    Task<User?> GetUserByEmailWithAuthDetailsAsync(string email);

    Task<EmailVerificationToken?> GetEmailVerificationTokenAsync(string token);

    Task AddRegistrationAsync(User user, UserRole userRole, EmailVerificationToken emailVerificationToken);

    Task UpdateEmailVerificationAsync(User user, EmailVerificationToken emailVerificationToken);

    Task AddRefreshTokenAsync(RefreshToken refreshToken);

    Task<RefreshToken?> GetRefreshTokenByHashWithUserAuthDetailsAsync(string tokenHash);
}
