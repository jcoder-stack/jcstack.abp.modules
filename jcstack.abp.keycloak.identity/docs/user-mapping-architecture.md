# Keycloak 用户映射架构

## 概述

从 v2.0 开始，JcStack.Abp.Keycloak.Identity 模块引入了新的用户映射架构，解决了 Keycloak sub 与系统用户 ID 的耦合问题。

## 架构变更

### 旧架构（v1.x - 已废弃）

```
┌─────────────────────────────────────────┐
│           Keycloak SSO 服务器             │
│  用户 ID: a1b2c3d4-...                    │
└─────────────────────────────────────────┘
         │ 使用 Keycloak ID
         ↓
┌─────────────────────────────────────────┐
│    ABP 系统 (MES/SRM/LIME)               │
│  IdentityUser.Id = a1b2c3d4-...          │
│  ↳ 直接使用 Keycloak ID 作为用户 ID      │
└─────────────────────────────────────────┘
```

**问题**：
- 系统用户 ID 完全依赖 Keycloak
- 无法独立管理用户 ID
- Keycloak 故障影响系统正常运行
- 难以迁移到其他身份提供商

### 新架构（v2.0+）

```
┌─────────────────────────────────────────┐
│           Keycloak SSO 服务器             │
│  用户 ID: a1b2c3d4-...                    │
└─────────────────────────────────────────┘
         │ JWT sub: a1b2c3d4-...
         ↓
┌─────────────────────────────────────────┐
│      KeycloakForwardMiddleware           │
│  查找 AbpUserLogins 表映射关系           │
│  a1b2c3d4-... → local-user-id            │
└─────────────────────────────────────────┘
         │ 替换 sub 为本地用户 ID
         ↓
┌─────────────────────────────────────────┐
│    ABP 系统 (MES/SRM/LIME)               │
│  IdentityUser.Id = local-user-id         │
│  ↳ 系统自己生成 ID                       │
│                                           │
│  AbpUserLogins 表：                       │
│  ┌────────────────────────────────────┐ │
│  │ UserId  | LoginProvider | ProviderKey │
│  ├────────────────────────────────────┤ │
│  │ local-1 | Keycloak      | a1b2c3... │ │
│  │ local-2 | Keycloak      | b2c3d4... │ │
│  └────────────────────────────────────┘ │
└─────────────────────────────────────────┘
```

**优势**：
- ✅ 系统自主管理用户 ID
- ✅ 通过 `AbpUserLogins` 表建立映射
- ✅ 支持多个身份提供商（未来扩展）
- ✅ Keycloak 故障不影响已登录用户
- ✅ 更好的数据独立性

## 核心组件

### 1. KeycloakForwardMiddleware

**位置**: `JcStack.Abp.Keycloak.Identity.Auth/Middleware/KeycloakForwardMiddleware.cs`

**功能**：
- 拦截已认证的请求
- 从 JWT 提取 Keycloak sub
- 查找 `AbpUserLogins` 表获取本地用户 ID
- 替换 Claims 中的 sub 为本地用户 ID
- 不存在映射则返回 403

**工作流程**：

```
HTTP Request with JWT
    │
    ↓
[UseAuthentication]  ← JWT 验证通过
    │
    ↓
[UseKeycloakForward]  ← 用户映射中间件
    │
    ├─ 1. 提取 Keycloak sub from Claims
    │
    ├─ 2. 查询缓存
    │     ├─ 缓存命中 → 使用缓存的本地用户 ID
    │     └─ 缓存未命中 ↓
    │
    ├─ 3. 查询 AbpUserLogins 表
    │     var user = await userManager.FindByLoginAsync("Keycloak", keycloakSub)
    │     ├─ 用户存在 → 获取 user.Id
    │     └─ 用户不存在 → 返回 403 Forbidden
    │
    ├─ 4. 写入缓存（24小时过期）
    │
    ├─ 5. 替换 Claims
    │     ├─ 移除原 sub (Keycloak ID)
    │     ├─ 添加新 sub (本地用户 ID)
    │     └─ 保留 keycloak_sub (原始 Keycloak ID)
    │
    ↓
[UseAuthorization]  ← 使用本地用户 ID 进行授权
    │
    ↓
业务逻辑处理
```

**配置选项**：

```json
{
  "Keycloak": {
    "LoginProviderName": "Keycloak",
    "EnableUserMapping": true
  }
}
```

**使用方式**：

