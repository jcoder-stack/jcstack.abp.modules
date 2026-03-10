# JcStack.Abp.Keycloak.Identity 快速开始指南

5 分钟快速集成 JcStack.Abp.Keycloak.Identity 模块到你的 ABP 应用。

## 前置条件

- ABP Framework 10.0+
- .NET 10.0+
- Keycloak 服务器（本地或远程）

## 步骤 1: 启动 Keycloak（如果还没有）

```bash
docker run -d \
  --name keycloak \
  -p 8080:8080 \
  -e KEYCLOAK_ADMIN=admin \
  -e KEYCLOAK_ADMIN_PASSWORD=admin \
  quay.io/keycloak/keycloak:23.0 \
  start-dev
```

访问 http://localhost:8080，登录 Admin Console (admin/admin)。

## 步骤 2: 配置 Keycloak

### 2.1 创建 Realm

1. 点击左上角下拉菜单
2. 点击 "Create Realm"
3. 输入名称: `abp-realm`
4. 点击 "Create"

### 2.2 创建 Admin Client

1. 进入 Clients → Create Client
2. 填写信息:
   - Client ID: `abp-admin-client`
   - Client Protocol: `openid-connect`
3. 点击 "Next"
4. 配置:
   - Client authentication: `ON`
   - Service accounts roles: `ON`
5. 点击 "Save"
6. 进入 "Credentials" 标签页，复制 Client Secret

### 2.3 配置权限

1. 进入 "Service account roles" 标签页
2. 点击 "Assign role"
3. 选择 "Filter by clients"
4. 添加以下角色:
   - `realm-management` → `manage-users`
   - `realm-management` → `manage-realm`

## 步骤 3: 添加模块到项目

### 3.1 添加项目引用

在 `Keycloak.Abp.HttpApi.Host.csproj` 中添加:

```xml
<ItemGroup>
  <ProjectReference Include="..\..\modules\jcstack.abp.keycloak.identity\src\JcStack.Abp.Keycloak.Identity.Domain\JcStack.Abp.Keycloak.Identity.Domain.csproj" />
  <ProjectReference Include="..\..\modules\jcstack.abp.keycloak.identity\src\JcStack.Abp.Keycloak.Identity.Application\JcStack.Abp.Keycloak.Identity.Application.csproj" />
  <ProjectReference Include="..\..\modules\jcstack.abp.keycloak.identity\src\JcStack.Abp.Keycloak.Identity.Authentication\JcStack.Abp.Keycloak.Identity.Authentication.csproj" />
</ItemGroup>
```

### 3.2 更新模块依赖

在 `AbpHttpApiHostModule.cs` 中:

```csharp
using JcStack.Abp.Keycloak.Identity.Authentication;
using JcStack.Abp.Keycloak.Identity.Application;
using JcStack.Abp.Keycloak.Identity.Domain;

[DependsOn(
    // ... 其他依赖
    typeof(JcStackAbpKeycloakIdentityDomainModule),
    typeof(JcStackAbpKeycloakIdentityApplicationModule),
    typeof(KeycloakAuthenticationModule),
    // ... 其他依赖
)]
public class AbpHttpApiHostModule : AbpModule
{
    // ...
}
```

## 步骤 4: 配置 appsettings.json

在 `appsettings.json` 中添加:

```json
{
  "Keycloak": {
    "ServerUrl": "http://localhost:8080",
    "Realm": "abp-realm",
    "AdminClientId": "abp-admin-client",
    "AdminClientSecret": "粘贴你的 Client Secret",
    "EnableUserSync": true,
    "EnableRoleSync": true,
    "SyncUserDeletionToKeycloak": true,
    "RolePrefix": "ABP",
    "SourceSystem": "ABP",
    "RetryPolicy": {
      "MaxRetryAttempts": 3,
      "InitialBackoffSeconds": 2,
      "MaxBackoffSeconds": 30
    }
  }
}
```

## 步骤 5: 运行应用

```bash
cd src/Keycloak.Abp.HttpApi.Host
dotnet run
```

## 步骤 6: 测试

### 6.1 创建用户

使用 ABP 的用户管理界面或 API 创建一个用户。

### 6.2 检查 Keycloak

1. 访问 Keycloak Admin Console
2. 进入 Users
3. 你应该看到刚创建的用户已经同步过来了！

### 6.3 查看日志

```bash
# 查看同步日志
tail -f Logs/logs.txt | grep "Keycloak"
```

你应该看到类似这样的日志:

```
[INF] Created user testuser in Keycloak with ID abc-123-def
[INF] User sync job completed successfully for user testuser
```

## 完成！🎉

你的 ABP 应用现在已经集成了 Keycloak！

## 下一步

### 替换 OpenIddict（可选）

如果你想完全替换 OpenIddict，查看 [集成示例文档](./docs/integration-example.md)。

### 配置生产环境

查看 [配置示例文档](./docs/configuration-examples.md) 了解生产环境配置。

### 迁移现有用户

查看 [迁移指南](./docs/migration-guide.md) 了解如何批量迁移现有用户。

## 常见问题

### Q: 用户创建成功但 Keycloak 中没有？

**A**: 检查以下几点:
1. Keycloak 服务器是否可访问
2. Client Secret 是否正确
3. 查看日志中的错误信息
4. 确认后台作业服务已启动

### Q: 如何禁用同步？

**A**: 在 `appsettings.json` 中设置:

```json
{
  "Keycloak": {
    "EnableUserSync": false,
    "EnableRoleSync": false
  }
}
```

### Q: 如何只同步特定租户的用户？

**A**: 可以在配置中为不同租户设置不同的 `SourceSystem` 标识，然后在 Keycloak 中根据这个属性进行过滤。

### Q: 同步失败会影响 ABP 操作吗？

**A**: 不会！这是模块的核心设计原则。ABP 操作总是先执行，Keycloak 同步是异步的，失败不会影响 ABP。

## 获取帮助

- 📖 [完整文档](./README.md)
- 🔧 [配置示例](./docs/configuration-examples.md)
- 🚀 [集成示例](./docs/integration-example.md)
- 📋 [迁移指南](./docs/migration-guide.md)

## 反馈

如有问题或建议，请联系：[待填写]
