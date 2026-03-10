using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using JcStack.Abp.Keycloak.Identity.Keycloak;

namespace JcStack.Abp.Keycloak.Identity.BackgroundJobs.Roles;

/// <summary>
/// 角色创建同步后台作业
/// 验证需求: 6.5, 6.7, 属性11
/// </summary>
public class RoleCreatedSyncJob : AsyncBackgroundJob<RoleCreatedSyncJobArgs>, ITransientDependency
{
    private readonly IKeycloakRoleService _keycloakRoleService;
    private readonly ILogger<RoleCreatedSyncJob> _logger;

    public RoleCreatedSyncJob(
        IKeycloakRoleService keycloakRoleService,
        ILogger<RoleCreatedSyncJob> logger)
    {
        _keycloakRoleService = keycloakRoleService;
        _logger = logger;
    }

    public override async Task ExecuteAsync(RoleCreatedSyncJobArgs args)
    {
        try
        {
            _logger.LogInformation(
                "Syncing role creation to Keycloak: {RoleName} (ABP Role ID: {RoleId})",
                args.RoleName, args.RoleId);

            // 在 Keycloak 中创建角色（如果不存在）
            var keycloakRoleId = await _keycloakRoleService.CreateRoleIfNotExistsAsync(
                args.RoleName);

            _logger.LogInformation(
                "Successfully synced role {RoleName} to Keycloak with ID {KeycloakRoleId}",
                args.RoleName, keycloakRoleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to sync role creation to Keycloak: {RoleName} (ABP Role ID: {RoleId})",
                args.RoleName, args.RoleId);
            throw;
        }
    }
}
