using System.Threading;
using System.Threading.Tasks;

namespace JcStack.Abp.Keycloak.Identity.Keycloak;

/// <summary>
/// 创建用户结果
/// </summary>
public record CreateUserResult(string KeycloakUserId, bool IsNewUser);

/// <summary>
/// 用户 ID 解析结果
/// </summary>
public record UserIdResolutionResult(
    string AppUserId,
    string? TenantId,
    UserIdSource Source,
    bool IsNewUser
);

/// <summary>
/// Keycloak 用户服务接口
/// 支持查询和创建用户（并发安全），以及用户属性管理
/// </summary>
public interface IKeycloakUserService
{
    /// <summary>
    /// 创建或获取用户（并发安全）
    /// 如果用户已存在，返回现有用户 ID
    /// 如果用户不存在，创建新用户并返回 ID
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="email">邮箱</param>
    /// <param name="appUserId">ABP 用户 ID（写入 app_user_id 属性）</param>
    /// <param name="password">密码（可选，不提供则用户需要重置密码）</param>
    /// <param name="firstName">名</param>
    /// <param name="lastName">姓</param>
    /// <param name="tenantId">租户 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户 ID 和是否为新创建</returns>
    Task<CreateUserResult> CreateOrGetUserAsync(
        string username,
        string email,
        string appUserId,
        string? password = null,
        string? firstName = null,
        string? lastName = null,
        string? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 通过用户名获取 Keycloak 用户 ID
    /// </summary>
    Task<string?> GetUserIdByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// 通过邮箱获取 Keycloak 用户 ID
    /// </summary>
    Task<string?> GetUserIdByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查 Keycloak 用户是否存在
    /// </summary>
    Task<bool> UserExistsAsync(string keycloakUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新用户密码
    /// </summary>
    /// <param name="keycloakUserId">Keycloak 用户 ID</param>
    /// <param name="newPassword">新密码</param>
    /// <param name="temporary">是否为临时密码（首次登录需要修改），默认 false</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功</returns>
    Task<bool> UpdateUserPasswordAsync(
        string keycloakUserId,
        string newPassword,
        bool temporary = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新用户基本信息
    /// </summary>
    /// <param name="keycloakUserId">Keycloak 用户 ID</param>
    /// <param name="email">邮箱</param>
    /// <param name="firstName">名</param>
    /// <param name="lastName">姓</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功</returns>
    Task<bool> UpdateUserProfileAsync(
        string keycloakUserId,
        string? email = null,
        string? firstName = null,
        string? lastName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新用户属性（app_user_id 和 tenant_id）
    /// </summary>
    /// <param name="keycloakUserId">Keycloak 用户 ID</param>
    /// <param name="appUserId">ABP 用户 ID</param>
    /// <param name="tenantId">租户 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task UpdateUserAttributesAsync(
        string keycloakUserId,
        string appUserId,
        string? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户的 app_user_id 属性
    /// </summary>
    /// <param name="keycloakUserId">Keycloak 用户 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>app_user_id 属性值，不存在则返回 null</returns>
    Task<string?> GetAppUserIdAsync(string keycloakUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户的 tenant_id 属性
    /// </summary>
    /// <param name="keycloakUserId">Keycloak 用户 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>tenant_id 属性值，不存在则返回 null</returns>
    Task<string?> GetTenantIdAsync(string keycloakUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 通过 app_user_id 查找 Keycloak 用户 ID
    /// </summary>
    /// <param name="appUserId">ABP 用户 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>Keycloak 用户 ID，不存在则返回 null</returns>
    Task<string?> GetKeycloakIdByAppUserIdAsync(string appUserId, CancellationToken cancellationToken = default);
}
