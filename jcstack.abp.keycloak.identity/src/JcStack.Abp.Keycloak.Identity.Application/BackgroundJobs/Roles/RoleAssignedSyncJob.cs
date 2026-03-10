using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using JcStack.Abp.Keycloak.Identity.Keycloak;

namespace JcStack.Abp.Keycloak.Identity.BackgroundJobs.Roles;

/// <summary>
/// 角色分配同步后台作业
/// 验证需求: 6.1, 6.5, 属性9, 属性11
/// </summary>
public class RoleAssignedSyncJob : AsyncBackgroundJob<RoleAssignedSyncJobArgs>, ITransientDependency
{
    private readonly IKeycloakRoleService _keycloakRoleService;
    private readonly ILogger<RoleAssignedSyncJob> _logger;

    public RoleAssignedSyncJob(
        IKeycloakRoleService keycloakRoleService,
        ILogger<RoleAssignedSyncJob> logger)
    {
        _keycloakRoleService = keycloakRoleService;
        _logger = logger;
    }

    public override async Task ExecuteAsync(RoleAssignedSyncJobArgs args)
    {
        try
        {
            // 1. 确保角色在 Keycloak 中存在
            await _keycloakRoleService.CreateRoleIfNotExistsAsync(args.RoleName);

            // 2. 为用户分配角色
            await _keycloakRoleService.AssignRoleToUserAsync(args.UserId, args.RoleName);

            _logger.LogInformation(
                "Successfully assigned role {RoleName} to user {UserId} in Keycloak",
                args.RoleName,
                args.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to assign role {RoleName} to user {UserId} in Keycloak",
                args.RoleName,
                args.UserId);
            throw;
        }
    }
}
