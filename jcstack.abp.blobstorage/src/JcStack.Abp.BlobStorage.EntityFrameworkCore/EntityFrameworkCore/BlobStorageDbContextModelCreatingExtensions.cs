using JcStack.Abp.BlobStorage.StoredFiles;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace JcStack.Abp.BlobStorage.EntityFrameworkCore;

public static class BlobStorageDbContextModelCreatingExtensions
{
    public static void ConfigureBlobStorage(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<StoredFile>(b =>
        {
            b.ToTable(BlobStorageDbProperties.DbTablePrefix + "StoredFiles", BlobStorageDbProperties.DbSchema);

            b.ConfigureByConvention();

            // Properties
            b.Property(x => x.FileName)
                .IsRequired()
                .HasMaxLength(StoredFileConsts.MaxFileNameLength);

            b.Property(x => x.ContentType)
                .IsRequired()
                .HasMaxLength(StoredFileConsts.MaxContentTypeLength);

            b.Property(x => x.FileSize)
                .IsRequired();

            b.Property(x => x.BlobContainerName)
                .IsRequired()
                .HasMaxLength(StoredFileConsts.MaxBlobContainerNameLength);

            b.Property(x => x.BlobName)
                .IsRequired()
                .HasMaxLength(StoredFileConsts.MaxBlobNameLength);

            b.Property(x => x.Checksum)
                .HasMaxLength(StoredFileConsts.MaxChecksumLength);

            // Indexes
            b.HasIndex(x => x.BlobName);
            b.HasIndex(x => new { x.TenantId, x.CreationTime });
        });
    }
}
