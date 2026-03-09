# JcStack.Abp.AuditLogging 模块

## 目标

在 `modules/jcstack.abp.auditlogging` 下新建一个模块，基于 ABP 开源版 `Volo.Abp.AuditLogging` 已有的 Domain 层实体（`AuditLog`、`AuditLogAction`、`EntityChange`、`EntityPropertyChange`）和 `IAuditLogRepository`，提供只读查询 API，替代 ABP Pro 的 `Volo.Abp.AuditLogging.Application` 模块。
不新建数据库表，直接复用 ABP 已有的 `AbpAuditLogs`、`AbpAuditLogActions`、`AbpEntityChanges`、`AbpEntityPropertyChanges` 四张表。

## 数据库表结构（已有，只读）

**AbpAuditLogs** — Id, ApplicationName, UserId, UserName, TenantId, TenantName, ImpersonatorUserId/UserName/TenantId/TenantName, ExecutionTime, ExecutionDuration, ClientIpAddress, ClientName, ClientId, CorrelationId, BrowserInfo, HttpMethod, Url, Exceptions, Comments, HttpStatusCode, ExtraProperties, ConcurrencyStamp
**AbpAuditLogActions** — Id, TenantId, AuditLogId(FK), ServiceName, MethodName, Parameters, ExecutionTime, ExecutionDuration, ExtraProperties
**AbpEntityChanges** — Id, AuditLogId(FK), TenantId, ChangeTime, ChangeType, EntityTenantId, EntityId, EntityTypeFullName, ExtraProperties
**AbpEntityPropertyChanges** — Id, TenantId, EntityChangeId(FK), NewValue, OriginalValue, PropertyName, PropertyTypeFullName

## 模板生成步骤

因 ABP 非 Pro 版本在已有解决方案内执行 `new-module` 会受许可限制，采用以下方式：
0. 从 `main` 分支拉取新分支：`git checkout -b feat/auditlogging-module`

1. 在系统临时目录创建模块：`abp new-module JcStack.Abp.AuditLogging` （在 `/tmp/abp-auditlogging-gen/` 下执行）
2. 将生成的 `JcStack.Abp.AuditLogging/` 目录复制到 `modules/jcstack.abp.auditlogging/`
3. 删除不需要的文件（test 目录、默认 .sln 等），保留 src/ 下的 8 个项目
4. 用 `jcstack.abp.identity` 的 `common.props` 作为模板，替换生成的 common.props（统一 Fody 配置等）
5. 逐层修改各项目的 csproj 和模块代码，使其依赖 `Volo.Abp.AuditLogging.`* 开源版 NuGet 包

## 模块结构（ABP CLI 生成后调整）

命名空间 `JcStack.Abp.AuditLogging`。
modules/jcstack.abp.auditlogging/
├── common.props
├── JcStack.Abp.AuditLogging.slnx
└── src/
    ├── JcStack.Abp.AuditLogging.Domain.Shared/
    ├── JcStack.Abp.AuditLogging.Domain/
    ├── JcStack.Abp.AuditLogging.Application.Contracts/
    ├── JcStack.Abp.AuditLogging.Application/
    ├── JcStack.Abp.AuditLogging.EntityFrameworkCore/
    ├── JcStack.Abp.AuditLogging.HttpApi/
    ├── JcStack.Abp.AuditLogging.HttpApi.Client/
    └── JcStack.Abp.AuditLogging.Installer/

## 各层内容

### Domain.Shared

- `JcStackAbpAuditLoggingRemoteServiceConsts` — RemoteServiceName / ModuleName
- `Localization/JcStackAbpAuditLoggingResource.cs`
- `Localization/JcStackAbpAuditLogging/en.json` + `zh-Hans.json`
- `JcStackAbpAuditLoggingDomainSharedModule` — 注册虚拟文件系统和本地化资源
- DependsOn: `AbpAuditLoggingDomainSharedModule`

### Domain

- 无新实体，直接依赖 `Volo.Abp.AuditLogging.Domain`（复用 `IAuditLogRepository`）
- `JcStackAbpAuditLoggingDomainModule`
- DependsOn: `AbpAuditLoggingDomainModule`, `JcStackAbpAuditLoggingDomainSharedModule`

### Application.Contracts

**DTO：**

- `AuditLogDto` — 映射 AuditLog 全字段 + `List<AuditLogActionDto> Actions` + `List<EntityChangeDto> EntityChanges`
- `AuditLogActionDto` — ServiceName, MethodName, Parameters, ExecutionTime, ExecutionDuration
- `EntityChangeDto` — ChangeTime, ChangeType, EntityId, EntityTypeFullName + `List<EntityPropertyChangeDto> PropertyChanges`
- `EntityPropertyChangeDto` — PropertyName, PropertyTypeFullName, OriginalValue, NewValue
- `GetAuditLogsInput : PagedAndSortedResultRequestDto` — StartTime?, EndTime?, UserName?, HttpMethod?, Url?, HttpStatusCode?, HasException(bool?), CorrelationId?
- `GetEntityChangesInput : PagedAndSortedResultRequestDto` — StartTime?, EndTime?, EntityTypeFullName?, EntityId?, ChangeType?
- `GetEntityChangeWithUsernameInput` — EntityChangeId
**服务接口：**
- `IAuditLogAppService : IApplicationService`
  - `Task<PagedResultDto<AuditLogDto>> GetListAsync(GetAuditLogsInput input)`
  - `Task<AuditLogDto> GetAsync(Guid id)`
  - `Task<PagedResultDto<EntityChangeDto>> GetEntityChangesAsync(GetEntityChangesInput input)`
  - `Task<EntityChangeDto> GetEntityChangeAsync(Guid entityChangeId)`
  - `Task<List<EntityChangeDto>> GetEntityChangesWithUsernameAsync(GetEntityChangeWithUsernameInput input)`
  **权限：**
