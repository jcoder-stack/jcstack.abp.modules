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
/// Keycloak 用户服务实现
/// 支持并发安全的用户创建和属性管理
/// </summary>
[ExposeServices(typeof(KeycloakUserService), typeof(IKeycloakUserService))]
public class KeycloakUserService : IKeycloakUserService, ITransientDependency
{
    private readonly KeycloakAdminClient _adminClient;
    private readonly IOptions<JcStackAbpKeycloakIdentityOptions> _options;
    private readonly ILogger<KeycloakUserService> _logger;

    /// <summary>
    /// 获取当前系统的 app_user_id 属性名
    /// </summary>
    private string AppUserIdAttributeName =>
        $"{_options.Value.Attributes.AppUserIdPrefix}{_options.Value.SourceSystem}";

    /// <summary>
    /// 获取当前系统的 tenant_id 属性名
    /// </summary>
    private string TenantIdAttributeName =>
        $"{_options.Value.Attributes.TenantIdPrefix}{_options.Value.SourceSystem}";

    public KeycloakUserService(
        KeycloakAdminClient adminClient,
        IOptions<JcStackAbpKeycloakIdentityOptions> options,
        ILogger<KeycloakUserService> logger)
    {
        _adminClient = adminClient;
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// 创建或获取用户（并发安全）
    /// 处理多系统同时创建同一用户的场景
    /// </summary>
    public async Task<CreateUserResult> CreateOrGetUserAsync(
        string username,
        string email,
        string appUserId,
        string? password = null,
        string? firstName = null,
        string? lastName = null,
        string? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        // 1. 先检查用户是否已存在
        var existingUserId = await GetUserIdByUsernameAsync(username, cancellationToken);
        if (existingUserId != null)
        {
            _logger.LogInformation("User {Username} already exists in Keycloak with ID {UserId}",
                username, existingUserId);

            // 用户已存在，更新当前系统的属性
            await UpdateUserAttributesAsync(existingUserId, appUserId, tenantId, cancellationToken);

            return new CreateUserResult(existingUserId, IsNewUser: false);
        }

        // 再按邮箱查找
        existingUserId = await GetUserIdByEmailAsync(email, cancellationToken);
        if (existingUserId != null)
        {
            _logger.LogInformation("User with email {Email} already exists in Keycloak with ID {UserId}",
                email, existingUserId);

            // 用户已存在，更新当前系统的属性
            await UpdateUserAttributesAsync(existingUserId, appUserId, tenantId, cancellationToken);

            return new CreateUserResult(existingUserId, IsNewUser: false);
        }

        // 2. 尝试创建用户
        try
        {
            var userId = await CreateUserInKeycloakAsync(
                username, email, appUserId, password, firstName, lastName, tenantId, cancellationToken);

            _logger.LogInformation("Created user {Username} in Keycloak with ID {UserId}",
                username, userId);
            return new CreateUserResult(userId, IsNewUser: true);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            // 409 冲突 - 并发创建场景，另一个系统刚创建了该用户
            _logger.LogWarning("Conflict creating user {Username}, fetching existing user", username);

            // 重新查询用户
            existingUserId = await GetUserIdByUsernameAsync(username, cancellationToken);
            if (existingUserId != null)
            {
                // 更新当前系统的属性
                await UpdateUserAttributesAsync(existingUserId, appUserId, tenantId, cancellationToken);
                return new CreateUserResult(existingUserId, IsNewUser: false);
            }

            // 如果还是找不到，可能是邮箱冲突
            existingUserId = await GetUserIdByEmailAsync(email, cancellationToken);
            if (existingUserId != null)
            {
                await UpdateUserAttributesAsync(existingUserId, appUserId, tenantId, cancellationToken);
                return new CreateUserResult(existingUserId, IsNewUser: false);
            }

            throw new InvalidOperationException(
                $"User {username} creation conflict but user not found", ex);
        }
    }

    /// <summary>
    /// 在 Keycloak 中创建用户
    /// </summary>
    private async Task<string> CreateUserInKeycloakAsync(
        string username,
        string email,
        string appUserId,
        string? password,
        string? firstName,
        string? lastName,
        string? tenantId,
        CancellationToken cancellationToken)
    {
        var attributes = new Dictionary<string, ICollection<string>>
        {
            // 写入当前系统的 app_user_id
            [AppUserIdAttributeName] = new List<string> { appUserId }
        };

        // 写入当前系统的 tenant_id
        if (!string.IsNullOrEmpty(tenantId))
        {
            attributes[TenantIdAttributeName] = new List<string> { tenantId };
        }

        var userRepresentation = new UserRepresentation
        {
            Username = username,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Enabled = true,
            EmailVerified = true,
            Attributes = attributes
        };

        // 如果提供了密码，设置临时密码（用户首次登录需要修改）
        if (!string.IsNullOrEmpty(password))
        {
            _logger.LogDebug("Setting password for user {Username}, length: {Length}", username, password.Length);
            userRepresentation.Credentials = new List<CredentialRepresentation>
            {
                new()
                {
                    Type = "password",
                    Value = password,
                    Temporary = true  // 首次登录需要修改密码
                }
            };
        }

        var response = await _adminClient.CreateUserAsync(userRepresentation, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Failed to create user {Username} in Keycloak. Status: {StatusCode}, Error: {Error}",
                username, response.StatusCode, errorContent);
            response.EnsureSuccessStatusCode();
        }

        // 从 Location header 提取用户 ID
        var location = response.Headers.Location?.ToString();
        var userId = location?.Split('/').Last();

        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidOperationException("Failed to extract user ID from Keycloak response");
        }

