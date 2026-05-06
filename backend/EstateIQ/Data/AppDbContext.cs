using EstateIQ.Models;
using Microsoft.EntityFrameworkCore;

namespace EstateIQ.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    private static readonly DateTime SeedCreatedAt = new(2026, 4, 22, 0, 0, 0, DateTimeKind.Utc);

    private static readonly Guid AdminRoleId = Guid.Parse("8b6f1a2d-3e4c-4a5b-9c7d-0f1e2d3c4b5a");
    private static readonly Guid CompanyAdminRoleId = Guid.Parse("2a7c9e4f-5b1d-42a8-86f3-1c9d0e7b6a5f");
    private static readonly Guid AgentRoleId = Guid.Parse("6f2d8a1b-4c3e-49f7-9a0b-5d6e7c8b9a01");
    private static readonly Guid UserRoleId = Guid.Parse("1d3b5f7a-8c9e-4b2a-91d0-6e5f4c3b2a10");

    private static readonly Guid ManageUsersPermissionId = Guid.Parse("d7a9c1e3-4f5b-46a8-90c2-1e3d5f7a9b0c");
    private static readonly Guid ManageCompaniesPermissionId = Guid.Parse("4b2a0c8e-6f1d-43b5-9a7c-2e4f6d8b0a1c");
    private static readonly Guid ManageAgentsPermissionId = Guid.Parse("9e1c3a5f-7b2d-40f6-8a9c-3d5e7f1b0c2a");
    private static readonly Guid CreatePropertyPermissionId = Guid.Parse("0f2e4d6c-8b1a-45c7-9e3f-4a6b8d0c2e1f");
    private static readonly Guid EditPropertyPermissionId = Guid.Parse("5c7a9e1d-3f2b-48d0-86a4-5b7c9e1f3d2a");
    private static readonly Guid DeletePropertyPermissionId = Guid.Parse("8d0b2e4f-6a1c-43e5-9b7d-6c8e0a2f4b1d");
    private static readonly Guid UploadPropertyImagesPermissionId = Guid.Parse("1a3c5e7f-9b0d-42f4-8c6a-7d9e1b3f5a0c");
    private static readonly Guid ViewPropertiesPermissionId = Guid.Parse("6b8d0f2e-4a1c-46e8-9d7b-8e0f2a4c6b1d");
    private static readonly Guid BookViewingPermissionId = Guid.Parse("3f5a7c9e-1b0d-44f6-8a2c-9f1e3d5b7a0c");

    private static readonly Guid AdminManageUsersRolePermissionId = Guid.Parse("93f8e4a1-2b7c-4d6e-8f90-a1b2c3d4e501");
    private static readonly Guid AdminManageCompaniesRolePermissionId = Guid.Parse("93f8e4a1-2b7c-4d6e-8f90-a1b2c3d4e502");
    private static readonly Guid AdminManageAgentsRolePermissionId = Guid.Parse("93f8e4a1-2b7c-4d6e-8f90-a1b2c3d4e503");
    private static readonly Guid AdminCreatePropertyRolePermissionId = Guid.Parse("93f8e4a1-2b7c-4d6e-8f90-a1b2c3d4e504");
    private static readonly Guid AdminEditPropertyRolePermissionId = Guid.Parse("93f8e4a1-2b7c-4d6e-8f90-a1b2c3d4e505");
    private static readonly Guid AdminDeletePropertyRolePermissionId = Guid.Parse("93f8e4a1-2b7c-4d6e-8f90-a1b2c3d4e506");
    private static readonly Guid AdminUploadPropertyImagesRolePermissionId = Guid.Parse("93f8e4a1-2b7c-4d6e-8f90-a1b2c3d4e507");
    private static readonly Guid AdminViewPropertiesRolePermissionId = Guid.Parse("93f8e4a1-2b7c-4d6e-8f90-a1b2c3d4e508");
    private static readonly Guid AdminBookViewingRolePermissionId = Guid.Parse("93f8e4a1-2b7c-4d6e-8f90-a1b2c3d4e509");

    private static readonly Guid CompanyAdminManageAgentsRolePermissionId = Guid.Parse("b4a7d2e5-6c8f-4a10-9b3d-e5f6a7b8c601");
    private static readonly Guid CompanyAdminCreatePropertyRolePermissionId = Guid.Parse("b4a7d2e5-6c8f-4a10-9b3d-e5f6a7b8c602");
    private static readonly Guid CompanyAdminEditPropertyRolePermissionId = Guid.Parse("b4a7d2e5-6c8f-4a10-9b3d-e5f6a7b8c603");
    private static readonly Guid CompanyAdminDeletePropertyRolePermissionId = Guid.Parse("b4a7d2e5-6c8f-4a10-9b3d-e5f6a7b8c604");
    private static readonly Guid CompanyAdminUploadPropertyImagesRolePermissionId = Guid.Parse("b4a7d2e5-6c8f-4a10-9b3d-e5f6a7b8c605");
    private static readonly Guid CompanyAdminViewPropertiesRolePermissionId = Guid.Parse("b4a7d2e5-6c8f-4a10-9b3d-e5f6a7b8c606");

    private static readonly Guid AgentCreatePropertyRolePermissionId = Guid.Parse("c5b8e3f6-7d90-4b21-8c4e-f6a7b8c9d701");
    private static readonly Guid AgentEditPropertyRolePermissionId = Guid.Parse("c5b8e3f6-7d90-4b21-8c4e-f6a7b8c9d702");
    private static readonly Guid AgentDeletePropertyRolePermissionId = Guid.Parse("c5b8e3f6-7d90-4b21-8c4e-f6a7b8c9d703");
    private static readonly Guid AgentUploadPropertyImagesRolePermissionId = Guid.Parse("c5b8e3f6-7d90-4b21-8c4e-f6a7b8c9d704");
    private static readonly Guid AgentViewPropertiesRolePermissionId = Guid.Parse("c5b8e3f6-7d90-4b21-8c4e-f6a7b8c9d705");

    private static readonly Guid UserViewPropertiesRolePermissionId = Guid.Parse("d6c9f4a7-8e01-4c32-9d5f-a7b8c9d0e801");
    private static readonly Guid UserBookViewingRolePermissionId = Guid.Parse("d6c9f4a7-8e01-4c32-9d5f-a7b8c9d0e802");

    public DbSet<Company> Companies => Set<Company>();

    public DbSet<Agent> Agents => Set<Agent>();

    public DbSet<AgentCompany> AgentCompanies => Set<AgentCompany>();

    public DbSet<PropertyType> PropertyTypes => Set<PropertyType>();

    public DbSet<PropertyStatus> PropertyStatuses => Set<PropertyStatus>();

    public DbSet<Property> Properties => Set<Property>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AssignedAt).HasDefaultValueSql("GETDATE()");

            entity.HasIndex(x => new { x.UserId, x.RoleId })
                .IsUnique()
                .HasDatabaseName("IX_UserRoles_UserId_RoleId");

            entity.HasOne(x => x.User)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Role)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new { x.RoleId, x.PermissionId })
                .IsUnique()
                .HasDatabaseName("IX_RolePermissions_RoleId_PermissionId");

            entity.HasOne(x => x.Role)
                .WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Permission)
                .WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasData(
                new RolePermission { Id = AdminManageUsersRolePermissionId, RoleId = AdminRoleId, PermissionId = ManageUsersPermissionId },
                new RolePermission { Id = AdminManageCompaniesRolePermissionId, RoleId = AdminRoleId, PermissionId = ManageCompaniesPermissionId },
                new RolePermission { Id = AdminManageAgentsRolePermissionId, RoleId = AdminRoleId, PermissionId = ManageAgentsPermissionId },
                new RolePermission { Id = AdminCreatePropertyRolePermissionId, RoleId = AdminRoleId, PermissionId = CreatePropertyPermissionId },
                new RolePermission { Id = AdminEditPropertyRolePermissionId, RoleId = AdminRoleId, PermissionId = EditPropertyPermissionId },
                new RolePermission { Id = AdminDeletePropertyRolePermissionId, RoleId = AdminRoleId, PermissionId = DeletePropertyPermissionId },
                new RolePermission { Id = AdminUploadPropertyImagesRolePermissionId, RoleId = AdminRoleId, PermissionId = UploadPropertyImagesPermissionId },
                new RolePermission { Id = AdminViewPropertiesRolePermissionId, RoleId = AdminRoleId, PermissionId = ViewPropertiesPermissionId },
                new RolePermission { Id = AdminBookViewingRolePermissionId, RoleId = AdminRoleId, PermissionId = BookViewingPermissionId },
                new RolePermission { Id = CompanyAdminManageAgentsRolePermissionId, RoleId = CompanyAdminRoleId, PermissionId = ManageAgentsPermissionId },
                new RolePermission { Id = CompanyAdminCreatePropertyRolePermissionId, RoleId = CompanyAdminRoleId, PermissionId = CreatePropertyPermissionId },
                new RolePermission { Id = CompanyAdminEditPropertyRolePermissionId, RoleId = CompanyAdminRoleId, PermissionId = EditPropertyPermissionId },
                new RolePermission { Id = CompanyAdminDeletePropertyRolePermissionId, RoleId = CompanyAdminRoleId, PermissionId = DeletePropertyPermissionId },
                new RolePermission { Id = CompanyAdminUploadPropertyImagesRolePermissionId, RoleId = CompanyAdminRoleId, PermissionId = UploadPropertyImagesPermissionId },
                new RolePermission { Id = CompanyAdminViewPropertiesRolePermissionId, RoleId = CompanyAdminRoleId, PermissionId = ViewPropertiesPermissionId },
                new RolePermission { Id = AgentCreatePropertyRolePermissionId, RoleId = AgentRoleId, PermissionId = CreatePropertyPermissionId },
                new RolePermission { Id = AgentEditPropertyRolePermissionId, RoleId = AgentRoleId, PermissionId = EditPropertyPermissionId },
                new RolePermission { Id = AgentDeletePropertyRolePermissionId, RoleId = AgentRoleId, PermissionId = DeletePropertyPermissionId },
                new RolePermission { Id = AgentUploadPropertyImagesRolePermissionId, RoleId = AgentRoleId, PermissionId = UploadPropertyImagesPermissionId },
                new RolePermission { Id = AgentViewPropertiesRolePermissionId, RoleId = AgentRoleId, PermissionId = ViewPropertiesPermissionId },
                new RolePermission { Id = UserViewPropertiesRolePermissionId, RoleId = UserRoleId, PermissionId = ViewPropertiesPermissionId },
                new RolePermission { Id = UserBookViewingRolePermissionId, RoleId = UserRoleId, PermissionId = BookViewingPermissionId });
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Description).HasMaxLength(255);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            entity.HasIndex(x => x.Name)
                .IsUnique()
                .HasDatabaseName("IX_Roles_Name");

            entity.HasData(
                new Role { Id = AdminRoleId, Name = "Admin", Description = "System administrator", CreatedAt = SeedCreatedAt },
                new Role { Id = CompanyAdminRoleId, Name = "CompanyAdmin", Description = "Company administrator", CreatedAt = SeedCreatedAt },
                new Role { Id = AgentRoleId, Name = "Agent", Description = "Real estate agent", CreatedAt = SeedCreatedAt },
                new Role { Id = UserRoleId, Name = "User", Description = "Public user", CreatedAt = SeedCreatedAt });
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Description).HasMaxLength(255);

            entity.HasIndex(x => x.Name)
                .IsUnique()
                .HasDatabaseName("IX_Permissions_Name");

            entity.HasData(
                new Permission { Id = ManageUsersPermissionId, Name = "ManageUsers", Description = "Manage users" },
                new Permission { Id = ManageCompaniesPermissionId, Name = "ManageCompanies", Description = "Manage companies" },
                new Permission { Id = ManageAgentsPermissionId, Name = "ManageAgents", Description = "Manage agents" },
                new Permission { Id = CreatePropertyPermissionId, Name = "CreateProperty", Description = "Create properties" },
                new Permission { Id = EditPropertyPermissionId, Name = "EditProperty", Description = "Edit properties" },
                new Permission { Id = DeletePropertyPermissionId, Name = "DeleteProperty", Description = "Delete properties" },
                new Permission { Id = UploadPropertyImagesPermissionId, Name = "UploadPropertyImages", Description = "Upload property images" },
                new Permission { Id = ViewPropertiesPermissionId, Name = "ViewProperties", Description = "View properties" },
                new Permission { Id = BookViewingPermissionId, Name = "BookViewing", Description = "Book property viewings" });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.LastName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Email)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.PasswordHash)
                .IsRequired();

            entity.Property(x => x.IsEmailConfirmed).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            entity.HasIndex(x => x.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("Companies");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Email).HasMaxLength(100);
            entity.Property(x => x.Phone).HasMaxLength(50);
            entity.Property(x => x.Address).HasMaxLength(300);
            entity.Property(x => x.City).HasMaxLength(100);
            entity.Property(x => x.Website).HasMaxLength(200);
            entity.Property(x => x.LogoUrl).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<Agent>(entity =>
        {
            entity.ToTable("Agents");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.LastName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Email)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Phone).HasMaxLength(50);
            entity.Property(x => x.Mobile).HasMaxLength(50);
            entity.Property(x => x.PhotoUrl).HasMaxLength(500);
            entity.Property(x => x.Bio).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            entity.HasIndex(x => x.Email)
                .IsUnique()
                .HasDatabaseName("IX_Agents_Email");
        });

        modelBuilder.Entity<AgentCompany>(entity =>
        {
            entity.ToTable("AgentCompanies");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Role).HasMaxLength(100);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            entity.HasIndex(x => new { x.AgentId, x.CompanyId })
                .IsUnique()
                .HasDatabaseName("IX_AgentCompanies_AgentId_CompanyId");

            entity.HasOne(x => x.Agent)
                .WithMany(x => x.AgentCompanies)
                .HasForeignKey(x => x.AgentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Company)
                .WithMany(x => x.AgentCompanies)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PropertyType>(entity =>
        {
            entity.ToTable("PropertyTypes");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            entity.HasIndex(x => x.Name)
                .IsUnique()
                .HasDatabaseName("IX_PropertyTypes_Name");

            entity.HasData(
                new PropertyType { Id = 1, Name = "Apartment", CreatedAt = SeedCreatedAt, IsActive = true },
                new PropertyType { Id = 2, Name = "House", CreatedAt = SeedCreatedAt, IsActive = true },
                new PropertyType { Id = 3, Name = "Villa", CreatedAt = SeedCreatedAt, IsActive = true },
                new PropertyType { Id = 4, Name = "Office", CreatedAt = SeedCreatedAt, IsActive = true },
                new PropertyType { Id = 5, Name = "Land", CreatedAt = SeedCreatedAt, IsActive = true },
                new PropertyType { Id = 6, Name = "Commercial", CreatedAt = SeedCreatedAt, IsActive = true },
                new PropertyType { Id = 7, Name = "Penthouse", CreatedAt = SeedCreatedAt, IsActive = true });
        });

        modelBuilder.Entity<PropertyStatus>(entity =>
        {
            entity.ToTable("PropertyStatuses");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.ColorCode).HasMaxLength(7);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            entity.HasIndex(x => x.Name)
                .IsUnique()
                .HasDatabaseName("IX_PropertyStatuses_Name");

            entity.HasData(
                new PropertyStatus { Id = 1, Name = "For Sale", ColorCode = "#007bff", CreatedAt = SeedCreatedAt, IsActive = true },
                new PropertyStatus { Id = 2, Name = "For Rent", ColorCode = "#ffc107", CreatedAt = SeedCreatedAt, IsActive = true },
                new PropertyStatus { Id = 3, Name = "Sold", ColorCode = "#28a745", CreatedAt = SeedCreatedAt, IsActive = true },
                new PropertyStatus { Id = 4, Name = "Rented", ColorCode = "#17a2b8", CreatedAt = SeedCreatedAt, IsActive = true },
                new PropertyStatus { Id = 5, Name = "Off Market", ColorCode = "#6c757d", CreatedAt = SeedCreatedAt, IsActive = true },
                new PropertyStatus { Id = 6, Name = "Under Contract", ColorCode = "#fd7e14", CreatedAt = SeedCreatedAt, IsActive = true });
        });

        modelBuilder.Entity<Property>(entity =>
        {
            entity.ToTable("Properties");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Description).HasColumnType("nvarchar(max)");
            entity.Property(x => x.Price).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Area).HasColumnType("decimal(10,2)");
            entity.Property(x => x.Address)
                .HasMaxLength(300)
                .IsRequired();
            entity.Property(x => x.City)
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(x => x.Latitude).HasColumnType("decimal(10,8)");
            entity.Property(x => x.Longitude).HasColumnType("decimal(11,8)");
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            entity.ToTable(t => t.HasCheckConstraint("CK_Properties_Price", "[Price] > 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_Properties_Area", "[Area] > 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_Properties_YearBuilt", "[YearBuilt] IS NULL OR ([YearBuilt] >= 1800 AND [YearBuilt] <= YEAR(GETDATE()))"));
            entity.ToTable(t => t.HasCheckConstraint("CK_Properties_Latitude", "[Latitude] IS NULL OR ([Latitude] >= -90 AND [Latitude] <= 90)"));
            entity.ToTable(t => t.HasCheckConstraint("CK_Properties_Longitude", "[Longitude] IS NULL OR ([Longitude] >= -180 AND [Longitude] <= 180)"));

            entity.HasIndex(x => x.City).HasDatabaseName("IX_Properties_City");
            entity.HasIndex(x => x.Price).HasDatabaseName("IX_Properties_Price");
            entity.HasIndex(x => x.PropertyTypeId).HasDatabaseName("IX_Properties_PropertyTypeId");
            entity.HasIndex(x => x.PropertyStatusId).HasDatabaseName("IX_Properties_PropertyStatusId");
            entity.HasIndex(x => x.CompanyId).HasDatabaseName("IX_Properties_CompanyId");
            entity.HasIndex(x => x.AgentId).HasDatabaseName("IX_Properties_AgentId");

            entity.HasOne(x => x.PropertyType)
                .WithMany(x => x.Properties)
                .HasForeignKey(x => x.PropertyTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PropertyStatus)
                .WithMany(x => x.Properties)
                .HasForeignKey(x => x.PropertyStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Company)
                .WithMany(x => x.Properties)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Agent)
                .WithMany(x => x.Properties)
                .HasForeignKey(x => x.AgentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
