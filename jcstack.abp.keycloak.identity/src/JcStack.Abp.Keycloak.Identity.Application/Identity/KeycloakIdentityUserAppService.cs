using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using JcStack.Abp.Keycloak.Identity.Keycloak;
using JcStack.Abp.Keycloak.Identity.Middleware;

using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace JcStack.Abp.Keycloak.Identity.Identity;

/// <summary>
/// Keycloak Identity 用户应用服务
/// 使用 Keycloak ID 作为 ABP 用户 ID，通过属性建立多系统映射
/// </summary>
/// <remarks>
/// 创建用户流程：
/// 1. 先在 Keycloak 创建用户，获取 Keycloak ID
/// 2. 使用 Keycloak ID 作为 ABP 用户 ID 创建本地用户
/// 3. Keycloak 属性 app_user_id = Keycloak ID（ID 一致）
/// 
/// 更新用户流程：
/// 1. 检查 Keycloak 是否存在该用户
/// 2. 如果不存在，创建并写入 app_user_id 属性
/// 3. 同步密码和用户信息
/// </remarks>
[ExposeServices(typeof(IdentityUserAppService), typeof(IIdentityUserAppService))]
public class JcStackAbpKeycloakIdentityUserAppService : IdentityUserAppService
{
    private readonly IIdentityUserRepository _userRepository;
    private readonly IKeycloakUserService _keycloakUserService;
    private readonly IKeycloakRoleService _keycloakRoleService;
    private readonly KeycloakAdminClient _keycloakAdminClient;
    private readonly IDistributedCache<UserIdMappingCacheItem> _mappingCache;
    private readonly IOptions<JcStackAbpKeycloakIdentityOptions> _keycloakOptions;
    private readonly ILogger<JcStackAbpKeycloakIdentityUserAppService> _logger;

    public JcStackAbpKeycloakIdentityUserAppService(
        IdentityUserManager userManager,
        IIdentityUserRepository userRepository,
        IIdentityRoleRepository roleRepository,
        IOptions<IdentityOptions> identityOptions,
        IPermissionChecker permissionChecker,
        IKeycloakUserService keycloakUserService,
        IKeycloakRoleService keycloakRoleService,
        KeycloakAdminClient keycloakAdminClient,
        IDistributedCache<UserIdMappingCacheItem> mappingCache,
        IOptions<JcStackAbpKeycloakIdentityOptions> keycloakOptions,
        ILogger<JcStackAbpKeycloakIdentityUserAppService> logger)
        : base(userManager, userRepository, roleRepository, identityOptions, permissionChecker)
    {
        _userRepository = userRepository;
        _keycloakUserService = keycloakUserService;
        _keycloakRoleService = keycloakRoleService;
        _keycloakAdminClient = keycloakAdminClient;
        _mappingCache = mappingCache;
        _keycloakOptions = keycloakOptions;
        _logger = logger;
    }

