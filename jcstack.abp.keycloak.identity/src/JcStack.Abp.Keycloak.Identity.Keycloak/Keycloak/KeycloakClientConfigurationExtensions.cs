using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JcStack.Abp.Keycloak.Identity.Keycloak;

/// <summary>
/// Keycloak 客户端配置扩展方法
/// 封装 Keycloak Admin API 客户端和 HttpClient 的注册逻辑
/// </summary>
public static class KeycloakClientConfigurationExtensions
{
    /// <summary>
    /// 添加 Keycloak Admin API 客户端配置
    /// 注册 KeycloakAdminClient 类型化 HttpClient
    /// 验证需求: 2.4, 11.1, 11.2
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置对象</param>
    public static IServiceCollection AddKeycloakAdminClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 注册类型化客户端 KeycloakAdminClient（用于自定义 Admin API 调用）
        services.AddHttpClient<KeycloakAdminClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<JcStackAbpKeycloakIdentityOptions>>().Value;

            // 如果 ServerUrl 未配置，使用占位符 URL（实际调用时会在 DataSeedContributor 中失败并记录日志）
            var serverUrl = string.IsNullOrWhiteSpace(options.ServerUrl)
                ? "http://localhost:8080"
                : options.ServerUrl;

            client.BaseAddress = new Uri(serverUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
