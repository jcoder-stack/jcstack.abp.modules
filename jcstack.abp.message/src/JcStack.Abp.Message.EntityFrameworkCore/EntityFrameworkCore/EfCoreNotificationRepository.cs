using System.Linq.Dynamic.Core;
using JcStack.Abp.Message.Notifications;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace JcStack.Abp.Message.EntityFrameworkCore;

/// <summary>
/// 通知仓储实现
/// </summary>
public class EfCoreNotificationRepository : EfCoreRepository<IMessageDbContext, Notification, Guid>, INotificationRepository
{
    public EfCoreNotificationRepository(IDbContextProvider<IMessageDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<Notification>> GetListAsync(
        int skipCount,
        int maxResultCount,
        string? sorting = null,
        string? sourceModule = null,
        NotificationType? type = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();

        query = ApplyFilter(query, sourceModule, type, startTime, endTime);

        if (string.IsNullOrWhiteSpace(sorting))
        {
            sorting = $"{nameof(Notification.CreationTime)} desc";
        }

        return await query
            .OrderBy(sorting)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public async Task<long> GetCountAsync(
        string? sourceModule = null,
        NotificationType? type = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();

        query = ApplyFilter(query, sourceModule, type, startTime, endTime);

        return await query.LongCountAsync(GetCancellationToken(cancellationToken));
    }

    private static IQueryable<Notification> ApplyFilter(
        IQueryable<Notification> query,
        string? sourceModule,
        NotificationType? type,
        DateTime? startTime,
        DateTime? endTime)
    {
        return query
            .WhereIf(!string.IsNullOrWhiteSpace(sourceModule), x => x.SourceModule == sourceModule)
            .WhereIf(type.HasValue, x => x.Type == type!.Value)
            .WhereIf(startTime.HasValue, x => x.CreationTime >= startTime!.Value)
            .WhereIf(endTime.HasValue, x => x.CreationTime <= endTime!.Value);
    }
}
