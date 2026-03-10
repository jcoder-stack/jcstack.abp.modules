using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using JcStack.Abp.Keycloak.Identity.BackgroundJobs.Roles;

namespace JcStack.Abp.Keycloak.Identity.Identity;

/// <summary>
/// Keycloak Identity 角色应用服务
/// 代理 ABP 的 IdentityRoleAppService，在角色操作后触发 Keycloak 同步
/// 验证需求: 6.1, 6.2, 6.5, 6.7
/// </summary>
[ExposeServices(typeof(IdentityRoleAppService), typeof(IIdentityRoleAppService))]
public class JcStackAbpKeycloakIdentityRoleAppService : IdentityRoleAppService
{
    private readonly IBackgroundJobManager _backgroundJobManager;
    private readonly IOptions<JcStackAbpKeycloakIdentityOptions> _keycloakOptions;

    public JcStackAbpKeycloakIdentityRoleAppService(
        IdentityRoleManager roleManager,
        IIdentityRoleRepository roleRepository,
        IBackgroundJobManager backgroundJobManager,
        IOptions<JcStackAbpKeycloakIdentityOptions> keycloakOptions)
        : base(roleManager, roleRepository)
    {
        _backgroundJobManager = backgroundJobManager;
        _keycloakOptions = keycloakOptions;
    }

    /// <summary>
    /// 创建角色
    /// 先调用 ABP 原生方法，成功后触发 Keycloak 同步
    /// </summary>
    public override async Task<IdentityRoleDto> CreateAsync(IdentityRoleCreateDto input)
    {
        // 1. 先执行 ABP 原生的角色创建
        var createdRole = await base.CreateAsync(input);

        // 2. 如果启用了角色同步，则加入后台作业队列
        if (_keycloakOptions.Value.EnableRoleSync)
        {
            await _backgroundJobManager.EnqueueAsync(new RoleCreatedSyncJobArgs
            {
                RoleId = createdRole.Id,
                RoleName = createdRole.Name
            });
        }

        return createdRole;
    }

    /// <summary>
    /// 更新角色
    /// 先调用 ABP 原生方法，成功后触发 Keycloak 同步
    /// </summary>
    public override async Task<IdentityRoleDto> UpdateAsync(Guid id, IdentityRoleUpdateDto input)
    {
        // 1. 执行 ABP 原生的角色更新
        var updatedRole = await base.UpdateAsync(id, input);

        // 2. 如果启用了角色同步，则加入后台作业队列
        // 注意：Keycloak 角色更新通常只需要确保角色存在即可
        if (_keycloakOptions.Value.EnableRoleSync)
        {
            await _backgroundJobManager.EnqueueAsync(new RoleUpdatedSyncJobArgs
            {
                RoleId = updatedRole.Id,
                RoleName = updatedRole.Name
            });
        }

        return updatedRole;
    }

    /// <summary>
    /// 删除角色
    /// 先调用 ABP 原生方法，成功后触发 Keycloak 同步
    /// </summary>
    public override async Task DeleteAsync(Guid id)
    {
        // 1. 获取角色信息
        var role = await RoleRepository.GetAsync(id);

        // 2. 执行 ABP 原生的角色删除
        await base.DeleteAsync(id);

        // 3. 如果启用了角色同步，则加入后台作业队列
        // 注意：通常不建议删除 Keycloak 中的角色，因为可能被其他系统使用
        if (_keycloakOptions.Value.EnableRoleSync &&
            _keycloakOptions.Value.SyncRoleDeletionToKeycloak)
        {
            await _backgroundJobManager.EnqueueAsync(new RoleDeletedSyncJobArgs
            {
                RoleId = role.Id,
                RoleName = role.Name
            });
        }
    }
}
