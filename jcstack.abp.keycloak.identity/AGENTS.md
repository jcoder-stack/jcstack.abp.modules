# AGENTS.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Project Overview

JcStack.Abp.Keycloak.Identity 是一个 **ABP Framework 模块**，用于 ABP 应用与 Keycloak SSO 集成。核心功能：
- 多业务系统（MES、SRM 等）共享 Keycloak SSO
- 并发安全的用户创建和同步
- 用户属性映射（`app_user_id_{System}`、`tenant_id_{System}`）
- 中间件实现 JWT sub 到应用用户 ID 的转换

## Build & Test Commands

```bash
# Build solution
dotnet build

# Run all tests
dotnet test

# Run specific test project
dotnet test test/JcStack.Abp.Keycloak.Identity.Application.Tests/JcStack.Abp.Keycloak.Identity.Application.Tests.csproj
dotnet test test/JcStack.Abp.Keycloak.Identity.Domain.Tests/JcStack.Abp.Keycloak.Identity.Domain.Tests.csproj

# Run single test
dotnet test --filter "FullyQualifiedName~TestMethodName"
```

## Project Structure

```
src/
├── JcStack.Abp.Keycloak.Identity.Domain.Shared      # 共享常量、枚举
├── JcStack.Abp.Keycloak.Identity.Domain             # 领域层：配置选项、实体
├── JcStack.Abp.Keycloak.Identity.Application.Contracts  # 应用层契约：DTO、接口
├── JcStack.Abp.Keycloak.Identity.Application        # 应用层实现
├── JcStack.Abp.Keycloak.Identity.Keycloak           # Keycloak 集成核心
│   ├── Keycloak/                        # Admin API 客户端、用户/角色服务
│   ├── Identity/                        # 用户种子数据
│   └── Middleware/                      # 用户 ID 解析中间件
├── JcStack.Abp.Keycloak.Identity.Auth               # JWT 认证配置
├── JcStack.Abp.Keycloak.Identity.HttpApi            # HTTP API 控制器
├── JcStack.Abp.Keycloak.Identity.HttpApi.Client     # 远程调用客户端
├── JcStack.Abp.Keycloak.Identity.EntityFrameworkCore # EF Core 配置
└── JcStack.Abp.Keycloak.Identity.Installer          # 模块安装器
```

## Key Components

### User ID Mapping Architecture (v2.0+)

系统使用 Keycloak 用户属性实现多系统 ID 映射：
- `app_user_id_{SourceSystem}` - 存储各系统的用户 ID
- `tenant_id_{SourceSystem}` - 存储各系统的租户 ID

**新用户**：Keycloak ID = ABP User ID（无需转换）
**存量用户**：中间件通过属性查找进行 ID 转换

### Core Services

| Service | Location | Purpose |
|---------|----------|---------|
| `JcStackAbpKeycloakIdentityUserAppService` | Application | 用户 CRUD，先创建 Keycloak 用户 |
| `KeycloakUserService` | Keycloak | 并发安全的用户操作、属性管理 |
| `KeycloakAdminClient` | Keycloak | HTTP 客户端封装 |
| `KeycloakUserIdResolver` | Middleware | JWT sub → app_user_id 解析 |
| `KeycloakUserIdMiddleware` | Middleware | 认证后 ID 转换 |

### Configuration

`JcStackAbpKeycloakIdentityOptions` (Domain) 配置项：
- `ServerUrl`, `Realm`, `AdminClientId`, `AdminClientSecret` - 连接配置
- `EnableUserSync`, `EnableRoleSync` - 同步开关
- `Attributes.AppUserIdPrefix`, `Attributes.TenantIdPrefix` - 属性前缀
- `UserIdResolution.SourceSystem` - 当前系统标识

## ABP Module Dependencies

模块依赖链（按层级）：
```
Domain.Shared → Domain → Application.Contracts → Application
                  ↓
              Keycloak → Middleware
                  ↓
              EntityFrameworkCore
```

## Code Patterns

### Type Ambiguity
`IdentityUser` 在 ABP 和 ASP.NET Core Identity 中都存在。需要时使用：
```csharp
using IdentityUser = Volo.Abp.Identity.IdentityUser;
// 或完全限定名
Volo.Abp.Identity.IdentityUser user
```

### Keycloak Admin API
所有 Keycloak 操作使用 `KeycloakAdminClient`，支持重试和并发处理：
```csharp
var result = await _keycloakUserService.CreateOrGetUserAsync(
    username, email, appUserId, password, firstName, lastName, tenantId);
```

### ABP Patterns
- 使用 `[ExposeServices]` 替换默认服务实现
- 使用 `IOptions<T>` 注入配置
- 后台作业使用 ABP `IBackgroundJob<T>`

## Version Info

- .NET: 10.0
- ABP Framework: 10.0.2
- Keycloak: 26.0+ (recommended)