    /// <summary>
    /// 创建用户
    /// 1. 先在 Keycloak 创建用户，获取 Keycloak ID
    /// 2. 使用 Keycloak ID 作为 ABP 用户 ID 创建本地用户
    /// </summary>
    public override async Task<IdentityUserDto> CreateAsync(IdentityUserCreateDto input)
    {
        if (!_keycloakOptions.Value.EnableUserSync)
        {
            return await base.CreateAsync(input);
        }

        // 1. 先在 Keycloak 创建用户，使用临时 ID（后续会用 Keycloak 返回的 ID）
        var tempAppUserId = GuidGenerator.Create().ToString();

        var keycloakResult = await _keycloakUserService.CreateOrGetUserAsync(
            username: input.UserName,
            email: input.Email,
            appUserId: tempAppUserId,
            password: input.Password,
            firstName: input.Name,
            lastName: input.Surname,
            tenantId: CurrentTenant.Id?.ToString());

        var keycloakUserId = Guid.Parse(keycloakResult.KeycloakUserId);

        _logger.LogInformation(
            "Got Keycloak user ID {KeycloakUserId} for {UserName}, IsNewUser: {IsNewUser}",
            keycloakUserId, input.UserName, keycloakResult.IsNewUser);

        // 2. 检查 ABP 中是否已存在该用户
        var existingUser = await _userRepository.FindAsync(keycloakUserId);
        if (existingUser != null)
        {
            _logger.LogWarning(
                "User {UserName} already exists in ABP with Keycloak ID {KeycloakUserId}",
                input.UserName, keycloakUserId);
            return ObjectMapper.Map<IdentityUser, IdentityUserDto>(existingUser);
        }

        // 3. 使用 Keycloak ID 创建 ABP 用户
        var user = new IdentityUser(
            keycloakUserId,
            input.UserName,
            input.Email,
            CurrentTenant.Id);

        user.Name = input.Name;
        user.Surname = input.Surname;
        user.SetPhoneNumber(input.PhoneNumber, false);
        user.SetIsActive(input.IsActive);

        var createResult = await UserManager.CreateAsync(user, input.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new UserFriendlyException($"Failed to create user: {errors}");
        }

        // 设置 LockoutEnabled
        await UserManager.SetLockoutEnabledAsync(user, input.LockoutEnabled);

        // 4. 添加 UserLogin 记录（支持 UseKeycloakForward 中间件）
        var providerName = _keycloakOptions.Value.LoginProviderName;
        await UserManager.AddLoginAsync(
            user,
            new UserLoginInfo(providerName, keycloakResult.KeycloakUserId, providerName));

        _logger.LogDebug(
            "Added UserLogin mapping: {UserName} -> Keycloak {KeycloakUserId}",
            input.UserName, keycloakResult.KeycloakUserId);

        // 5. 分配角色
        if (input.RoleNames != null && input.RoleNames.Any())
        {
            await UserManager.SetRolesAsync(user, input.RoleNames);

            // 同步角色到 Keycloak
            if (_keycloakOptions.Value.EnableRoleSync)
            {
                await SyncRolesToKeycloakAsync(keycloakResult.KeycloakUserId, input.RoleNames);
            }
        }

        _logger.LogInformation(
            "Created ABP user {UserName} with Keycloak ID {KeycloakUserId}",
            input.UserName, keycloakUserId);

        return ObjectMapper.Map<IdentityUser, IdentityUserDto>(user);
    }

    /// <summary>
    /// 更新用户
    /// 1. 检查 Keycloak 是否存在该用户
    /// 2. 如果不存在，创建并写入 app_user_id 属性
    /// 3. 同步密码到 Keycloak
    /// 4. 同步角色到 Keycloak
    /// </summary>
    public override async Task<IdentityUserDto> UpdateAsync(Guid id, IdentityUserUpdateDto input)
    {
        var existingUser = await _userRepository.GetAsync(id);

        // 禁用用户名更新（用户名由 Keycloak 管理）
        if (input.UserName != existingUser.UserName)
        {
            input.UserName = existingUser.UserName;
        }

        var newPassword = input.Password;
        var result = await base.UpdateAsync(id, input);

        if (_keycloakOptions.Value.EnableUserSync)
        {
            // 确保 Keycloak 用户存在
            await EnsureKeycloakUserAsync(existingUser, newPassword);

            // 同步基本信息到 Keycloak
            await SyncUserProfileToKeycloakAsync(id, input);

            // 如果密码有更新，同步到 Keycloak
            if (!newPassword.IsNullOrEmpty())
            {
                await SyncPasswordToKeycloakAsync(id, newPassword);
            }

            // 如果角色有更新，同步到 Keycloak
            if (input.RoleNames != null && _keycloakOptions.Value.EnableRoleSync)
            {
                await SyncRolesToKeycloakAsync(id, input.RoleNames);
            }
        }

        return result;
    }

    /// <summary>
    /// 确保 Keycloak 用户存在并有正确的属性
    /// </summary>
    private async Task EnsureKeycloakUserAsync(IdentityUser user, string? password)
    {
        try
        {
            // 1. 检查 Keycloak 是否存在该用户（通过 ABP 用户 ID）
            var keycloakUserExists = await _keycloakUserService.UserExistsAsync(user.Id.ToString());

            if (keycloakUserExists)
            {
                _logger.LogDebug(
                    "User {UserName} exists in Keycloak with ID {UserId}",
                    user.UserName, user.Id);
                return;
            }

            // 2. 通过用户名查找
            var keycloakUserId = await _keycloakUserService.GetUserIdByUsernameAsync(user.UserName);

            if (keycloakUserId != null)
            {
                // Keycloak 存在该用户名，添加 UserLogin 记录
                _logger.LogInformation(
                    "Found Keycloak user by username {UserName}: {KeycloakUserId}",
                    user.UserName, keycloakUserId);

                await EnsureUserLoginAsync(user, keycloakUserId);
                return;
            }

            // 3. Keycloak 中完全不存在，创建新用户
            _logger.LogInformation(
                "User {UserName} does not exist in Keycloak, creating with app_user_id {UserId}...",
                user.UserName, user.Id);

            var keycloakResult = await _keycloakUserService.CreateOrGetUserAsync(
                username: user.UserName,
                email: user.Email,
                appUserId: user.Id.ToString(),
                password: password,
                firstName: user.Name,
                lastName: user.Surname,
                tenantId: user.TenantId?.ToString());

            // 添加 UserLogin 记录
            await EnsureUserLoginAsync(user, keycloakResult.KeycloakUserId);

            _logger.LogInformation(
                "Created Keycloak user for {UserName}: {KeycloakUserId}, app_user_id: {AppUserId}",
                user.UserName, keycloakResult.KeycloakUserId, user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to ensure Keycloak user for {UserName}. Update succeeded but Keycloak sync failed.",
                user.UserName);
        }
    }

