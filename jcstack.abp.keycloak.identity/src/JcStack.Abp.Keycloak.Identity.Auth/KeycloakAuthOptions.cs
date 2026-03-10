using System;
using System.Collections.Generic;

namespace JcStack.Abp.Keycloak.Identity.Auth;

/// <summary>
/// Keycloak 认证配置选项
/// 支持继承模块覆盖配置
/// </summary>
public class KeycloakAuthOptions
{
    /// <summary>
    /// Keycloak Authority URL (e.g., https://keycloak.example.com/realms/myrealm)
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// 主要 Audience
    /// 留空则自动使用 Authority (issuer URL)
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// 允许的 Audiences 列表
    /// 留空则自动使用 Authority (issuer URL)
    /// </summary>
    public string[] ValidAudiences { get; set; } = [];
    
    /// <summary>
    /// 是否验证 Audience
    /// Keycloak token 默认不包含 aud claim，默认 false
    /// </summary>
    public bool ValidateAudience { get; set; } = false;

    /// <summary>
    /// 是否要求 HTTPS (生产环境应为 true)
    /// </summary>
    public bool RequireHttpsMetadata { get; set; } = true;

    /// <summary>
    /// 是否在首次登录时自动创建 ABP 用户
    /// </summary>
    public bool AutoCreateUser { get; set; } = true;

    /// <summary>
    /// 是否在登录时同步更新用户信息
    /// </summary>
    public bool SyncUserInfoOnLogin { get; set; } = true;

    /// <summary>
    /// 是否启用动态 Claims (从数据库加载额外的 claims)
    /// </summary>
    public bool EnableDynamicClaims { get; set; } = true;

    /// <summary>
    /// JWT Claims 映射配置
    /// Key: Keycloak claim name, Value: ABP claim name
    /// </summary>
    public Dictionary<string, string> ClaimMappings { get; set; } = new()
    {
        ["preferred_username"] = "preferred_username",
        ["email"] = "email",
        ["given_name"] = "given_name",
        ["family_name"] = "family_name",
        ["tenant_id"] = "tenant_id"
    };

    /// <summary>
    /// 自定义属性到 Claims 的映射
    /// </summary>
    public List<string> CustomClaimsToInclude { get; set; } = ["tenant_id", "department"];

    /// <summary>
    /// SignalR Hub 路径前缀
    /// 用于从 query string 读取 access_token（WebSocket 不支持 Header 传递 Token）
    /// </summary>
    public string SignalRHubPath { get; set; } = "/signalr-hubs";
}
