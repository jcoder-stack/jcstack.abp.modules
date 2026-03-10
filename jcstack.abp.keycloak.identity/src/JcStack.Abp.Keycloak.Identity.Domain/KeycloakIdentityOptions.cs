namespace JcStack.Abp.Keycloak.Identity;

/// <summary>
/// Keycloak Identity 模块配置选项
/// </summary>
public class JcStackAbpKeycloakIdentityOptions
{
    /// <summary>
    /// Keycloak 服务器 URL
    /// </summary>
    public string ServerUrl { get; set; } = string.Empty;

    /// <summary>
    /// Keycloak 领域名称
    /// </summary>
    public string Realm { get; set; } = string.Empty;

    /// <summary>
    /// 客户端 ID（用于 Admin API）
    /// </summary>
    public string AdminClientId { get; set; } = string.Empty;

    /// <summary>
    /// 客户端密钥（用于 Admin API）
    /// </summary>
    public string AdminClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// 客户端 ID（用于角色映射）
    /// 用于从 resource_access 中提取 Client Roles
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用用户同步
    /// </summary>
    public bool EnableUserSync { get; set; } = true;

    /// <summary>
    /// 是否启用角色同步
    /// </summary>
    public bool EnableRoleSync { get; set; } = true;

    /// <summary>
    /// 是否启用声明同步
    /// </summary>
    public bool EnableClaimSync { get; set; } = true;

    /// <summary>
    /// 是否将用户删除同步到 Keycloak（默认 true）
    /// </summary>
    public bool SyncUserDeletionToKeycloak { get; set; } = true;

    /// <summary>
    /// 是否将角色删除同步到 Keycloak（默认 false）
    /// 通常不建议删除 Keycloak 中的角色，因为可能被其他系统使用
    /// </summary>
    public bool SyncRoleDeletionToKeycloak { get; set; } = false;

    /// <summary>
    /// 是否等待同步完成（默认 false，异步同步）
    /// 设置为 true 时，ABP 操作会等待 Keycloak 同步完成后才返回
    /// 这会增加响应时间，但确保强一致性
    /// </summary>
    public bool WaitForSyncCompletion { get; set; } = false;

    /// <summary>
    /// 同步等待超时时间（秒），仅在 WaitForSyncCompletion = true 时有效
    /// </summary>
    public int SyncWaitTimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// 角色名称前缀（用于避免冲突）
    /// 仅用于 Realm Roles，Client Roles 不需要前缀
    /// </summary>
    public string RolePrefix { get; set; } = string.Empty;

    /// <summary>
    /// 来源系统标识（如 "MES", "SRM", "LIME"）
    /// </summary>
    public string SourceSystem { get; set; } = string.Empty;

    /// <summary>
    /// 角色同步模式
    /// - ClientRoles: 同步到 Client Roles（推荐用于多系统）
    /// - RealmRoles: 同步到 Realm Roles（需要配置 RolePrefix）
    /// </summary>
    public RoleSyncMode RoleSyncMode { get; set; } = RoleSyncMode.ClientRoles;

    /// <summary>
    /// UserLogin 的 LoginProvider 名称
    /// 用于在 AbpUserLogins 表中建立 Keycloak sub 到本地用户 ID 的映射
    /// </summary>
    public string LoginProviderName { get; set; } = "Keycloak";

    /// <summary>
    /// API Audience 名称（用于 token 验证）
    /// 会自动创建包含 audience mapper 的 client scope
    /// 并关联到配置的客户端
    /// </summary>
    public string ApiAudience { get; set; } = string.Empty;

    /// <summary>
    /// 是否自动创建 API audience scope
    /// </summary>
    public bool AutoCreateAudienceScope { get; set; } = true;

    /// <summary>
    /// Keycloak 用户属性配置
    /// </summary>
    public KeycloakAttributesOptions Attributes { get; set; } = new();

    /// <summary>
    /// 用户 ID 解析配置
    /// </summary>
    public UserIdResolutionOptions UserIdResolution { get; set; } = new();

    /// <summary>
    /// Magic Link 配置
    /// </summary>
    public MagicLinkOptions MagicLink { get; set; } = new();
}

/// <summary>
/// 角色同步模式
/// </summary>
public enum RoleSyncMode
{
    /// <summary>
    /// 同步到 Client Roles（推荐用于多系统场景）
    /// </summary>
    ClientRoles,

    /// <summary>
    /// 同步到 Realm Roles（需要配置 RolePrefix 避免冲突）
    /// </summary>
    RealmRoles
}

/// <summary>
/// Keycloak 用户属性配置
/// </summary>
public class KeycloakAttributesOptions
{
    /// <summary>
    /// 用户 ID 属性前缀
    /// 完整属性名: {AppUserIdPrefix}{SourceSystem}
    /// 例如: app_user_id_MES
    /// </summary>
    public string AppUserIdPrefix { get; set; } = "app_user_id_";

    /// <summary>
    /// 租户 ID 属性前缀
    /// 完整属性名: {TenantIdPrefix}{SourceSystem}
    /// 例如: tenant_id_MES
    /// </summary>
    public string TenantIdPrefix { get; set; } = "tenant_id_";
}

/// <summary>
/// 用户 ID 解析配置
/// </summary>
public class UserIdResolutionOptions
{
    /// <summary>
    /// 当 app_user_id 属性不存在时，是否回退到使用 Keycloak ID
    /// </summary>
    public bool FallbackToKeycloakId { get; set; } = true;

    /// <summary>
    /// 是否强制要求当前系统的 app_user_id 属性存在
    /// 如果为 true 且属性不存在，将拒绝访问
    /// </summary>
    public bool RequireSystemAttribute { get; set; } = false;

    /// <summary>
    /// 是否允许自动创建本地用户
    /// 当用户通过 Keycloak 认证但本地不存在时
    /// </summary>
    public bool AllowAutoCreateUser { get; set; } = true;

    /// <summary>
    /// 用户 ID 映射缓存时间（分钟）
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 30;
}

/// <summary>
/// Magic Link 配置选项
/// </summary>
public class MagicLinkOptions
{
    /// <summary>
    /// 是否启用 Magic Link 功能
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 默认链接有效期（秒），默认 2 天
    /// </summary>
    public int DefaultExpirationSeconds { get; set; } = 172800;

    /// <summary>
    /// Magic Link 使用的客户端 ID
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// 前端应用基础 URL（用于构建 redirect_uri）
    /// 例如：http://localhost:3000
    /// </summary>
    public string RedirectBaseUrl { get; set; } = string.Empty;
}

/// <summary>
/// 用户 ID 来源
/// </summary>
public enum UserIdSource
{
    /// <summary>
    /// 从 Keycloak 属性获取
    /// </summary>
    Attribute,

    /// <summary>
    /// 直接使用 Keycloak ID（新用户或回退）
    /// </summary>
    KeycloakId,

    /// <summary>
    /// 从缓存获取
    /// </summary>
    Cache
}
