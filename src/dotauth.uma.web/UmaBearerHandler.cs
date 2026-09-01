namespace DotAuth.Uma.Web;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

/// <summary>
/// An <see cref="AuthenticationHandler{TOptions}"/> that validates UMA Requesting Party Tokens (RPTs)
/// and issues UMA permission tickets when access is denied or the token is absent.
/// </summary>
public partial class UmaBearerHandler : AuthenticationHandler<UmaBearerOptions>
{
    private readonly ITokenClient _tokenClient;
    private readonly IResourceMap _resourceMap;
    private readonly IUmaPermissionClient _permissionClient;

    /// <summary>
    /// Initializes a new instance of <see cref="UmaBearerHandler"/>.
    /// All UMA-specific configuration (realm, resource ID parameters, etc.) is read from
    /// <see cref="UmaBearerOptions"/> at request time so that the handler is fully injectable
    /// via the standard ASP.NET Core <c>AddScheme&lt;TOptions,THandler&gt;</c> mechanism.
    /// </summary>
    public UmaBearerHandler(
        IOptionsMonitor<UmaBearerOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IResourceMap resourceMap,
        IUmaPermissionClient permissionClient,
        ITokenClient tokenClient)
        : base(options, logger, encoder)
    {
        _resourceMap = resourceMap;
        _permissionClient = permissionClient;
        _tokenClient = tokenClient;
    }

    /// <summary>
    /// The handler calls methods on the events which give the application control at certain points where processing is occurring.
    /// If it is not provided a default instance is supplied which does nothing when the methods are called.
    /// </summary>
    protected new UmaBearerEvents Events
    {
        get { return (UmaBearerEvents)base.Events!; }
        set { base.Events = value; }
    }

    /// <inheritdoc />
    protected override Task<object> CreateEventsAsync() => Task.FromResult<object>(new UmaBearerEvents());

    /// <summary>
    /// Validates the RPT (Requesting Party Token) carried in the <c>Authorization: Bearer</c> header.
    /// On success the resulting <see cref="ClaimsPrincipal"/> preserves the <c>permissions</c> claim
    /// so that <see cref="ClaimsPrincipalExtensions.CheckResourceAccess"/> works in the
    /// downstream authorization layer.
    /// </summary>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            var messageReceivedContext = new MessageReceivedContext(Context, Scheme, Options);

            await Events.OnMessageReceived(messageReceivedContext).ConfigureAwait(false);
            if (messageReceivedContext.Result != null)
            {
                return messageReceivedContext.Result;
            }

            var token = messageReceivedContext.Token;

            if (string.IsNullOrEmpty(token))
            {
                var authorization = Request.Headers.Authorization.ToString();

                if (string.IsNullOrEmpty(authorization))
                {
                    return AuthenticateResult.NoResult();
                }

                if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = authorization["Bearer ".Length..].Trim();
                }

