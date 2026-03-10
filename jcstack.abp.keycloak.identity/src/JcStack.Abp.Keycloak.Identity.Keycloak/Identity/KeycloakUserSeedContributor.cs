using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using JcStack.Abp.Keycloak.Identity.Keycloak;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.Uow;

namespace JcStack.Abp.Keycloak.Identity.Identity;

/// <summary>
/// Keycloak 用户种子数据贡献者
/// 将 ABP 种子用户同步到 Keycloak 并建立 UserLogin 映射
/// </summary>
/// <remarks>
/// 工作流程：
/// 1. ABP IdentityDataSeeder 先创建用户（系统自己生成 ID）
/// 2. 本贡献者将用户同步到 Keycloak
/// 3. 建立 UserLogin 映射（Keycloak sub -> ABP User ID）
/// 
/// 依赖顺序：需要在 IdentityDataSeeder 之后执行
/// 
/// 遵循 ABP 标准，从 DataSeedContext 获取管理员用户名
/// </remarks>
public class KeycloakUserSeedContributor : IDataSeedContributor, ITransientDependency
{
    /// <summary>
    /// DataSeedContext 中管理员用户名属性名
    /// 与 ABP 的 IdentityDataSeedContributor.AdminUserNamePropertyName 一致
    /// </summary>
    public const string AdminUserNamePropertyName = "AdminUserName";

    /// <summary>
    /// 默认管理员用户名
    /// 与 ABP 的 IdentityDataSeedContributor.AdminUserNameDefaultValue 一致
    /// </summary>
    public const string AdminUserNameDefaultValue = "admin";

    /// <summary>
    /// DataSeedContext 中管理员密码属性名
    /// 与 ABP 的 IdentityDataSeedContributor.AdminPasswordPropertyName 一致
    /// </summary>
    public const string AdminPasswordPropertyName = "AdminPassword";
    protected IKeycloakUserService KeycloakUserService { get; }
    protected IKeycloakRoleService KeycloakRoleService { get; }
    protected IdentityUserManager UserManager { get; }
    protected IIdentityUserRepository UserRepository { get; }
    protected IOptions<JcStackAbpKeycloakIdentityOptions> KeycloakOptions { get; }
    protected ILogger<KeycloakUserSeedContributor> Logger { get; }

    public KeycloakUserSeedContributor(
        IKeycloakUserService keycloakUserService,
        IKeycloakRoleService keycloakRoleService,
        IdentityUserManager userManager,
        IIdentityUserRepository userRepository,
        IOptions<JcStackAbpKeycloakIdentityOptions> keycloakOptions,
        ILogger<KeycloakUserSeedContributor> logger)
    {
        KeycloakUserService = keycloakUserService;
        KeycloakRoleService = keycloakRoleService;
        UserManager = userManager;
        UserRepository = userRepository;
        KeycloakOptions = keycloakOptions;
        Logger = logger;
    }

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        if (!KeycloakOptions.Value.EnableUserSync)
        {
            Logger.LogInformation("Keycloak user sync is disabled, skipping user seed contributor");
            return;
        }

        // 从 DataSeedContext 获取管理员用户名和密码，与 ABP IdentityDataSeedContributor 保持一致
        var adminUserName = context?[AdminUserNamePropertyName] as string ?? AdminUserNameDefaultValue;
        var adminPassword = context?[AdminPasswordPropertyName] as string;

        Logger.LogInformation(
            "Starting Keycloak user seed contributor for admin user: {AdminUserName}",
            adminUserName);

        try
        {
            // 同步管理员用户
            await SyncAdminUserAsync(adminUserName, adminPassword, context?.TenantId);

            Logger.LogInformation("Keycloak user seed contributor completed successfully");
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex,
                "Failed to connect to Keycloak during user seeding. " +
                "Please ensure Keycloak is running and properly configured in appsettings.json");