- `JcStackAbpAuditLoggingPermissions`
  - `GroupName = "JcStackAbpAuditLogging"`
  - `AuditLogs.Default` — 查看审计日志
  - `AuditLogs.EntityChanges` — 查看实体变更
- `JcStackAbpAuditLoggingPermissionDefinitionProvider` — 注册上述权限
DependsOn: `AbpAuditLoggingDomainSharedModule`, `JcStackAbpAuditLoggingDomainSharedModule`

### Application

- `AuditLogAppService` 继承 `JcStackAbpAuditLoggingAppServiceBase`
  - 注入 `IAuditLogRepository`（ABP 开源版已提供此仓储接口）
  - `GetListAsync` — 调用 `IAuditLogRepository.GetListAsync` + `GetCountAsync` 组装分页
  - `GetAsync` — 调用 `IAuditLogRepository.GetAsync(id)` 含 Actions/EntityChanges/PropertyChanges
  - `GetEntityChangesAsync` — 调用仓储查询 EntityChange 分页
  - `GetEntityChangeAsync` — 按 EntityChangeId 查询含 PropertyChanges
- `JcStackAbpAuditLoggingAppServiceBase` — 设置 LocalizationResource, ObjectMapperContext
- `JcStackAbpAuditLoggingApplicationModule` — 注册 Mapperly
- Mapperly mapper 或手动 DTO 映射
DependsOn: `AbpAuditLoggingDomainModule`, `AbpMapperlyModule`, `JcStackAbpAuditLoggingDomainModule`, `JcStackAbpAuditLoggingApplicationContractsModule`

### EntityFrameworkCore

- 无自定义 DbContext（直接依赖 ABP 的 AuditLogging EF Core 模块和已有的 `IAuditLogRepository` EF Core 实现）
- `JcStackAbpAuditLoggingEntityFrameworkCoreModule`
- DependsOn: `AbpAuditLoggingEntityFrameworkCoreModule`, `JcStackAbpAuditLoggingDomainModule`

### HttpApi

- `AuditLogController : JcStackAbpAuditLoggingControllerBase, IAuditLogAppService`
  - `[Route("api/audit-logging/audit-logs")]`
  - `[RemoteService(Name = RemoteServiceName)]`
  - `[Area(ModuleName)]`
  - GET `/` — GetListAsync
  - GET `/{id}` — GetAsync
  - GET `/entity-changes` — GetEntityChangesAsync
  - GET `/entity-changes/{entityChangeId}` — GetEntityChangeAsync
  - GET `/entity-changes-with-username` — GetEntityChangesWithUsernameAsync
- `JcStackAbpAuditLoggingControllerBase` — 设置 LocalizationResource
- `JcStackAbpAuditLoggingHttpApiModule`
DependsOn: `JcStackAbpAuditLoggingApplicationContractsModule`

### HttpApi.Client

- `JcStackAbpAuditLoggingHttpApiClientModule` — 注册 HttpClientProxies
DependsOn: `AbpHttpClientModule`, `JcStackAbpAuditLoggingApplicationContractsModule`

### Installer

- `JcStackAbpAuditLoggingInstallerModule` — 聚合 Application + EFCore + HttpApi

## 主项目集成

### csproj 引用（在对应的 src/ 项目中添加）

- `Asms.Domain.Shared.csproj` → `JcStack.Abp.AuditLogging.Domain.Shared`
- `Asms.Domain.csproj` → `JcStack.Abp.AuditLogging.Domain`
- `Asms.Application.Contracts.csproj` → `JcStack.Abp.AuditLogging.Application.Contracts`
- `Asms.Application.csproj` → `JcStack.Abp.AuditLogging.Application`
- `Asms.EntityFrameworkCore.csproj` → `JcStack.Abp.AuditLogging.EntityFrameworkCore`
- `Asms.HttpApi.csproj` → `JcStack.Abp.AuditLogging.HttpApi`

### Module DependsOn 注册

- `AsmsDomainSharedModule` 添加 `JcStackAbpAuditLoggingDomainSharedModule`
- `AsmsDomainModule` 添加 `JcStackAbpAuditLoggingDomainModule`
- `AsmsApplicationContractsModule` 添加 `JcStackAbpAuditLoggingApplicationContractsModule`
- `AsmsApplicationModule` 添加 `JcStackAbpAuditLoggingApplicationModule`
- `AsmsEntityFrameworkCoreModule` 添加 `JcStackAbpAuditLoggingEntityFrameworkCoreModule`
- `AsmsHttpApiModule` 添加 `JcStackAbpAuditLoggingHttpApiModule`

### Swagger 分组

AuditLog Controller 的 API 归属 `abp` 分组（和其他框架 API 一致），无需修改 Swagger 配置。

## 关键决策

1. **只读模块** — 不提供 Create/Update/Delete API，审计日志由 ABP 框架自动写入
2. **不新增数据库表** — 复用 ABP 已有的 AuditLogging 表结构，无需 migration
3. **依赖开源版** — 所有 NuGet 包均使用 `Volo.Abp.AuditLogging.`* (10.0.2)，不依赖 Pro 包
4. **权限控制** — 审计日志查看属于敏感操作，通过独立权限组 `JcStackAbpAuditLogging` 控制

