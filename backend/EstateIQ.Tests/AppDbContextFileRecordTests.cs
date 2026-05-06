using EstateIQ.Data;
using EstateIQ.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EstateIQ.Tests;

public class AppDbContextFileRecordTests
{
    [Fact]
    public void Model_IncludesFilesTableWithRequiredFieldsAndIndexes()
    {
        using var dbContext = CreateContext();

        var fileEntity = dbContext.Model.FindEntityType(typeof(FileRecord));

        Assert.NotNull(fileEntity);
        Assert.Equal("Files", fileEntity!.GetTableName());

        AssertRequiredString(fileEntity, nameof(FileRecord.Entity), 100);
        AssertRequiredString(fileEntity, nameof(FileRecord.FileName), 255);
        AssertRequiredString(fileEntity, nameof(FileRecord.FilePath), 500);
        AssertRequiredString(fileEntity, nameof(FileRecord.ContentType), 100);

        var entityLookupIndex = fileEntity.GetIndexes()
            .SingleOrDefault(index => index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(FileRecord.Entity), nameof(FileRecord.EntityId)]));

        Assert.NotNull(entityLookupIndex);
        Assert.Equal("IX_Files_Entity_EntityId", entityLookupIndex!.GetDatabaseName());

        var uploadedByIndex = fileEntity.GetIndexes()
            .SingleOrDefault(index => index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(FileRecord.UploadedBy)]));

        Assert.NotNull(uploadedByIndex);
        Assert.Equal("IX_Files_UploadedBy", uploadedByIndex!.GetDatabaseName());
        Assert.NotNull(fileEntity.FindNavigation(nameof(FileRecord.UploadedByUser)));
    }

    private static void AssertRequiredString(Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType, string propertyName, int maxLength)
    {
        var property = entityType.FindProperty(propertyName);

        Assert.NotNull(property);
        Assert.Equal(maxLength, property!.GetMaxLength());
        Assert.False(property.IsNullable);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
