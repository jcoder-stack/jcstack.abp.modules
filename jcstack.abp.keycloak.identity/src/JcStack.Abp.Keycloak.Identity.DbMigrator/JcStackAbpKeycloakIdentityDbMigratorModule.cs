using Volo.Abp.Modularity;

namespace JcStack.Abp.Keycloak.Identity;

/// <summary>
/// Keycloak Identity DbMigrator 模块
/// 专门用于 DbMigrator 场景的轻量模块
/// 
/// 职责：
/// 1. 提供 Keycloak Admin 客户端（通过 IdentityKeycloakModule）
/// 2. 提供数据种子贡献者（JcStackAbpKeycloakIdentityDataSeedContributor、KeycloakClientDataSeedContributor）
/// 3. 配置验证和连接测试
/// 
/// 不包含：
/// - Application Service
/// - Object Mapper
/// - Background Jobs
/// 
/// 依赖关系：
/// - IdentityKeycloakModule：Keycloak 基础设施
/// 
/// 使用场景：
/// - Asms.DbMigrator 项目依赖此模块
/// </summary>
[DependsOn(
    typeof(IdentityKeycloakModule)
)]
public class JcStackAbpKeycloakIdentityDbMigratorModule : AbpModule
{
}
