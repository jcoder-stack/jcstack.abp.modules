using System;

namespace JcStack.Abp.Keycloak.Identity;

/// <summary>
/// 当前 Keycloak 用户信息接口
/// 类似 ABP 的 ICurrentUser，用于获取当前请求的 Keycloak 用户信息
/// </summary>
public interface ICurrentKeycloakUser
{
    /// <summary>
    /// 是否已认证
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Keycloak 用户 ID (sub claim)
    /// </summary>
    string? KeycloakUserId { get; }

    /// <summary>
    /// 本地 ABP 用户 ID
    /// </summary>
    Guid? LocalUserId { get; }

    /// <summary>
    /// 是否存在本地用户映射
    /// </summary>
    bool HasLocalUser { get; }
}
