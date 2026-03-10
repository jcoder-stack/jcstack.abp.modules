using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using JcStack.Abp.Keycloak.Identity.Keycloak;

namespace JcStack.Abp.Keycloak.Identity.BackgroundJobs.Roles;

/// <summary>
/// 角色更新同步后台作业
/// 注意：Keycloak 角色更新通常只需要确保角色存在即可
/// </summary>
public class RoleUpdatedSyncJob : AsyncBackgroundJob<RoleUpdatedSyncJobArgs>, ITransientDependency
{
    private readonly IKeycloakRoleService _keycloakRoleService;
    private readonly ILogger<RoleUpdatedSyncJob> _logger;

    public RoleUpdatedSyncJob(
        IKeycloakRoleService keycloakRoleService,
        ILogger<RoleUpdatedSyncJob> logger)
    {
        _keycloakRoleService = keycloakRoleService;
        _logger = logger;
    }

    public override async Task ExecuteAsync(RoleUpdatedSyncJobArgs args)
    {
        try
        {
            _logger.LogInformation(
                "Syncing role update to Keycloak: {RoleName} (ABP Role ID: {RoleId})",
                args.RoleName, args.RoleId);

            // Keycloak 角色更新：确保角色存在
            // 注意：Keycloak 角色通常不需要更新，只需要确保存在
            var keycloakRoleId = await _keycloakRoleService.CreateRoleIfNotExistsAsync(
                args.RoleName);

            _logger.LogInformation(
                "Successfully synced role update to Keycloak: {RoleName}",
                args.RoleName);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to sync role update to Keycloak: {RoleName} (ABP Role ID: {RoleId})",
                args.RoleName, args.RoleId);
            throw;
        }
    }
}
