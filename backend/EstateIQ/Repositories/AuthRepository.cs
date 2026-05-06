using EstateIQ.Data;
using EstateIQ.Interfaces;
using EstateIQ.Models;
using Microsoft.EntityFrameworkCore;

namespace EstateIQ.Repositories;

public class AuthRepository(AppDbContext dbContext) : IAuthRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public Task<bool> EmailExistsAsync(string email)
    {
        return _dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.Email == email);
    }

    public Task<Role?> GetRoleByNameAsync(string roleName)
    {
        return _dbContext.Roles
            .AsNoTracking()
            .SingleOrDefaultAsync(role => role.Name == roleName);
    }

    public Task<User?> GetUserByEmailWithAuthDetailsAsync(string email)
    {
        return _dbContext.Users
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                    .ThenInclude(role => role.RolePermissions)
                        .ThenInclude(rolePermission => rolePermission.Permission)
            .SingleOrDefaultAsync(user => user.Email == email);
    }

    public Task<EmailVerificationToken?> GetEmailVerificationTokenAsync(string token)
    {
        return _dbContext.EmailVerificationTokens
            .Include(emailVerificationToken => emailVerificationToken.User)
            .SingleOrDefaultAsync(emailVerificationToken => emailVerificationToken.Token == token);
    }

    public async Task AddRegistrationAsync(User user, UserRole userRole, EmailVerificationToken emailVerificationToken)
    {
        _dbContext.Users.Add(user);
        _dbContext.UserRoles.Add(userRole);
        _dbContext.EmailVerificationTokens.Add(emailVerificationToken);

        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateEmailVerificationAsync(User user, EmailVerificationToken emailVerificationToken)
    {
        _dbContext.Users.Update(user);
        _dbContext.EmailVerificationTokens.Update(emailVerificationToken);

        await _dbContext.SaveChangesAsync();
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
    {
        _dbContext.RefreshTokens.Add(refreshToken);

        await _dbContext.SaveChangesAsync();
    }

    public Task<RefreshToken?> GetRefreshTokenByHashWithUserAuthDetailsAsync(string tokenHash)
    {
        return _dbContext.RefreshTokens
            .Include(refreshToken => refreshToken.User)
                .ThenInclude(user => user.UserRoles)
                    .ThenInclude(userRole => userRole.Role)
                        .ThenInclude(role => role.RolePermissions)
                            .ThenInclude(rolePermission => rolePermission.Permission)
            .SingleOrDefaultAsync(refreshToken => refreshToken.TokenHash == tokenHash);
    }

    public Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash)
    {
        return _dbContext.RefreshTokens
            .SingleOrDefaultAsync(refreshToken => refreshToken.TokenHash == tokenHash);
    }

    public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
    {
        _dbContext.RefreshTokens.Update(refreshToken);

        await _dbContext.SaveChangesAsync();
    }
}
