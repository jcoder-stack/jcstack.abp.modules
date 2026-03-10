using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Claims;

namespace JcStack.Abp.Keycloak.Identity.Auth;

/// <summary>
/// Keycloak JWT Bearer 认证事件处理器
/// 只负责 Claims 映射处理，用户映射由 KeycloakForwardMiddleware 处理
/// 支持继承扩展
/// </summary>
public class KeycloakJwtBearerEvents : JwtBearerEvents, ITransientDependency
{
    /// <summary>
    /// 处理消息接收事件
    /// 支持从 query string 读取 access_token（SignalR WebSocket 场景）
    /// </summary>
    public override Task MessageReceived(MessageReceivedContext context)
    {
        // SignalR WebSocket 连接不能通过 HTTP Header 传递 Token
        // 需要从 query string 的 access_token 参数读取
        var accessToken = context.Request.Query["access_token"];

        if (!string.IsNullOrEmpty(accessToken))
        {
            var serviceProvider = context.HttpContext.RequestServices;
            var options = serviceProvider.GetRequiredService<IOptions<KeycloakAuthOptions>>().Value;
            var signalRPath = options.SignalRHubPath;

            if (!string.IsNullOrEmpty(signalRPath))
            {
                var path = context.HttpContext.Request.Path;
                // 仅对 SignalR Hub 路径应用
                if (path.StartsWithSegments(signalRPath))
                {
                    context.Token = accessToken;
                }
            }
        }

        return base.MessageReceived(context);
    }

    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var serviceProvider = context.HttpContext.RequestServices;
        var logger = serviceProvider.GetRequiredService<ILogger<KeycloakJwtBearerEvents>>();
        var options = serviceProvider.GetRequiredService<IOptions<KeycloakAuthOptions>>().Value;

        try
        {
            // 处理 Claims 映射
            ProcessClaimMappings(context, options, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during JWT token validation claims processing");
            // 不要 Fail，让用户继续使用 JWT claims
        }

        await base.TokenValidated(context);
    }

    /// <summary>
    /// 处理 Claims 映射
    /// 子类可重写以自定义 Claims 处理逻辑
    /// </summary>
    protected virtual void ProcessClaimMappings(
        TokenValidatedContext context,
        KeycloakAuthOptions options,
        ILogger logger)
    {
        if (context.Principal?.Identity is not ClaimsIdentity identity)
            return;

        // 映射标准 Claims
        MapStandardClaims(identity, context, options);

        // 映射自定义 Claims
        MapCustomClaims(identity, context, options);

        logger.LogDebug("Processed JWT claims mapping for user {Sub}",
            context.Principal.FindFirstValue("sub"));
    }

    /// <summary>
    /// 映射标准 Claims
    /// </summary>
    protected virtual void MapStandardClaims(
        ClaimsIdentity identity,
        TokenValidatedContext context,
        KeycloakAuthOptions options)
    {
        // 映射 preferred_username -> AbpClaimTypes.UserName
        var userName = context.Principal?.FindFirstValue("preferred_username");
        AddClaimIfMissing(identity, AbpClaimTypes.UserName, userName);

        // 映射 email -> AbpClaimTypes.Email
        var email = context.Principal?.FindFirstValue(ClaimTypes.Email)
                 ?? context.Principal?.FindFirstValue("email");
        AddClaimIfMissing(identity, AbpClaimTypes.Email, email);

        // 映射 name
        var name = context.Principal?.FindFirstValue(ClaimTypes.Name)
                ?? context.Principal?.FindFirstValue("name");
        AddClaimIfMissing(identity, AbpClaimTypes.Name, name);
    }

    /// <summary>
    /// 映射自定义 Claims
    /// </summary>
    protected virtual void MapCustomClaims(
        ClaimsIdentity identity,
        TokenValidatedContext context,
        KeycloakAuthOptions options)
    {
        foreach (var claimName in options.CustomClaimsToInclude)
        {
            var claimValue = context.Principal?.FindFirstValue(claimName);
            if (!string.IsNullOrEmpty(claimValue))
            {
                // 特殊处理 tenant_id
                if (claimName == "tenant_id")
                {
                    AddClaimIfMissing(identity, AbpClaimTypes.TenantId, claimValue);
                }
                else
                {
                    AddClaimIfMissing(identity, claimName, claimValue);
                }
            }
        }
    }

    /// <summary>
    /// 添加 Claim（如果不存在）
    /// </summary>
    protected static void AddClaimIfMissing(ClaimsIdentity identity, string type, string? value)
    {
        if (!string.IsNullOrEmpty(value) && !identity.HasClaim(c => c.Type == type))
        {
            identity.AddClaim(new Claim(type, value));
        }
    }
}
