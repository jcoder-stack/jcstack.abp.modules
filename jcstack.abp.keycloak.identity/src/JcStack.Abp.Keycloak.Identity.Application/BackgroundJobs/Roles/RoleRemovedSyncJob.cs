using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using JcStack.Abp.Keycloak.Identity.Keycloak;

namespace JcStack.Abp.Keycloak.Identity.BackgroundJobs.Roles;

/// <summary>
/// 角色移除同步后台作业
/// 验证需求: 6.2, 属性10
/// </summary>
public class RoleRemovedSyncJob : AsyncBackgroundJob<RoleRemovedSyncJobArgs>, ITransientDependency
{
    private readonly IKeycloakRoleService _keycloakRoleService;
    private readonly ILogger<RoleRemovedSyncJob> _logger;

    public RoleRemovedSyncJob(
        IKeycloakRoleService keycloakRoleService,
        ILogger<RoleRemovedSyncJob> logger)
    {
        _keycloakRoleService = keycloakRoleService;
        _logger = logger;
    }

    public override async Task ExecuteAsync(RoleRemovedSyncJobArgs args)
    {
        try
        {
            await _keycloakRoleService.RemoveRoleFromUserAsync(args.UserId, args.RoleName);

            _logger.LogInformation(
                "Successfully removed role {RoleName} from user {UserId} in Keycloak",
                args.RoleName,
                args.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to remove role {RoleName} from user {UserId} in Keycloak",
                args.RoleName,
                args.UserId);
            throw;
        }
    }
}
