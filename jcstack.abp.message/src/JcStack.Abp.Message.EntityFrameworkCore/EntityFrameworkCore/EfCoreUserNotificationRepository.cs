using JcStack.Abp.Message.Notifications;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace JcStack.Abp.Message.EntityFrameworkCore;

/// <summary>
/// 用户通知仓储实现
/// </summary>
public class EfCoreUserNotificationRepository : EfCoreRepository<IMessageDbContext, UserNotification, Guid>, IUserNotificationRepository
{
    public EfCoreUserNotificationRepository(IDbContextProvider<IMessageDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<UserNotification>> GetListByUserAsync(
        Guid userId,
        int skipCount,
        int maxResultCount,
        UserNotificationState? state = null,
        NotificationType? type = null,
        NotificationSeverity? severity = null,
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();

        query = query
            .Where(x => x.UserId == userId)
            .WhereIf(state.HasValue, x => x.State == state!.Value);

        if (includeDetails)
        {
            query = query.Include(x => x.Notification);
        }

        if (type.HasValue || severity.HasValue)
        {
            query = query
                .WhereIf(type.HasValue, x => x.Notification != null && x.Notification.Type == type!.Value)
                .WhereIf(severity.HasValue, x => x.Notification != null && x.Notification.Severity == severity!.Value);
        }

        return await query
            .OrderByDescending(x => x.CreationTime)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public async Task<long> GetCountByUserAsync(
        Guid userId,
        UserNotificationState? state = null,
        NotificationType? type = null,
        NotificationSeverity? severity = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();

        query = query
            .Where(x => x.UserId == userId)
            .WhereIf(state.HasValue, x => x.State == state!.Value);

        if (type.HasValue || severity.HasValue)
        {
            query = query
                .Include(x => x.Notification)
                .WhereIf(type.HasValue, x => x.Notification != null && x.Notification.Type == type!.Value)
                .WhereIf(severity.HasValue, x => x.Notification != null && x.Notification.Severity == severity!.Value);
        }

        return await query.LongCountAsync(GetCancellationToken(cancellationToken));
    }

    public async Task<int> GetUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();

        return await query
            .Where(x => x.UserId == userId && x.State == UserNotificationState.Unread)
            .CountAsync(GetCancellationToken(cancellationToken));
    }

    public async Task<UserNotification?> FindByUserAndNotificationAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();

        return await query
            .Include(x => x.Notification)
            .FirstOrDefaultAsync(
                x => x.UserId == userId && x.NotificationId == notificationId,
                GetCancellationToken(cancellationToken));
    }

    public async Task MarkAsReadAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var now = DateTime.UtcNow;

        await dbContext.Set<UserNotification>()
            .Where(x => x.UserId == userId && x.NotificationId == notificationId && x.State == UserNotificationState.Unread)
            .ExecuteUpdateAsync(
                x => x.SetProperty(p => p.State, UserNotificationState.Read)
                      .SetProperty(p => p.ReadTime, now),
                GetCancellationToken(cancellationToken));
    }

    public async Task MarkAllAsReadAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var now = DateTime.UtcNow;

        await dbContext.Set<UserNotification>()
            .Where(x => x.UserId == userId && x.State == UserNotificationState.Unread)
            .ExecuteUpdateAsync(
                x => x.SetProperty(p => p.State, UserNotificationState.Read)
                      .SetProperty(p => p.ReadTime, now),
                GetCancellationToken(cancellationToken));
    }

    public async Task MarkBatchAsReadAsync(
        Guid userId,
        List<Guid> notificationIds,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var now = DateTime.UtcNow;

        await dbContext.Set<UserNotification>()
            .Where(x => x.UserId == userId
                && notificationIds.Contains(x.NotificationId)
                && x.State == UserNotificationState.Unread)
            .ExecuteUpdateAsync(
                x => x.SetProperty(p => p.State, UserNotificationState.Read)
                      .SetProperty(p => p.ReadTime, now),
                GetCancellationToken(cancellationToken));
    }
}
