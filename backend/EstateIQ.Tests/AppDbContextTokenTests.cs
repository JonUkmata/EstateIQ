using EstateIQ.Data;
using EstateIQ.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EstateIQ.Tests;

public class AppDbContextTokenTests
{
    [Theory]
    [InlineData(typeof(RefreshToken), "RefreshTokens", nameof(RefreshToken.UserId), "IX_RefreshTokens_UserId")]
    [InlineData(typeof(EmailVerificationToken), "EmailVerificationTokens", nameof(EmailVerificationToken.UserId), "IX_EmailVerificationTokens_UserId")]
    [InlineData(typeof(PasswordResetToken), "PasswordResetTokens", nameof(PasswordResetToken.UserId), "IX_PasswordResetTokens_UserId")]
    public void Model_IncludesTokenTableWithUserIdIndex(Type entityType, string tableName, string userIdPropertyName, string indexName)
    {
        using var dbContext = CreateContext();

        var tokenEntity = dbContext.Model.FindEntityType(entityType);

        Assert.NotNull(tokenEntity);
        Assert.Equal(tableName, tokenEntity!.GetTableName());
        Assert.NotNull(tokenEntity.FindNavigation("User"));

        var userIdIndex = tokenEntity.GetIndexes()
            .SingleOrDefault(index => index.Properties.Select(property => property.Name)
                .SequenceEqual([userIdPropertyName]));

        Assert.NotNull(userIdIndex);
        Assert.Equal(indexName, userIdIndex!.GetDatabaseName());
    }

    [Fact]
    public void Model_ConfiguresRefreshTokenHashAsRequired()
    {
        using var dbContext = CreateContext();

        var refreshTokenEntity = dbContext.Model.FindEntityType(typeof(RefreshToken));
        var tokenHashProperty = refreshTokenEntity!.FindProperty(nameof(RefreshToken.TokenHash));

        Assert.NotNull(tokenHashProperty);
        Assert.False(tokenHashProperty!.IsNullable);
    }

    [Theory]
    [InlineData(typeof(EmailVerificationToken), nameof(EmailVerificationToken.Token), "IX_EmailVerificationTokens_Token")]
    [InlineData(typeof(PasswordResetToken), nameof(PasswordResetToken.Token), "IX_PasswordResetTokens_Token")]
    public void Model_ConfiguresVerificationAndResetTokensAsRequiredAndUnique(Type entityType, string tokenPropertyName, string indexName)
    {
        using var dbContext = CreateContext();

        var tokenEntity = dbContext.Model.FindEntityType(entityType);
        var tokenProperty = tokenEntity!.FindProperty(tokenPropertyName);

        Assert.NotNull(tokenProperty);
        Assert.Equal(255, tokenProperty!.GetMaxLength());
        Assert.False(tokenProperty.IsNullable);

        var tokenIndex = tokenEntity.GetIndexes()
            .SingleOrDefault(index => index.Properties.Select(property => property.Name)
                .SequenceEqual([tokenPropertyName]));

        Assert.NotNull(tokenIndex);
        Assert.True(tokenIndex!.IsUnique);
        Assert.Equal(indexName, tokenIndex.GetDatabaseName());
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
