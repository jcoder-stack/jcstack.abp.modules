using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using JcStack.Abp.BlobStorage.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace JcStack.Abp.BlobStorage.StoredFiles;

/// <summary>
/// 存储文件仓储实现
/// </summary>
public class EfCoreStoredFileRepository
    : EfCoreRepository<BlobStorageDbContext, StoredFile, Guid>, IStoredFileRepository
{
    public EfCoreStoredFileRepository(IDbContextProvider<BlobStorageDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    /// <inheritdoc />
    public async Task<StoredFile?> FindByBlobNameAsync(
        string blobName,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .FirstOrDefaultAsync(x => x.BlobName == blobName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<StoredFile>> GetListAsync(
        int skipCount,
        int maxResultCount,
        string? sorting = null,
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet
            .WhereIf(!string.IsNullOrWhiteSpace(filter),
                x => x.FileName.Contains(filter!))
            .OrderBy(sorting.IsNullOrWhiteSpace() ? nameof(StoredFile.CreationTime) + " desc" : sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<long> GetCountAsync(
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet
            .WhereIf(!string.IsNullOrWhiteSpace(filter),
                x => x.FileName.Contains(filter!))
            .LongCountAsync(cancellationToken);
    }
}
