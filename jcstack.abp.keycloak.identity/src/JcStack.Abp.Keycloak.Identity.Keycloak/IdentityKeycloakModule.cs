using System;
using System.Threading.Tasks;
using JcStack.Abp.Keycloak.Identity.Keycloak;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace JcStack.Abp.Keycloak.Identity;

/// <summary>
/// Keycloak 基础设施模块
/// 提供 Keycloak Admin API 客户端和数据种子贡献者
/// 
/// 职责：
/// 1. 注册 KeycloakAdminClient（HTTP 客户端）
/// 2. 注册 IKeycloakUserService（用户管理服务）
/// 3. 注册 DataSeedContributor（用户和客户端种子）
/// 4. 配置验证和连接测试
/// 
/// 依赖关系：
/// - JcStackAbpKeycloakIdentityDomainModule（领域模型和选项）
/// 
/// 被依赖：
/// - JcStackAbpKeycloakIdentityApplicationModule（应用层）
/// - JcStackAbpKeycloakIdentityDbMigratorModule（DbMigrator）
/// </summary>
[DependsOn(
    typeof(JcStackAbpKeycloakIdentityDomainModule)
)]
public class IdentityKeycloakModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        // 绑定 Keycloak 配置选项（确保在 AddKeycloakAdminClient 之前绑定）
        context.Services.Configure<JcStackAbpKeycloakIdentityOptions>(
            configuration.GetSection("Keycloak"));

        // 注册 Keycloak Admin API 客户端
        context.Services.AddKeycloakAdminClient(configuration);
    }

    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        var logger = context.ServiceProvider.GetRequiredService<ILogger<IdentityKeycloakModule>>();
        var options = context.ServiceProvider.GetRequiredService<IOptions<JcStackAbpKeycloakIdentityOptions>>().Value;

        // 验证配置
        if (!ValidateConfiguration(options, logger))
        {
            logger.LogWarning(
                "Keycloak configuration is incomplete. Keycloak integration features will be disabled.");
            return;
        }

        // 测试 Keycloak 连接
        await TestKeycloakConnectionAsync(context.ServiceProvider, logger);
    }

    private static bool ValidateConfiguration(JcStackAbpKeycloakIdentityOptions options, ILogger logger)
    {
        var errors = new System.Collections.Generic.List<string>();

        if (string.IsNullOrWhiteSpace(options.ServerUrl))
        {
            errors.Add("Keycloak:ServerUrl is required");
        }

        if (string.IsNullOrWhiteSpace(options.Realm))
        {
            errors.Add("Keycloak:Realm is required");
        }

        if (string.IsNullOrWhiteSpace(options.AdminClientId))
        {
            errors.Add("Keycloak:AdminClientId is required");
        }

        if (string.IsNullOrWhiteSpace(options.AdminClientSecret))
        {
            errors.Add("Keycloak:AdminClientSecret is required");
        }

        if (errors.Count > 0)
        {
            logger.LogWarning(
                "Keycloak configuration validation failed:\n- {Errors}",
                string.Join("\n- ", errors));
            return false;
        }

        logger.LogInformation(
            "Keycloak configuration validated. ServerUrl: {ServerUrl}, Realm: {Realm}",
            options.ServerUrl, options.Realm);

        return true;
    }

    private static async Task TestKeycloakConnectionAsync(IServiceProvider serviceProvider, ILogger logger)
    {
        try
        {
            var adminClient = serviceProvider.GetRequiredService<KeycloakAdminClient>();

            // 尝试获取用户列表来验证连接和凭据
            var users = await adminClient.GetUsersAsync(search: "admin", cancellationToken: default);

            logger.LogInformation(
                "Successfully connected to Keycloak. Found {UserCount} user(s) matching 'admin'",
                users?.Count ?? 0);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to connect to Keycloak. Data seeding may fail. " +
                "Please ensure Keycloak is running and credentials are correct.");
        }
    }
}