    /// <summary>
    /// 同步用户基本信息到 Keycloak
    /// </summary>
    private async Task SyncUserProfileToKeycloakAsync(Guid userId, IdentityUserUpdateDto input)
    {
        try
        {
            var user = await UserManager.GetByIdAsync(userId);
            var logins = await UserManager.GetLoginsAsync(user);
            var keycloakLogin = logins.FirstOrDefault(l =>
                l.LoginProvider == _keycloakOptions.Value.LoginProviderName);

            if (keycloakLogin == null)
            {
                _logger.LogWarning(
                    "User {UserName} does not have Keycloak login mapping, skipping profile sync",
                    user.UserName);
                return;
            }

            var success = await _keycloakUserService.UpdateUserProfileAsync(
                keycloakLogin.ProviderKey,
                email: input.Email,
                firstName: input.Name,
                lastName: input.Surname);

            if (success)
            {
                _logger.LogInformation(
                    "Successfully synced profile to Keycloak for user {UserName}",
                    user.UserName);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to sync profile to Keycloak for user {UserName}",
                    user.UserName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error syncing profile to Keycloak for user {UserId}. ABP profile updated but Keycloak sync failed.",
                userId);
            // 不抛出异常，ABP 信息已更新成功
        }
    }

    /// <summary>
    /// 同步密码到 Keycloak
    /// </summary>
    private async Task SyncPasswordToKeycloakAsync(Guid userId, string newPassword)
    {
        try
        {
            var user = await UserManager.GetByIdAsync(userId);
            var logins = await UserManager.GetLoginsAsync(user);
            var keycloakLogin = logins.FirstOrDefault(l =>
                l.LoginProvider == _keycloakOptions.Value.LoginProviderName);

            if (keycloakLogin == null)
            {
                _logger.LogWarning(
                    "User {UserName} does not have Keycloak login mapping, skipping password sync",
                    user.UserName);
                return;
            }

            var success = await _keycloakUserService.UpdateUserPasswordAsync(
                keycloakLogin.ProviderKey,
                newPassword,
                temporary: false);

            if (success)
            {
                _logger.LogInformation(
                    "Successfully synced password to Keycloak for user {UserName}",
                    user.UserName);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to sync password to Keycloak for user {UserName}",
                    user.UserName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error syncing password to Keycloak for user {UserId}. ABP password updated but Keycloak sync failed.",
                userId);
            // 不抛出异常，ABP 密码已更新成功
        }
    }