                if (string.IsNullOrEmpty(token))
                {
                    return AuthenticateResult.NoResult();
                }
            }

            var tvp = await SetupTokenValidationParametersAsync().ConfigureAwait(false);

            // Ensure the non-standard UMA 'permissions' claim is never remapped to a different
            // claim URI by the inbound-claim-type mapping.  We preserve it by keeping the original
            // claim type after validation via explicit pass-through (see below).
            List<Exception>? validationFailures = null;
            SecurityToken? validatedToken = null;
            ClaimsPrincipal? principal = null;

            if (!Options.UseSecurityTokenValidators)
            {
                foreach (var tokenHandler in Options.TokenHandlers)
                {
                    try
                    {
                        var tokenValidationResult = await tokenHandler.ValidateTokenAsync(token, tvp).ConfigureAwait(false);
                        if (tokenValidationResult.IsValid)
                        {
                            principal = new ClaimsPrincipal(tokenValidationResult.ClaimsIdentity);
                            validatedToken = tokenValidationResult.SecurityToken;

                            // Preserve the 'permissions' claim under its original name so
                            // CheckResourceAccess can deserialise it correctly.
                            EnsurePermissionsClaimPreserved(principal, tokenValidationResult.ClaimsIdentity);
                            break;
                        }

                        validationFailures ??= new List<Exception>(1);
                        RecordTokenValidationError(
                            tokenValidationResult.Exception ??
                            new SecurityTokenValidationException(
                                $"The TokenHandler: '{tokenHandler}', was unable to validate the Token."),
                            validationFailures);
                    }
                    catch (Exception ex)
                    {
                        validationFailures ??= new List<Exception>(1);
                        RecordTokenValidationError(ex, validationFailures);
                    }
                }
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                foreach (var validator in Options.SecurityTokenValidators)
                {
                    if (!validator.CanReadToken(token))
                    {
                        continue;
                    }

                    try
                    {
                        principal = validator.ValidateToken(token, tvp, out validatedToken);
                    }
                    catch (Exception ex)
                    {
                        validationFailures ??= new List<Exception>(1);
                        RecordTokenValidationError(ex, validationFailures);
                    }
                }
#pragma warning restore CS0618 // Type or member is obsolete
            }

            if (principal != null && validatedToken != null)
            {
                Logger.LogDebug("Successfully validated the token");

                // Check whether this RPT covers the current resource.
                // This makes the authentication scheme self-sufficient: an RPT that does not
                // include any permission for the resolved resource set is treated the same as
                // a missing token — authentication fails and HandleChallengeAsync issues a new
                // permission ticket.  The check is skipped when no ResourceIdParameters are
                // configured so that the handler can still be used purely for JWT validation.
                if (Options.ResourceIdParameters.Length > 0)
                {
                    var resourceSetId = await ResolveResourceSetIdFromRouteAsync().ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(resourceSetId)
                        && !principal.CheckResourceAccess(resourceSetId))
                    {
                        // Store the resolved ID so HandleChallengeAsync can issue the right ticket.
                        Context.Items["uma:resource_set_id"] = resourceSetId;
                        LogRptDoesNotCoverResourceSet(Logger, resourceSetId);
                        return AuthenticateResult.Fail(
                            $"The RPT does not include permission for resource set '{resourceSetId}'.");
                    }
                }

                var tokenValidatedContext = new TokenValidatedContext(Context, Scheme, Options)
                {
                    Principal = principal,
                    SecurityToken = validatedToken,
                    Properties =
                    {
                        ExpiresUtc = GetSafeDateTime(validatedToken.ValidTo),
                        IssuedUtc = GetSafeDateTime(validatedToken.ValidFrom)
                    }
                };

                await Events.OnTokenValidated(tokenValidatedContext).ConfigureAwait(false);
                if (tokenValidatedContext.Result != null)
                {
                    return tokenValidatedContext.Result;
                }

                if (Options.SaveToken)
                {
                    tokenValidatedContext.Properties.StoreTokens([
                        new AuthenticationToken { Name = "access_token", Value = token }
                    ]);
                }

                tokenValidatedContext.Success();
                return tokenValidatedContext.Result!;
            }

            if (validationFailures != null)
            {
                var authenticationFailedContext = new AuthenticationFailedContext(Context, Scheme, Options)
                {
                    Exception = (validationFailures.Count == 1)
                        ? validationFailures[0]
                        : new AggregateException(validationFailures)
                };

                await Events.OnAuthenticationFailed(authenticationFailedContext).ConfigureAwait(false);
                return authenticationFailedContext.Result ??
                    AuthenticateResult.Fail(authenticationFailedContext.Exception);
            }

            if (!Options.UseSecurityTokenValidators)
            {
                return AuthenticateResults.TokenHandlerUnableToValidate;
            }

            return AuthenticateResults.ValidatorNotFound;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception occurred while processing message");

            var authenticationFailedContext = new AuthenticationFailedContext(Context, Scheme, Options)
            {
                Exception = ex
            };

            await Events.OnAuthenticationFailed(authenticationFailedContext).ConfigureAwait(false);
            if (authenticationFailedContext.Result != null)
            {
                return authenticationFailedContext.Result;
            }

            throw;
        }
    }

    /// <summary>
    /// Implements the UMA 2.0 challenge flow: obtains a protection API token, registers a permission
    /// with the Authorization Server's Permission Endpoint, and returns
    /// <c>HTTP 401 WWW-Authenticate: UMA realm="…", as_uri="…", ticket="…"</c>.
    /// Returns HTTP 503 when the protection token or the Permission Endpoint is unavailable.
    /// </summary>
    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var authResult = await HandleAuthenticateOnceSafeAsync().ConfigureAwait(false);
        var eventContext = new UmaBearerChallengeContext(Context, Scheme, Options, properties)
        {
            AuthenticateFailure = authResult.Failure
        };

        await IssuePermissionTicketAsync(eventContext, properties).ConfigureAwait(false);
    }

    /// <summary>
    /// Implements the UMA 2.0 insufficient-permissions response.
    /// Per UMA 2.0 §3.3.1 the RS must return HTTP 401 + a new permission ticket even when the
    /// client presented a valid RPT that lacked sufficient permissions — not HTTP 403.
    /// HTTP 403 is reserved for cases where the AS has definitively denied the request.
    /// </summary>
    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        var eventContext = new UmaBearerChallengeContext(Context, Scheme, Options, properties);

        // Raise OnForbidden before any headers are written so the application can override.
        var forbiddenContext = new ForbiddenContext(Context, Scheme, Options);
        await Events.OnForbidden(forbiddenContext).ConfigureAwait(false);

        await IssuePermissionTicketAsync(eventContext, properties).ConfigureAwait(false);
    }

    // ---------------------------------------------------------------------------
    // Shared UMA ticket-issuance logic
    // ---------------------------------------------------------------------------

    /// <summary>
    /// Core UMA ticket-issuance routine shared by <see cref="HandleChallengeAsync"/> and
    /// <see cref="HandleForbiddenAsync"/>:
    /// <list type="number">
    ///   <item>Resolves the resource-set ID from <c>HttpContext.Items</c>, <paramref name="properties"/>, or route values.</item>
    ///   <item>Obtains a protection API token from <see cref="ITokenClient"/>.</item>
    ///   <item>Calls the Permission Endpoint via <see cref="IUmaPermissionClient"/>.</item>
    ///   <item>Writes <c>HTTP 401 WWW-Authenticate: UMA …</c>; on failure writes HTTP 503.</item>
    /// </list>
    /// </summary>
    private async Task IssuePermissionTicketAsync(
        UmaBearerChallengeContext eventContext,
        AuthenticationProperties properties)
    {
        // 1. Resolve the resource-set ID — from HttpContext.Items (set during HandleAuthenticateAsync
        //    when the RPT was valid but lacked permissions), then from properties, then from route values.
        var resourceSetId =
            (Context.Items.TryGetValue("uma:resource_set_id", out var ctxRsid) ? ctxRsid as string : null)
            ?? (properties.Items.TryGetValue("uma:resource_set_id", out var rsid) ? rsid : null);

        if (string.IsNullOrEmpty(resourceSetId))
        {
            resourceSetId = await ResolveResourceSetIdFromRouteAsync().ConfigureAwait(false);
        }

        if (string.IsNullOrEmpty(resourceSetId))
        {
            LogFailedToResolveResourceSetId(Logger);
            Response.StatusCode = 503;
            return;
        }

        // 2. Obtain the required scopes — from properties or from the permission client.
        string[] scopes;
        if (properties.Items.TryGetValue("uma:scopes", out var scopeStr) && !string.IsNullOrEmpty(scopeStr))
        {
            scopes = scopeStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }
        else
        {
            scopes = await _permissionClient.GetResourceSetScopes(resourceSetId, Context.RequestAborted)
                .ConfigureAwait(false);
        }

        // 3. Obtain a protection API token (PAT) with scope uma_protection.
        var patOption = await _tokenClient
            .GetToken(TokenRequest.FromScopes(DotAuth.Uma.UmaConstants.UmaProtectionScope), Context.RequestAborted)
            .ConfigureAwait(false);

        if (patOption is not Option<GrantedTokenResponse>.Result patResult)
        {
            LogCouldNotRetrieveProtectionToken(Logger);
            Response.StatusCode = 503;
            return;
        }

        // 4. Register permissions at the AS Permission Endpoint.
        Option<TicketResponse> permissionOption;
        try
        {
            permissionOption = await _permissionClient.RequestPermission(
                patResult.Item.AccessToken,
                Context.RequestAborted,
                new PermissionRequest { ResourceSetId = resourceSetId, Scopes = scopes })
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Permission endpoint call failed");
            Response.StatusCode = 503;
            return;
        }

        if (permissionOption is not Option<TicketResponse>.Result ticketResult)
        {
            Logger.LogError("Permission endpoint returned an error");
            Response.StatusCode = 503;
            return;
        }

        // 5. Populate the challenge context and raise the OnChallenge event.
        eventContext.TicketId = ticketResult.Item.TicketId;
        eventContext.AsUri = _permissionClient.Authority.AbsoluteUri;

        await Events.OnChallenge(eventContext).ConfigureAwait(false);
        if (eventContext.Handled)
        {
            return;
        }

        // 6. Write the UMA-compliant 401 challenge.
        LogTicketIssuedForResourceSet(Logger, resourceSetId, ticketResult.Item.TicketId);

        Response.StatusCode = 401;

        var sb = new StringBuilder("UMA");
        if (!string.IsNullOrEmpty(Options.Realm))
        {
            sb.Append($" realm=\"{Options.Realm}\",");
        }

        sb.Append($" as_uri=\"{eventContext.AsUri}\", ticket=\"{eventContext.TicketId}\"");
        Response.Headers.Append(HeaderNames.WWWAuthenticate, sb.ToString());
    }

    private async Task<string?> ResolveResourceSetIdFromRouteAsync()
    {
        if (Options.ResourceIdParameters.Length == 0)
        {
            return null;
        }

        var values = Options.ResourceIdParameters.Select(p => Request.RouteValues[p]).ToArray();
        var resourceId = Options.ResourceSetIdFormat is null
            ? string.Join("", values.Select(v => (v ?? "").ToString()))
            : string.Format(Options.ResourceSetIdFormat, values);

        return await _resourceMap.GetResourceSetId(resourceId, Context.RequestAborted).ConfigureAwait(false);
    }

    private void RecordTokenValidationError(Exception? exception, List<Exception> exceptions)
    {
        if (exception != null)
        {
            Logger.LogInformation(exception, "Failed to validate the token");
            exceptions.Add(exception);
        }

        if (Options is { RefreshOnIssuerKeyNotFound: true, ConfigurationManager: not null }
         && exception is SecurityTokenSignatureKeyNotFoundException)
        {
            Options.ConfigurationManager.RequestRefresh();
        }
    }

    private async Task<TokenValidationParameters> SetupTokenValidationParametersAsync()
    {
        var tokenValidationParameters = Options.TokenValidationParameters.Clone();

        if (Options.ConfigurationManager is BaseConfigurationManager baseConfigurationManager)
        {
            tokenValidationParameters.ConfigurationManager = baseConfigurationManager;
        }
        else
        {
            if (Options.ConfigurationManager == null)
            {
                return tokenValidationParameters;
            }

            var configuration = await Options.ConfigurationManager.GetConfigurationAsync(Context.RequestAborted).ConfigureAwait(false);
            var issuers = new[] { configuration.Issuer };
            tokenValidationParameters.ValidIssuers = (tokenValidationParameters.ValidIssuers == null
                ? issuers
                : tokenValidationParameters.ValidIssuers.Concat(issuers));
            tokenValidationParameters.IssuerSigningKeys = (tokenValidationParameters.IssuerSigningKeys == null
                ? configuration.SigningKeys
                : tokenValidationParameters.IssuerSigningKeys.Concat(configuration.SigningKeys));
        }

        return tokenValidationParameters;
    }

    private static DateTime? GetSafeDateTime(DateTime dateTime)
    {
        if (dateTime == DateTime.MinValue)
        {
            return null;
        }

        return dateTime;
    }

    /// <summary>
    /// Ensures the UMA <c>permissions</c> claim survives inbound claim-type mapping.
    /// The <see cref="Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler"/> with
    /// <c>MapInboundClaims = true</c> renames well-known OIDC claim types to XML URIs.
    /// <c>permissions</c> is not a standard OIDC claim and should pass through unchanged,
    /// but this method explicitly re-adds it under its original name if mapping has removed it.
    /// </summary>
    private static void EnsurePermissionsClaimPreserved(ClaimsPrincipal principal, ClaimsIdentity identity)
    {
        // "permissions" is the UMA RPT claim name (DotAuth.Shared.UmaConstants.RptClaims.Permissions).
        // DotAuth.Shared.UmaConstants is internal; use the string literal directly.
        const string permissionsClaimType = "permissions";

        // If the claim is already present under its original name, nothing to do.
        if (identity.HasClaim(c => c.Type == permissionsClaimType))
        {
            return;
        }

        // Look for a renamed version (mapped to a different claim URI) and re-add under the
        // canonical name so CheckResourceAccess can find it.
        var renamed = principal.FindAll(
            c => c.Type.EndsWith("/permissions", StringComparison.OrdinalIgnoreCase) ||
                 c.Type.Equals(permissionsClaimType, StringComparison.OrdinalIgnoreCase));

        foreach (var claim in renamed)
        {
            if (!identity.HasClaim(permissionsClaimType, claim.Value))
            {
                identity.AddClaim(new Claim(permissionsClaimType, claim.Value, claim.ValueType, claim.Issuer));
            }
        }
    }

    [LoggerMessage(LogLevel.Error, "Could not retrieve protection API token (uma_protection scope)")]
    static partial void LogCouldNotRetrieveProtectionToken(ILogger logger);

    [LoggerMessage(LogLevel.Error, "Could not resolve a resource-set ID from route values")]
    static partial void LogFailedToResolveResourceSetId(ILogger logger);

    [LoggerMessage(LogLevel.Debug, "Permission ticket {TicketId} issued for resource set {ResourceSetId}")]
    static partial void LogTicketIssuedForResourceSet(ILogger logger, string resourceSetId, string ticketId);

    [LoggerMessage(LogLevel.Information, "RPT does not include any permission for resource set {ResourceSetId}")]
    static partial void LogRptDoesNotCoverResourceSet(ILogger logger, string resourceSetId);
}
