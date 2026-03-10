using Microsoft.AspNetCore.Builder;

namespace JcStack.Abp.Keycloak.Identity.Auth.Middleware;

/// <summary>
/// Keycloak 转发中间件扩展方法
/// </summary>
public static class KeycloakForwardExtensions
{
    /// <summary>
    /// 启用 Keycloak 用户转发中间件
    /// 将 Keycloak sub 映射为本地用户 ID
    /// </summary>
    /// <remarks>
    /// 应在 UseAuthentication() 之后、UseAuthorization() 之前调用：
    /// <code>
    /// app.UseAuthentication();
    /// app.UseKeycloakForward();
    /// app.UseAuthorization();
    /// </code>
    /// </remarks>
    public static IApplicationBuilder UseKeycloakForward(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<KeycloakForwardMiddleware>();
    }
}