    /// <summary>
    /// 确保 UserLogin 记录存在
    /// </summary>
    private async Task EnsureUserLoginAsync(IdentityUser user, string keycloakUserId)
    {
        var providerName = _keycloakOptions.Value.LoginProviderName;
        var logins = await UserManager.GetLoginsAsync(user);
        var existingLogin = logins.FirstOrDefault(l => l.LoginProvider == providerName);

        if (existingLogin != null)
        {
            // 已存在映射
            if (existingLogin.ProviderKey == keycloakUserId)
            {
                return;
            }

            // 映射不一致，移除旧的
            _logger.LogWarning(
                "Removing stale UserLogin for {UserName}: old={OldKey}, new={NewKey}",
                user.UserName, existingLogin.ProviderKey, keycloakUserId);

            await UserManager.RemoveLoginAsync(user, providerName, existingLogin.ProviderKey);
        }

        // 添加新的映射
        var result = await UserManager.AddLoginAsync(
            user,
            new UserLoginInfo(providerName, keycloakUserId, providerName));

        if (result.Succeeded)
        {
            _logger.LogInformation(
                "Added UserLogin mapping: {UserName} -> Keycloak {KeycloakUserId}",
                user.UserName, keycloakUserId);
        }
        else
        {
            _logger.LogError(
                "Failed to add UserLogin for {UserName}: {Errors}",
                user.UserName, string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    /// <summary>
    /// 同步角色到 Keycloak（使用 ABP User ID）
    /// </summary>
    /// <param name="userId">ABP 用户 ID</param>
    /// <param name="roleNames">角色名称列表</param>
    private async Task SyncRolesToKeycloakAsync(Guid userId, string[] roleNames)
    {
        try
        {
            var user = await UserManager.GetByIdAsync(userId);
            var logins = await UserManager.GetLoginsAsync(user);
            var keycloakLogin = logins.FirstOrDefault(l =>
                l.LoginProvider == _keycloakOptions.Value.LoginProviderName);

            if (keycloakLogin == null)
            {
                _logger.LogWarning(
                    "User {UserName} does not have Keycloak login mapping, skipping role sync",
                    user.UserName);
                return;
            }

            await SyncRolesToKeycloakAsync(keycloakLogin.ProviderKey, roleNames);

            _logger.LogInformation(
                "Successfully synced {RoleCount} roles to Keycloak for user {UserName}",
                roleNames.Length, user.UserName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error syncing roles to Keycloak for user {UserId}. ABP roles updated but Keycloak sync failed.",
                userId);
            // 不抛出异常，ABP 角色已更新成功
        }
    }

    /// <summary>
    /// 同步角色到 Keycloak（使用 Keycloak User ID）
    /// </summary>
    /// <param name="keycloakUserId">Keycloak 用户 ID</param>
    /// <param name="roleNames">角色名称列表</param>
    private async Task SyncRolesToKeycloakAsync(string keycloakUserId, string[] roleNames)
    {
        foreach (var roleName in roleNames)
        {
            try
            {
                await _keycloakRoleService.AssignRoleToKeycloakUserAsync(keycloakUserId, roleName);

                _logger.LogDebug(
                    "Synced role {RoleName} to Keycloak for user {KeycloakUserId}",
                    roleName, keycloakUserId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to sync role {RoleName} to Keycloak for user {KeycloakUserId}, continuing with other roles",
                    roleName, keycloakUserId);
            }
        }
    }

    /// <summary>
    /// 删除用户
    /// 1. 先获取用户的 Keycloak Login 映射信息
    /// 2. 删除本地用户
    /// 3. 清除缓存
    /// 4. 可选同步删除 Keycloak 用户
    /// </summary>
    public override async Task DeleteAsync(Guid id)
    {
        // 1. 获取用户信息和 Keycloak 登录映射
        var user = await UserManager.GetByIdAsync(id);
        var logins = await UserManager.GetLoginsAsync(user);
        var keycloakLogin = logins.FirstOrDefault(l =>
            l.LoginProvider == _keycloakOptions.Value.LoginProviderName);

        var keycloakUserId = keycloakLogin?.ProviderKey;

        // 2. 删除本地用户
        await base.DeleteAsync(id);

        // 3. 清除缓存
        if (keycloakLogin != null)
        {
            var cacheKey = $"{keycloakLogin.LoginProvider}:{keycloakLogin.ProviderKey}";
            await _mappingCache.RemoveAsync(cacheKey);

            _logger.LogInformation(
                "Cleared user mapping cache for Keycloak sub {KeycloakSub}",
                keycloakLogin.ProviderKey);
        }

        // 4. 可选同步删除 Keycloak 用户
        if (_keycloakOptions.Value.SyncUserDeletionToKeycloak &&
            !string.IsNullOrEmpty(keycloakUserId))
        {
            try
            {
                var response = await _keycloakAdminClient.DeleteUserAsync(keycloakUserId);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "Deleted user {UserName} from Keycloak (ID: {KeycloakUserId})",
                        user.UserName, keycloakUserId);
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to delete user {UserName} from Keycloak. Status: {StatusCode}",
                        user.UserName, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                // 记录错误但不抛出异常，本地删除已成功
                _logger.LogError(ex,
                    "Error deleting user {UserName} from Keycloak. Local deletion succeeded.",
                    user.UserName);
            }
        }
    }
}
