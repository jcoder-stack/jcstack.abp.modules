using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Security.Claims;

namespace JcStack.Abp.Keycloak.Identity.Auth;

/// <summary>
/// Keycloak 认证扩展方法
/// </summary>
public static class KeycloakAuthenticationExtensions
{
    /// <summary>
    /// 添加 Keycloak 认证
    /// 从配置文件读取默认配置，支持自定义覆盖
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <param name="configureOptions">自定义配置 (可选)</param>
    /// <returns></returns>
    public static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<KeycloakAuthOptions>? configureOptions = null)
    {
        return services.AddKeycloakAuthentication<KeycloakJwtBearerEvents>(configuration, configureOptions);
    }

    /// <summary>
    /// 添加 Keycloak 认证（支持自定义事件处理器类型）
    /// </summary>
    /// <typeparam name="TEvents">自定义 JWT Bearer Events 类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <param name="configureOptions">自定义配置 (可选)</param>
    /// <returns></returns>
    public static IServiceCollection AddKeycloakAuthentication<TEvents>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<KeycloakAuthOptions>? configureOptions = null)
        where TEvents : KeycloakJwtBearerEvents
    {
        // 1. 配置选项
        var options = new KeycloakAuthOptions();

        // 从配置文件绑定 (支持 AuthServer 和 Keycloak:Auth 两种配置节)
        var authServerSection = configuration.GetSection("AuthServer");
        if (authServerSection.Exists())
        {
            options.Authority = authServerSection["Authority"] ?? options.Authority;
            options.Audience = authServerSection["Audience"] ?? options.Audience;
            options.RequireHttpsMetadata = authServerSection.GetValue("RequireHttpsMetadata", options.RequireHttpsMetadata);
            options.ValidateAudience = authServerSection.GetValue("ValidateAudience", options.ValidateAudience);
            
            var validAudiences = authServerSection.GetSection("ValidAudiences").Get<string[]>();
            if (validAudiences?.Length > 0)
            {
                options.ValidAudiences = validAudiences;
            }
        }

        var keycloakAuthSection = configuration.GetSection("Keycloak:Auth");
        if (keycloakAuthSection.Exists())
        {
            keycloakAuthSection.Bind(options);
        }

        // 应用自定义配置
        configureOptions?.Invoke(options);

        // 注册配置到 DI
        services.Configure<KeycloakAuthOptions>(opt =>
        {
            opt.Authority = options.Authority;
            opt.Audience = options.Audience;
            opt.ValidAudiences = options.ValidAudiences;
            opt.ValidateAudience = options.ValidateAudience;
            opt.RequireHttpsMetadata = options.RequireHttpsMetadata;
            opt.AutoCreateUser = options.AutoCreateUser;
            opt.SyncUserInfoOnLogin = options.SyncUserInfoOnLogin;
            opt.EnableDynamicClaims = options.EnableDynamicClaims;
            opt.ClaimMappings = options.ClaimMappings;
            opt.CustomClaimsToInclude = options.CustomClaimsToInclude;
            opt.SignalRHubPath = options.SignalRHubPath;
        });

        // 2. 注册事件处理器
        services.AddScoped<TEvents>();

        // 3. 配置 JWT Bearer 认证
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwtOptions =>
            {
                jwtOptions.Authority = options.Authority;
                jwtOptions.RequireHttpsMetadata = options.RequireHttpsMetadata;
                
                // Audience 配置：留空则使用 issuer URL
                jwtOptions.Audience = string.IsNullOrWhiteSpace(options.Audience) 
                    ? options.Authority 
                    : options.Audience;

                // ValidAudiences：留空则使用 issuer URL
                jwtOptions.TokenValidationParameters.ValidateAudience = options.ValidateAudience;
                jwtOptions.TokenValidationParameters.ValidAudiences = options.ValidAudiences.Length > 0 
                    ? options.ValidAudiences 
                    : [options.Authority];

                // 注册事件处理器类型
                jwtOptions.EventsType = typeof(TEvents);
            });

        // 4. 配置 ABP Claims Principal Factory
        if (options.EnableDynamicClaims)
        {
            services.Configure<AbpClaimsPrincipalFactoryOptions>(opt =>
            {
                opt.IsDynamicClaimsEnabled = true;
            });
        }

        return services;
    }

    /// <summary>
    /// 添加 Keycloak 认证 (仅使用 Action 配置)
    /// </summary>
    public static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services,
        Action<KeycloakAuthOptions> configureOptions)
    {
        var options = new KeycloakAuthOptions();
        configureOptions(options);

        services.Configure<KeycloakAuthOptions>(opt =>
        {
            opt.Authority = options.Authority;
            opt.Audience = options.Audience;
            opt.ValidAudiences = options.ValidAudiences;
            opt.ValidateAudience = options.ValidateAudience;
            opt.RequireHttpsMetadata = options.RequireHttpsMetadata;
            opt.AutoCreateUser = options.AutoCreateUser;
            opt.SyncUserInfoOnLogin = options.SyncUserInfoOnLogin;
            opt.EnableDynamicClaims = options.EnableDynamicClaims;
            opt.ClaimMappings = options.ClaimMappings;
            opt.CustomClaimsToInclude = options.CustomClaimsToInclude;
            opt.SignalRHubPath = options.SignalRHubPath;
        });

        services.AddScoped<KeycloakJwtBearerEvents>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwtOptions =>
            {
                jwtOptions.Authority = options.Authority;
                jwtOptions.RequireHttpsMetadata = options.RequireHttpsMetadata;
                
                jwtOptions.Audience = string.IsNullOrWhiteSpace(options.Audience) 
                    ? options.Authority 
                    : options.Audience;
                    
                jwtOptions.TokenValidationParameters.ValidateAudience = options.ValidateAudience;
                jwtOptions.TokenValidationParameters.ValidAudiences = options.ValidAudiences.Length > 0 
                    ? options.ValidAudiences 
                    : [options.Authority];
                    
                jwtOptions.EventsType = typeof(KeycloakJwtBearerEvents);
            });

        if (options.EnableDynamicClaims)
        {
            services.Configure<AbpClaimsPrincipalFactoryOptions>(opt =>
            {
                opt.IsDynamicClaimsEnabled = true;
            });
        }

        return services;
    }
}
