using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JcStack.Abp.Keycloak.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.Caching;
using Volo.Abp.Identity;

namespace JcStack.Abp.Keycloak.Identity.Auth.Middleware;

/// <summary>
/// Keycloak 用户转发中间件
/// 将 Keycloak sub 映射为本地用户 ID
/// </summary>
/// <remarks>
/// 工作流程：
/// 1. 从 JWT 获取 Keycloak sub
/// 2. 通过 UserLogins 表查找本地用户 ID（带缓存）
/// 3. 不存在映射则返回 403
/// 4. 存在则替换 Claims 中的 sub 为本地用户 ID
/// </remarks>
public class KeycloakForwardMiddleware
{
    private readonly RequestDelegate _next;

    public KeycloakForwardMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IdentityUserManager userManager,
        IOptions<JcStackAbpKeycloakIdentityOptions> options,
        IDistributedCache<KeycloakUserMappingCacheItem> cache,
        CurrentKeycloakUser currentKeycloakUser,
        ILogger<KeycloakForwardMiddleware> logger)
    {
        // 未认证的请求直接放行
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        // 未启用用户同步则直接放行
        if (!options.Value.EnableUserSync)
        {
            await _next(context);
            return;
        }

        var keycloakSub = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? context.User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(keycloakSub))
        {
            await _next(context);
            return;
        }

        var providerName = options.Value.LoginProviderName;
        var cacheKey = $"{providerName}:{keycloakSub}";

        // 1. 先查缓存
        var cached = await cache.GetAsync(cacheKey);
        Guid? localUserId = cached?.LocalUserId;

        if (localUserId == null)
        {
            // 2. 查数据库（通过 UserManager.FindByLoginAsync）
            var user = await userManager.FindByLoginAsync(providerName, keycloakSub);

            if (user == null)
            {
                // 用户不存在，拒绝访问
                logger.LogWarning(
                    "User with Keycloak sub {KeycloakSub} not found in system. Access denied.",
                    keycloakSub);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "user_not_found",
                    message = "User is not registered in this system. Please contact administrator."
                });
                return;
            }

            localUserId = user.Id;

            // 3. 写入缓存
            await cache.SetAsync(cacheKey, new KeycloakUserMappingCacheItem(localUserId.Value),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                });

            logger.LogDebug(
                "Cached user mapping: Keycloak {KeycloakSub} -> Local {LocalUserId}",
                keycloakSub, localUserId.Value);
        }

        // 4. 替换 Claims 中的 sub
        ReplaceSub(context, localUserId.Value, keycloakSub);

        // 5. 设置 ICurrentKeycloakUser
        currentKeycloakUser.SetUser(keycloakSub, localUserId.Value);

        // 6. 设置上下文（供其他中间件/服务使用）
        context.Items["LocalUserId"] = localUserId.Value;
        context.Items["KeycloakSub"] = keycloakSub;

        // 7. 设置 Header（Gateway 模式下可传递给下游服务）
        context.Request.Headers["X-Local-User-Id"] = localUserId.Value.ToString();
        context.Request.Headers["X-Keycloak-Sub"] = keycloakSub;

        logger.LogDebug(
            "Keycloak forward: {KeycloakSub} -> {LocalUserId}",
            keycloakSub, localUserId.Value);

        await _next(context);
    }

    /// <summary>
    /// 替换 Claims 中的 sub 为本地用户 ID
    /// </summary>
    private static void ReplaceSub(HttpContext context, Guid localUserId, string keycloakSub)
    {
        if (context.User.Identity is not ClaimsIdentity identity)
            return;

        // 移除原始 sub claims
        var subClaims = identity.FindAll(c =>
            c.Type == ClaimTypes.NameIdentifier || c.Type == "sub").ToList();

        foreach (var claim in subClaims)
        {
            identity.RemoveClaim(claim);
        }

        // 添加新的 sub（本地用户ID）
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, localUserId.ToString()));
        identity.AddClaim(new Claim("sub", localUserId.ToString()));

        // 保留原始 Keycloak sub
        identity.AddClaim(new Claim("keycloak_sub", keycloakSub));
    }
}
