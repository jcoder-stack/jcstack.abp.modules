using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JcStack.Abp.Keycloak.Identity.Middleware;

/// <summary>
/// Keycloak 用户 ID 转换中间件
/// 将 JWT sub (Keycloak ID) 转换为应用用户 ID
/// </summary>
public class KeycloakUserIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<KeycloakUserIdMiddleware> _logger;

    public KeycloakUserIdMiddleware(
        RequestDelegate next,
        ILogger<KeycloakUserIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IKeycloakUserIdResolver resolver,
        IOptions<JcStackAbpKeycloakIdentityOptions> options)
    {
        // 1. 检查是否已认证
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        // 2. 检查是否启用用户同步
        if (!options.Value.EnableUserSync)
        {
            await _next(context);
            return;
        }

        // 3. 获取 JWT sub (Keycloak ID)
        var keycloakSub = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? context.User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(keycloakSub))
        {
            _logger.LogWarning("No sub claim found in authenticated user");
            await _next(context);
            return;
        }

        try
        {
            // 4. 解析用户 ID
            var result = await resolver.ResolveAsync(keycloakSub, context.RequestAborted);

            // 5. 如果 AppUserId 与 KeycloakSub 不同，替换 Claims
            if (result.AppUserId != keycloakSub)
            {
                var identity = context.User.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    // 移除原有的 sub claim
                    var existingSubClaims = identity.FindAll(ClaimTypes.NameIdentifier).ToList();
                    foreach (var claim in existingSubClaims)
                    {
                        identity.RemoveClaim(claim);
                    }

                    var existingSubClaims2 = identity.FindAll("sub").ToList();
                    foreach (var claim in existingSubClaims2)
                    {
                        identity.RemoveClaim(claim);
                    }

                    // 添加新的 sub claim (使用 AppUserId)
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, result.AppUserId));
                    identity.AddClaim(new Claim("sub", result.AppUserId));

                    // 保留原始 Keycloak ID
                    identity.AddClaim(new Claim("keycloak_sub", keycloakSub));

                    _logger.LogDebug(
                        "Replaced user ID: Keycloak {KeycloakSub} -> App {AppUserId} (Source: {Source})",
                        keycloakSub, result.AppUserId, result.Source);
                }
            }

            // 6. 如果有租户 ID，添加到 Claims
            if (!string.IsNullOrEmpty(result.TenantId))
            {
                var identity = context.User.Identity as ClaimsIdentity;
                if (identity != null && !identity.HasClaim(c => c.Type == "tenantid"))
                {
                    identity.AddClaim(new Claim("tenantid", result.TenantId));
                }
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "User {KeycloakSub} is not authorized", keycloakSub);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving user ID for {KeycloakSub}", keycloakSub);
            // 继续请求，不阻止访问
        }

        await _next(context);
    }
}

/// <summary>
/// 中间件扩展方法
/// </summary>
public static class KeycloakUserIdMiddlewareExtensions
{
    /// <summary>
    /// 使用 Keycloak 用户 ID 转换中间件
    /// </summary>
    public static IApplicationBuilder UseKeycloakUserIdMapping(this IApplicationBuilder app)
    {
        return app.UseMiddleware<KeycloakUserIdMiddleware>();
    }
}
