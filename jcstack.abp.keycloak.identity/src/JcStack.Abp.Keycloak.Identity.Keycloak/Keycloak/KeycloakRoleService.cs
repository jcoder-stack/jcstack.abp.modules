using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace JcStack.Abp.Keycloak.Identity.Keycloak;

/// <summary>
/// Keycloak 角色服务实现（使用类型化客户端）
/// 验证需求: 6.5, 属性11
/// </summary>
[ExposeServices(typeof(KeycloakRoleService), typeof(IKeycloakRoleService))]
public class KeycloakRoleService : IKeycloakRoleService, ITransientDependency
{
    private readonly KeycloakAdminClient _adminClient;
    private readonly IOptions<JcStackAbpKeycloakIdentityOptions> _options;
    private readonly ILogger<KeycloakRoleService> _logger;

    /// <summary>
    /// 构造函数 - 注入类型化客户端 KeycloakAdminClient
    /// </summary>
    public KeycloakRoleService(
        KeycloakAdminClient adminClient,
        IOptions<JcStackAbpKeycloakIdentityOptions> options,
        ILogger<KeycloakRoleService> logger)
    {
        _adminClient = adminClient;
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// 创建角色（如果不存在）
    /// 验证需求: 6.5, 6.7, 属性11
    /// 注意：重试由 ABP BackgroundJob 自动处理
    /// </summary>
    public async Task<string> CreateRoleIfNotExistsAsync(string roleName, CancellationToken cancellationToken = default)
    {
        try
        {
            // 添加前缀（如果配置了）
            var keycloakRoleName = string.IsNullOrEmpty(_options.Value.RolePrefix)
                ? roleName
                : $"{_options.Value.RolePrefix}_{roleName}";

            // 检查角色是否已存在
            try
            {
                var existingRole = await _adminClient.GetRoleByNameAsync(keycloakRoleName, cancellationToken);

                if (existingRole != null)
                {
                    _logger.LogDebug(
                        "Role {RoleName} already exists in Keycloak",
                        keycloakRoleName);
                    return existingRole.Id!;
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // 角色不存在，继续创建
                _logger.LogDebug(
                    "Role {RoleName} not found in Keycloak, will create",
                    keycloakRoleName);
            }

            // 创建新角色
            var roleRepresentation = new RoleRepresentation
            {
                Name = keycloakRoleName,
                Description = $"Role synced from ABP: {roleName}"
            };

            var response = await _adminClient.CreateRoleAsync(roleRepresentation, cancellationToken);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation(
                "Created role {RoleName} in Keycloak",
                keycloakRoleName);

            // 重新获取以获得 ID
            var createdRole = await _adminClient.GetRoleByNameAsync(keycloakRoleName, cancellationToken);

            return createdRole?.Id ?? throw new InvalidOperationException($"Failed to get role ID for {keycloakRoleName}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            // 409 冲突 - 角色已存在（并发创建场景）
            _logger.LogWarning(
                ex,
                "Role {RoleName} creation conflict, retrieving existing role",
                roleName);

            var keycloakRoleName = string.IsNullOrEmpty(_options.Value.RolePrefix)
                ? roleName
                : $"{_options.Value.RolePrefix}_{roleName}";

            var existingRole = await _adminClient.GetRoleByNameAsync(keycloakRoleName, cancellationToken);
            return existingRole?.Id ?? throw new InvalidOperationException($"Role {keycloakRoleName} conflict but not found", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create role {RoleName} in Keycloak",
                roleName);
            throw; // ABP BackgroundJob 会自动重试
        }
    }

    /// <summary>
    /// 为用户分配角色（通过 ABP UserId 查找 Keycloak 用户）
    /// </summary>
    public async Task AssignRoleToUserAsync(Guid abpUserId, string roleName, CancellationToken cancellationToken = default)
    {
        // 获取 Keycloak 用户 ID
        var keycloakUserId = await GetKeycloakUserIdAsync(abpUserId, cancellationToken);
        if (keycloakUserId == null)
        {
            _logger.LogWarning(
                "User with ABP ID {AbpUserId} not found in Keycloak, cannot assign role",
                abpUserId);
            return;
        }

        await AssignRoleToKeycloakUserAsync(keycloakUserId, roleName, cancellationToken);

        _logger.LogInformation(
            "Assigned role {RoleName} to user with ABP ID {AbpUserId} in Keycloak",
            roleName, abpUserId);
    }

    /// <summary>
    /// 为用户分配角色（直接使用 Keycloak UserId）
    /// </summary>
    public async Task AssignRoleToKeycloakUserAsync(string keycloakUserId, string roleName, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. 确保角色存在
            await CreateRoleIfNotExistsAsync(roleName, cancellationToken);

            // 2. 获取角色详情
            var keycloakRoleName = string.IsNullOrEmpty(_options.Value.RolePrefix)
                ? roleName
                : $"{_options.Value.RolePrefix}_{roleName}";

            var role = await _adminClient.GetRoleByNameAsync(keycloakRoleName, cancellationToken);

            if (role == null)
            {
                throw new InvalidOperationException($"Role {keycloakRoleName} not found");
            }

            // 3. 分配角色
            var response = await _adminClient.AssignRoleToUserAsync(
                keycloakUserId,
                [role],
                cancellationToken);

            response.EnsureSuccessStatusCode();

            _logger.LogDebug(
                "Assigned role {RoleName} to Keycloak user {KeycloakUserId}",
                keycloakRoleName, keycloakUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to assign role {RoleName} to Keycloak user {KeycloakUserId}",
                roleName, keycloakUserId);
            throw;
        }
    }

    /// <summary>
    /// 从用户移除角色
    /// 验证需求: 6.2, 属性10, 10.1
    /// 注意：重试由 ABP BackgroundJob 自动处理
    /// </summary>
    public async Task RemoveRoleFromUserAsync(Guid abpUserId, string roleName, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. 获取 Keycloak 用户 ID
            var keycloakUserId = await GetKeycloakUserIdAsync(abpUserId, cancellationToken);
            if (keycloakUserId == null)
            {
                _logger.LogWarning(
                    "User with ABP ID {AbpUserId} not found in Keycloak, cannot remove role",
                    abpUserId);
                return;
            }

            // 2. 获取角色详情
            var keycloakRoleName = string.IsNullOrEmpty(_options.Value.RolePrefix)
                ? roleName
                : $"{_options.Value.RolePrefix}_{roleName}";

            var role = await _adminClient.GetRoleByNameAsync(keycloakRoleName, cancellationToken);

            if (role == null)
            {
                _logger.LogWarning(
                    "Role {RoleName} not found in Keycloak, nothing to remove",
                    keycloakRoleName);
                return;
            }

            // 3. 移除角色
            var response = await _adminClient.RemoveRoleFromUserAsync(
                keycloakUserId,
                new[] { role },
                cancellationToken);

            response.EnsureSuccessStatusCode();

            _logger.LogInformation(
                "Removed role {RoleName} from user with ABP ID {AbpUserId} in Keycloak",
                roleName,
                abpUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to remove role {RoleName} from user with ABP ID {AbpUserId}",
                roleName,
                abpUserId);
            throw; // ABP BackgroundJob 会自动重试
        }
    }

    /// <summary>
    /// 通过 ABP 用户 ID 获取 Keycloak 用户 ID
    /// </summary>
    private async Task<string?> GetKeycloakUserIdAsync(Guid abpUserId, CancellationToken cancellationToken)
    {
        var users = await _adminClient.GetUsersAsync(
            search: $"abp_user_id:{abpUserId}",
            cancellationToken: cancellationToken);

        return users?.FirstOrDefault()?.Id;
    }
}
