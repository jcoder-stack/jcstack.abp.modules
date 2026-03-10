using System;
using Volo.Abp.DependencyInjection;

namespace JcStack.Abp.Keycloak.Identity;

/// <summary>
/// 空实现，用于非 Web 场景（如后台任务、控制台应用）
/// </summary>
[ExposeServices(typeof(ICurrentKeycloakUser))]
public class NullCurrentKeycloakUser : ICurrentKeycloakUser, ISingletonDependency
{
    public static NullCurrentKeycloakUser Instance { get; } = new();

    public bool IsAuthenticated => false;

    public string? KeycloakUserId => null;

    public Guid? LocalUserId => null;

    public bool HasLocalUser => false;
}