            // 不抛出异常，允许系统继续启动
            Logger.LogWarning("Skipping Keycloak user seeding due to connection error");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during Keycloak user seeding");
            throw;
        }
    }

    /// <summary>
    /// 同步管理员用户到 Keycloak
    /// </summary>
    /// <param name="adminUserName">管理员用户名，从 DataSeedContext 获取</param>
    /// <param name="adminPassword">管理员密码，从 DataSeedContext 获取</param>
    /// <param name="tenantId">租户 ID</param>
    protected virtual async Task SyncAdminUserAsync(string adminUserName, string? adminPassword, Guid? tenantId)
    {
        var providerName = KeycloakOptions.Value.LoginProviderName;

        // 1. 查找本地管理员用户
        var adminUser = await UserRepository.FindByNormalizedUserNameAsync(
            UserManager.NormalizeName(adminUserName));

        if (adminUser == null)
        {
            Logger.LogDebug("Admin user not found in local database, skipping Keycloak sync");
            return;
        }

        // 2. 检查是否已有 Keycloak 映射
        var existingLogins = await UserManager.GetLoginsAsync(adminUser);
        var keycloakLogin = existingLogins.FirstOrDefault(l => l.LoginProvider == providerName);

        if (keycloakLogin != null)
        {
            // 3. 验证 Keycloak 用户是否真实存在
            var keycloakUserExists = await KeycloakUserService.UserExistsAsync(keycloakLogin.ProviderKey);

            if (keycloakUserExists)
            {
                Logger.LogDebug("Admin user already has valid Keycloak login mapping, skipping");
                return;
            }

            // Keycloak 用户不存在（可能是 Keycloak 数据被重置），需要移除旧映射并重新同步
            Logger.LogWarning(
                "Keycloak user {KeycloakUserId} not found, removing stale login mapping for user {UserName}",
                keycloakLogin.ProviderKey, adminUser.UserName);

            await UserManager.RemoveLoginAsync(adminUser, providerName, keycloakLogin.ProviderKey);
        }

        // 4. 在 Keycloak 创建或获取用户
        var keycloakResult = await KeycloakUserService.CreateOrGetUserAsync(
            username: adminUser.UserName!,
            email: adminUser.Email!,
            appUserId: adminUser.Id.ToString(),
            password: adminPassword, // 从 DataSeedContext 获取的密码
            firstName: adminUser.Name,
            lastName: adminUser.Surname,
            tenantId: tenantId?.ToString());

        Logger.LogInformation(
            "Keycloak user sync: {UserName} -> Keycloak ID {KeycloakUserId}, IsNewUser: {IsNewUser}",
            adminUser.UserName, keycloakResult.KeycloakUserId, keycloakResult.IsNewUser);

        // 5. 建立 UserLogin 映射
        var loginResult = await UserManager.AddLoginAsync(
            adminUser,
            new UserLoginInfo(providerName, keycloakResult.KeycloakUserId, providerName));

        if (!loginResult.Succeeded)
        {
            var errors = string.Join(", ", loginResult.Errors);
            Logger.LogError(
                "Failed to add Keycloak login for user {UserName}: {Errors}",
                adminUser.UserName, errors);
            throw new InvalidOperationException($"Failed to add Keycloak login: {errors}");
        }

        Logger.LogInformation(
            "Created UserLogin mapping: User {UserId} ({UserName}) -> Keycloak {KeycloakUserId}",
            adminUser.Id, adminUser.UserName, keycloakResult.KeycloakUserId);

        // 6. 同步用户角色到 Keycloak
        if (KeycloakOptions.Value.EnableRoleSync)
        {
            await SyncUserRolesToKeycloakAsync(adminUser, keycloakResult.KeycloakUserId);
        }
    }

    /// <summary>
    /// 同步用户角色到 Keycloak
    /// </summary>
    /// <param name="user">ABP 用户</param>
    /// <param name="keycloakUserId">Keycloak 用户 ID</param>
    protected virtual async Task SyncUserRolesToKeycloakAsync(Volo.Abp.Identity.IdentityUser user, string keycloakUserId)
    {
        var roles = await UserManager.GetRolesAsync(user);

        if (roles.Count == 0)
        {
            Logger.LogDebug("User {UserName} has no roles to sync", user.UserName);
            return;
        }

        foreach (var roleName in roles)
        {
            try
            {
                await KeycloakRoleService.AssignRoleToKeycloakUserAsync(keycloakUserId, roleName);

                Logger.LogInformation(
                    "Synced role {RoleName} to Keycloak for user {UserName}",
                    roleName, user.UserName);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex,
                    "Failed to sync role {RoleName} to Keycloak for user {UserName}, continuing with other roles",
                    roleName, user.UserName);
            }
        }
    }
}
