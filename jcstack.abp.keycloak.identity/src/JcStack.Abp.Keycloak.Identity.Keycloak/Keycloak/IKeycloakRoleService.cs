using System;
using System.Threading;
using System.Threading.Tasks;

namespace JcStack.Abp.Keycloak.Identity.Keycloak;

/// <summary>
/// Keycloak 角色服务接口
/// </summary>
public interface IKeycloakRoleService
{
    /// <summary>
    /// 创建角色（如果不存在）
    /// </summary>
    Task<string> CreateRoleIfNotExistsAsync(string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 为用户分配角色（通过 ABP UserId 查找 Keycloak 用户）
    /// </summary>
    Task AssignRoleToUserAsync(Guid abpUserId, string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 为用户分配角色（直接使用 Keycloak UserId）
    /// </summary>
    Task AssignRoleToKeycloakUserAsync(string keycloakUserId, string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 从用户移除角色
    /// </summary>
    Task RemoveRoleFromUserAsync(Guid abpUserId, string roleName, CancellationToken cancellationToken = default);
}
