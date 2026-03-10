# 更新日志

本文档记录 JcStack.Abp.Keycloak.Identity 模块的所有重要变更。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.0.0/)，
版本号遵循 [语义化版本](https://semver.org/lang/zh-CN/)。

## [未发布]

### 新增
- 初始版本发布
- 核心功能实现完成
- 文档编写完成

## [0.2.0] - 2026-01-23

### 新增
- **Client Scopes 配置支持 (Keycloak 26+)**
  - 添加 `KeycloakClientDataSeedContributor` 自动注册 OIDC 客户端
  - 支持通过 `appsettings.json` 配置客户端 scopes
  - 支持 `DefaultClientScopes` 和 `OptionalClientScopes` 配置
  - 支持 `FullScopeAllowed` 配置（遵循最小权限原则）

### 变更
- **更新默认 Client Scopes 配置**
  - 添加 `basic` scope（包含 `sub` 和 `auth_time` claims）
  - 添加 `openid` scope（OIDC 规范要求）
  - 添加 `acr` scope（Authentication Context Class Reference）
  - 保留 `profile`, `email`, `roles`, `web-origins` scopes
  - 默认配置从 `["profile", "email", "roles", "web-origins"]` 更新为  
    `["basic", "openid", "profile", "email", "roles", "web-origins", "acr"]`

### 文档
- 更新 `README.md`
  - 添加 "Client Scopes 配置说明" 章节
  - 详细说明 Keycloak 26+ 默认 scopes
  - 解释 `basic` 和 `openid` scope 的重要性
  - 添加 scope 在 token 中的表现说明
  - 添加客户端数据种子配置示例
- 更新 `docs/configuration-examples.md`
  - 添加专门的 "Client Scopes 配置" 章节
  - 详细说明为什么需要正确配置
  - 提供标准配置模板
  - 列出常见错误配置
  - 添加验证配置的方法
  - 在 Keycloak 客户端配置章节中添加 Client Scopes 配置步骤
- 更新版本兼容性说明
  - Keycloak 26.0+ (推荐)
  - Keycloak 25.0+ (支持 `basic` scope)
  - Keycloak 20.0-24.x (需要手动配置)

### 修复
- 修复 Keycloak 26+ 中缺少 `basic` scope 导致 token 没有 `sub` claim 的问题
- 修复缺少 `openid` scope 导致 `/userinfo` 端点返回 403 的问题
- 修复与 OIDC 规范不兼容的问题

### 技术细节
- 基于 DeepWiki MCP 查询 Keycloak 仓库获得的权威信息
- `basic` scope 是 Keycloak 25.0.0+ 引入的新 scope
- `basic` scope 的 `include.in.token.scope` 设置为 `false`，不会出现在 token 的 `scope` claim 中
- Keycloak 26 的标准默认 scopes 包括：`web-origins`, `acr`, `profile`, `roles`, `basic`, `email`

### 重要提示
⚠️ 如果你正在使用 Keycloak 26+，必须更新你的 `DefaultClientScopes` 配置以包含 `basic` 和 `openid` scopes。否则会导致认证失败。

## [0.1.0] - 2026-01-17

### 新增

#### 领域层
- 创建 `JcStackAbpKeycloakIdentityOptions` 配置类，支持完整的 Keycloak 连接和同步配置
- 实现领域事件系统:
  - `UserCreatedEto` - 用户创建事件
  - `UserUpdatedEto` - 用户更新事件
  - `UserDeletedEto` - 用户删除事件
  - `RoleAssignedEto` - 角色分配事件
  - `RoleRemovedEto` - 角色移除事件
- 实现 `KeycloakUserManager` 透明代理:
  - 继承 `IdentityUserManager`
  - 覆盖 `CreateAsync`、`UpdateAsync`、`DeleteAsync` 方法
  - 覆盖 `AddToRoleAsync`、`RemoveFromRoleAsync` 方法
  - 确保 ABP 操作优先执行，Keycloak 同步不阻塞主流程

#### 应用层
- 集成 `Keycloak.AuthServices.Sdk` v2.8.0
- 实现 Keycloak 用户服务:
  - `IKeycloakUserService` 接口
  - `KeycloakUserService` 实现
  - 支持用户创建、更新、删除
  - 唯一性检查和冲突处理
- 实现 Keycloak 角色服务:
  - `IKeycloakRoleService` 接口
  - `KeycloakRoleService` 实现
  - 支持角色创建、分配、移除
  - 角色名称前缀支持
- 实现后台作业系统:
  - `UserCreatedSyncJob` - 用户创建同步作业
  - `UserUpdatedSyncJob` - 用户更新同步作业
  - `UserDeletedSyncJob` - 用户删除同步作业
  - `RoleAssignedSyncJob` - 角色分配同步作业
  - `RoleRemovedSyncJob` - 角色移除同步作业
- 实现事件处理器:
  - `UserCreatedEventHandler`
  - `UserUpdatedEventHandler`
  - `UserDeletedEventHandler`
  - `RoleAssignedEventHandler`
  - `RoleRemovedEventHandler`
- 实现智能重试策略:
  - `IKeycloakRetryPolicy` 接口
  - `KeycloakRetryPolicy` 实现
  - 指数退避算法
  - 区分临时性错误和永久性错误
  - HTTP 409 冲突自动处理

#### 认证层
- 创建 `JcStack.Abp.Keycloak.Identity.Authentication` 模块
- 集成 `Keycloak.AuthServices.Authentication` v2.8.0
- 集成 `Keycloak.AuthServices.Authorization` v2.8.0
- 实现 `KeycloakAuthenticationModule`:
  - JWT Bearer 认证配置（Web API）
  - OpenID Connect 认证配置（Web 应用）
  - Cookie 认证配置
  - 授权策略配置
  - 认证失败事件处理

#### 文档
- 创建 `README.md` - 主文档
  - 模块概述和特性
  - 安装指南
  - 配置选项详解
  - 工作原理说明
  - 错误处理文档
  - 属性映射表
  - 日志记录示例
  - 监控和故障排查
  - 性能考虑
  - 安全最佳实践
- 创建 `QUICK_START.md` - 快速开始指南
  - 5 分钟快速集成步骤
  - Keycloak 配置指南
  - 常见问题解答
- 创建 `docs/configuration-examples.md` - 配置示例
  - 基本配置
  - Web API 配置
  - Web 应用配置
  - 多租户配置
  - 生产环境配置
  - Docker Compose 示例
  - Keycloak 客户端配置
  - 故障排查
  - 性能优化
  - 安全最佳实践
- 创建 `docs/migration-guide.md` - 迁移指南
  - 从 OpenIddict 迁移步骤
  - 迁移影响分析
  - 准备工作清单
  - 数据迁移工具
  - 测试验证清单
  - 回滚计划
  - 常见问题解答
- 创建 `docs/integration-example.md` - 集成示例
  - 完整的 HttpApi.Host 集成示例
  - 替换 OpenIddict 详细步骤
  - Swagger 配置
  - 配置文件示例
  - 测试步骤
- 创建 `IMPLEMENTATION_STATUS.md` - 实施状态文档
- 创建 `CHANGELOG.md` - 更新日志

### 技术细节

#### 架构设计
- 采用透明代理模式，不影响现有 ABP Identity 功能
- 事件驱动架构，异步同步
- 最终一致性模型，ABP 是主数据源
- 智能错误处理和重试机制

#### 依赖项
- ABP Framework 10.0+
- .NET 10.0
- Keycloak.AuthServices.Sdk 2.8.0
- Keycloak.AuthServices.Authentication 2.8.0
- Keycloak.AuthServices.Authorization 2.8.0
- Volo.Abp.BackgroundJobs
- Volo.Abp.Identity.Domain
- Volo.Abp.AspNetCore.Authentication.OpenIdConnect

#### 编译状态
- ✅ 所有项目编译成功
- ✅ 无编译错误
- ⚠️ 1 个警告（Fody 配置文件自动生成，可忽略）

### 已知限制

- 测试套件尚未实现
- 批量迁移工具尚未实现
- 性能基准测试尚未完成

### 下一步计划

- 实现单元测试
- 实现集成测试
- 实现属性测试
- 创建批量迁移工具
- 性能优化和基准测试
- 在实际项目中验证

## [0.0.1] - 2026-01-17

### 新增
- 项目初始化
- 基础项目结构创建

---

## 版本说明

### 版本号格式

版本号格式: `主版本号.次版本号.修订号`

- **主版本号**: 不兼容的 API 变更
- **次版本号**: 向下兼容的功能新增
- **修订号**: 向下兼容的问题修正

### 变更类型

- **新增**: 新功能
- **变更**: 现有功能的变更
- **弃用**: 即将移除的功能
- **移除**: 已移除的功能
- **修复**: 问题修复
- **安全**: 安全相关的修复

---

## 贡献

欢迎贡献！请确保:
1. 遵循现有的代码风格
2. 添加适当的测试
3. 更新相关文档
4. 在 CHANGELOG.md 中记录变更

---

## 许可证

[待定]

---

## 联系方式

如有问题或建议，请联系：[待填写]
