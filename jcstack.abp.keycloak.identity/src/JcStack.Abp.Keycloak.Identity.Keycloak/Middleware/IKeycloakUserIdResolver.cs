using System.Threading;
using System.Threading.Tasks;
using JcStack.Abp.Keycloak.Identity.Keycloak;

namespace JcStack.Abp.Keycloak.Identity.Middleware;

/// <summary>
/// Keycloak 用户 ID 解析器接口
/// 负责将 JWT sub (Keycloak ID) 解析为应用用户 ID
/// </summary>
public interface IKeycloakUserIdResolver
{
    /// <summary>
    /// 解析 Keycloak sub 为应用用户 ID
    /// </summary>
    /// <param name="keycloakSub">JWT 中的 sub (Keycloak 用户 ID)</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>解析结果</returns>
    Task<UserIdResolutionResult> ResolveAsync(string keycloakSub, CancellationToken cancellationToken = default);
}
