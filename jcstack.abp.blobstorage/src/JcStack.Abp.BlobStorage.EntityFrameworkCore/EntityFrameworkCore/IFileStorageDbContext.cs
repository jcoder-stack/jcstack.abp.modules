using JcStack.Abp.BlobStorage.StoredFiles;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace JcStack.Abp.BlobStorage.EntityFrameworkCore;

[ConnectionStringName(BlobStorageDbProperties.ConnectionStringName)]
public interface IBlobStorageDbContext : IEfCoreDbContext
{
    DbSet<StoredFile> StoredFiles { get; }
}