        return userId;
    }

    /// <summary>
    /// 通过用户名获取 Keycloak 用户 ID
    /// </summary>
    public async Task<string?> GetUserIdByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var users = await _adminClient.GetUsersAsync(
            username: username,
            exact: true,
            cancellationToken: cancellationToken);

        return users?.FirstOrDefault()?.Id;
    }

    /// <summary>
    /// 通过邮箱获取 Keycloak 用户 ID
    /// </summary>
    public async Task<string?> GetUserIdByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var users = await _adminClient.GetUsersAsync(
            email: email,
            exact: true,
            cancellationToken: cancellationToken);

        return users?.FirstOrDefault()?.Id;
    }

    /// <summary>
    /// 检查 Keycloak 用户是否存在
    /// </summary>
    public async Task<bool> UserExistsAsync(string keycloakUserId, CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _adminClient.GetUsersAsync(
                search: keycloakUserId,
                cancellationToken: cancellationToken);

            return users?.Any(u => u.Id == keycloakUserId) ?? false;
        }
        catch
        {
            _logger.LogWarning("Failed to check if user {UserId} exists in Keycloak", keycloakUserId);
            return false;
        }
    }

    /// <summary>
    /// 更新用户密码
    /// </summary>
    public async Task<bool> UpdateUserPasswordAsync(
        string keycloakUserId,
        string newPassword,
        bool temporary = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating password for Keycloak user {UserId}, temporary: {Temporary}",
            keycloakUserId, temporary);

        var response = await _adminClient.ResetUserPasswordAsync(
            keycloakUserId,
            newPassword,
            temporary: temporary,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Successfully updated password for Keycloak user {UserId}", keycloakUserId);
            return true;
        }

        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogWarning(
            "Failed to update password for Keycloak user {UserId}. Status: {StatusCode}, Error: {Error}",
            keycloakUserId, response.StatusCode, errorContent);
        return false;
    }

    /// <summary>
    /// 更新用户基本信息
    /// </summary>
    public async Task<bool> UpdateUserProfileAsync(
        string keycloakUserId,
        string? email = null,
        string? firstName = null,
        string? lastName = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 获取现有用户信息
            var user = await _adminClient.GetUserAsync(keycloakUserId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User {KeycloakUserId} not found in Keycloak", keycloakUserId);
                return false;
            }

            // 更新基本信息
            if (email != null) user.Email = email;
            if (firstName != null) user.FirstName = firstName;
            if (lastName != null) user.LastName = lastName;

            var response = await _adminClient.UpdateUserAsync(keycloakUserId, user, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Updated Keycloak user {KeycloakUserId} profile: Email={Email}, FirstName={FirstName}, LastName={LastName}",
                    keycloakUserId, email, firstName, lastName);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "Failed to update profile for Keycloak user {KeycloakUserId}. Status: {StatusCode}, Error: {Error}",
                keycloakUserId, response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update profile for Keycloak user {KeycloakUserId}", keycloakUserId);
            return false;
        }
    }

    /// <summary>
    /// 更新用户属性（app_user_id 和 tenant_id）
    /// </summary>
    public async Task UpdateUserAttributesAsync(
        string keycloakUserId,
        string appUserId,
        string? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 获取现有用户信息
            var user = await _adminClient.GetUserAsync(keycloakUserId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User {KeycloakUserId} not found in Keycloak", keycloakUserId);
                return;
            }

            // 更新属性
            user.Attributes ??= new Dictionary<string, ICollection<string>>();
            user.Attributes[AppUserIdAttributeName] = new List<string> { appUserId };

            if (!string.IsNullOrEmpty(tenantId))
            {
                user.Attributes[TenantIdAttributeName] = new List<string> { tenantId };
            }

            await _adminClient.UpdateUserAsync(keycloakUserId, user, cancellationToken);

            _logger.LogInformation(
                "Updated Keycloak user {KeycloakUserId} attributes: {AppUserIdAttr}={AppUserId}, {TenantIdAttr}={TenantId}",
                keycloakUserId, AppUserIdAttributeName, appUserId, TenantIdAttributeName, tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to update attributes for Keycloak user {KeycloakUserId}",
                keycloakUserId);
            throw;
        }
    }

    /// <summary>
    /// 获取用户的 app_user_id 属性
    /// </summary>
    public async Task<string?> GetAppUserIdAsync(string keycloakUserId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _adminClient.GetUserAsync(keycloakUserId, cancellationToken);
            if (user?.Attributes == null)
            {
                return null;
            }

            if (user.Attributes.TryGetValue(AppUserIdAttributeName, out var values) && values.Any())
            {
                return values.First();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get app_user_id for Keycloak user {KeycloakUserId}", keycloakUserId);
            return null;
        }
    }

    /// <summary>
    /// 获取用户的 tenant_id 属性
    /// </summary>
    public async Task<string?> GetTenantIdAsync(string keycloakUserId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _adminClient.GetUserAsync(keycloakUserId, cancellationToken);
            if (user?.Attributes == null)
            {
                return null;
            }

            if (user.Attributes.TryGetValue(TenantIdAttributeName, out var values) && values.Any())
            {
                return values.First();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get tenant_id for Keycloak user {KeycloakUserId}", keycloakUserId);
            return null;
        }
    }

    /// <summary>
    /// 通过 app_user_id 查找 Keycloak 用户 ID
    /// </summary>
    public async Task<string?> GetKeycloakIdByAppUserIdAsync(string appUserId, CancellationToken cancellationToken = default)
    {
        try
        {
            // 使用属性查询
            var users = await _adminClient.GetUsersAsync(
                q: $"{AppUserIdAttributeName}:{appUserId}",
                cancellationToken: cancellationToken);

            return users?.FirstOrDefault()?.Id;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to find Keycloak user by app_user_id {AppUserId}", appUserId);
            return null;
        }
    }
}