```csharp
// AsmsHttpApiHostModule.cs
public override void OnApplicationInitialization(ApplicationInitializationContext context)
{
    var app = context.GetApplicationBuilder();
    
    app.UseAuthentication();
    app.UseKeycloakForward();  // ← 必须在 UseAuthentication 之后
    app.UseAuthorization();
}
```

### 2. KeycloakUserSeedContributor

**位置**: `JcStack.Abp.Keycloak.Identity.Keycloak/Identity/KeycloakUserSeedContributor.cs`

**功能**：
- 将 ABP 种子用户同步到 Keycloak
- 建立 UserLogin 映射关系
- 仅在 DbMigrator 运行时执行
- **遵循 ABP 标准**：从 `DataSeedContext` 获取管理员用户名

**常量定义**：

```csharp
// 与 ABP IdentityDataSeedContributor 保持一致
public const string AdminUserNamePropertyName = "AdminUserName";
public const string AdminUserNameDefaultValue = "admin";
```

**工作流程**：

```
DbMigrator 启动
    │
    ↓
IdentityDataSeeder  ← ABP 创建 admin 用户（系统生成 ID）
    │ admin: Id = local-admin-id
    │ DataSeedContext["AdminUserName"] = "admin"
    │
    ↓
KeycloakUserSeedContributor  ← 同步到 Keycloak
    │
    ├─ 0. 从 DataSeedContext 获取用户名
    │     var adminUserName = context?["AdminUserName"] as string ?? "admin"
    │
    ├─ 1. 查找本地管理员用户
    │     var adminUser = await UserRepository.FindByNormalizedUserNameAsync(adminUserName)
    │
    ├─ 2. 检查是否已有 Keycloak 映射
    │     var logins = await UserManager.GetLoginsAsync(adminUser)
    │     if (logins.Any(l => l.LoginProvider == "Keycloak"))
    │         return  // 已有映射，跳过
    │
    ├─ 3. 在 Keycloak 创建或获取用户
    │     var result = await KeycloakUserService.CreateOrGetUserAsync(
    │         username: "admin",
    │         email: "admin@example.com",
    │         password: null  // 不设置密码
    │     )
    │     // result.KeycloakUserId = "a1b2c3d4-..."
    │
    ├─ 4. 建立 UserLogin 映射
    │     await UserManager.AddLoginAsync(
    │         adminUser,
    │         new UserLoginInfo("Keycloak", result.KeycloakUserId, "Keycloak")
    │     )
    │
    ↓
AbpUserLogins 表新增记录：
┌──────────────────┬───────────────┬──────────────────┐
│ UserId           │ LoginProvider │ ProviderKey       │
├──────────────────┼───────────────┼──────────────────┤
│ local-admin-id   │ Keycloak      │ a1b2c3d4-...     │
└──────────────────┴───────────────┴──────────────────┘
```

### 3. JcStackAbpKeycloakIdentityUserAppService

**位置**: `JcStack.Abp.Keycloak.Identity.Application/Identity/JcStackAbpKeycloakIdentityUserAppService.cs`

**功能**：
- 重写 `CreateAsync` 方法
- 先用 ABP 原生方式创建用户（系统生成 ID）
- 然后同步到 Keycloak
- 最后建立 UserLogin 映射

**工作流程**：

```
POST /api/identity/users
    │
    ↓
JcStackAbpKeycloakIdentityUserAppService.CreateAsync(input)
    │
    ├─ 1. 调用 base.CreateAsync(input)  ← ABP 创建用户
    │     var userDto = await base.CreateAsync(input)
    │     // userDto.Id = local-user-id (ABP 生成)
    │
    ├─ 2. 同步用户到 Keycloak
    │     var keycloakResult = await KeycloakUserService.CreateOrGetUserAsync(
    │         username: input.UserName,
    │         email: input.Email,
    │         password: input.Password
    │     )
    │     // keycloakResult.KeycloakUserId = "a1b2c3d4-..."
    │
    ├─ 3. 建立 UserLogin 映射
    │     var user = await UserManager.GetByIdAsync(userDto.Id)
    │     await UserManager.AddLoginAsync(
    │         user,
    │         new UserLoginInfo("Keycloak", keycloakResult.KeycloakUserId, "Keycloak")
    │     )
    │
    ↓
返回 userDto
```

**错误处理**：
- Keycloak 同步失败不影响用户创建
- 仅记录日志，用户已在 ABP 中创建成功
- 可后续手动同步

### 4. JcStackAbpKeycloakIdentityDataSeeder（已废弃）

