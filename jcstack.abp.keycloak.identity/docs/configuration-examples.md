# JcStack.Abp.Keycloak.Identity 配置示例

## 目录

1. [Client Scopes 配置（Keycloak 26+）](#client-scopes-配置-keycloak-26)
2. [基本配置](#基本配置)
3. [Web API 配置](#web-api-配置)
4. [Web 应用配置](#web-应用配置)
5. [多租户配置](#多租户配置)
6. [生产环境配置](#生产环境配置)
7. [Docker Compose 示例](#docker-compose-示例)

## Client Scopes 配置 (Keycloak 26+)

> ⚠️ **重要**：Keycloak 26+ 对 client scopes 的配置至关重要。错误的配置会导致 token 中缺少关键 claims、认证失败。

### 为什么需要正确配置？

1. **`basic` scope 必需** ✅
   - 包含 `sub` (subject) claim - OIDC token 的核心用户标识符
   - 包含 `auth_time` claim - 认证时间戳
   - 缺少此 scope 会导致 OIDC 客户端无法识别用户

2. **`openid` scope 必需** ✅
   - [OIDC 规范](https://openid.net/specs/openid-connect-core-1_0.html#AuthRequest)要求
   - 缺少此 scope，`/userinfo` 端点返回 403 错误

3. **其他推荐 scopes**
   - `profile` - 用户信息
   - `email` - 邮箱信息
   - `roles` - 用户角色和权限
   - `web-origins` - CORS 配置
   - `acr` - 认证上下文

### 标准配置

```json
{
  "Keycloak": {
    "Clients": {
      "WebApp": {
        "ClientId": "my-app",
        "RootUrl": "http://localhost:3000",
        "DefaultClientScopes": [
          "basic",       // ✅ sub + auth_time
          "openid",      // ✅ OIDC 协议
          "profile",     // ✅ 用户信息
          "email",       // ✅ 邮箱
          "roles",       // ✅ 角色
          "web-origins", // ✅ CORS
          "acr"          // ✅ 认证上下文
        ]
      }
    }
  }
}
```

### 常见错误

❌ **缺少 `basic`** - token 没有 `sub` claim  
❌ **缺少 `openid`** - `/userinfo` 返回 403  
❌ **使用旧配置** - Keycloak 25+ 需要这两个 scope

---

## 基本配置

### appsettings.json

```json
{
  "Keycloak": {
    "ServerUrl": "https://keycloak.example.com",
    "Realm": "my-realm",
    "AdminClientId": "abp-admin-client",
    "AdminClientSecret": "your-client-secret-here",
    "EnableUserSync": true,
    "EnableRoleSync": true,
    "EnableClaimSync": true,
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

### 环境变量配置

```bash
# Keycloak 连接
export Keycloak__ServerUrl="https://keycloak.example.com"
export Keycloak__Realm="my-realm"
export Keycloak__AdminClientId="abp-admin-client"
export Keycloak__AdminClientSecret="your-client-secret-here"

# 同步配置
export Keycloak__EnableUserSync="true"
export Keycloak__EnableRoleSync="true"
export Keycloak__SourceSystem="MES"
```

## Web API 配置

### 1. 添加模块依赖

```csharp
[DependsOn(
    typeof(JcStackAbpKeycloakIdentityDomainModule),
    typeof(JcStackAbpKeycloakIdentityApplicationModule),
    typeof(KeycloakAuthenticationModule),  // 添加认证模块
    // ... 其他依赖
)]
public class YourHttpApiHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 其他配置...
    }
}
```

### 2. appsettings.json 完整配置

```json
{
  "App": {
    "SelfUrl": "https://api.example.com",
    "CorsOrigins": "https://app.example.com,https://admin.example.com"
  },
  "Keycloak": {
    "ServerUrl": "https://keycloak.example.com",
    "Realm": "my-realm",
    "AdminClientId": "abp-api-client",
    "AdminClientSecret": "api-client-secret",
    "EnableUserSync": true,
    "EnableRoleSync": true,
    "SyncUserDeletionToKeycloak": true,
    "RolePrefix": "API",
    "SourceSystem": "API",
    "RetryPolicy": {
      "MaxRetryAttempts": 5,
      "InitialBackoffSeconds": 1,
      "MaxBackoffSeconds": 60
    }
  },
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=MyApp;Trusted_Connection=True"
  }
}
```

### 3. 使用认证

```csharp
[Authorize] // 需要认证
[Route("api/[controller]")]
public class ProductsController : AbpController
{
    [HttpGet]
    public async Task<List<ProductDto>> GetListAsync()
    {
        // 只有认证用户可以访问
        return await _productAppService.GetListAsync();
    }

    [HttpPost]
    [Authorize(Roles = "admin")] // 需要 admin 角色
    public async Task<ProductDto> CreateAsync(CreateProductDto input)
    {
        return await _productAppService.CreateAsync(input);
    }
}
```

## Web 应用配置

### 1. 添加模块依赖

```csharp
[DependsOn(
    typeof(JcStackAbpKeycloakIdentityDomainModule),
    typeof(JcStackAbpKeycloakIdentityApplicationModule),
    typeof(KeycloakAuthenticationModule),
    // ... 其他依赖
)]
public class YourWebModule : AbpModule
{
    public override void Configure(ServiceConfigurationContext context)
    {
        var app = context.GetApplicationBuilder();
        
        // 添加认证中间件
        app.UseAuthentication();
        app.UseAuthorization();
        
        // ... 其他中间件
    }
}
```

### 2. appsettings.json

```json
{
  "App": {
    "SelfUrl": "https://app.example.com"
  },
  "Keycloak": {
    "ServerUrl": "https://keycloak.example.com",
    "Realm": "my-realm",
    "AdminClientId": "abp-web-client",
    "AdminClientSecret": "web-client-secret",
    "EnableUserSync": true,
    "EnableRoleSync": true,
    "SyncUserDeletionToKeycloak": true,
    "RolePrefix": "WEB",
    "SourceSystem": "WEB"
  }
}
```

### 3. 登录/登出

```csharp
public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login(string returnUrl = "/")
    {
        return Challenge(new AuthenticationProperties
        {
            RedirectUri = returnUrl
        }, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("Cookies");
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        return Redirect("/");
    }
}
```

## 多租户配置

### 1. 租户解析配置

```csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    Configure<AbpTenantResolveOptions>(options =>
    {
        // 从子域名解析租户
        options.AddDomainTenantResolver("{0}.example.com");
        
        // 从 Header 解析租户
        options.TenantResolvers.Add(new HeaderTenantResolveContributor());
    });
}
```

### 2. 租户特定配置

```json
{
  "Keycloak": {
    "ServerUrl": "https://keycloak.example.com",
    "Realm": "multi-tenant-realm",
    "AdminClientId": "abp-admin-client",
    "AdminClientSecret": "admin-secret",
    "EnableUserSync": true,
    "EnableRoleSync": true,
    "SyncUserDeletionToKeycloak": true,
    "RolePrefix": "",
    "SourceSystem": "MULTI_TENANT"
  },
  "Tenants": {
    "tenant1": {
      "Keycloak": {
        "RolePrefix": "TENANT1",
        "SourceSystem": "TENANT1"
      }
    },
    "tenant2": {
      "Keycloak": {
        "RolePrefix": "TENANT2",
        "SourceSystem": "TENANT2"
      }
    }
  }
}
```

## 生产环境配置

### 1. 使用 Azure Key Vault

```csharp
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // 添加 Azure Key Vault
        builder.Configuration.AddAzureKeyVault(
            new Uri("https://your-keyvault.vault.azure.net/"),
            new DefaultAzureCredential());
        
        await builder.AddApplicationAsync<YourModule>();
        var app = builder.Build();
        await app.InitializeApplicationAsync();
        await app.RunAsync();
        return 0;
    }
}
```

### 2. appsettings.Production.json

```json
{
  "Keycloak": {
    "ServerUrl": "https://keycloak.production.com",
    "Realm": "production-realm",
    "AdminClientId": "abp-prod-client",
    "AdminClientSecret": "#{KeyVault:KeycloakClientSecret}#",
    "EnableUserSync": true,
    "EnableRoleSync": true,
    "SyncUserDeletionToKeycloak": true,
    "RolePrefix": "PROD",
    "SourceSystem": "PRODUCTION",
    "RetryPolicy": {
      "MaxRetryAttempts": 5,
      "InitialBackoffSeconds": 2,
      "MaxBackoffSeconds": 60
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Keycloak": "Warning"
    }
  }
}
```

### 3. 健康检查配置

```csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    context.Services.AddHealthChecks()
        .AddCheck("keycloak", () =>
        {
            // 检查 Keycloak 连接
            try
            {
                var keycloakClient = context.Services.GetRequiredService<IKeycloakClient>();
                // 执行简单的健康检查请求
                return HealthCheckResult.Healthy("Keycloak is reachable");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Keycloak is unreachable", ex);
            }
        });
}
```

## Docker Compose 示例

### docker-compose.yml

```yaml
version: '3.8'

services:
  # Keycloak 服务
  keycloak:
    image: quay.io/keycloak/keycloak:23.0
    container_name: keycloak
    environment:
      - KEYCLOAK_ADMIN=admin
      - KEYCLOAK_ADMIN_PASSWORD=admin
      - KC_DB=postgres
      - KC_DB_URL=jdbc:postgresql://postgres:5432/keycloak
      - KC_DB_USERNAME=keycloak
      - KC_DB_PASSWORD=keycloak
      - KC_HOSTNAME=localhost
      - KC_HTTP_ENABLED=true
    ports:
      - "8080:8080"
    command:
      - start-dev
    depends_on:
      - postgres
    networks:
      - app-network

  # PostgreSQL 数据库（Keycloak）
  postgres:
    image: postgres:16
    container_name: postgres-keycloak
    environment:
      - POSTGRES_DB=keycloak
      - POSTGRES_USER=keycloak
      - POSTGRES_PASSWORD=keycloak
    volumes:
      - postgres-data:/var/lib/postgresql/data
    networks:
      - app-network

  # ABP 应用
  abp-api:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: abp-api
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - Keycloak__ServerUrl=http://keycloak:8080
      - Keycloak__Realm=my-realm
      - Keycloak__AdminClientId=abp-admin-client
      - Keycloak__AdminClientSecret=your-secret
      - ConnectionStrings__Default=Server=sqlserver;Database=MyApp;User=sa;Password=YourPassword123
    ports:
      - "5000:80"
    depends_on:
      - keycloak
      - sqlserver
    networks:
      - app-network

  # SQL Server 数据库（ABP）
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: sqlserver
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword123
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql
    networks:
      - app-network

volumes:
  postgres-data:
  sqlserver-data:

networks:
  app-network:
    driver: bridge
```

### 启动命令

```bash
# 启动所有服务
docker-compose up -d

# 查看日志
docker-compose logs -f

# 停止服务
docker-compose down

# 停止并删除数据
docker-compose down -v
```

## Keycloak 客户端配置

### 1. 创建 Realm

1. 登录 Keycloak Admin Console: `http://localhost:8080`
2. 创建新 Realm: `my-realm`

### 2. 创建 Admin Client

1. 进入 Clients → Create Client
2. 配置：
   - Client ID: `abp-admin-client`
   - Client Protocol: `openid-connect`
   - Access Type: `confidential`
   - Service Accounts Enabled: `ON`
   - Authorization Enabled: `ON`

3. 在 Credentials 标签页获取 Client Secret

4. 在 Service Account Roles 标签页添加角色：
   - realm-management → manage-users
   - realm-management → manage-realm

### 3. 创建 Web Client

1. 进入 Clients → Create Client
2. 配置：
   - Client ID: `abp-web-client`
   - Client Protocol: `openid-connect`
   - Access Type: `confidential`
   - Standard Flow Enabled: `ON`
   - Valid Redirect URIs: `https://app.example.com/*`
   - Web Origins: `https://app.example.com`

3. 配置 Client Scopes（**Keycloak 26+ 重要**）：
   - 进入 Client Scopes 标签页
   - 确保 Default Client Scopes 包含：
     - `basic` ✅ 必需：包含 sub (subject) claim
     - `openid` ✅ 必需：OIDC 协议要求
     - `profile` ✅ 推荐
     - `email` ✅ 推荐
     - `roles` ✅ 推荐
     - `web-origins` ✅ 推荐
     - `acr` ✅ 推荐
   - Optional Client Scopes 可包含：
     - `offline_access` ⚪ 如需 refresh token
     - `address` ⚪ 地址信息
     - `phone` ⚪ 电话信息

### 4. 配置角色映射

1. 进入 Client Scopes → roles → Mappers
2. 创建新 Mapper：
   - Name: `realm roles`
   - Mapper Type: `User Realm Role`
   - Token Claim Name: `roles`
   - Claim JSON Type: `String`
   - Add to ID token: `ON`
   - Add to access token: `ON`
   - Add to userinfo: `ON`

### 5. 验证 Client Scopes 配置

可以通过获取 token 来验证 scopes 是否配置正确：

```bash
# 获取 access token
TOKEN=$(curl -X POST "http://localhost:8080/realms/my-realm/protocol/openid-connect/token" \
  -d "client_id=abp-web-client" \
  -d "client_secret=your-secret" \
  -d "grant_type=password" \
  -d "username=admin" \
  -d "password=admin" \
  -d "scope=openid profile email" | jq -r '.access_token')

# 解码 token 检查 claims
echo $TOKEN | cut -d'.' -f2 | base64 -d | jq .
```

应该看到：
```json
{
  "sub": "user-id",           // 来自 basic scope
  "auth_time": 1234567890,     // 来自 basic scope
  "scope": "openid profile email",
  "email": "user@example.com",
  "name": "User Name",
  "preferred_username": "username",
  "roles": ["admin"],          // 来自 roles scope
  "allowed-origins": ["..."]   // 来自 web-origins scope
}
```

## 故障排查

### 常见问题

#### 1. 连接 Keycloak 失败

```bash
# 检查 Keycloak 是否可访问
curl http://localhost:8080/realms/my-realm/.well-known/openid-configuration

# 检查网络连接
ping keycloak.example.com
```

#### 2. 认证失败

检查日志：
```bash
# 查看 ABP 日志
tail -f logs/app.log | grep "Keycloak"

# 查看 Keycloak 日志
docker logs keycloak -f
```

#### 3. 用户同步失败

检查后台作业：
```sql
-- 查询失败的后台作业
SELECT * FROM AbpBackgroundJobs 
WHERE JobName LIKE '%Keycloak%' 
AND IsAbandoned = 1
ORDER BY CreationTime DESC
```

## 性能优化

### 1. 连接池配置

```json
{
  "Keycloak": {
    "HttpClient": {
      "MaxConnectionsPerServer": 100,
      "PooledConnectionLifetime": "00:05:00",
      "PooledConnectionIdleTimeout": "00:02:00"
    }
  }
}
```

### 2. 缓存配置

```csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    Configure<AbpDistributedCacheOptions>(options =>
    {
        options.KeyPrefix = "MyApp:";
        options.GlobalCacheEntryOptions.SlidingExpiration = TimeSpan.FromMinutes(20);
    });
}
```

### 3. 后台作业配置

```json
{
  "BackgroundJobs": {
    "IsJobExecutionEnabled": true,
    "CleanupInterval": "01:00:00",
    "MaxJobFetchCount": 10
  }
}
```

## 安全最佳实践

### 1. 使用 HTTPS

```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://*:443",
        "Certificate": {
          "Path": "/path/to/cert.pfx",
          "Password": "cert-password"
        }
      }
    }
  }
}
```

### 2. 配置 CORS

```csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    Configure<AbpCorsOptions>(options =>
    {
        options.AddPolicy("MyPolicy", builder =>
        {
            builder
                .WithOrigins("https://app.example.com")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });
}
```

### 3. 密钥管理

不要在代码中硬编码密钥：

```csharp
// ❌ 错误
var clientSecret = "my-secret-key";

// ✅ 正确
var clientSecret = configuration["Keycloak:AdminClientSecret"];
```

使用环境变量或密钥管理服务（Azure Key Vault, AWS Secrets Manager 等）。
