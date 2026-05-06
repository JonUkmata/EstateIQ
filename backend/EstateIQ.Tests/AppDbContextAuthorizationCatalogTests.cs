using EstateIQ.Data;
using EstateIQ.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace EstateIQ.Tests;

public class AppDbContextAuthorizationCatalogTests
{
    private static readonly string[] ExpectedRoles =
    [
        "Admin",
        "CompanyAdmin",
        "Agent",
        "User"
    ];

    private static readonly string[] ExpectedPermissions =
    [
        "ManageUsers",
        "ManageCompanies",
        "ManageAgents",
        "CreateProperty",
        "EditProperty",
        "DeleteProperty",
        "UploadPropertyImages",
        "ViewProperties",
        "BookViewing"
    ];

    [Fact]
    public void Model_IncludesRolesWithUniqueNameIndexAndSeedData()
    {
        using var dbContext = CreateContext();

        var roleEntity = GetDesignTimeEntityType<Role>(dbContext);

        Assert.NotNull(roleEntity);
        Assert.Equal("Roles", roleEntity!.GetTableName());

        var nameProperty = roleEntity.FindProperty(nameof(Role.Name));
        Assert.NotNull(nameProperty);
        Assert.Equal(100, nameProperty!.GetMaxLength());
        Assert.False(nameProperty.IsNullable);

        var nameIndex = roleEntity.GetIndexes()
            .SingleOrDefault(index => index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(Role.Name)]));

        Assert.NotNull(nameIndex);
        Assert.True(nameIndex!.IsUnique);
        Assert.Equal("IX_Roles_Name", nameIndex.GetDatabaseName());

        var seededRoles = roleEntity.GetSeedData()
            .Select(seed => seed[nameof(Role.Name)])
            .Cast<string>()
            .Order()
            .ToArray();

        Assert.Equal(ExpectedRoles.Order().ToArray(), seededRoles);
    }

    [Fact]
    public void Model_IncludesPermissionsWithUniqueNameIndexAndSeedData()
    {
        using var dbContext = CreateContext();

        var permissionEntity = GetDesignTimeEntityType<Permission>(dbContext);

        Assert.NotNull(permissionEntity);
        Assert.Equal("Permissions", permissionEntity!.GetTableName());

        var nameProperty = permissionEntity.FindProperty(nameof(Permission.Name));
        Assert.NotNull(nameProperty);
        Assert.Equal(100, nameProperty!.GetMaxLength());
        Assert.False(nameProperty.IsNullable);

        var nameIndex = permissionEntity.GetIndexes()
            .SingleOrDefault(index => index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(Permission.Name)]));

        Assert.NotNull(nameIndex);
        Assert.True(nameIndex!.IsUnique);
        Assert.Equal("IX_Permissions_Name", nameIndex.GetDatabaseName());

        var seededPermissions = permissionEntity.GetSeedData()
            .Select(seed => seed[nameof(Permission.Name)])
            .Cast<string>()
            .Order()
            .ToArray();

        Assert.Equal(ExpectedPermissions.Order().ToArray(), seededPermissions);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static IEntityType? GetDesignTimeEntityType<TEntity>(AppDbContext dbContext)
    {
        return dbContext.GetService<IDesignTimeModel>()
            .Model
            .FindEntityType(typeof(TEntity));
    }
}
