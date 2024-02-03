using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace DotAuth.Uma.Web;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// Defines the UMA filter attribute
/// </summary>
[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface,
    AllowMultiple = true)]
public class UmaFilterAttribute : Attribute, IFilterFactory, IAuthorizeData
{
    private const string IdTokenParameter = "id_token";
    private readonly string? _allowedOauthScope;
    private readonly string[] _resourceIdParameters;
    private readonly string _idTokenHeader;
    private readonly string? _resourceIdFormat;
    private readonly string? _realm;
    private readonly string[] _resourceSetAccessScope;

    /// <summary>
    /// Initializes a new instance of the <see cref="UmaFilterAttribute"/> class.
    /// </summary>
    /// <param name="resourceIdParameter">The parameter name identifying the resource id.</param>
    /// <param name="allowedOauthScope">OAuth scope in token allowed to access resource.</param>
    /// <param name="idTokenHeader">The request header or query parameter where an id token will be looked for.</param>
    /// <param name="realm">The resource realm</param>
    /// <param name="resourceSetAccessScope">The resource set access scopes needed to access the web resource.</param>
    /// <summary>
    /// <para>Filters the incoming request to check permission using the UMA2 standard.</para>
    /// <para>If required, the id token if retrieved from either a query parameter or a request header (in that order) with the given <paramref name="idTokenHeader"/> name.</para>
    /// </summary>
    public UmaFilterAttribute(
        string resourceIdParameter,
        string idTokenHeader = IdTokenParameter,
        string? allowedOauthScope = null,
        string? realm = null,
        params string[] resourceSetAccessScope)
        : this(null, [resourceIdParameter], idTokenHeader, allowedOauthScope, realm, resourceSetAccessScope)
    {
        _allowedOauthScope = allowedOauthScope;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UmaFilterAttribute"/> class.
    /// </summary>
    /// <param name="resourceIdFormat">The format string setting how the parameters build the identifier.</param>
    /// <param name="resourceIdParameters">The names of the parameters identifying the resource id.</param>
    /// <param name="allowedOauthScope">OAuth scope in token allowed to access resource.</param>
    /// <param name="idTokenHeader">The request header or query parameter where an id token will be looked for.</param>
    /// <param name="realm">The resource realm</param>
    /// <param name="resourceSetAccessScope">The resource set access scopes needed to access the web resource.</param>
    public UmaFilterAttribute(
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string? resourceIdFormat,
        string[] resourceIdParameters,
        string idTokenHeader = IdTokenParameter,
        string? allowedOauthScope = null,
        string? realm = null,
        params string[] resourceSetAccessScope)
    {
        _resourceIdParameters = resourceIdParameters;
        _idTokenHeader = idTokenHeader;
        _resourceIdFormat = resourceIdFormat;
        _realm = realm;
        _allowedOauthScope = allowedOauthScope;
        _resourceSetAccessScope = resourceSetAccessScope;
    }

    /// <inheritdoc />
    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return new UmaAuthorizationFilter(
            serviceProvider.GetRequiredService<ITokenClient>(),
            serviceProvider.GetRequiredService<IUmaPermissionClient>(),
            serviceProvider.GetRequiredService<IResourceMap>(),
            serviceProvider.GetRequiredService<ILogger<UmaFilterAttribute>>(),
            _resourceIdParameters,
            realm: _realm,
            idTokenHeader: _idTokenHeader,
            resourceSetIdFormat: _resourceIdFormat,
            allowedOauthScope: _allowedOauthScope,
            requiredResourceSetScopes: _resourceSetAccessScope);
    }

    /// <inheritdoc />
    public bool IsReusable
    {
        get { return true; }
    }

    public string? Policy { get; set; }
    public string? Roles { get; set; }
    public string? AuthenticationSchemes { get; set; }

    private class UmaAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly ITokenClient _tokenClient;
        private readonly IUmaPermissionClient _permissionClient;
        private readonly IResourceMap _resourceMap;
        private readonly ILogger _logger;
        private readonly string? _realm;
        private readonly string[] _resourceIdParameters;
        private readonly string _idTokenHeader;
        private readonly string? _resourceSetIdFormat;
        private readonly string? _allowedOauthScope;
        private readonly string[] _requiredResourceSetScopes;

        public UmaAuthorizationFilter(
            ITokenClient tokenClient,
            IUmaPermissionClient permissionClient,
            IResourceMap resourceMap,
            ILogger logger,
            string[] resourceIdParameters,
            string idTokenHeader = IdTokenParameter,
            string? allowedOauthScope = null,
            string? realm = null,
            string? resourceSetIdFormat = null,
            params string[] requiredResourceSetScopes)
        {
            _tokenClient = tokenClient;
            _permissionClient = permissionClient;
            _resourceMap = resourceMap;
            _logger = logger;
            _realm = realm;
            _resourceIdParameters = resourceIdParameters;
            _idTokenHeader = idTokenHeader;
            _resourceSetIdFormat = resourceSetIdFormat;
            _allowedOauthScope = allowedOauthScope;
            _requiredResourceSetScopes = requiredResourceSetScopes;
        }

