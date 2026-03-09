using Volo.Abp.Domain.Repositories;

namespace JcStack.Abp.Message.Notifications;

/// <summary>
/// 通知仓储接口
/// </summary>
public interface INotificationRepository : IBasicRepository<Notification, Guid>
{
    /// <summary>
    /// 获取通知列表
    /// </summary>
    Task<List<Notification>> GetListAsync(
        int skipCount,
        int maxResultCount,
        string? sorting = null,
        string? sourceModule = null,
        NotificationType? type = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取通知数量
    /// </summary>
    Task<long> GetCountAsync(
        string? sourceModule = null,
        NotificationType? type = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        CancellationToken cancellationToken = default);
}
