using EstateIQ.Models;

namespace EstateIQ.Interfaces;

public interface IAuthRepository
{
    Task<bool> EmailExistsAsync(string email);

    Task<Role?> GetRoleByNameAsync(string roleName);

    Task<EmailVerificationToken?> GetEmailVerificationTokenAsync(string token);

    Task AddRegistrationAsync(User user, UserRole userRole, EmailVerificationToken emailVerificationToken);

    Task UpdateEmailVerificationAsync(User user, EmailVerificationToken emailVerificationToken);
}
