# JcStack.Abp.Keycloak.Identity 集成模块

## 概述

JcStack.Abp.Keycloak.Identity 是一个 **ABP Framework 模块**，专为**多业务系统共享 Keycloak SSO** 场景设计。它允许多个 ABP 应用（如 MES、SRM、LIME）在 Keycloak 中 **并发安全地**创建和管理用户，同时保持各自的 ABP Identity 数据源独立。

### 设计意图

本模块解决以下核心问题：

1. **多系统共享 SSO**：
   - 多个 ABP 应用共享同一个 Keycloak 实例
   - 每个系统保持自己的 ABP Identity 数据库
   - Keycloak 作为中央 SSO 认证服务器

2. **并发安全**：
   - 当多个系统同时创建同一用户时，自动处理冲突
   - 使用 Keycloak ID 作为跨系统的用户标识符
   - 确保数据一致性

3. **最小侵入**：
   - ABP Identity 仍然是主数据源
   - Keycloak 故障不影响 ABP 应用运行
   - 无需修改现有业务代码

4. **灵活配置**：
   - 支持 Client Roles 和 Realm Roles 两种模式
   - 支持角色前缀防止冲突
   - 支持来源系统标识

## 🆕 v2.0 重要变更：用户映射架构

**从 v2.0 开始**，本模块引入了新的用户映射架构，**解决了 Keycloak sub 与系统用户 ID 的耦合问题**。

### 架构变化

**旧架构（v1.x）**：
- 直接使用 Keycloak ID 作为 ABP 用户 ID
- 系统用户 ID 完全依赖 Keycloak

**新架构（v2.0+）**：
- ✅ 系统自主生成用户 ID
- ✅ 通过 `AbpUserLogins` 表建立 Keycloak sub 到本地用户 ID 的映射
- ✅ `KeycloakForwardMiddleware` 在认证后替换 sub
- ✅ 支持多个身份提供商（未来扩展）

📚 **详细文档**：[用户映射架构说明](./docs/user-mapping-architecture.md)

### 快速上手

**新项目**：无需额外操作，按照安装指南集成即可。

