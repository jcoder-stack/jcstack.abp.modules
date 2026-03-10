using System;
using System.Threading;
using System.Threading.Tasks;
using JcStack.Abp.Keycloak.Identity.Keycloak;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;

namespace JcStack.Abp.Keycloak.Identity.Middleware;

/// <summary>
/// 用户 ID 映射缓存项
/// </summary>
public class UserIdMappingCacheItem
{
    public string AppUserId { get; set; } = string.Empty;
    public string? TenantId { get; set; }
    public UserIdSource Source { get; set; }
}

/// <summary>
/// Keycloak 用户 ID 解析器实现
/// 按优先级规则解析 Keycloak sub 为应用用户 ID
/// </summary>
public class KeycloakUserIdResolver : IKeycloakUserIdResolver, ITransientDependency
{
    private readonly IKeycloakUserService _keycloakUserService;
    private readonly IDistributedCache<UserIdMappingCacheItem> _cache;
    private readonly IOptions<JcStackAbpKeycloakIdentityOptions> _options;
    private readonly ILogger<KeycloakUserIdResolver> _logger;

    public KeycloakUserIdResolver(
        IKeycloakUserService keycloakUserService,
        IDistributedCache<UserIdMappingCacheItem> cache,
        IOptions<JcStackAbpKeycloakIdentityOptions> options,
        ILogger<KeycloakUserIdResolver> logger)
    {
        _keycloakUserService = keycloakUserService;
        _cache = cache;
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// 解析 Keycloak sub 为应用用户 ID
    /// 优先级规则：
    /// 1. app_user_id_{System} 属性存在 → 使用属性值
    /// 2. 属性不存在 + FallbackToKeycloakId=true → 使用 Keycloak ID
    /// 3. 属性不存在 + RequireSystemAttribute=true → 返回失败
    /// </summary>
    public async Task<UserIdResolutionResult> ResolveAsync(
        string keycloakSub,
        CancellationToken cancellationToken = default)
    {
        var sourceSystem = _options.Value.SourceSystem;
        var cacheKey = $"{sourceSystem}:{keycloakSub}";

        // 1. 查询缓存
        var cached = await _cache.GetAsync(cacheKey, token: cancellationToken);
        if (cached != null)
        {
            _logger.LogDebug(
                "Resolved user ID from cache: Keycloak {KeycloakSub} -> App {AppUserId}",
                keycloakSub, cached.AppUserId);

            return new UserIdResolutionResult(
                cached.AppUserId,
                cached.TenantId,
                UserIdSource.Cache,
                IsNewUser: false);
        }

        // 2. 查询 Keycloak 属性
        var appUserId = await _keycloakUserService.GetAppUserIdAsync(keycloakSub, cancellationToken);
        var tenantId = await _keycloakUserService.GetTenantIdAsync(keycloakSub, cancellationToken);

        UserIdResolutionResult result;

        if (!string.IsNullOrEmpty(appUserId))
        {
            // 规则 1: 属性存在，使用属性值
            result = new UserIdResolutionResult(
                appUserId,
                tenantId,
                UserIdSource.Attribute,
                IsNewUser: false);

            _logger.LogDebug(
                "Resolved user ID from Keycloak attribute: Keycloak {KeycloakSub} -> App {AppUserId}",
                keycloakSub, appUserId);
        }
        else if (_options.Value.UserIdResolution.RequireSystemAttribute)
        {
            // 规则 3: 强制要求属性存在
            _logger.LogWarning(
                "User {KeycloakSub} does not have {System} attribute and RequireSystemAttribute is true",
                keycloakSub, sourceSystem);

            throw new UnauthorizedAccessException(
                $"User does not have required system attribute for {sourceSystem}");
        }
        else if (_options.Value.UserIdResolution.FallbackToKeycloakId)
        {
            // 规则 2: 回退到 Keycloak ID
            result = new UserIdResolutionResult(
                keycloakSub,
                tenantId,
                UserIdSource.KeycloakId,
                IsNewUser: true); // 可能是新用户

            _logger.LogDebug(
                "Fallback to Keycloak ID: {KeycloakSub} (no {System} attribute found)",
                keycloakSub, sourceSystem);
        }
        else
        {
            // 不允许回退
            _logger.LogWarning(
                "User {KeycloakSub} does not have {System} attribute and FallbackToKeycloakId is false",
                keycloakSub, sourceSystem);

            throw new UnauthorizedAccessException(
                $"User does not have system attribute for {sourceSystem} and fallback is disabled");
        }

        // 3. 缓存结果
        await _cache.SetAsync(
            cacheKey,
            new UserIdMappingCacheItem
            {
                AppUserId = result.AppUserId,
                TenantId = result.TenantId,
                Source = result.Source
            },
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(
                    _options.Value.UserIdResolution.CacheDurationMinutes)
            },
            token: cancellationToken);

        return result;
    }
}
