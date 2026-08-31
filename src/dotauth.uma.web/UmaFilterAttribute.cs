namespace DotAuth.Uma.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DotAuth.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// An MVC authorization filter that enforces UMA 2.0 resource-level access control.
/// When the requesting party lacks sufficient permissions the filter delegates the UMA ticket-issuance
/// flow to <see cref="UmaBearerHandler"/> by calling
/// <see cref="IAuthenticationService.ChallengeAsync"/> with the resource context encoded in
/// <see cref="AuthenticationProperties"/>, rather than issuing tickets inline.
/// </summary>
[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface,
    AllowMultiple = true)]
public partial class UmaFilterAttribute : Attribute, IFilterFactory, IAuthorizeData
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
    /// <param name="resourceIdParameter">The route-value parameter name identifying the resource.</param>
    /// <param name="idTokenHeader">Header/query-parameter name where the ID token is read from.</param>
    /// <param name="allowedOauthScope">OAuth scope that bypasses UMA checks entirely.</param>
    /// <param name="realm">UMA realm for the challenge.</param>
    /// <param name="resourceSetAccessScope">Required UMA resource-set scopes.</param>
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
    /// Initializes a new instance of the <see cref="UmaFilterAttribute"/> class with a composite resource-ID format.
    /// </summary>
    public UmaFilterAttribute(
        string? resourceIdFormat,
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
    public bool IsReusable => true;

    /// <inheritdoc />
    public string? Policy { get; set; }

    /// <inheritdoc />
    public string? Roles { get; set; }

    /// <inheritdoc />
    public string? AuthenticationSchemes { get; set; }

    private partial class UmaAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IResourceMap _resourceMap;
        private readonly ILogger _logger;
        private readonly string? _realm;
        private readonly string[] _resourceIdParameters;
        private readonly string _idTokenHeader;
        private readonly string? _resourceSetIdFormat;
        private readonly string? _allowedOauthScope;
        private readonly string[] _requiredResourceSetScopes;

        public UmaAuthorizationFilter(
            IResourceMap resourceMap,
            ILogger logger,
            string[] resourceIdParameters,
            string idTokenHeader = IdTokenParameter,
            string? allowedOauthScope = null,
            string? realm = null,
            string? resourceSetIdFormat = null,
            params string[] requiredResourceSetScopes)
        {
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
                LogUserIsNotAuthenticated();
                // Not authenticated — delegate the full UMA ticket flow to the handler.
                await IssueChallengeAsync(context, resourceId: string.Empty, resourceSetId: null)
                    .ConfigureAwait(false);
                return;
            }

            if (CheckHasScopeAccess(user, _allowedOauthScope))
            {
                // User's OAuth token carries a scope that short-circuits UMA checks.
                return;
            }

            var values = _resourceIdParameters.Select(x => context.RouteData.Values[x]).ToArray();
            var resourceId = _resourceSetIdFormat == null
                ? string.Join("", values.Select(v => (v ?? "").ToString()).ToArray())
                : string.Format(_resourceSetIdFormat, values);
            LogAttemptingToMapResourceId(resourceId);

            var resourceSetId = await _resourceMap.GetResourceSetId(resourceId).ConfigureAwait(false);
            if (resourceSetId == null)
            {
                LogFailedToMapResourceIdToResourceSet(resourceId);
                await IssueChallengeAsync(context, resourceId, resourceSetId: null).ConfigureAwait(false);
                return;
            }

            if (user.CheckResourceAccess(resourceSetId, _requiredResourceSetScopes))
            {
                var subject = user.GetSubject();
                var scopes = string.Join(",", _requiredResourceSetScopes);
                LogReceivedValidTokenForResourceIdScopesScopesFromSubject(resourceId, scopes, subject);
                return;
            }

            // RPT is valid but does not include the required permissions — delegate to the handler.
            LogInsufficientPermissionsForResourceId(resourceId, resourceSetId);
            await IssueChallengeAsync(context, resourceId, resourceSetId).ConfigureAwait(false);
        }

        /// <summary>
        /// Calls <c>ChallengeAsync</c> on the <see cref="UmaBearerDefaults.AuthenticationScheme"/> scheme,
        /// passing the resolved <paramref name="resourceSetId"/> and required scopes via
        /// <see cref="AuthenticationProperties"/> so that
        /// <see cref="UmaBearerHandler.HandleChallengeAsync"/> can register the exact permission with the AS.
        /// </summary>
        private async Task IssueChallengeAsync(
            AuthorizationFilterContext context,
            string resourceId,
            string? resourceSetId)
        {
            var props = new AuthenticationProperties();
            if (!string.IsNullOrEmpty(resourceSetId))
            {
                props.Items["uma:resource_set_id"] = resourceSetId;
                props.Items["uma:scopes"] = string.Join(" ", _requiredResourceSetScopes);
            }

            LogDelegatingChallengeToHandler(resourceId, resourceSetId ?? "(unknown)");
            await context.HttpContext.ChallengeAsync(UmaBearerDefaults.AuthenticationScheme, props)
                .ConfigureAwait(false);

            // EmptyResult prevents MVC from writing a second response body after the handler responds.
            context.Result = new EmptyResult();
        }

        private bool CheckHasScopeAccess(ClaimsPrincipal user, string? allowedOauthScope)
        {
            if (allowedOauthScope == null
             || !user.HasClaim(
                     c => c.Type == StandardClaimNames.Scopes
                      && c.Value.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                             .Contains(allowedOauthScope)))
            {
                return false;
            }

            LogAllowingAccessForUserSubjectInRoleAllowedScope(user.GetSubject(), allowedOauthScope);
            return true;
        }

        [LoggerMessage(LogLevel.Information, "User is not authenticated")]
        partial void LogUserIsNotAuthenticated();

        [LoggerMessage(LogLevel.Debug, "Attempting to map {ResourceId}")]
        partial void LogAttemptingToMapResourceId(string resourceId);

        [LoggerMessage(LogLevel.Error, "Failed to map {ResourceId} to resource set")]
        partial void LogFailedToMapResourceIdToResourceSet(string resourceId);

        [LoggerMessage(LogLevel.Debug, "Received valid token for {ResourceId}, scopes {Scopes} from {Subject}")]
        partial void LogReceivedValidTokenForResourceIdScopesScopesFromSubject(string resourceId, string scopes, string? subject);

        [LoggerMessage(LogLevel.Information, "Insufficient permissions for {ResourceId} (resource set {ResourceSetId}), delegating to UMA handler")]
        partial void LogInsufficientPermissionsForResourceId(string resourceId, string resourceSetId);

        [LoggerMessage(LogLevel.Debug, "Delegating UMA challenge for {ResourceId} / {ResourceSetId} to authentication handler")]
        partial void LogDelegatingChallengeToHandler(string resourceId, string resourceSetId);

        [LoggerMessage(LogLevel.Debug, "Allowing access for user {Subject} in role {AllowedScope}")]
        partial void LogAllowingAccessForUserSubjectInRoleAllowedScope(string? subject, string allowedScope);
    }
}
