using EstateIQ.Data;
using EstateIQ.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EstateIQ.Tests;

public class AppDbContextUserTests
{
    [Fact]
    public void Model_IncludesUsersWithUniqueEmailIndex()
    {
        using var dbContext = CreateContext();

        var userEntity = dbContext.Model.FindEntityType(typeof(User));

        Assert.NotNull(userEntity);
        Assert.Equal("Users", userEntity!.GetTableName());

        var emailProperty = userEntity.FindProperty(nameof(User.Email));
        Assert.NotNull(emailProperty);
        Assert.Equal(255, emailProperty!.GetMaxLength());
        Assert.False(emailProperty.IsNullable);

        var emailIndex = userEntity.GetIndexes()
            .SingleOrDefault(index => index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(User.Email)]));

        Assert.NotNull(emailIndex);
        Assert.True(emailIndex!.IsUnique);
        Assert.Equal("IX_Users_Email", emailIndex.GetDatabaseName());
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
