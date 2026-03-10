using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using JcStack.Abp.Keycloak.Identity.Keycloak;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace JcStack.Abp.Keycloak.Identity.Identity;

/// <summary>
/// Keycloak 客户端数据种子贡献者（通用）
/// 负责在 Keycloak 中注册 OAuth2/OIDC 客户端
/// 
/// 通过配置文件 (appsettings.json) 驱动，支持多个客户端的自动注册
/// 配置路径：Keycloak:Clients
/// 
/// 替代 ABP 的 OpenIddictDataSeedContributor，将客户端注册到 Keycloak 而非本地数据库
/// </summary>
public class KeycloakClientDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    protected KeycloakAdminClient AdminClient { get; }
    protected IConfiguration Configuration { get; }
    protected ILogger<KeycloakClientDataSeedContributor> Logger { get; }

    public KeycloakClientDataSeedContributor(
        KeycloakAdminClient adminClient,
        IConfiguration configuration,
        ILogger<KeycloakClientDataSeedContributor> logger)
    {
        AdminClient = adminClient;
        Configuration = configuration;
        Logger = logger;
    }

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        Logger.LogInformation("Starting Keycloak client data seeding");

        try
        {
            await CreateClientsFromConfigurationAsync();
            Logger.LogInformation("Keycloak client data seeding completed successfully");
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex,
                "Failed to connect to Keycloak during client seeding. " +
                "Please ensure Keycloak is running and properly configured in appsettings.json");

            // 不抛出异常，允许系统继续启动
            Logger.LogWarning("Skipping Keycloak client seeding due to connection error");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during Keycloak client data seeding");
            throw;
        }
    }

    /// <summary>
    /// 从配置文件读取并创建所有客户端
    /// </summary>
    private async Task CreateClientsFromConfigurationAsync()
    {
        var clientsSection = Configuration.GetSection("Keycloak:Clients");
        var clients = clientsSection.GetChildren();

        foreach (var clientSection in clients)
        {
            await CreateClientFromConfigAsync(clientSection);
        }
    }

    /// <summary>
    /// 从配置节点创建客户端
    /// </summary>
    private async Task CreateClientFromConfigAsync(IConfigurationSection clientSection)
    {
        var clientId = clientSection["ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
        {
            Logger.LogWarning("Client configuration missing ClientId, skipping: {SectionPath}", clientSection.Path);
            return;
        }

        var rootUrl = clientSection["RootUrl"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(rootUrl))
        {
            Logger.LogWarning("Client {ClientId} missing RootUrl, skipping", clientId);
            return;
        }

        // 构建客户端配置
        var client = new ClientRepresentation
        {
            ClientId = clientId,
            Name = clientSection["Name"] ?? clientId,
            Description = clientSection["Description"],
            Enabled = bool.Parse(clientSection["Enabled"] ?? "true"),
            Protocol = clientSection["Protocol"] ?? "openid-connect",
            PublicClient = bool.Parse(clientSection["PublicClient"] ?? "true"),
            StandardFlowEnabled = bool.Parse(clientSection["StandardFlowEnabled"] ?? "true"),
            ImplicitFlowEnabled = bool.Parse(clientSection["ImplicitFlowEnabled"] ?? "false"),
            DirectAccessGrantsEnabled = bool.Parse(clientSection["DirectAccessGrantsEnabled"] ?? "false"),
            ServiceAccountsEnabled = bool.Parse(clientSection["ServiceAccountsEnabled"] ?? "false"),
            RootUrl = rootUrl,
            BaseUrl = clientSection["BaseUrl"] ?? rootUrl,
            AdminUrl = clientSection["AdminUrl"]
        };

        // 读取 RedirectUris
        var redirectUris = clientSection.GetSection("RedirectUris").GetChildren()
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToList();

        if (redirectUris.Count == 0)
        {
            // 默认值
            redirectUris.Add($"{rootUrl}/*");
        }
        client.RedirectUris = redirectUris!;

        // 读取 WebOrigins
        var webOrigins = clientSection.GetSection("WebOrigins").GetChildren()
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToList();

        if (webOrigins.Count == 0)
        {
            // 默认值
            webOrigins.Add(rootUrl);
            webOrigins.Add("+");
        }
        client.WebOrigins = webOrigins!;

        // 读取 Attributes（仅使用显式配置的属性）
        var attributes = new Dictionary<string, string>();
        var attributesSection = clientSection.GetSection("Attributes");
        foreach (var attr in attributesSection.GetChildren())
        {
            if (!string.IsNullOrWhiteSpace(attr.Value))
            {
                attributes[attr.Key] = attr.Value;
            }
        }

        client.Attributes = attributes;

        // 读取 DefaultClientScopes
        var defaultScopes = clientSection.GetSection("DefaultClientScopes").GetChildren()
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToList();

        if (defaultScopes.Count == 0)
        {
            // Keycloak 26+ 标准默认 scopes（按照 OIDCLoginProtocolFactory 配置）
            // - basic: 包含 sub (subject) 和 auth_time 等基础 OIDC claims
            // - openid: OIDC 协议要求（用于 /userinfo 等端点）
            // - profile, email: OIDC 标准 scopes
            // - roles, web-origins: Keycloak 特定 scopes
            // - acr: Authentication Context Class Reference
            defaultScopes = ["basic", "openid", "profile", "email", "roles", "web-origins", "acr"];
        }
        client.DefaultClientScopes = defaultScopes!;

        // 读取 OptionalClientScopes
        var optionalScopes = clientSection.GetSection("OptionalClientScopes").GetChildren()
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToList();

        if (optionalScopes.Count > 0)
        {
            client.OptionalClientScopes = optionalScopes!;
        }

        // FullScopeAllowed - 默认为 false (遵循最小权限原则)
        client.FullScopeAllowed = bool.Parse(clientSection["FullScopeAllowed"] ?? "false");

        // 读取 ClientSecret（仅对机密客户端有效，PublicClient = false）
        var clientSecret = clientSection["ClientSecret"];
        if (!string.IsNullOrWhiteSpace(clientSecret) && client.PublicClient == false)
        {
            client.Secret = clientSecret;
        }

        await CreateOrUpdateClientAsync(client);
    }

    /// <summary>
    /// 创建或更新客户端
    /// </summary>
    private async Task CreateOrUpdateClientAsync(ClientRepresentation client)
    {
        try
        {
            var response = await AdminClient.CreateOrUpdateClientAsync(client);

            if (response.IsSuccessStatusCode)
            {
                Logger.LogInformation("Successfully created/updated client {ClientId}", client.ClientId);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Logger.LogError("Failed to create/update client {ClientId}: {Error}", client.ClientId, error);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating/updating client {ClientId}", client.ClientId);
            throw;
        }
    }
}
