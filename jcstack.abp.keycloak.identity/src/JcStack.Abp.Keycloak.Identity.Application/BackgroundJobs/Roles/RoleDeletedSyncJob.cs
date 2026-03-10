using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace JcStack.Abp.Keycloak.Identity.BackgroundJobs.Roles;

/// <summary>
/// 角色删除同步后台作业
/// 注意：通常不建议删除 Keycloak 中的角色，因为可能被其他系统使用
/// 此作业仅记录日志，不执行实际删除操作
/// </summary>
public class RoleDeletedSyncJob : AsyncBackgroundJob<RoleDeletedSyncJobArgs>, ITransientDependency
{
    private readonly ILogger<RoleDeletedSyncJob> _logger;

    public RoleDeletedSyncJob(ILogger<RoleDeletedSyncJob> logger)
    {
        _logger = logger;
    }

    public override Task ExecuteAsync(RoleDeletedSyncJobArgs args)
    {
        // 注意：我们不实际删除 Keycloak 中的角色
        // 因为角色可能被其他系统或用户使用
        // 只记录日志以供审计
        _logger.LogWarning(
            "Role {RoleName} (ABP Role ID: {RoleId}) was deleted from ABP. " +
            "Keycloak role was NOT deleted to prevent breaking other systems.",
            args.RoleName, args.RoleId);

        return Task.CompletedTask;
    }
}
