# JcStack.Abp.Keycloak.Identity 集成示例

本文档展示如何将 JcStack.Abp.Keycloak.Identity 模块集成到现有的 ABP 应用程序中，完全替换 OpenIddict。

## 目录

1. [集成步骤](#集成步骤)
2. [HttpApi.Host 集成](#httpapihost-集成)
3. [配置文件示例](#配置文件示例)
4. [测试集成](#测试集成)

## 集成步骤

### 步骤 1: 添加项目引用

在 `Keycloak.Abp.HttpApi.Host.csproj` 中添加：

```xml
<ItemGroup>
  <!-- 添加 JcStack.Abp.Keycloak.Identity 模块引用 -->
  <ProjectReference Include="..\..\modules\jcstack.abp.keycloak.identity\src\JcStack.Abp.Keycloak.Identity.Domain\JcStack.Abp.Keycloak.Identity.Domain.csproj" />
  <ProjectReference Include="..\..\modules\jcstack.abp.keycloak.identity\src\JcStack.Abp.Keycloak.Identity.Application\JcStack.Abp.Keycloak.Identity.Application.csproj" />
  <ProjectReference Include="..\..\modules\jcstack.abp.keycloak.identity\src\JcStack.Abp.Keycloak.Identity.Authentication\JcStack.Abp.Keycloak.Identity.Authentication.csproj" />
</ItemGroup>
```

### 步骤 2: 更新模块依赖

修改 `AbpHttpApiHostModule.cs`:

```csharp
using JcStack.Abp.Keycloak.Identity.Authentication;
using JcStack.Abp.Keycloak.Identity.Application;
using JcStack.Abp.Keycloak.Identity.Domain;

[DependsOn(
    // ... 其他依赖
    
    // 移除 OpenIddict 相关依赖
    // typeof(AbpAccountWebOpenIddictModule),  // 移除
    
    // 添加 Keycloak 模块依赖
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

### 步骤 3: 移除 OpenIddict 配置

从 `AbpHttpApiHostModule.cs` 中移除所有 OpenIddict 相关配置：

```csharp
public override void PreConfigureServices(ServiceConfigurationContext context)
{
    // 移除这些代码
    /*
    PreConfigure<OpenIddictBuilder>(builder =>
    {
        builder.AddValidation(options =>
        {
            options.AddAudiences("Abp");
            options.UseLocalServer();
            options.UseAspNetCore();
        });
    });
    
    PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
    {
        // ...
    });
    */
}
```

### 步骤 4: 更新认证配置

修改 `ConfigureAuthentication` 方法：

```csharp
private void ConfigureAuthentication(ServiceConfigurationContext context)
{
    // Keycloak 认证已通过 KeycloakAuthenticationModule 自动配置
    // 只需配置动态声明
    context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
    {
        options.IsDynamicClaimsEnabled = true;
    });
}
```

### 步骤 5: 更新应用初始化

修改 `OnApplicationInitialization` 方法：

```csharp
public override void OnApplicationInitialization(ApplicationInitializationContext context)
{
    var app = context.GetApplicationBuilder();
    var env = context.GetEnvironment();

    app.UseForwardedHeaders();

    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseAbpRequestLocalization();

    if (!env.IsDevelopment())
    {
        app.UseErrorPage();
    }

    app.UseRouting();
    app.MapAbpStaticAssets();
    app.UseAbpStudioLink();
    app.UseAbpSecurityHeaders();
    app.UseCors();
    
    // 使用 Keycloak 认证
    app.UseAuthentication();
    // 移除: app.UseAbpOpenIddictValidation();

    if (MultiTenancyConsts.IsEnabled)
    {
        app.UseMultiTenancy();
    }

    app.UseUnitOfWork();
    app.UseDynamicClaims();
    app.UseAuthorization();

    app.UseSwagger();
    app.UseAbpSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Abp API");

        var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
        options.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
    });
    
    app.UseAuditing();
    app.UseAbpSerilogEnrichers();
    app.UseConfiguredEndpoints();
}
```

### 步骤 6: 更新 Swagger 配置

修改 `ConfigureSwagger` 方法以使用 Keycloak 端点：

```csharp
private static void ConfigureSwagger(ServiceConfigurationContext context, IConfiguration configuration)
{
    var keycloakServerUrl = configuration["Keycloak:ServerUrl"];
    var keycloakRealm = configuration["Keycloak:Realm"];
    var authorizationUrl = $"{keycloakServerUrl}/realms/{keycloakRealm}/protocol/openid-connect/auth";
    var tokenUrl = $"{keycloakServerUrl}/realms/{keycloakRealm}/protocol/openid-connect/token";

    context.Services.AddAbpSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "Abp API", Version = "v1" });
        options.DocInclusionPredicate((docName, description) => true);
        options.CustomSchemaIds(type => type.FullName);

        // 配置 OAuth2 认证
        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri(authorizationUrl),
                    TokenUrl = new Uri(tokenUrl),
                    Scopes = new Dictionary<string, string>
                    {
                        { "openid", "OpenID" },
                        { "profile", "Profile" },
                        { "email", "Email" },
                        { "roles", "Roles" }
                    }
                }
            }
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "oauth2"
                    }
                },
                new[] { "openid", "profile", "email", "roles" }
            }
        });
    });
}
```

## HttpApi.Host 集成

### 完整的 AbpHttpApiHostModule.cs 示例

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Volo.Abp;
using Volo.Abp.Studio;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Swashbuckle;
using Volo.Abp.Studio.Client.AspNetCore;
using Volo.Abp.Security.Claims;
using JcStack.Abp.Keycloak.Identity.Authentication;
using JcStack.Abp.Keycloak.Identity.Application;
using JcStack.Abp.Keycloak.Identity.Domain;
using Keycloak.Abp.EntityFrameworkCore;
using Keycloak.Abp.MultiTenancy;
using Keycloak.Abp.HealthChecks;

namespace Keycloak.Abp;

[DependsOn(
    typeof(AbpHttpApiModule),
    typeof(AbpStudioClientAspNetCoreModule),
    typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(AbpApplicationModule),
    typeof(AbpEntityFrameworkCoreModule),
    typeof(JcStackAbpKeycloakIdentityDomainModule),
    typeof(JcStackAbpKeycloakIdentityApplicationModule),
    typeof(KeycloakAuthenticationModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAspNetCoreSerilogModule)
)]
public class AbpHttpApiHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        if (!configuration.GetValue<bool>("App:DisablePII"))
        {
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.LogCompleteSecurityArtifact = true;
        }

        ConfigureStudio(hostingEnvironment);
        ConfigureAuthentication(context);
        ConfigureUrls(configuration);
        ConfigureBundles();
        ConfigureConventionalControllers();
        ConfigureHealthChecks(context);
        ConfigureSwagger(context, configuration);
        ConfigureVirtualFileSystem(context);
        ConfigureCors(context, configuration);
    }

    private void ConfigureStudio(IHostEnvironment hostingEnvironment)
    {
        if (hostingEnvironment.IsProduction())
        {
            Configure<AbpStudioClientOptions>(options =>
            {
                options.IsLinkEnabled = false;
            });
        }
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        // Keycloak 认证已通过 KeycloakAuthenticationModule 自动配置
        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            options.Applications["Angular"].RootUrl = configuration["App:AngularUrl"];
            options.RedirectAllowedUrls.AddRange(
                configuration["App:RedirectAllowedUrls"]?.Split(',') ?? Array.Empty<string>()
            );
        });
    }

    private void ConfigureBundles()
    {
        Configure<AbpBundlingOptions>(options =>
        {
            options.StyleBundles.Configure(
                LeptonXLiteThemeBundles.Styles.Global,
                bundle => bundle.AddFiles("/global-styles.css")
            );

            options.ScriptBundles.Configure(
                LeptonXLiteThemeBundles.Scripts.Global,
                bundle => bundle.AddFiles("/global-scripts.js")
            );
        });
    }

    private void ConfigureVirtualFileSystem(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<AbpDomainSharedModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, 
                    $"..{Path.DirectorySeparatorChar}Keycloak.Abp.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<AbpDomainModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, 
                    $"..{Path.DirectorySeparatorChar}Keycloak.Abp.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<AbpApplicationContractsModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, 
                    $"..{Path.DirectorySeparatorChar}Keycloak.Abp.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<AbpApplicationModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, 
                    $"..{Path.DirectorySeparatorChar}Keycloak.Abp.Application"));
            });
        }
    }

    private void ConfigureConventionalControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(AbpApplicationModule).Assembly);
        });
    }

    private static void ConfigureSwagger(ServiceConfigurationContext context, IConfiguration configuration)
    {
        var keycloakServerUrl = configuration["Keycloak:ServerUrl"];
        var keycloakRealm = configuration["Keycloak:Realm"];
        var authorizationUrl = $"{keycloakServerUrl}/realms/{keycloakRealm}/protocol/openid-connect/auth";
        var tokenUrl = $"{keycloakServerUrl}/realms/{keycloakRealm}/protocol/openid-connect/token";

        context.Services.AddAbpSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Abp API", Version = "v1" });
            options.DocInclusionPredicate((docName, description) => true);
            options.CustomSchemaIds(type => type.FullName);

            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(authorizationUrl),
                        TokenUrl = new Uri(tokenUrl),
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "OpenID" },
                            { "profile", "Profile" },
                            { "email", "Email" },
                            { "roles", "Roles" }
                        }
                    }
                }
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "oauth2"
                        }
                    },
                    new[] { "openid", "profile", "email", "roles" }
                }
            });
        });
    }

    private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(
                        configuration["App:CorsOrigins"]?
                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(o => o.Trim().RemovePostFix("/"))
                            .ToArray() ?? Array.Empty<string>()
                    )
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    private void ConfigureHealthChecks(ServiceConfigurationContext context)
    {
        context.Services.AddAbpHealthChecks();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        app.UseForwardedHeaders();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
        }

        app.UseRouting();
        app.MapAbpStaticAssets();
        app.UseAbpStudioLink();
        app.UseAbpSecurityHeaders();
        app.UseCors();
        app.UseAuthentication();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();

        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Abp API");

            var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
            options.OAuthClientId(configuration["Keycloak:AdminClientId"]);
        });
        
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
```

