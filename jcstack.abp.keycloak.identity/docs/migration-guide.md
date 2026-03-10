# 从 OpenIddict 迁移到 Keycloak 指南

## 目录

1. [迁移概述](#迁移概述)
2. [准备工作](#准备工作)
3. [迁移步骤](#迁移步骤)
4. [数据迁移](#数据迁移)
5. [测试验证](#测试验证)
6. [回滚计划](#回滚计划)

## 迁移概述

### 为什么迁移？

- **集中式 SSO**: Keycloak 提供企业级的集中式单点登录
- **更好的管理**: 统一的用户和权限管理界面
- **多系统集成**: 支持多个系统（MES、SRM、LIME）共享同一个认证服务器
- **标准协议**: 完全支持 OpenID Connect 和 OAuth 2.0 标准

### 迁移影响

| 组件 | 影响程度 | 说明 |
|------|---------|------|
| 用户数据 | 低 | ABP Identity 数据库保持不变 |
| 认证流程 | 中 | 需要更新认证配置 |
| 授权逻辑 | 低 | 基于角色的授权逻辑不变 |
| 客户端应用 | 中 | 需要更新认证端点 |
| API 调用 | 低 | Bearer Token 验证方式相同 |

## 准备工作

### 1. 环境要求

- Keycloak 20.0+ 服务器
- ABP Framework 10.0+
- .NET 10.0+
- 数据库备份

### 2. Keycloak 服务器部署

#### 使用 Docker

```bash
docker run -d \
  --name keycloak \
  -p 8080:8080 \
  -e KEYCLOAK_ADMIN=admin \
  -e KEYCLOAK_ADMIN_PASSWORD=admin \
  quay.io/keycloak/keycloak:23.0 \
  start-dev
```

#### 使用 Docker Compose

参考 [configuration-examples.md](./configuration-examples.md#docker-compose-示例)

### 3. 创建 Keycloak Realm 和 Client

1. 访问 Keycloak Admin Console: `http://localhost:8080`
2. 创建 Realm: `my-realm`
3. 创建 Admin Client（用于用户同步）
4. 创建 Web/API Client（用于认证）

详细步骤参考 [configuration-examples.md](./configuration-examples.md#keycloak-客户端配置)

## 迁移步骤

### 步骤 1: 安装 JcStack.Abp.Keycloak.Identity 模块

#### 1.1 添加 NuGet 包引用

在你的项目中添加模块引用：

```xml
<ItemGroup>
  <ProjectReference Include="path/to/JcStack.Abp.Keycloak.Identity.Domain/JcStack.Abp.Keycloak.Identity.Domain.csproj" />
  <ProjectReference Include="path/to/JcStack.Abp.Keycloak.Identity.Application/JcStack.Abp.Keycloak.Identity.Application.csproj" />
  <ProjectReference Include="path/to/JcStack.Abp.Keycloak.Identity.Authentication/JcStack.Abp.Keycloak.Identity.Authentication.csproj" />
</ItemGroup>
```

#### 1.2 更新模块依赖

**Domain 模块**:
```csharp
[DependsOn(
    typeof(JcStackAbpKeycloakIdentityDomainModule),  // 添加
    typeof(AbpJcStackAbpKeycloakIdentityDomainModule),
    // ... 其他依赖
)]
public class YourDomainModule : AbpModule
{
}
```

**Application 模块**:
```csharp
[DependsOn(
    typeof(JcStackAbpKeycloakIdentityApplicationModule),  // 添加
    typeof(AbpJcStackAbpKeycloakIdentityApplicationModule),
    // ... 其他依赖
)]
public class YourApplicationModule : AbpModule
{
}
```

**HttpApi.Host 模块**:
```csharp
[DependsOn(
    typeof(KeycloakAuthenticationModule),  // 添加
    typeof(AbpAspNetCoreAuthenticationOpenIdConnectModule),
    // ... 其他依赖
)]
public class YourHttpApiHostModule : AbpModule
{
}
```

### 步骤 2: 移除 OpenIddict 依赖

#### 2.1 移除 NuGet 包

从 `.csproj` 文件中移除：

```xml
<!-- 移除这些包 -->
<PackageReference Include="Volo.Abp.Account.Application" Version="10.0.2" />
<PackageReference Include="Volo.Abp.Account.HttpApi" Version="10.0.2" />
<PackageReference Include="Volo.Abp.Account.Web.OpenIddict" Version="10.0.2" />
```

#### 2.2 移除模块依赖

从模块类中移除：

```csharp
// 移除这些依赖
[DependsOn(
    // typeof(AbpAccountApplicationModule),  // 移除
    // typeof(AbpAccountHttpApiModule),      // 移除
    // typeof(AbpAccountWebOpenIddictModule), // 移除
)]
```

#### 2.3 移除 OpenIddict 配置

从 `appsettings.json` 中移除 OpenIddict 相关配置：

```json
{
  // 移除这些配置
  // "OpenIddict": { ... }
}
```

### 步骤 3: 配置 Keycloak

#### 3.1 添加 Keycloak 配置

在 `appsettings.json` 中添加：

```json
{
  "Keycloak": {
    "ServerUrl": "http://localhost:8080",
    "Realm": "my-realm",
    "AdminClientId": "abp-admin-client",
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

#### 3.2 更新认证配置

在 `Program.cs` 或模块的 `ConfigureServices` 中：

```csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    // Keycloak 认证已通过 KeycloakAuthenticationModule 自动配置
    // 无需额外配置
}
```

### 步骤 4: 更新客户端应用

#### 4.1 Angular 应用

更新 `environment.ts`:

```typescript
export const environment = {
  production: false,
  application: {
    name: 'MyApp',
  },
  oAuthConfig: {
    issuer: 'http://localhost:8080/realms/my-realm',
    clientId: 'abp-web-client',
    redirectUri: window.location.origin,
    responseType: 'code',
    scope: 'openid profile email roles',
    requireHttps: false
  },
  apis: {
    default: {
      url: 'https://localhost:44300',
    },
  },
};
```

更新 `app.module.ts`:

```typescript
import { OAuthModule } from 'angular-oauth2-oidc';

@NgModule({
  imports: [
    // ...
    OAuthModule.forRoot({
      resourceServer: {
        allowedUrls: ['https://localhost:44300'],
        sendAccessToken: true,
      },
    }),
  ],
})
export class AppModule {}
```

#### 4.2 Blazor 应用

更新 `Program.cs`:

```csharp
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Keycloak", options.ProviderOptions);
    options.ProviderOptions.Authority = "http://localhost:8080/realms/my-realm";
    options.ProviderOptions.ClientId = "abp-blazor-client";
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.DefaultScopes.Add("openid");
    options.ProviderOptions.DefaultScopes.Add("profile");
    options.ProviderOptions.DefaultScopes.Add("email");
    options.ProviderOptions.DefaultScopes.Add("roles");
});
```

#### 4.3 MVC 应用

已通过 `KeycloakAuthenticationModule` 自动配置，无需额外更改。

### 步骤 5: 数据库迁移

#### 5.1 备份数据库

```bash
# SQL Server
sqlcmd -S localhost -U sa -P YourPassword -Q "BACKUP DATABASE MyAppDb TO DISK='C:\Backup\MyAppDb.bak'"

# PostgreSQL
pg_dump -U postgres MyAppDb > myappdb_backup.sql

# MySQL
mysqldump -u root -p MyAppDb > myappdb_backup.sql
```

#### 5.2 运行迁移

```bash
# 如果有新的数据库迁移
dotnet ef database update
```

## 数据迁移

### 批量同步现有用户到 Keycloak

#### 方法 1: 使用迁移工具（推荐）

创建一个迁移服务：

```csharp
public class KeycloakMigrationService : ITransientDependency
{
    private readonly IIdentityUserRepository _userRepository;
    private readonly IKeycloakUserService _keycloakUserService;
    private readonly ILogger<KeycloakMigrationService> _logger;

    public KeycloakMigrationService(
        IIdentityUserRepository userRepository,
        IKeycloakUserService keycloakUserService,
        ILogger<KeycloakMigrationService> logger)
    {
        _userRepository = userRepository;
        _keycloakUserService = keycloakUserService;
        _logger = logger;
    }

    public async Task MigrateAllUsersAsync(bool dryRun = true)
    {
        var users = await _userRepository.GetListAsync();
        var successCount = 0;
        var failCount = 0;

        _logger.LogInformation(
            "Starting migration of {Count} users (DryRun: {DryRun})",
            users.Count, dryRun);

        foreach (var user in users)
        {
            try
            {
                if (!dryRun)
                {
                    var userDto = new KeycloakUserDto
                    {
                        AbpUserId = user.Id,
                        Username = user.UserName,
                        Email = user.Email,
                        FirstName = user.Name,
                        LastName = user.Surname,
                        PhoneNumber = user.PhoneNumber,
                        TenantId = user.TenantId
                    };

                    await _keycloakUserService.CreateOrUpdateUserAsync(userDto);
                }

                successCount++;
                _logger.LogInformation(
                    "Migrated user: {Username} ({Email})",
                    user.UserName, user.Email);
            }
            catch (Exception ex)
            {
                failCount++;
                _logger.LogError(
                    ex,
                    "Failed to migrate user: {Username} ({Email})",
                    user.UserName, user.Email);
            }
        }

        _logger.LogInformation(
            "Migration completed. Success: {Success}, Failed: {Failed}",
            successCount, failCount);
    }
}
```

运行迁移：

```csharp
// 在 DbMigrator 或控制台应用中
public class MigrationHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public MigrationHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var migrationService = scope.ServiceProvider
            .GetRequiredService<KeycloakMigrationService>();

        // 先试运行
        await migrationService.MigrateAllUsersAsync(dryRun: true);

        // 确认后执行实际迁移
        Console.WriteLine("Press Y to continue with actual migration...");
        if (Console.ReadKey().Key == ConsoleKey.Y)
        {
            await migrationService.MigrateAllUsersAsync(dryRun: false);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

#### 方法 2: 使用 API 端点

创建一个管理 API：

```csharp
[Authorize(Roles = "admin")]
[Route("api/admin/keycloak-migration")]
public class KeycloakMigrationController : AbpController
{
    private readonly KeycloakMigrationService _migrationService;

    public KeycloakMigrationController(KeycloakMigrationService migrationService)
    {
        _migrationService = migrationService;
    }

    [HttpPost("migrate-users")]
    public async Task<IActionResult> MigrateUsers([FromQuery] bool dryRun = true)
    {
        await _migrationService.MigrateAllUsersAsync(dryRun);
        return Ok(new { message = "Migration completed" });
    }
}
```

调用 API：

```bash
# 试运行
curl -X POST "https://api.example.com/api/admin/keycloak-migration/migrate-users?dryRun=true" \
  -H "Authorization: Bearer YOUR_TOKEN"

# 实际迁移
curl -X POST "https://api.example.com/api/admin/keycloak-migration/migrate-users?dryRun=false" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## 测试验证

### 1. 功能测试清单

- [ ] 用户登录
- [ ] 用户注册
- [ ] 用户登出
- [ ] 密码重置
- [ ] 角色分配
- [ ] 权限验证
- [ ] API 调用（Bearer Token）
- [ ] 刷新 Token
- [ ] 多租户切换

### 2. 测试脚本

#### 测试用户登录

```bash
# 获取 Token
curl -X POST "http://localhost:8080/realms/my-realm/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=abp-web-client" \
  -d "client_secret=your-secret" \
  -d "grant_type=password" \
  -d "username=admin" \
  -d "password=1q2w3E*"
```

#### 测试 API 调用

```bash
# 使用 Token 调用 API
curl -X GET "https://api.example.com/api/app/products" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

### 3. 性能测试

使用 Apache Bench 或 k6 进行负载测试：

```bash
# Apache Bench
ab -n 1000 -c 10 -H "Authorization: Bearer TOKEN" \
  https://api.example.com/api/app/products

# k6
k6 run load-test.js
```

## 回滚计划

### 如果需要回滚到 OpenIddict

#### 1. 恢复代码

```bash
git revert <migration-commit-hash>
```

#### 2. 恢复配置

恢复 `appsettings.json` 中的 OpenIddict 配置。

#### 3. 恢复数据库

```bash
# SQL Server
sqlcmd -S localhost -U sa -P YourPassword \
  -Q "RESTORE DATABASE MyAppDb FROM DISK='C:\Backup\MyAppDb.bak' WITH REPLACE"
```

#### 4. 重新部署

```bash
dotnet publish -c Release
# 部署到服务器
```

### 保留双认证（过渡期）

在过渡期可以同时支持 OpenIddict 和 Keycloak：

```csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    var configuration = context.Services.GetConfiguration();
    var useKeycloak = configuration.GetValue<bool>("UseKeycloak");

    if (useKeycloak)
    {
        // 使用 Keycloak
        context.Services.AddKeycloakWebApiAuthentication(/* ... */);
    }
    else
    {
        // 使用 OpenIddict
        context.Services.AddAbpOpenIddict(/* ... */);
    }
}
```

## 常见问题

### Q1: 迁移后用户无法登录？

**A**: 检查以下几点：
1. Keycloak 中是否已创建用户
2. 用户密码是否正确（可能需要重置）
3. 客户端配置是否正确
4. Redirect URI 是否匹配

### Q2: API 调用返回 401 Unauthorized？

**A**: 检查：
1. Token 是否有效
2. Token 的 Audience 是否正确
3. Token 的 Issuer 是否匹配
4. 时钟偏差设置

### Q3: 角色权限不生效？

**A**: 确认：
1. Keycloak 中角色映射配置正确
2. Token 中包含 roles claim
3. ABP 授权策略配置正确

### Q4: 性能下降？

**A**: 优化：
1. 启用 Token 缓存
2. 调整连接池大小
3. 使用 Redis 缓存
4. 优化 Keycloak 服务器配置

## 支持资源

- [Keycloak 官方文档](https://www.keycloak.org/documentation)
- [ABP Framework 文档](https://docs.abp.io)
- [OpenID Connect 规范](https://openid.net/connect/)
- [OAuth 2.0 规范](https://oauth.net/2/)

## 联系支持

如有问题，请联系：
- Email: support@example.com
- 技术支持: https://support.example.com