**从 v1.x 升级**：
1. 查看 [迁移指南](./docs/user-mapping-architecture.md#迁移指南)
2. 运行数据迁移脚本（为现有用户建立映射）
3. 添加 `UseKeycloakForward()` 中间件

---

## 🚀 快速开始

**5 分钟快速集成！** 查看 [快速开始指南](./QUICK_START.md)

## 特性

### 核心特性

- ✅ **并发安全**: 多系统同时创建用户，自动处理冲突
- ✅ **跨系统 ID 统一**: 使用 Keycloak UUID 作为用户标识符
- ✅ **透明集成**: 继承 ABP IdentityUserAppService，无需修改业务代码
- ✅ **最小侵入**: Keycloak 故障不影响 ABP 应用运行
- ✅ **自动客户端注册**: 通过配置文件自动注册 OIDC 客户端
- ✅ **Keycloak 26+ 兼容**: 自动配置正确的 Client Scopes

### 角色管理

- ✅ **Client Roles 模式**: 支持多系统角色隔离（推荐）
- ✅ **Realm Roles 模式**: 支持全局角色共享
- ✅ **角色前缀**: 防止多系统角色名称冲突
- ✅ **自动角色创建**: 分配角色时自动在 Keycloak 创建

### 认证和安全

- ✅ **JWT Bearer 认证**: 开箱即用的 Keycloak JWT 配置
- ✅ **Claim 映射**: 自动处理 Keycloak Claim 结构
- ✅ **PKCE 支持**: 默认启用 PKCE (S256)
- ✅ **来源系统标识**: 追踪用户来源系统

### 多租户和分布式

- ✅ **多租户支持**: 支持 ABP 多租户架构
- ✅ **租户隔离**: 自动在 Keycloak 属性中存储租户 ID
- ✅ **分布式一致性**: 处理多系统的最终一致性

### 配置和扩展

- ✅ **灵活配置**: 支持多种配置模式（配置文件、环境变量）
- ✅ **可选同步**: 可关闭用户/角色同步
- ✅ **自定义配置**: 支持继承和覆盖默认配置

## 使用场景

### 场景 1：多业务系统共享 SSO

你有 MES、SRM、LIME 等多个 ABP 应用，希望：
- 用户只需登录一次就能访问所有系统
- 每个系统保持自己的用户数据库
- 当一个系统创建用户时，其他系统也能使用该用户

**解决方案**：
- 每个系统集成此模块
- 使用 Keycloak 作为共享 SSO
- 自动处理并发创建用户的冲突

### 场景 2：从 OpenIddict 迁移到 Keycloak

你的 ABP 应用正在使用 OpenIddict，但希望：
- 迁移到企业级的 Keycloak SSO
- 支持多系统集成
- 保持 ABP Identity 的数据和 API

**解决方案**：
- 查看 [迁移指南](./docs/migration-guide.md)
- 替换 OpenIddict 模块
- 无需修改业务代码

### 场景 3：新项目集成 Keycloak

你正在开发新的 ABP 应用，希望从一开始就使用 Keycloak：
- 不想使用 ABP 自带的 OpenIddict
- 需要企业级 SSO 功能
- 计划未来集成多个系统

**解决方案**：
- 查看 [快速开始指南](./QUICK_START.md)
- 5 分钟快速集成
- 自动配置 OIDC 客户端

### 场景 4：单系统使用

你只有一个 ABP 应用，但希望：
- 使用 Keycloak 的高级功能（MFA、Social Login 等）
- 与企业现有的 Keycloak 集成
- 保持 ABP Identity 的灵活性

**解决方案**：
- 集成此模块
- 配置 `RoleSyncMode = ClientRoles` 或 `RealmRoles`
- 享受 Keycloak 的所有功能

## 快速开始

### 完整集成指南

查看 [集成示例文档](./docs/integration-example.md) 获取完整的集成步骤，包括：
- 如何替换 OpenIddict
- HttpApi.Host 完整配置示例
- Keycloak 服务器配置
- 测试和故障排查

### 配置示例

查看 [配置示例文档](./docs/configuration-examples.md) 获取各种场景的配置：
- Web API 配置
- Web 应用配置
- 多租户配置
- Docker Compose 部署

### 迁移指南

如果你正在从 OpenIddict 迁移，查看 [迁移指南](./docs/migration-guide.md)。

## 安装

### 1. 添加模块依赖

在你的 ABP 应用程序模块中添加依赖：

```csharp
[DependsOn(
    typeof(JcStackAbpKeycloakIdentityDomainModule),           // 领域层
    typeof(JcStackAbpKeycloakIdentityApplicationModule),      // 应用层
    // ... 其他依赖
)]
public class YourApplicationModule : AbpModule
{
    // ...
}
```

### 2. 配置 Keycloak 连接

在 `appsettings.json` 中添加 Keycloak 配置：

```json
{
  "Keycloak": {
    "ServerUrl": "https://your-keycloak-server.com",
    "Realm": "your-realm",
    "AdminClientId": "admin-client",
    "AdminClientSecret": "your-client-secret",
    "EnableUserSync": true,
    "EnableRoleSync": true,
    "SyncUserDeletionToKeycloak": true,
    "RolePrefix": "ABP",
    "SourceSystem": "MES",
    "RetryPolicy": {
      "MaxRetryAttempts": 3,
      "InitialBackoffSeconds": 2,
      "MaxBackoffSeconds": 30
    }
  }
}
```

### 3. 配置客户端数据种子（Keycloak 26+ 必需）

本模块使用 `KeycloakClientDataSeedContributor` 在 Keycloak 中自动注册 OIDC 客户端。在 `appsettings.json` 中配置客户端：

```json
{
  "Keycloak": {
    "Clients": {
      "WebApp": {
        "ClientId": "asms-web-app",
        "Name": "ASMS Web Application",
        "RootUrl": "http://localhost:3000",
        "RedirectUris": ["http://localhost:3000/*"],
        "WebOrigins": ["http://localhost:3000", "+"],
        "DefaultClientScopes": [
          "basic",      // 必需：包含 sub (subject) 和 auth_time
          "openid",    // 必需：OIDC 协议要求，/userinfo 端点需要
          "profile",   // 标准：用户个人信息
          "email",     // 标准：邮箱信息
          "roles",     // Keycloak 特定：用户角色
          "web-origins", // Keycloak 特定：CORS
          "acr"        // 标准：认证上下文类引用
        ]
      }
    }
  }
}
```

**重要提示（Keycloak 26+）**：
- `basic` scope：包含 `sub` (subject) claim，这是 OIDC 令牌的核心标识符
- `openid` scope：OIDC 规范要求，没有此 scope 会导致 `/userinfo` 端点返回 403
- `acr` scope：Authentication Context Class Reference，Keycloak 26 默认包含

这些 scopes 与 Keycloak 26 的 `OIDCLoginProtocolFactory` 默认配置保持一致。

### 4. 认证集成（由 Host 自行实现）

本模块不提供 `KeycloakAuthenticationModule`。认证应由业务 Host 项目自行实现（例如 HttpApi.Host 或 Web）。
建议按 ABP 标准方式配置 JWT Bearer，核心思路如下：

1. 在 Host 项目的模块类中配置认证与授权（使用 `AddAbpJwtBearer`）。
2. 从配置中读取 `AuthServer` 段（Authority、RequireHttpsMetadata、ValidAudiences）。
3. 按 Keycloak 的 Claim 结构设置 `NameClaimType`、`RoleClaimType`，并禁用默认映射。

可参考本仓库的 Host 项目实现方式，或在你的 Host 中自行完成等价配置。

## 配置选项

### 基本配置

| 选项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `ServerUrl` | string | - | Keycloak 服务器 URL |
| `Realm` | string | - | Keycloak 领域名称 |
| `AdminClientId` | string | - | Admin API 客户端 ID |
| `AdminClientSecret` | string | - | Admin API 客户端密钥 |

### 同步配置

| 选项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `EnableUserSync` | bool | true | 是否启用用户同步 |
| `EnableRoleSync` | bool | true | 是否启用角色同步 |
| `SyncUserDeletionToKeycloak` | bool | true | 是否将用户删除同步到 Keycloak |
| `RolePrefix` | string | "" | 角色名称前缀（避免冲突） |
| `SourceSystem` | string | "" | 来源系统标识 |

### 重试策略配置

| 选项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `RetryPolicy.MaxRetryAttempts` | int | 3 | 最大重试次数 |
| `RetryPolicy.InitialBackoffSeconds` | int | 2 | 初始退避时间（秒） |
| `RetryPolicy.MaxBackoffSeconds` | int | 30 | 最大退避时间（秒） |

## 架构设计

### 整体架构

```
┌─────────────────────────────────────────┐
│           Keycloak SSO 服务器               │
│  ┌─────────────────────────────────┐  │
│  │  用户 (ID=Keycloak UUID)          │  │
│  │  - username: "john.doe"          │  │
│  │  - email: "john@example.com"     │  │
│  │  - attributes:                   │  │
│  │    - source_system: "MES"        │  │
│  │    - tenant_id: "tenant-1"        │  │
│  └─────────────────────────────────┘  │
└─────────────────────────────────────────┘
         │                       ↑
         │ Admin API             │ OIDC/JWT
         │ (Create/Update)       │ (认证)
         │                       │
┌────────┴───────────────────────┴─────────┐
│    MES 系统 (ABP)            SRM 系统 (ABP)   │
│  ┌──────────────────┐  ┌──────────────────┐  │
│  │ IdentityUser       │  │ IdentityUser       │  │
│  │ ID=Keycloak UUID   │  │ ID=Keycloak UUID   │  │
│  │ UserName="john.doe"│  │ UserName="john.doe"│  │
│  │ Email="john@..."   │  │ Email="john@..."   │  │
│  └──────────────────┘  └──────────────────┘  │
│  ↳ MES DB               ↳ SRM DB             │
└─────────────────────────────────────────┘
```

**关键点**：
1. MES 和 SRM 各自有独立的 ABP Identity 数据库
2. 使用 Keycloak UUID 作为用户 ID，确保跨系统唯一性
3. Keycloak 作为单一认证源
4. 每个系统通过 Admin API 同步用户到 Keycloak

### 核心组件

#### 1. `JcStackAbpKeycloakIdentityUserAppService` （并发安全的用户创建）

```csharp
public override async Task<IdentityUserDto> CreateAsync(IdentityUserCreateDto input)
{
    // 1. 先在 Keycloak 创建或获取用户（并发安全）
    var result = await _keycloakUserService.CreateOrGetUserAsync(...);
    var keycloakUserId = Guid.Parse(result.KeycloakUserId);
    
    // 2. 在 ABP 中创建用户，使用 Keycloak ID
    var user = new IdentityUser(
        keycloakUserId,  // 使用 Keycloak 返回的 ID
        input.UserName,
        input.Email,
        CurrentTenant.Id);
    
    await UserManager.CreateAsync(user, input.Password);
    return ObjectMapper.Map<IdentityUser, IdentityUserDto>(user);
}
```

**作用**：
- 处理多系统并发创建同一用户的场景
- 自动处理 409 Conflict，如果用户已存在则获取现有 ID
- 确保 ABP 和 Keycloak 使用相同的用户 ID

#### 2. `KeycloakUserService.CreateOrGetUserAsync` （并发安全逻辑）

```csharp
public async Task<CreateUserResult> CreateOrGetUserAsync(...)
{
    // 1. 检查用户是否已存在
    var existingUserId = await GetUserIdByUsernameAsync(username);
    if (existingUserId != null)
        return new CreateUserResult(existingUserId, IsNewUser: false);
    
    // 2. 尝试创建用户
    try
    {
        var userId = await CreateUserInKeycloakAsync(...);
        return new CreateUserResult(userId, IsNewUser: true);
    }
    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
    {
        // 3. 处理并发冲突，重新查询
        existingUserId = await GetUserIdByUsernameAsync(username);
        return new CreateUserResult(existingUserId, IsNewUser: false);
    }
}
```

**作用**：
- 实现“检查-创建-重试”模式
- 处理分布式系统中的竞态条件
- 确保最终一致性

#### 3. `KeycloakClientDataSeedContributor` （客户端注册）

```csharp
public async Task SeedAsync(DataSeedContext context)
{
    // 从 appsettings.json 读取客户端配置
    var clientsSection = Configuration.GetSection("Keycloak:Clients");
    
    foreach (var clientSection in clientsSection.GetChildren())
    {
        // 自动注册 OIDC 客户端到 Keycloak
        await CreateClientFromConfigAsync(clientSection);
    }
}
```

**作用**：
- 替代 ABP 的 `OpenIddictDataSeedContributor`
- 自动在 Keycloak 中注册 OIDC 客户端
- 支持多客户端配置（Web、Swagger 等）
- 自动配置 Client Scopes（Keycloak 26+ 必需）

#### 4. `IdentityAuthModule` （认证集成）

```csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    var configuration = context.Services.GetConfiguration();
    
    // 配置 Keycloak JWT Bearer 认证
    context.Services.AddKeycloakAuthentication(configuration);
}
```

**作用**：
- 提供 Keycloak JWT 认证的默认配置
- 支持自定义配置覆盖
- 处理 Keycloak 的 Claim 映射

## 工作原理

### 1. 用户创建流程（并发安全）

```
系统 A (MES)                  Keycloak                  系统 B (SRM)
    │                            │                           │
    │ 1. CreateUser(john.doe)    │                           │
    ├────────────────────────────>│                           │
    │                            │ 2. 检查用户是否存在       │
    │                            │    (不存在)                 │
    │                            │                           │
    │                            │ 3. 创建用户              │
    │                            │    ID=uuid-1234           │
    │<───────────────────────────│                           │
    │ 4. 返回 ID=uuid-1234       │                           │
    │                            │                           │
    │ 5. 在 ABP 创建用户        │                           │
    │    ID=uuid-1234            │                           │
    │                            │ 6. CreateUser(john.doe)    │
    │                            │<──────────────────────────┤
    │                            │ 7. 检查用户是否存在       │
    │                            │    (已存在)                 │
    │                            │ 8. 返回现有 ID          │
    │                            ├──────────────────────────>│
    │                            │    ID=uuid-1234           │
    │                            │                           │
    │                            │                │ 9. 在 ABP 创建用户
    │                            │                │    ID=uuid-1234
```

**关键点**：
- 系统 A 和 B 同时创建用户，Keycloak 保证只创建一次
- 两个系统最终都使用相同的 Keycloak ID
- 无需人工干预，自动处理冲突

### 3. 角色同步模式

本模块支持两种角色同步模式：

#### 模式 1：Client Roles（推荐用于多系统）

```json
{
  "Keycloak": {
    "ClientId": "mes-api",
    "RoleSyncMode": "ClientRoles"
  }
}
```

**优点**：
- 每个系统的角色隔离，不会冲突
- 支持精细化权限控制
- 适合 MES、SRM、LIME 等多系统共存

**Token 结构**：
```json
{
  "resource_access": {
    "mes-api": {
      "roles": ["admin", "operator"]
    },
    "srm-api": {
      "roles": ["purchaser", "approver"]
    }
  }
}
```

#### 模式 2：Realm Roles（需要配置前缀）

```json
{
  "Keycloak": {
    "RoleSyncMode": "RealmRoles",
    "RolePrefix": "MES"
  }
}
```

**优点**：
- 角色在整个 Realm 共享
- 适合单一系统或紧密集成的系统

**Token 结构**：
```json
{
  "realm_access": {
    "roles": ["MES_admin", "MES_operator", "SRM_purchaser"]
  }
}
```

**注意**：Realm Roles 模式需要配置 `RolePrefix` 避免多系统角色名称冲突。

### 2. 角色同步流程

```
ABP 应用 → KeycloakUserManager.AddToRoleAsync()
    ↓
1. 调用 base.AddToRoleAsync() (ABP Identity)
    ↓
2. 发布 RoleAssignedEto 事件
    ↓
3. RoleAssignedEventHandler 接收事件
    ↓
4. 将 RoleAssignedSyncJob 加入后台队列
    ↓
5. 后台作业异步执行
    ↓
6. 确保角色在 Keycloak 中存在（自动创建）
    ↓
7. 为用户分配角色
```

## 错误处理

模块内置了完善的错误处理机制：

### 1. 重试策略

- 使用指数退避算法
- 自动重试临时性错误（网络错误、5xx 服务器错误、429 限流）
- 不重试永久性错误（4xx 客户端错误）

### 2. 冲突处理

- 用户/角色已存在冲突：自动切换为更新操作
- 并发创建冲突：重新查询并更新

### 3. Keycloak 不可用

- Keycloak 故障不影响 ABP 操作
- 后台作业会自动重试
- 达到最大重试次数后记录错误日志

## 属性映射

### 用户属性

ABP 用户属性会映射到 Keycloak 用户的自定义属性：

| ABP 属性 | Keycloak 属性 | 说明 |
|----------|---------------|------|
| `Id` | `attributes.abp_user_id` | ABP 用户 ID |
| `UserName` | `username` | 用户名 |
| `Email` | `email` | 邮箱 |
| `Name` | `firstName` | 名 |
| `Surname` | `lastName` | 姓 |
| `PhoneNumber` | `attributes.phone_number` | 电话号码 |
| `TenantId` | `attributes.tenant_id` | 租户 ID（多租户） |
| - | `attributes.source_system` | 来源系统标识 |

### 角色映射

- ABP 角色名称会添加配置的前缀（如果有）
- 例如：`Admin` → `ABP_Admin`（如果 `RolePrefix` 设置为 "ABP"）

## 日志记录

模块使用结构化日志记录所有操作：

```csharp
// 成功日志
_logger.LogInformation(
    "Created user {Username} in Keycloak with ID {KeycloakUserId}",
    username, keycloakUserId);

// 警告日志
_logger.LogWarning(
    "Keycloak operation failed (attempt {Attempt}/{MaxAttempts}). Retrying...",
    attempt, maxAttempts);

// 错误日志
_logger.LogError(
    ex,
    "Failed to sync user {Username} to Keycloak after {Attempts} attempts",
    username, attempts);
```

## 监控和故障排查

### 1. 检查后台作业状态

使用 ABP 的后台作业管理界面查看同步作业的执行状态。

### 2. 查看日志

搜索包含 "Keycloak" 关键字的日志：

```bash
# 查看错误日志
grep "Keycloak.*Error" logs/app.log

# 查看重试日志
grep "Keycloak.*Retrying" logs/app.log
```

### 3. 常见问题

#### 问题：用户在 ABP 中创建成功，但 Keycloak 中没有

**可能原因**：
1. Keycloak 服务器不可达
2. Admin API 客户端配置错误
3. 后台作业未启动

**解决方法**：
1. 检查 Keycloak 服务器连接
2. 验证 `AdminClientId` 和 `AdminClientSecret` 配置
3. 确认 ABP 后台作业服务已启动

#### 问题：同步失败并不断重试

**可能原因**：
1. Keycloak 服务器临时故障
2. 网络问题
3. API 限流

**解决方法**：
1. 检查 Keycloak 服务器状态
2. 检查网络连接
3. 调整重试策略配置

## 性能考虑

### 1. 异步同步

- 所有 Keycloak 同步操作都是异步的
- 不会阻塞 ABP 主流程
- 用户体验不受影响

### 2. 批量操作

对于批量用户导入场景，建议：
1. 使用批量导入工具（待实现）
2. 或临时禁用同步：`EnableUserSync = false`

### 3. 资源使用

- 后台作业使用独立线程池
- 重试策略使用指数退避，避免过度重试
- HTTP 连接使用连接池复用

## 安全考虑

### 1. 凭据管理

- 不要在代码中硬编码 `AdminClientSecret`
- 使用 ABP 的配置加密功能
- 或使用环境变量/密钥管理服务

### 2. 最小权限原则

Keycloak Admin Client 应该只授予必要的权限：
- `manage-users`
- `manage-realm` (仅用于角色管理)

### 3. 审计日志

- 所有同步操作都有日志记录
- 可以追踪用户和角色的变更历史

## Keycloak Client Scopes 配置说明

### Keycloak 26+ 默认 Client Scopes

根据 Keycloak 26 的 `OIDCLoginProtocolFactory` 配置，标准的默认 client scopes 包括：

| Scope | 类型 | 说明 | 必需 |
|-------|------|------|------|
| `basic` | Keycloak 特定 | 包含 `sub` (subject) 和 `auth_time` 等基础 OIDC claims | ✅ |
| `openid` | OIDC 标准 | OIDC 协议要求，用于 `/userinfo` 等端点 | ✅ |
| `profile` | OIDC 标准 | 用户个人信息（name, family_name, given_name 等） | ✅ |
| `email` | OIDC 标准 | 邮箱相关信息（email, email_verified） | ✅ |
| `roles` | Keycloak 特定 | 用户角色，添加到 access token | ✅ |
| `web-origins` | Keycloak 特定 | CORS 配置，`allowed-origins` claim | ✅ |
| `acr` | OIDC 标准 | Authentication Context Class Reference | ✅ |
| `offline_access` | OIDC 标准 | Refresh token 支持 | ⚪ (可选) |
| `address` | OIDC 标准 | 地址信息 | ⚪ (可选) |
| `phone` | OIDC 标准 | 电话信息 | ⚪ (可选) |

### 为什么 `basic` scope 至关重要？

`basic` scope 是 Keycloak 25.0.0+ 引入的，包含：
- **`sub` (subject) claim**：用户的唯一标识符，所有 OIDC token 的核心 claim
- **`auth_time` claim**：用户认证时间戳

如果没有 `basic` scope，令牌将缺少 `sub` claim，导致：
- OIDC 客户端无法识别用户
- 与 OIDC 规范不兼容
- 可能导致认证流程失败

### 为什么 `openid` scope 至关重要？

根据 [OIDC 规范](https://openid.net/specs/openid-connect-core-1_0.html#AuthRequest)：
- `scope` 参数是**必需**的，并且必须包含 `openid` 值
- 没有 `openid` scope，Keycloak 会拒绝 `/userinfo` 等端点的访问（403 错误）
- 认证请求将不符合 OIDC 标准

### Scope 在 Token 中的表现

- **OIDC 标准 scopes** (`openid`, `profile`, `email`)：`include.in.token.scope = true`，会出现在 token 的 `scope` claim 中
- **Keycloak 特定 scopes** (`basic`, `roles`, `web-origins`)：`include.in.token.scope = false`，不会出现在 `scope` claim 中，但其 mappers 仍然会添加相应的 claims

### 配置示例

```json
{
  "Keycloak": {
    "Clients": {
      "WebApp": {
        "ClientId": "my-app",
        "RootUrl": "http://localhost:3000",
        "DefaultClientScopes": [
          "basic",       // 必需：sub + auth_time
          "openid",      // 必需：OIDC 协议
          "profile",     // 推荐：用户信息
          "email",       // 推荐：邮箱
          "roles",       // 推荐：权限管理
          "web-origins", // 推荐：CORS
          "acr"          // 推荐：认证上下文
        ],
        "OptionalClientScopes": [
          "offline_access", // 如需 refresh token
          "address",        // 如需地址信息
          "phone"           // 如需电话信息
        ]
      }
    }
  }
}
```

## 版本兼容性

- ABP Framework: 10.0+
- .NET: 10.0+
- Keycloak: 26.0+ (推荐)
- Keycloak: 25.0+ (支持 `basic` scope)
- Keycloak: 20.0-24.x (基本支持，但需要手动配置 `sub` claim mapper)

## 许可证

[您的许可证信息]

## 支持

如有问题或建议，请联系：[您的联系方式]