        /// <inheritdoc />
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (user.Identities.All(x => !x.IsAuthenticated))
            {
                _logger.LogInformation("User is not authenticated");
                context.Result = new UnauthorizedResult();
                return;
            }

            if (CheckHasScopeAccess(user, _allowedOauthScope))
            {
                // User has OAuth token with scope which allows access to resource.
                return;
            }

            var values = _resourceIdParameters.Select(x => context.RouteData.Values[x]).ToArray();
            var resourceId = _resourceSetIdFormat == null
                ? string.Join("", values.Select(v => (v ?? "").ToString()).ToArray())
                : string.Format(_resourceSetIdFormat, values);
            _logger.LogDebug("Attempting to map {ResourceId}", resourceId);
            var resourceSetId = await _resourceMap.GetResourceSetId(resourceId).ConfigureAwait(false);
            if (resourceSetId == null)
            {
                _logger.LogError("Failed to map {ResourceId} to resource set", resourceId);
                context.Result = new UnauthorizedResult();
                return;
            }

            if (user.CheckResourceAccess(resourceSetId, _requiredResourceSetScopes))
            {
                var subject = user.GetSubject();
                var scopes = string.Join(",", _requiredResourceSetScopes);
                _logger.LogDebug(
                    "Received valid token for {ResourceId}, scopes {Scopes} from {Subject}",
                    resourceId,
                    scopes,
                    subject);
                return;
            }

            var serverToken = await HasServerAccessToken().ConfigureAwait(false);
            if (serverToken == null)
            {
                _logger.LogError("Could not retrieve access token for server");
                context.Result = new UmaServerUnreachableResult();
                return;
            }

            var idToken = await GetIdToken(context);
            if (idToken == null)
            {
                _logger.LogError("No valid id token to request permission for {ResourceId}", resourceId);
                context.Result = new UmaServerUnreachableResult();
                return;
            }

            var permission = await _permissionClient.RequestPermission(
                    serverToken.AccessToken,
                    CancellationToken.None,
                    new PermissionRequest
                        { IdToken = idToken, ResourceSetId = resourceSetId, Scopes = _requiredResourceSetScopes })
                .ConfigureAwait(false);
            switch (permission)
            {
                case Option<TicketResponse>.Error error:
                    _logger.LogError("Title: {Title}, Details: {Detail}", error.Details.Title, error.Details.Detail);
                    context.Result = new UmaServerUnreachableResult();
                    break;
                case Option<TicketResponse>.Result result:
                    _logger.LogDebug(
                        "Ticket {TicketId} received from {Uri}",
                        result.Item.TicketId,
                        _permissionClient.Authority.AbsoluteUri);
                    context.Result = new UmaTicketResult(
                        new UmaTicketInfo(result.Item.TicketId, _permissionClient.Authority.AbsoluteUri, _realm));
                    break;
            }
        }

        private async Task<string?> GetIdToken(AuthorizationFilterContext context)
        {
            var request = context.HttpContext.Request;
            var idToken = await request.HttpContext.GetTokenAsync("id_token");
            if (!string.IsNullOrEmpty(idToken))
            {
                return idToken;
            }

            if (request.Query.TryGetValue(_idTokenHeader, out var token))
            {
                return token;
            }

            return AuthenticationHeaderValue.TryParse(request.Headers[_idTokenHeader], out var idTokenHeader)
                ? idTokenHeader.Parameter
                : default;
        }

        private async Task<GrantedTokenResponse?> HasServerAccessToken()
        {
            var option = await _tokenClient.GetToken(TokenRequest.FromScopes(UmaConstants.UmaProtectionScope))
                .ConfigureAwait(false);
            return option is Option<GrantedTokenResponse>.Result accessToken ? accessToken.Item : null;
        }

        private bool CheckHasScopeAccess(ClaimsPrincipal user, string? allowedOauthScope)
        {
            // If OAuth scope is allowed, then access is granted.
            if (allowedOauthScope == null
             || !user.HasClaim(
                    c => c.Type == StandardClaimNames.Scopes
                     && c.Value.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                            .Contains(allowedOauthScope)))
            {
                return false;
            }

            _logger.LogDebug(
                "Allowing access for user {Subject} in role {AllowedScope}",
                user.GetSubject(),
                allowedOauthScope);
            return true;
        }
    }
}
