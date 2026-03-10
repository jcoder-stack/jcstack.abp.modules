using System;

namespace JcStack.Abp.Keycloak.Identity;

/// <summary>
/// Keycloak 用户映射缓存项
/// 用于缓存 Keycloak sub 到本地用户 ID 的映射
/// </summary>
[Serializable]
public class KeycloakUserMappingCacheItem
{
    public Guid LocalUserId { get; set; }

    public KeycloakUserMappingCacheItem()
    {
    }

    public KeycloakUserMappingCacheItem(Guid localUserId)
    {
        LocalUserId = localUserId;
    }
}
