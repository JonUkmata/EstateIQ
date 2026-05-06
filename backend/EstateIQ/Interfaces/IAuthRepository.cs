using EstateIQ.Models;

namespace EstateIQ.Interfaces;

public interface IAuthRepository
{
    Task<bool> EmailExistsAsync(string email);

    Task<Role?> GetRoleByNameAsync(string roleName);

    Task AddRegistrationAsync(User user, UserRole userRole, EmailVerificationToken emailVerificationToken);
}
