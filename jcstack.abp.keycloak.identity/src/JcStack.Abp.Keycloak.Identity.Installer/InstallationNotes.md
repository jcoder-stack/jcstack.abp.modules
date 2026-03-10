# JcStack.Abp.Keycloak.Identity 安装说明

此模块包含以下包：

## 可安装包（通过 ABP CLI）

### 应用层模块（用于 HttpApi.Host）
- JcStack.Abp.Keycloak.Identity.Domain.Shared - 共享常量、枚举、本地化
- JcStack.Abp.Keycloak.Identity.Domain - 领域层（Options、NullIdentityDataSeeder）
- JcStack.Abp.Keycloak.Identity.Application.Contracts - 应用层契约
- JcStack.Abp.Keycloak.Identity.Application - 应用层实现（包含 Keycloak 基础设施）
- JcStack.Abp.Keycloak.Identity.EntityFrameworkCore - EF Core 集成
- JcStack.Abp.Keycloak.Identity.HttpApi - HTTP API 控制器
- JcStack.Abp.Keycloak.Identity.HttpApi.Client - HTTP 客户端代理
- JcStack.Abp.Keycloak.Identity.Auth - Keycloak JWT 认证集成

### DbMigrator 模块（用于 DbMigrator 项目）
- JcStack.Abp.Keycloak.Identity.DbMigrator - 轻量 DbMigrator 模块（包含数据种子功能）

## 内部包（不需要单独安装）
- JcStack.Abp.Keycloak.Identity.Keycloak - Keycloak Admin API 客户端和数据种子（内部基础设施）
  - 此模块被 `JcStackAbpKeycloakIdentityApplicationModule` 和 `JcStackAbpKeycloakIdentityDbMigratorModule` 内部引用
  - 不需要在外部项目中直接依赖

## 使用 ABP CLI 安装

在目标模块的根目录执行：

- 本地安装：`abp install-local-module <模块路径> -t <目标模块>.abpmdl`
- 远程安装：`abp install-module JcStack.Abp.Keycloak.Identity -t <目标模块>.abpmdl`

## 模块依赖配置

### HTTP API Host 项目
```csharp
[DependsOn(
    typeof(JcStackAbpKeycloakIdentityApplicationModule),    // 应用层
    typeof(JcStackAbpKeycloakIdentityHttpApiModule),        // HTTP API
    typeof(IdentityAuthModule)            // Keycloak JWT 认证
)]
public class YourHttpApiHostModule : AbpModule { }
```

### DbMigrator 项目
```csharp
[DependsOn(
    typeof(JcStackAbpKeycloakIdentityDbMigratorModule)      // 轻量 DbMigrator 模块
)]
public class YourDbMigratorModule : AbpModule { }
```

## 配置要求

在 `appsettings.json` 中添加 Keycloak 配置：

```json
{
  "Keycloak": {
    "ServerUrl": "http://localhost:8080",
    "Realm": "your-realm",
    "AdminClientId": "your-admin-client",
    "AdminClientSecret": "your-secret",
    "ClientId": "your-app-client",
    "SourceSystem": "YourSystem"
  }
}
```
