using EstateIQ.Data;
using EstateIQ.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace EstateIQ.Tests;

public class AppDbContextRoleAssignmentTests
{
    private static readonly Dictionary<string, string[]> ExpectedRolePermissions = new()
    {
        ["Admin"] =
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
        ],
        ["CompanyAdmin"] =
        [
            "ManageAgents",
            "CreateProperty",
            "EditProperty",
            "DeleteProperty",
            "UploadPropertyImages",
            "ViewProperties"
        ],
        ["Agent"] =
        [
            "CreateProperty",
            "EditProperty",
            "DeleteProperty",
            "UploadPropertyImages",
            "ViewProperties"
        ],
        ["User"] =
        [
            "ViewProperties",
            "BookViewing"
        ]
    };

    [Fact]
    public void Model_IncludesUserRolesWithRelationshipsAndUniqueAssignmentIndex()
    {
        using var dbContext = CreateContext();

        var userRoleEntity = GetDesignTimeEntityType<UserRole>(dbContext);

        Assert.NotNull(userRoleEntity);
        Assert.Equal("UserRoles", userRoleEntity!.GetTableName());
        Assert.NotNull(userRoleEntity.FindNavigation(nameof(UserRole.User)));
        Assert.NotNull(userRoleEntity.FindNavigation(nameof(UserRole.Role)));

        var assignmentIndex = userRoleEntity.GetIndexes()
            .SingleOrDefault(index => index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(UserRole.UserId), nameof(UserRole.RoleId)]));

        Assert.NotNull(assignmentIndex);
        Assert.True(assignmentIndex!.IsUnique);
        Assert.Equal("IX_UserRoles_UserId_RoleId", assignmentIndex.GetDatabaseName());
    }

    [Fact]
    public void Model_IncludesRolePermissionsWithRelationshipsUniqueIndexAndSeedMapping()
    {
        using var dbContext = CreateContext();

        var rolePermissionEntity = GetDesignTimeEntityType<RolePermission>(dbContext);

        Assert.NotNull(rolePermissionEntity);
        Assert.Equal("RolePermissions", rolePermissionEntity!.GetTableName());
        Assert.NotNull(rolePermissionEntity.FindNavigation(nameof(RolePermission.Role)));
        Assert.NotNull(rolePermissionEntity.FindNavigation(nameof(RolePermission.Permission)));

        var rolePermissionIndex = rolePermissionEntity.GetIndexes()
            .SingleOrDefault(index => index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(RolePermission.RoleId), nameof(RolePermission.PermissionId)]));

        Assert.NotNull(rolePermissionIndex);
        Assert.True(rolePermissionIndex!.IsUnique);
        Assert.Equal("IX_RolePermissions_RoleId_PermissionId", rolePermissionIndex.GetDatabaseName());

        var actualRolePermissions = GetSeededRolePermissions(dbContext);

        Assert.Equal(ExpectedRolePermissions.Count, actualRolePermissions.Count);
        foreach (var expectedRolePermission in ExpectedRolePermissions)
        {
            Assert.True(actualRolePermissions.TryGetValue(expectedRolePermission.Key, out var actualPermissions));
            Assert.Equal(expectedRolePermission.Value.Order().ToArray(), actualPermissions!.Order().ToArray());
        }
    }

    private static Dictionary<string, string[]> GetSeededRolePermissions(AppDbContext dbContext)
    {
        var roleEntity = GetDesignTimeEntityType<Role>(dbContext)!;
        var permissionEntity = GetDesignTimeEntityType<Permission>(dbContext)!;
        var rolePermissionEntity = GetDesignTimeEntityType<RolePermission>(dbContext)!;

        var rolesById = roleEntity.GetSeedData()
            .ToDictionary(seed => GetSeedValue<Guid>(seed, nameof(Role.Id)), seed => GetSeedValue<string>(seed, nameof(Role.Name)));

        var permissionsById = permissionEntity.GetSeedData()
            .ToDictionary(seed => GetSeedValue<Guid>(seed, nameof(Permission.Id)), seed => GetSeedValue<string>(seed, nameof(Permission.Name)));

        return rolePermissionEntity.GetSeedData()
            .GroupBy(seed => rolesById[GetSeedValue<Guid>(seed, nameof(RolePermission.RoleId))])
            .ToDictionary(
                group => group.Key,
                group => group.Select(seed => permissionsById[GetSeedValue<Guid>(seed, nameof(RolePermission.PermissionId))]).ToArray());
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

    private static TValue GetSeedValue<TValue>(IDictionary<string, object?> seed, string propertyName)
    {
        return Assert.IsType<TValue>(seed[propertyName]);
    }
}
