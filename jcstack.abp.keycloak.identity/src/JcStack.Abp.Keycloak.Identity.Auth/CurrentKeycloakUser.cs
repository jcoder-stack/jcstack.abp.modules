using System;
using Volo.Abp.DependencyInjection;

namespace JcStack.Abp.Keycloak.Identity.Auth;

/// <summary>
/// 当前 Keycloak 用户信息实现（Web 场景）
/// 由 KeycloakForwardMiddleware 设置用户信息
/// </summary>
[ExposeServices(typeof(ICurrentKeycloakUser), typeof(CurrentKeycloakUser))]
[Dependency(ReplaceServices = true)]
public class CurrentKeycloakUser : ICurrentKeycloakUser, IScopedDependency
{
    /// <inheritdoc />
    public bool IsAuthenticated { get; private set; }
    
    /// <inheritdoc />
    public string? KeycloakUserId { get; private set; }
    
    /// <inheritdoc />
    public Guid? LocalUserId { get; private set; }
    
    /// <inheritdoc />
    public bool HasLocalUser => LocalUserId.HasValue;
    
    /// <summary>
    /// 设置当前 Keycloak 用户信息（仅供中间件调用）
    /// </summary>
    internal void SetUser(string keycloakUserId, Guid? localUserId)
    {
        IsAuthenticated = true;
        KeycloakUserId = keycloakUserId;
        LocalUserId = localUserId;
    }
    
    /// <summary>
    /// 清除用户信息
    /// </summary>
    internal void Clear()
    {
        IsAuthenticated = false;
        KeycloakUserId = null;
        LocalUserId = null;
    }
}