**位置**: `JcStack.Abp.Keycloak.Identity.Keycloak/Identity/JcStackAbpKeycloakIdentityDataSeeder.cs`

**状态**: 已标记 `[Obsolete]`，移除了 `[Dependency(ReplaceServices = true)]`

**原因**：
- 旧架构使用 Keycloak ID 作为用户 ID
- 与新架构不兼容
- 已被 `KeycloakUserSeedContributor` 替代

## 配置说明

### KeycloakAuthOptions

```csharp
public class KeycloakAuthOptions
{
    /// <summary>
    /// 登录提供者名称（存储在 AbpUserLogins.LoginProvider）
    /// </summary>
    public string LoginProviderName { get; set; } = "Keycloak";
    
    /// <summary>
    /// 是否启用用户映射（使用 AbpUserLogins 表）
    /// 启用后，系统会自己生成用户ID，通过 UserLogin 映射 Keycloak sub
    /// </summary>
    public bool EnableUserMapping { get; set; } = true;
}
```

### JcStackAbpKeycloakIdentityOptions

```csharp
public class JcStackAbpKeycloakIdentityOptions
{
    /// <summary>
    /// UserLogin 的 LoginProvider 名称
    /// 用于在 AbpUserLogins 表中建立 Keycloak sub 到本地用户 ID 的映射
    /// </summary>
    public string LoginProviderName { get; set; } = "Keycloak";
}
```

**注意**：两个配置中的 `LoginProviderName` 应保持一致。

## 数据库表结构

### AbpUserLogins 表

ABP Framework 自带的表，用于存储外部登录提供商映射。

```sql
CREATE TABLE AbpUserLogins (
    UserId uuid NOT NULL,                      -- 本地用户 ID
    LoginProvider character varying(64) NOT NULL,  -- 提供商名称 (e.g., "Keycloak")
    ProviderKey character varying(196) NOT NULL,   -- 提供商用户 ID (Keycloak sub)
    ProviderDisplayName character varying(128),    -- 显示名称
    TenantId uuid,                             -- 租户 ID（多租户场景）
    
    PRIMARY KEY (UserId, LoginProvider),
    FOREIGN KEY (UserId) REFERENCES AbpUsers(Id)
);

-- 索引：快速通过 Keycloak sub 查找用户
CREATE INDEX IX_AbpUserLogins_LoginProvider_ProviderKey 
    ON AbpUserLogins (LoginProvider, ProviderKey);
```

**示例数据**：

```sql
-- admin 用户的映射
INSERT INTO AbpUserLogins (UserId, LoginProvider, ProviderKey, ProviderDisplayName)
VALUES (
    '123e4567-e89b-12d3-a456-426614174000',  -- 本地 admin ID
    'Keycloak',
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',  -- Keycloak admin ID
    'Keycloak'
);
```

## 缓存策略

### KeycloakUserMappingCacheItem

**缓存键格式**: `{LoginProviderName}:{KeycloakSub}`

**示例**: `Keycloak:a1b2c3d4-e5f6-7890-abcd-ef1234567890`

**过期时间**: 24 小时

**缓存内容**:
```csharp
public class KeycloakUserMappingCacheItem
{
    public Guid LocalUserId { get; set; }
}
```

**缓存流程**:
```
首次请求：
  查询数据库 → 写入缓存 → 返回本地用户 ID

后续请求（24小时内）：
  直接从缓存读取 → 返回本地用户 ID

24小时后：
  缓存过期 → 重新查询数据库 → 更新缓存
```

## 迁移指南

### 从 v1.x 迁移到 v2.0

#### 步骤 1：理解影响

**重要提示**：此迁移涉及用户 ID 的改变，需要谨慎操作！

- 旧版本：用户 ID = Keycloak sub
- 新版本：用户 ID = 系统生成，通过 UserLogins 映射

#### 步骤 2：数据迁移脚本

```sql
-- 为现有用户创建 UserLogin 映射
-- 假设现有用户的 Id 就是 Keycloak sub
INSERT INTO AbpUserLogins (UserId, LoginProvider, ProviderKey, ProviderDisplayName, TenantId)
SELECT 
    u.Id,                    -- 当前用户 ID（即 Keycloak sub）
    'Keycloak',              -- 登录提供商
    u.Id::text,              -- Keycloak sub（与 Id 相同）
    'Keycloak',              -- 显示名称
    u.TenantId               -- 租户 ID
FROM AbpUsers u
WHERE NOT EXISTS (
    SELECT 1 FROM AbpUserLogins l 
    WHERE l.UserId = u.Id AND l.LoginProvider = 'Keycloak'
);
```

