# JcStack.Abp.Keycloak.Identity 模块安装指南

## 模块架构

```
JcStack.Abp.Keycloak.Identity 模块
├── 可安装包（外部项目使用）
│   ├── JcStackAbpKeycloakIdentityApplicationModule（应用层，用于 HttpApi.Host）
│   └── JcStackAbpKeycloakIdentityDbMigratorModule（DbMigrator 专用）
│
└── 内部包（模块内部使用，不对外暴露）
    └── IdentityKeycloakModule（Keycloak 基础设施）
```

## 使用 ABP CLI 安装

### 1. 安装到 HttpApi.Host 项目

```bash
# 在解决方案根目录执行
abp install-local-module modules/jcstack.abp.keycloak.identity -t YourProject.abpmdl
```

ABP CLI 会自动将模块安装到正确的项目层：
- `JcStackAbpKeycloakIdentityApplicationModule` → `YourProject.Application`
- `JcStackAbpKeycloakIdentityHttpApiModule` → `YourProject.HttpApi`
- `IdentityAuthModule` → `YourProject.HttpApi.Host`

### 2. 安装到 DbMigrator 项目

DbMigrator 需要手动添加依赖：

#### 步骤 1：添加项目引用

编辑 `YourProject.DbMigrator.csproj`：

```xml
<ItemGroup>
  <ProjectReference Include="..\..\modules\jcstack.abp.keycloak.identity\src\JcStack.Abp.Keycloak.Identity.DbMigrator\JcStack.Abp.Keycloak.Identity.DbMigrator.csproj" />
</ItemGroup>
```

#### 步骤 2：添加模块依赖

编辑 `YourProject.DbMigrator\YourProjectDbMigratorModule.cs`：

```csharp
using JcStack.Abp.Keycloak.Identity;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(YourProjectEntityFrameworkCoreModule),
    typeof(YourProjectApplicationContractsModule),
    typeof(JcStackAbpKeycloakIdentityDbMigratorModule)  // 添加这一行
)]
public class YourProjectDbMigratorModule : AbpModule
{
}
```

#### 步骤 3：配置工作目录

确保 DbMigrator 从项目目录运行，编辑 `YourProject.DbMigrator.csproj`：

```xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <TargetFramework>net10.0</TargetFramework>
  <RunWorkingDirectory>$(MSBuildProjectDirectory)</RunWorkingDirectory>
</PropertyGroup>
```

## 配置

在 `appsettings.json` 中添加 Keycloak 配置：

```json
{
  "Keycloak": {
    "ServerUrl": "http://localhost:8080",
    "Realm": "your-realm",
    "AdminClientId": "your-admin-client",
    "AdminClientSecret": "your-admin-client-secret",
    "ClientId": "your-app-client",
    "SourceSystem": "YourSystem",
    "Clients": {
      "WebApp": {
        "ClientId": "your-web-app",
        "RootUrl": "http://localhost:3000",
        "RedirectUris": ["http://localhost:3000/*"],
        "WebOrigins": ["http://localhost:3000", "+"]
      }
    }
  }
}
```

## Keycloak 配置要求

### 创建 Admin Client

1. 登录 Keycloak Admin Console
2. 选择你的 Realm
3. 创建新的 Client（例如 `your-admin-client`）
4. 配置：
   - Client authentication: ON
   - Service accounts enabled: ON
5. 保存后，在 Credentials 标签页获取 Client Secret

### 分配 Admin 权限

1. 进入 Clients → `your-admin-client` → Service Account Roles
2. 点击 Assign role
3. 选择 Filter by clients → `realm-management`
4. 分配以下角色：
   - `manage-users`
   - `view-users`
   - `manage-clients`
   - `view-clients`
   - `manage-realm`
   - `view-realm`

## 功能说明

### JcStackAbpKeycloakIdentityApplicationModule（应用层）

- 包含完整的应用服务、AppService
- 自动包含 `IdentityKeycloakModule`（Keycloak 基础设施）
- 提供用户、角色同步功能
- 提供 Keycloak Admin API 客户端

**适用场景**：HttpApi.Host 项目

### JcStackAbpKeycloakIdentityDbMigratorModule（DbMigrator）

- 轻量级模块，只包含数据种子功能
- 自动包含 `IdentityKeycloakModule`
- 提供 `JcStackAbpKeycloakIdentityDataSeedContributor`（用户种子）
- 提供 `KeycloakClientDataSeedContributor`（客户端种子）
- 配置验证和连接测试

**适用场景**：DbMigrator 项目

**优势**：
- 不加载 AppService、Mapper、BackgroundJobs
- 更快的启动速度
- 更小的依赖树

## 数据种子行为

### 用户种子（JcStackAbpKeycloakIdentityDataSeedContributor）

1. 在 Keycloak 创建或获取用户
2. 使用 Keycloak User ID 作为本地 IdentityUser 的 ID
3. 确保 JWT token 中的 `sub` claim 与系统 userId 一致

### 客户端种子（KeycloakClientDataSeedContributor）

从配置文件（`Keycloak:Clients` 节点）读取客户端配置，自动注册到 Keycloak：
- 支持多个客户端
- 自动配置 RedirectUris、WebOrigins
- 启用 PKCE（S256）

## 运行 DbMigrator

```bash
# 从项目目录运行
cd src/YourProject.DbMigrator
dotnet run

# 或从根目录运行（需要配置 RunWorkingDirectory）
dotnet run --project src/YourProject.DbMigrator
```

## 故障排查

### 403 Forbidden 错误

**原因**：Admin Client 没有足够权限

**解决**：检查 Service Account Roles 配置（参见上文"分配 Admin 权限"）

### 配置未加载

**原因**：工作目录不正确

**解决**：在 csproj 中添加 `<RunWorkingDirectory>$(MSBuildProjectDirectory)</RunWorkingDirectory>`

### Connection refused

**原因**：Keycloak 未启动或 URL 配置错误

**解决**：
1. 检查 Keycloak 是否运行：`curl http://localhost:8080`
2. 检查 `appsettings.json` 中的 `Keycloak:ServerUrl`

## 更多信息

- [ABP Framework 文档](https://docs.abp.io)
- [Keycloak 文档](https://www.keycloak.org/documentation)
