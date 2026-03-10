using System;

namespace JcStack.Abp.Keycloak.Identity.Samples;

/// <summary>
/// 当前用户信息 DTO
/// </summary>
public class CurrentUserInfoDto
{
    /// <summary>
    /// ABP CurrentUser 信息
    /// </summary>
    public AbpUserDto? AbpUser { get; set; }

    /// <summary>
    /// Keycloak 用户信息
    /// </summary>
    public KeycloakUserDto? KeycloakUser { get; set; }
}

public class AbpUserDto
{
    public Guid? Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public bool IsAuthenticated { get; set; }
    public string[]? Roles { get; set; }
}

public class KeycloakUserDto
{
    public string? KeycloakUserId { get; set; }
    public Guid? LocalUserId { get; set; }
    public bool IsAuthenticated { get; set; }
    public bool HasLocalUser { get; set; }
}
