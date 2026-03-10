using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JcStack.Abp.Keycloak.Identity.Keycloak;

/// <summary>
/// Keycloak Admin API 类型化客户端
/// 封装所有 Keycloak Admin API HTTP 调用
/// </summary>
public class KeycloakAdminClient
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<JcStackAbpKeycloakIdentityOptions> _options;
    private readonly ILogger<KeycloakAdminClient> _logger;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _accessToken;
    private DateTimeOffset _accessTokenExpiresAt;

    /// <summary>
    /// 构造函数 - HttpClient 由 IHttpClientFactory 自动注入
    /// </summary>
    public KeycloakAdminClient(
        HttpClient httpClient,
        IOptions<JcStackAbpKeycloakIdentityOptions> options,
        ILogger<KeycloakAdminClient> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    #region Access Token

    private async Task EnsureAccessTokenAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        if (!string.IsNullOrWhiteSpace(_accessToken) && _accessTokenExpiresAt > now.AddSeconds(30))
        {
            return;
        }

        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            now = DateTimeOffset.UtcNow;
            if (!string.IsNullOrWhiteSpace(_accessToken) && _accessTokenExpiresAt > now.AddSeconds(30))
            {
                return;
            }

            var tokenResponse = await RequestAccessTokenAsync(cancellationToken);
            _accessToken = tokenResponse.AccessToken;
            _accessTokenExpiresAt = now.AddSeconds(tokenResponse.ExpiresIn > 0 ? tokenResponse.ExpiresIn : 60);
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task<TokenResponse> RequestAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.Value.AdminClientId) ||
            string.IsNullOrWhiteSpace(_options.Value.AdminClientSecret))
        {
            throw new InvalidOperationException(
                "Keycloak admin client credentials are not configured. Please set Keycloak:AdminClientId and Keycloak:AdminClientSecret.");
        }

        var tokenEndpoint = $"/realms/{_options.Value.Realm}/protocol/openid-connect/token";
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _options.Value.AdminClientId,
            ["client_secret"] = _options.Value.AdminClientSecret
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = content
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);
        if (token == null || string.IsNullOrWhiteSpace(token.AccessToken))
        {
            throw new InvalidOperationException("Failed to acquire Keycloak admin access token.");
        }

        return token;
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string url,
        object? body,
        CancellationToken cancellationToken)
    {
        await EnsureAccessTokenAsync(cancellationToken);

        using var request = new HttpRequestMessage(method, url);
        if (body != null)
        {
            request.Content = JsonContent.Create(body);
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        return await _httpClient.SendAsync(request, cancellationToken);
    }

    private static string BuildQueryString(Dictionary<string, string?> parameters)
    {
        var parts = parameters
            .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
            .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value!)}")
            .ToList();

        return parts.Count == 0 ? string.Empty : $"?{string.Join("&", parts)}";
    }

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }

    #endregion

    #region 角色管理 API

    /// <summary>
    /// 获取角色（按名称）
    /// </summary>
    public async Task<RoleRepresentation?> GetRoleByNameAsync(
        string roleName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await SendAsync(
                HttpMethod.Get,
                $"/admin/realms/{_options.Value.Realm}/roles/{roleName}",
                body: null,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<RoleRepresentation>(
                    cancellationToken: cancellationToken);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get role {RoleName}", roleName);
            throw;
        }
    }

    /// <summary>
    /// 创建角色
    /// </summary>
    public async Task<HttpResponseMessage> CreateRoleAsync(
        RoleRepresentation role,
        CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(
            HttpMethod.Post,
            $"/admin/realms/{_options.Value.Realm}/roles",
            role,
            cancellationToken);

        return response;
    }

    /// <summary>
    /// 为用户分配角色
    /// </summary>
    public async Task<HttpResponseMessage> AssignRoleToUserAsync(
        string userId,
        IEnumerable<RoleRepresentation> roles,
        CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(
            HttpMethod.Post,
            $"/admin/realms/{_options.Value.Realm}/users/{userId}/role-mappings/realm",
            roles,
            cancellationToken);

        return response;
    }

    /// <summary>
    /// 从用户移除角色
    /// </summary>
    public async Task<HttpResponseMessage> RemoveRoleFromUserAsync(
        string userId,
        IEnumerable<RoleRepresentation> roles,
        CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(
            HttpMethod.Delete,
            $"/admin/realms/{_options.Value.Realm}/users/{userId}/role-mappings/realm",
            roles,
            cancellationToken);
        return response;
    }

    #endregion

    #region 客户端管理 API

    /// <summary>
    /// 获取所有客户端
    /// </summary>
    public async Task<List<ClientRepresentation>?> GetClientsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await SendAsync(
                HttpMethod.Get,
                $"/admin/realms/{_options.Value.Realm}/clients",
                body: null,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ClientRepresentation>>(
                    cancellationToken: cancellationToken);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get clients");
            throw;
        }
    }

    /// <summary>
    /// 获取客户端（按 ClientId）
    /// </summary>
    public async Task<ClientRepresentation?> GetClientByClientIdAsync(
        string clientId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var clients = await GetClientsAsync(cancellationToken);
            return clients?.FirstOrDefault(c => c.ClientId == clientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get client {ClientId}", clientId);
            throw;
        }
    }

    /// <summary>
    /// 获取客户端（按 ID）
    /// </summary>
    public async Task<ClientRepresentation?> GetClientByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await SendAsync(
                HttpMethod.Get,
                $"/admin/realms/{_options.Value.Realm}/clients/{id}",
                body: null,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ClientRepresentation>(
                    cancellationToken: cancellationToken);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get client by ID {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// 创建客户端
    /// </summary>
    public async Task<HttpResponseMessage> CreateClientAsync(
        ClientRepresentation client,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await SendAsync(
                HttpMethod.Post,
                $"/admin/realms/{_options.Value.Realm}/clients",
                client,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully created client {ClientId}", client.ClientId);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to create client {ClientId}: {Error}", client.ClientId, error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create client {ClientId}", client.ClientId);
            throw;
        }
    }

    /// <summary>
    /// 更新客户端
    /// </summary>
    public async Task<HttpResponseMessage> UpdateClientAsync(
        string id,
        ClientRepresentation client,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await SendAsync(
                HttpMethod.Put,
                $"/admin/realms/{_options.Value.Realm}/clients/{id}",
                client,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully updated client {ClientId}", client.ClientId);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to update client {ClientId}: {Error}", client.ClientId, error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update client {ClientId}", client.ClientId);
            throw;
        }
    }

    /// <summary>
    /// 删除客户端
    /// </summary>
    public async Task<HttpResponseMessage> DeleteClientAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await SendAsync(
                HttpMethod.Delete,
                $"/admin/realms/{_options.Value.Realm}/clients/{id}",
                body: null,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully deleted client {Id}", id);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to delete client {Id}: {Error}", id, error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete client {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// 创建或更新客户端（如果存在则更新，否则创建）
    /// </summary>
    public async Task<HttpResponseMessage> CreateOrUpdateClientAsync(
        ClientRepresentation client,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var existingClient = await GetClientByClientIdAsync(client.ClientId!, cancellationToken);

            if (existingClient != null)
            {
                // 更新现有客户端
                client.Id = existingClient.Id;
                return await UpdateClientAsync(existingClient.Id!, client, cancellationToken);
            }
            else
            {
                // 创建新客户端
                return await CreateClientAsync(client, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create or update client {ClientId}", client.ClientId);
            throw;
        }
    }

    /// <summary>
    /// 获取客户端的默认客户端作用域
    /// </summary>
    public async Task<List<ClientScopeRepresentation>?> GetClientDefaultScopesAsync(
        string clientId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await SendAsync(
                HttpMethod.Get,
                $"/admin/realms/{_options.Value.Realm}/clients/{clientId}/default-client-scopes",
                body: null,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ClientScopeRepresentation>>(
                    cancellationToken: cancellationToken);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get default scopes for client {ClientId}", clientId);
            throw;
        }
    }

    /// <summary>
    /// 添加默认客户端作用域到客户端
    /// </summary>
    public async Task<HttpResponseMessage> AddDefaultClientScopeAsync(
        string clientId,
        string scopeId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await SendAsync(
                HttpMethod.Put,
                $"/admin/realms/{_options.Value.Realm}/clients/{clientId}/default-client-scopes/{scopeId}",
                body: null,
                cancellationToken);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add default scope {ScopeId} to client {ClientId}", scopeId, clientId);
            throw;
        }
    }

    #endregion

    #region 用户管理 API

    /// <summary>
    /// 获取用户列表（按条件）
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="email">邮箱</param>
    /// <param name="search">搜索关键词</param>
    /// <param name="q">属性查询，格式: attribute_name:attribute_value</param>
    /// <param name="exact">是否精确匹配</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task<List<UserRepresentation>?> GetUsersAsync(
        string? username = null,
        string? email = null,
        string? search = null,
        string? q = null,
        bool? exact = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = BuildQueryString(new Dictionary<string, string?>
            {
                ["username"] = username,
                ["email"] = email,
                ["search"] = search,
                ["q"] = q,
                ["exact"] = exact?.ToString()?.ToLowerInvariant()
            });

            var response = await SendAsync(
                HttpMethod.Get,
                $"/admin/realms/{_options.Value.Realm}/users{query}",
                body: null,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<UserRepresentation>>(
                    cancellationToken: cancellationToken);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get users");
            throw;
        }
    }

    /// <summary>
    /// 获取单个用户
    /// </summary>
    public async Task<UserRepresentation?> GetUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await SendAsync(
                HttpMethod.Get,
                $"/admin/realms/{_options.Value.Realm}/users/{userId}",
                body: null,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserRepresentation>(
                    cancellationToken: cancellationToken);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    public async Task<HttpResponseMessage> CreateUserAsync(
        UserRepresentation user,
        CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(
            HttpMethod.Post,
            $"/admin/realms/{_options.Value.Realm}/users",
            user,
            cancellationToken);

        return response;
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    public async Task<HttpResponseMessage> UpdateUserAsync(
        string userId,
        UserRepresentation user,
        CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(
            HttpMethod.Put,
            $"/admin/realms/{_options.Value.Realm}/users/{userId}",
            user,
            cancellationToken);

        return response;
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    public async Task<HttpResponseMessage> DeleteUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(
            HttpMethod.Delete,
            $"/admin/realms/{_options.Value.Realm}/users/{userId}",
            body: null,
            cancellationToken);

        return response;
    }

    /// <summary>
    /// 重置用户密码
    /// </summary>
    public async Task<HttpResponseMessage> ResetUserPasswordAsync(
        string userId,
        string password,
        bool temporary = true,
        CancellationToken cancellationToken = default)
    {
        var credential = new CredentialRepresentation
        {
            Type = "password",
            Value = password,
            Temporary = temporary
        };

        var response = await SendAsync(
            HttpMethod.Put,
            $"/admin/realms/{_options.Value.Realm}/users/{userId}/reset-password",
            credential,
            cancellationToken);

        return response;
    }

    #endregion

    #region Magic Link API

    /// <summary>
    /// 调用 keycloak-magic-link 扩展 REST API 创建 Magic Link
    /// POST /realms/{realm}/magic-link
    /// </summary>
    public async Task<MagicLinkResponse?> CreateMagicLinkAsync(
        MagicLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await SendAsync(
                HttpMethod.Post,
                $"/realms/{_options.Value.Realm}/magic-link",
                request,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<MagicLinkResponse>(
                    cancellationToken: cancellationToken);
                _logger.LogInformation(
                    "Created magic link for {Email}, userId={UserId}",
                    request.Email, result?.UserId);
                return result;
            }

            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "Failed to create magic link for {Email}. Status: {StatusCode}, Error: {Error}",
                request.Email, response.StatusCode, error);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create magic link for {Email}", request.Email);
            throw;
        }
    }

    #endregion

    #region Client Scope 管理 API

    /// <summary>
    /// 获取所有 Client Scopes
    /// </summary>
    public async Task<List<ClientScopeRepresentation>?> GetClientScopesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await SendAsync(
                HttpMethod.Get,
                $"/admin/realms/{_options.Value.Realm}/client-scopes",
                body: null,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ClientScopeRepresentation>>(
                    cancellationToken: cancellationToken);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get client scopes");
            throw;
        }
    }

    /// <summary>
    /// 按名称获取 Client Scope
    /// </summary>
    public async Task<ClientScopeRepresentation?> GetClientScopeByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var scopes = await GetClientScopesAsync(cancellationToken);
        return scopes?.FirstOrDefault(s => s.Name == name);
    }

    /// <summary>
    /// 创建 Client Scope
    /// </summary>
    public async Task<HttpResponseMessage> CreateClientScopeAsync(
        ClientScopeRepresentation scope,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await SendAsync(
                HttpMethod.Post,
                $"/admin/realms/{_options.Value.Realm}/client-scopes",
                scope,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully created client scope {ScopeName}", scope.Name);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to create client scope {ScopeName}: {Error}", scope.Name, error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create client scope {ScopeName}", scope.Name);
            throw;
        }
    }

    /// <summary>
    /// 更新 Client Scope
    /// </summary>
    public async Task<HttpResponseMessage> UpdateClientScopeAsync(
        string scopeId,
        ClientScopeRepresentation scope,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await SendAsync(
                HttpMethod.Put,
                $"/admin/realms/{_options.Value.Realm}/client-scopes/{scopeId}",
                scope,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully updated client scope {ScopeName}", scope.Name);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to update client scope {ScopeName}: {Error}", scope.Name, error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update client scope {ScopeName}", scope.Name);
            throw;
        }
    }

    /// <summary>
    /// 创建或更新 Client Scope
    /// </summary>
    public async Task<HttpResponseMessage> CreateOrUpdateClientScopeAsync(
        ClientScopeRepresentation scope,
        CancellationToken cancellationToken = default)
    {
        var existingScope = await GetClientScopeByNameAsync(scope.Name!, cancellationToken);

        if (existingScope != null)
        {
            scope.Id = existingScope.Id;
            return await UpdateClientScopeAsync(existingScope.Id!, scope, cancellationToken);
        }
        else
        {
            return await CreateClientScopeAsync(scope, cancellationToken);
        }
    }

    /// <summary>
    /// 为 Client Scope 添加 Protocol Mapper
    /// </summary>
    public async Task<HttpResponseMessage> AddProtocolMapperToClientScopeAsync(
        string scopeId,
        ProtocolMapperRepresentation mapper,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await SendAsync(
                HttpMethod.Post,
                $"/admin/realms/{_options.Value.Realm}/client-scopes/{scopeId}/protocol-mappers/models",
                mapper,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully added protocol mapper {MapperName} to scope {ScopeId}",
                    mapper.Name, scopeId);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to add protocol mapper {MapperName} to scope {ScopeId}: {Error}",
                    mapper.Name, scopeId, error);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add protocol mapper {MapperName} to scope {ScopeId}",
                mapper.Name, scopeId);
            throw;
        }
    }

    /// <summary>
    /// 获取 Client Scope 的所有 Protocol Mappers
    /// </summary>
    public async Task<List<ProtocolMapperRepresentation>?> GetClientScopeProtocolMappersAsync(
        string scopeId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await SendAsync(
                HttpMethod.Get,
                $"/admin/realms/{_options.Value.Realm}/client-scopes/{scopeId}/protocol-mappers/models",
                body: null,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ProtocolMapperRepresentation>>(
                    cancellationToken: cancellationToken);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get protocol mappers for scope {ScopeId}", scopeId);
            throw;
        }
    }

    #endregion
}