**说明**：
- 此脚本为现有用户建立映射关系
- 用户 ID 不变（仍为 Keycloak sub）
- 之后创建的新用户将使用系统生成的 ID

#### 步骤 3：更新代码

1. 在 `AsmsHttpApiHostModule.cs` 添加中间件：

```csharp
using JcStack.Abp.Keycloak.Identity.Auth.Middleware;

public override void OnApplicationInitialization(ApplicationInitializationContext context)
{
    var app = context.GetApplicationBuilder();
    
    app.UseAuthentication();
    app.UseKeycloakForward();  // ← 添加此行
    app.UseAuthorization();
}
```

2. 运行 DbMigrator（会自动为 admin 用户建立映射）

3. 测试登录功能

#### 步骤 4：验证

1. 登录系统，检查是否正常
2. 查询 `AbpUserLogins` 表，确认映射已建立
3. 创建新用户，验证新用户使用系统生成的 ID

### 回滚方案

如果迁移后出现问题，可以临时禁用用户映射：

```json
{
  "Keycloak": {
    "EnableUserMapping": false
  }
}
```

**注意**：禁用后，新架构不生效，但已创建的映射仍然保留。

## 常见问题

### Q: 用户登录时返回 403 Forbidden

**原因**: `AbpUserLogins` 表中没有该用户的映射记录。

**解决方法**:
1. 检查用户是否在 ABP 系统中存在
2. 为该用户手动创建映射：

```sql
INSERT INTO AbpUserLogins (UserId, LoginProvider, ProviderKey, ProviderDisplayName)
VALUES (
    '<本地用户 ID>',
    'Keycloak',
    '<Keycloak sub>',
    'Keycloak'
);
```

### Q: 中间件报错 "User not found in system"

**原因**: 用户在 Keycloak 中存在，但不在 ABP 系统中。

**解决方法**:
1. 在 ABP 系统中创建该用户（通过 UI 或 API）
2. 系统会自动同步到 Keycloak 并建立映射

### Q: 缓存导致用户映射不更新

**原因**: 映射关系已缓存，更新数据库后缓存未失效。

**解决方法**:
1. 清除 Redis/内存缓存
2. 或等待 24 小时缓存自动过期

### Q: 如何支持多个身份提供商？

**答**: 新架构已支持，只需：
1. 添加其他提供商的映射（如 Google、Azure AD）
2. 在 `AbpUserLogins` 表中为同一用户添加多条记录（不同 `LoginProvider`）

示例：
```sql
-- 同一用户绑定 Keycloak 和 Google
INSERT INTO AbpUserLogins (UserId, LoginProvider, ProviderKey) VALUES
('local-user-1', 'Keycloak', 'keycloak-sub-123'),
('local-user-1', 'Google', 'google-sub-456');
```

## 性能优化建议

### 1. 数据库索引

确保 `AbpUserLogins` 表有合适的索引：

```sql
-- ABP 默认已创建，无需手动添加
CREATE INDEX IX_AbpUserLogins_LoginProvider_ProviderKey 
    ON AbpUserLogins (LoginProvider, ProviderKey);
```

### 2. 缓存配置

使用 Redis 作为分布式缓存（生产环境推荐）：

```json
{
  "Redis": {
    "Configuration": "127.0.0.1:6379"
  }
}
```

### 3. 监控查询性能

定期检查慢查询：

```sql
-- PostgreSQL 慢查询日志
SELECT * FROM pg_stat_statements 
WHERE query LIKE '%AbpUserLogins%' 
ORDER BY mean_exec_time DESC;
```

## 架构优势总结

### 数据独立性
- ✅ 系统自主管理用户 ID
- ✅ 不依赖外部身份提供商的 ID 生成

### 灵活性
- ✅ 支持多个身份提供商
- ✅ 易于迁移到其他 SSO 方案

### 可靠性
- ✅ Keycloak 故障不影响已登录用户
- ✅ 映射关系有缓存保护

### 可维护性
- ✅ 符合 ABP Framework 标准实践
- ✅ 使用 ABP 自带的 UserLogins 表

## 相关文档

- [ABP Identity 模块文档](https://docs.abp.io/en/abp/latest/Modules/Identity)
- [ABP External Login 文档](https://docs.abp.io/en/abp/latest/UI/AspNetCore/External-Login)
- [Keycloak Admin REST API](https://www.keycloak.org/docs-api/latest/rest-api/index.html)
