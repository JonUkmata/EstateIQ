using EstateIQ.Models;
using Microsoft.EntityFrameworkCore;

namespace EstateIQ.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    private static readonly DateTime SeedCreatedAt = new(2026, 4, 22, 0, 0, 0, DateTimeKind.Utc);

    public DbSet<Company> Companies => Set<Company>();

    public DbSet<Agent> Agents => Set<Agent>();

    public DbSet<AgentCompany> AgentCompanies => Set<AgentCompany>();

    public DbSet<PropertyType> PropertyTypes => Set<PropertyType>();

    public DbSet<PropertyStatus> PropertyStatuses => Set<PropertyStatus>();

    public DbSet<Property> Properties => Set<Property>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
