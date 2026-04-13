using EstateIQ.Models;
using Microsoft.EntityFrameworkCore;

namespace EstateIQ.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TestTable> TestTables => Set<TestTable>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestTable>(entity =>
        {
            entity.ToTable("TestTable");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.CreatedAtUtc)
                .HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
