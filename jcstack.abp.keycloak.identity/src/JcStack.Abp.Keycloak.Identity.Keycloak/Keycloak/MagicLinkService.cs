using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace JcStack.Abp.Keycloak.Identity.Keycloak;

/// <summary>
/// Magic Link 服务实现
/// 通过 Keycloak magic-link 扩展 REST API 生成一次性登录链接
/// </summary>
[ExposeServices(typeof(MagicLinkService), typeof(IMagicLinkService))]
public class MagicLinkService : IMagicLinkService, ITransientDependency
{
    private readonly KeycloakAdminClient _adminClient;
    private readonly IOptions<JcStackAbpKeycloakIdentityOptions> _options;
    private readonly ILogger<MagicLinkService> _logger;

    public MagicLinkService(
        KeycloakAdminClient adminClient,
        IOptions<JcStackAbpKeycloakIdentityOptions> options,
        ILogger<MagicLinkService> logger)
    {
        _adminClient = adminClient;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<MagicLinkResult?> CreateMagicLinkAsync(
        string email,
        string redirectUri,
        int expirationSeconds = 0,
        CancellationToken cancellationToken = default)
    {
        var magicLinkOptions = _options.Value.MagicLink;

        if (!magicLinkOptions.Enabled)
        {
            _logger.LogWarning("Magic Link is disabled, skipping creation for {Email}", email);
            return null;
        }

        if (string.IsNullOrWhiteSpace(magicLinkOptions.ClientId))
        {
            _logger.LogError("Magic Link ClientId is not configured");
            return null;
        }

        var expiration = expirationSeconds > 0
            ? expirationSeconds
            : magicLinkOptions.DefaultExpirationSeconds;

        var request = new MagicLinkRequest
        {
            Email = email,
            ClientId = magicLinkOptions.ClientId,
            RedirectUri = redirectUri,
            ExpirationSeconds = expiration,
            SendEmail = false, // ASMS 自行发送邮件
            ForceCreate = false // 不自动创建用户
        };

        var response = await _adminClient.CreateMagicLinkAsync(request, cancellationToken);

        if (response == null || string.IsNullOrWhiteSpace(response.Link))
        {
            _logger.LogWarning(
                "Failed to create magic link for {Email}, redirectUri={RedirectUri}",
                email, redirectUri);
            return null;
        }

        return new MagicLinkResult(
            response.UserId ?? string.Empty,
            response.Link,
            response.Sent);
    }

    /// <inheritdoc />
    public async Task<MagicLinkResult?> CreateMagicLinkByPathAsync(
        string email,
        string path,
        int expirationSeconds = 0,
        CancellationToken cancellationToken = default)
    {
        var magicLinkOptions = _options.Value.MagicLink;

        if (string.IsNullOrWhiteSpace(magicLinkOptions.RedirectBaseUrl))
        {
            _logger.LogError("Magic Link RedirectBaseUrl is not configured");
            return null;
        }

        // 确保 path 以 / 开头
        if (!path.StartsWith('/'))
        {
            path = "/" + path;
        }

        var redirectUri = magicLinkOptions.RedirectBaseUrl.TrimEnd('/') + path;

        return await CreateMagicLinkAsync(email, redirectUri, expirationSeconds, cancellationToken);
    }
}