## 配置文件示例

### appsettings.json

```json
{
  "App": {
    "SelfUrl": "https://localhost:44300",
    "AngularUrl": "http://localhost:4200",
    "CorsOrigins": "https://*.Abp.com,http://localhost:4200,https://localhost:44307",
    "RedirectAllowedUrls": "http://localhost:4200,https://localhost:44307",
    "DisablePII": "false"
  },
  "Keycloak": {
    "ServerUrl": "http://localhost:8080",
    "Realm": "abp-realm",
    "AdminClientId": "abp-api-client",
    "AdminClientSecret": "your-client-secret-here",
    "EnableUserSync": true,
    "EnableRoleSync": true,
    "SyncUserDeletionToKeycloak": true,
    "RolePrefix": "ABP",
    "SourceSystem": "ABP_API",
    "RetryPolicy": {
      "MaxRetryAttempts": 3,
      "InitialBackoffSeconds": 2,
      "MaxBackoffSeconds": 30
    }
  },
  "ConnectionStrings": {
    "Default": "Server=(LocalDb)\\MSSQLLocalDB;Database=Abp;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "StringEncryption": {
    "DefaultPassPhrase": "gsKxGZ012HLL3MI5"
  }
}
```

### appsettings.Development.json

```json
{
  "App": {
    "SelfUrl": "https://localhost:44300",
    "AngularUrl": "http://localhost:4200"
  },
  "Keycloak": {
    "ServerUrl": "http://localhost:8080",
    "Realm": "abp-realm",
    "AdminClientId": "abp-api-client",
    "AdminClientSecret": "dev-secret"
  }
}
```

