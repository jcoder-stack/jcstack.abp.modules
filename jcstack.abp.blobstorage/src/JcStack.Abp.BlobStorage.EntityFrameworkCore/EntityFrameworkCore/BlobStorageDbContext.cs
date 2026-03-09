using JcStack.Abp.BlobStorage.StoredFiles;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace JcStack.Abp.BlobStorage.EntityFrameworkCore;

[ConnectionStringName(BlobStorageDbProperties.ConnectionStringName)]
public class BlobStorageDbContext : AbpDbContext<BlobStorageDbContext>, IBlobStorageDbContext
{
    public DbSet<StoredFile> StoredFiles { get; set; } = null!;

    public BlobStorageDbContext(DbContextOptions<BlobStorageDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureBlobStorage();
    }
}
