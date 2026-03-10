using System.Threading;
using System.Threading.Tasks;

namespace JcStack.Abp.Keycloak.Identity.Keycloak;

/// <summary>
/// Magic Link 生成结果
/// </summary>
/// <param name="UserId">Keycloak 用户 ID</param>
/// <param name="Link">Magic Link URL</param>
/// <param name="Sent">是否由 Keycloak 发送邮件</param>
public record MagicLinkResult(string UserId, string Link, bool Sent);

/// <summary>
/// Magic Link 服务接口
/// 用于生成 Keycloak Magic Link（一次性登录链接）
/// </summary>
public interface IMagicLinkService
{
    /// <summary>
    /// 为指定用户生成 Magic Link
    /// </summary>
    /// <param name="email">用户邮箱</param>
    /// <param name="redirectUri">登录后跳转地址（完整 URL）</param>
    /// <param name="expirationSeconds">链接有效期（秒），0 则使用默认配置</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>Magic Link 结果，失败返回 null</returns>
    Task<MagicLinkResult?> CreateMagicLinkAsync(
        string email,
        string redirectUri,
        int expirationSeconds = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 为指定用户生成 Magic Link（使用相对路径，自动拼接 RedirectBaseUrl）
    /// </summary>
    /// <param name="email">用户邮箱</param>
    /// <param name="path">前端页面路径（如 /aftersales/orders/{id}）</param>
    /// <param name="expirationSeconds">链接有效期（秒），0 则使用默认配置</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>Magic Link 结果，失败返回 null</returns>
    Task<MagicLinkResult?> CreateMagicLinkByPathAsync(
        string email,
        string path,
        int expirationSeconds = 0,
        CancellationToken cancellationToken = default);
}