### appsettings.Production.json

```json
{
  "App": {
    "SelfUrl": "https://api.production.com",
    "AngularUrl": "https://app.production.com"
  },
  "Keycloak": {
    "ServerUrl": "https://keycloak.production.com",
    "Realm": "production-realm",
    "AdminClientId": "abp-api-client",
    "AdminClientSecret": "#{KeyVault:KeycloakClientSecret}#"
  }
}
```

## 测试集成

### 1. 启动 Keycloak

```bash
docker run -d \
  --name keycloak \
  -p 8080:8080 \
  -e KEYCLOAK_ADMIN=admin \
  -e KEYCLOAK_ADMIN_PASSWORD=admin \
  quay.io/keycloak/keycloak:23.0 \
  start-dev
```

### 2. 配置 Keycloak

1. 访问 http://localhost:8080
2. 登录 Admin Console (admin/admin)
3. 创建 Realm: `abp-realm`
4. 创建 Client: `abp-api-client`
   - Client Protocol: openid-connect
   - Access Type: confidential
   - Service Accounts Enabled: ON
5. 获取 Client Secret
6. 配置 Service Account Roles:
   - realm-management → manage-users
   - realm-management → manage-realm

### 3. 运行应用

```bash
cd src/Keycloak.Abp.HttpApi.Host
dotnet run
```

### 4. 测试 API

访问 Swagger UI: https://localhost:44300/swagger

点击 "Authorize" 按钮，使用 Keycloak 登录。

### 5. 测试用户同步

```csharp
// 创建用户
var user = new IdentityUser(
    Guid.NewGuid(),
    "testuser",
    "test@example.com"
);

await _userManager.CreateAsync(user, "Password123!");

// 检查 Keycloak
// 用户应该自动同步到 Keycloak
```

### 6. 查看日志

```bash
# 查看同步日志
tail -f Logs/logs.txt | grep "Keycloak"
```

## 故障排查

### 问题 1: 编译错误

确保所有项目引用正确：

```bash
dotnet restore
dotnet build
```

### 问题 2: Keycloak 连接失败

检查配置：
- ServerUrl 是否正确
- Realm 是否存在
- Client ID 和 Secret 是否正确

### 问题 3: 认证失败

检查：
- Token 端点是否可访问
- Client 配置是否正确
- Redirect URI 是否匹配

## 下一步

1. 配置 Angular 前端使用 Keycloak
2. 实现批量用户迁移
3. 配置生产环境
4. 设置监控和告警

## 参考资源

- [Keycloak 文档](https://www.keycloak.org/documentation)
- [ABP Framework 文档](https://docs.abp.io)
- [配置示例](./configuration-examples.md)
- [迁移指南](./migration-guide.md)
