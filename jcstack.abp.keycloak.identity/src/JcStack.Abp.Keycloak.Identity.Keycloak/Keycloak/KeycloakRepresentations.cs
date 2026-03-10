using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JcStack.Abp.Keycloak.Identity.Keycloak;

/// <summary>
/// Keycloak Role Representation (minimal)
/// </summary>
public class RoleRepresentation
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

/// <summary>
/// Keycloak Client Representation
/// </summary>
public class ClientRepresentation
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("clientId")]
    public string? ClientId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }

    [JsonPropertyName("protocol")]
    public string? Protocol { get; set; } = "openid-connect";

    [JsonPropertyName("publicClient")]
    public bool? PublicClient { get; set; }

    [JsonPropertyName("bearerOnly")]
    public bool? BearerOnly { get; set; }

    [JsonPropertyName("serviceAccountsEnabled")]
    public bool? ServiceAccountsEnabled { get; set; }

    [JsonPropertyName("standardFlowEnabled")]
    public bool? StandardFlowEnabled { get; set; }

    [JsonPropertyName("implicitFlowEnabled")]
    public bool? ImplicitFlowEnabled { get; set; }

    [JsonPropertyName("directAccessGrantsEnabled")]
    public bool? DirectAccessGrantsEnabled { get; set; }

    [JsonPropertyName("redirectUris")]
    public List<string>? RedirectUris { get; set; }

    [JsonPropertyName("webOrigins")]
    public List<string>? WebOrigins { get; set; }

    [JsonPropertyName("rootUrl")]
    public string? RootUrl { get; set; }

    [JsonPropertyName("baseUrl")]
    public string? BaseUrl { get; set; }

    [JsonPropertyName("adminUrl")]
    public string? AdminUrl { get; set; }

    [JsonPropertyName("attributes")]
    public Dictionary<string, string>? Attributes { get; set; }

    [JsonPropertyName("defaultClientScopes")]
    public List<string>? DefaultClientScopes { get; set; }

    [JsonPropertyName("optionalClientScopes")]
    public List<string>? OptionalClientScopes { get; set; }

    [JsonPropertyName("fullScopeAllowed")]
    public bool? FullScopeAllowed { get; set; }

    /// <summary>
    /// Client secret (only for confidential clients where PublicClient = false)
    /// </summary>
    [JsonPropertyName("secret")]
    public string? Secret { get; set; }
}

/// <summary>
/// Keycloak Client Scope Representation
/// </summary>
public class ClientScopeRepresentation
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("protocol")]
    public string? Protocol { get; set; } = "openid-connect";

    [JsonPropertyName("attributes")]
    public Dictionary<string, string>? Attributes { get; set; }

    [JsonPropertyName("protocolMappers")]
    public List<ProtocolMapperRepresentation>? ProtocolMappers { get; set; }
}

/// <summary>
/// Keycloak Protocol Mapper Representation
/// Used to define token mappers (e.g., audience, user attribute, etc.)
/// </summary>
public class ProtocolMapperRepresentation
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("protocol")]
    public string? Protocol { get; set; } = "openid-connect";

    /// <summary>
    /// Mapper type, e.g.:
    /// - "oidc-audience-mapper" for audience
    /// - "oidc-usermodel-attribute-mapper" for user attributes
    /// - "oidc-hardcoded-claim-mapper" for hardcoded claims
    /// </summary>
    [JsonPropertyName("protocolMapper")]
    public string? ProtocolMapper { get; set; }

    [JsonPropertyName("consentRequired")]
    public bool? ConsentRequired { get; set; } = false;

    /// <summary>
    /// Mapper configuration, varies by mapper type.
    /// Common keys for audience mapper:
    /// - "included.client.audience": client ID to include as audience
    /// - "id.token.claim": "true" to add to ID token
    /// - "access.token.claim": "true" to add to access token
    /// </summary>
    [JsonPropertyName("config")]
    public Dictionary<string, string>? Config { get; set; }

    /// <summary>
    /// Creates an audience mapper configuration
    /// </summary>
    public static ProtocolMapperRepresentation CreateAudienceMapper(
        string name,
        string clientAudience,
        bool addToIdToken = false,
        bool addToAccessToken = true)
    {
        return new ProtocolMapperRepresentation
        {
            Name = name,
            Protocol = "openid-connect",
            ProtocolMapper = "oidc-audience-mapper",
            ConsentRequired = false,
            Config = new Dictionary<string, string>
            {
                ["included.client.audience"] = clientAudience,
                ["id.token.claim"] = addToIdToken.ToString().ToLowerInvariant(),
                ["access.token.claim"] = addToAccessToken.ToString().ToLowerInvariant(),
                ["introspection.token.claim"] = addToAccessToken.ToString().ToLowerInvariant()
            }
        };
    }
}

/// <summary>
/// Keycloak User Representation
/// </summary>
public class UserRepresentation
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }

    [JsonPropertyName("emailVerified")]
    public bool? EmailVerified { get; set; }

    [JsonPropertyName("attributes")]
    public Dictionary<string, ICollection<string>>? Attributes { get; set; }

    [JsonPropertyName("credentials")]
    public List<CredentialRepresentation>? Credentials { get; set; }
}

/// <summary>
/// Keycloak Credential Representation
/// </summary>
public class CredentialRepresentation
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("temporary")]
    public bool? Temporary { get; set; }
}

/// <summary>
/// Magic Link 请求体（keycloak-magic-link 扩展 REST API）
/// </summary>
public class MagicLinkRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = string.Empty;

    [JsonPropertyName("redirect_uri")]
    public string RedirectUri { get; set; } = string.Empty;

    [JsonPropertyName("expiration_seconds")]
    public int ExpirationSeconds { get; set; }

    /// <summary>
    /// 是否由 Keycloak 发送邮件（false = ASMS 自行发送）
    /// </summary>
    [JsonPropertyName("send_email")]
    public bool SendEmail { get; set; }

    /// <summary>
    /// 用户不存在时是否自动创建
    /// </summary>
    [JsonPropertyName("force_create")]
    public bool ForceCreate { get; set; }
}

/// <summary>
/// Magic Link 响应（keycloak-magic-link 扩展 REST API）
/// </summary>
public class MagicLinkResponse
{
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    [JsonPropertyName("link")]
    public string? Link { get; set; }

    [JsonPropertyName("sent")]
    public bool Sent { get; set; }
}
