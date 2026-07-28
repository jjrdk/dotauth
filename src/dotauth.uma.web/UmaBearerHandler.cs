namespace DotAuth.Uma.Web;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
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
/// An <see cref="AuthenticationHandler{TOptions}"/> that can perform JWT-bearer based authentication.
/// </summary>
public partial class UmaBearerHandler : AuthenticationHandler<UmaBearerOptions>
{
    private const string IdTokenParameter = "id_token";
    private readonly string _idTokenHeader;
    private readonly string[] _resourceIdParameters;
    private readonly ITokenClient _tokenClient;
    private readonly IResourceMap _resourceMap;
    private readonly IUmaPermissionClient _permissionClient;
    private readonly string? _resourceSetIdFormat;
    private readonly string? _realm;

    /// <summary>
    /// Initializes a new instance of <see cref="UmaBearerHandler"/>.
    /// </summary>
    /// <inheritdoc />
    public UmaBearerHandler(
        IOptionsMonitor<UmaBearerOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IResourceMap resourceMap,
        IUmaPermissionClient permissionClient,
        ITokenClient tokenClient,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
        string? resourceSetIdFormat,
        string idTokenHeader = IdTokenParameter,
        string[]? resourceIdParameters = null,
        string? realm = null)
        : base(options, logger, encoder)
    {
        _resourceMap = resourceMap;
        _permissionClient = permissionClient;
        _tokenClient = tokenClient;
        _resourceSetIdFormat = resourceSetIdFormat;
        _idTokenHeader = idTokenHeader;
        _realm = realm;
        _resourceIdParameters = resourceIdParameters ?? [];
    }

    /// <summary>
    /// The handler calls methods on the events which give the application control at certain points where processing is occurring.
    /// If it is not provided a default instance is supplied which does nothing when the methods are called.
    /// </summary>
    protected new UmaBearerEvents Events
    {
        get => (UmaBearerEvents)base.Events!;
        set => base.Events = value;
    }

    /// <inheritdoc />
    protected override Task<object> CreateEventsAsync() => Task.FromResult<object>(new UmaBearerEvents());

    /// <summary>
    /// Searches the 'Authorization' header for a 'Bearer' token. If the 'Bearer' token is found, it is validated using <see cref="TokenValidationParameters"/> set in the options.
    /// </summary>
    /// <returns></returns>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            // Give application opportunity to find from a different location, adjust, or reject token
            var messageReceivedContext = new MessageReceivedContext(Context, Scheme, Options);

            // event can set the token
            await Events.OnMessageReceived(messageReceivedContext).ConfigureAwait(false);
            if (messageReceivedContext.Result != null)
            {
                return messageReceivedContext.Result;
            }

            // If application retrieved token from somewhere else, use that.
            var token = messageReceivedContext.Token;

            if (string.IsNullOrEmpty(token))
            {
                var authorization = Request.Headers.Authorization.ToString();

                // If no authorization header found, nothing to process further
                if (string.IsNullOrEmpty(authorization))
                {
                    return AuthenticateResult.NoResult();
                }

                if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = authorization["Bearer ".Length..].Trim();
                }

                // If no token found, no further work possible
                if (string.IsNullOrEmpty(token))
                {
                    return AuthenticateResult.NoResult();
                }
            }

            var tvp = await SetupTokenValidationParametersAsync().ConfigureAwait(false);
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

    private void RecordTokenValidationError(Exception? exception, List<Exception> exceptions)
    {
        if (exception != null)
        {
            Logger.LogInformation(exception, "Failed to validate the token");
            exceptions.Add(exception);
        }

        // Refresh the configuration for exceptions that may be caused by key rollovers. The user can also request a refresh in the event.
        // Refreshing on SecurityTokenSignatureKeyNotFound may be redundant if Last-Known-Good is enabled, it won't do much harm, most likely will be a nop.
        if (Options is { RefreshOnIssuerKeyNotFound: true, ConfigurationManager: not null }
         && exception is SecurityTokenSignatureKeyNotFoundException)
        {
            Options.ConfigurationManager.RequestRefresh();
        }
    }

    private async Task<TokenValidationParameters> SetupTokenValidationParametersAsync()
    {
        // Clone to avoid cross request race conditions for updated configurations.
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

            // GetConfigurationAsync has a time interval that must pass before new http request will be issued.
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
        // Assigning DateTime.MinValue or default(DateTime) to a DateTimeOffset when in a UTC+X timezone will throw
        // Since we don't really care about DateTime.MinValue in this case let's just set the field to null
        if (dateTime == DateTime.MinValue)
        {
            return null;
        }

        return dateTime;
    }

    /// <inheritdoc />
    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var authResult = await HandleAuthenticateOnceSafeAsync().ConfigureAwait(false);
        var eventContext = new UmaBearerChallengeContext(Context, Scheme, Options, properties)
        {
            AuthenticateFailure = authResult.Failure
        };

        // Avoid returning error=invalid_token if the error is not caused by an authentication failure (e.g missing token).
        if (Options.IncludeErrorDetails && eventContext.AuthenticateFailure != null)
        {
            eventContext.Error = "invalid_token";
            eventContext.ErrorDescription = CreateErrorDescription(eventContext.AuthenticateFailure);
        }

        await Events.OnChallenge(eventContext).ConfigureAwait(false);
        if (eventContext.Handled)
        {
            return;
        }

        Response.StatusCode = 401;

        if (string.IsNullOrEmpty(eventContext.Error) &&
            string.IsNullOrEmpty(eventContext.ErrorDescription) &&
            string.IsNullOrEmpty(eventContext.ErrorUri))
        {
            Response.Headers.Append(HeaderNames.WWWAuthenticate, Options.Challenge);
        }
        else
        {
            // https://tools.ietf.org/html/rfc6750#section-3.1
            // WWW-Authenticate: Bearer realm="example", error="invalid_token", error_description="The access token expired"
            var builder = new StringBuilder(Options.Challenge);
            if (Options.Challenge.IndexOf(' ') > 0)
            {
                // Only add a comma after the first param, if any
                builder.Append(',');
            }

            if (!string.IsNullOrEmpty(eventContext.Error))
            {
                builder.Append(" error=\"");
                builder.Append(eventContext.Error);
                builder.Append('\"');
            }

            if (!string.IsNullOrEmpty(eventContext.ErrorDescription))
            {
                if (!string.IsNullOrEmpty(eventContext.Error))
                {
                    builder.Append(',');
                }

                builder.Append(" error_description=\"");
                builder.Append(eventContext.ErrorDescription);
                builder.Append('\"');
            }

            if (!string.IsNullOrEmpty(eventContext.ErrorUri))
            {
                if (!string.IsNullOrEmpty(eventContext.Error) ||
                    !string.IsNullOrEmpty(eventContext.ErrorDescription))
                {
                    builder.Append(',');
                }

                builder.Append(" error_uri=\"");
                builder.Append(eventContext.ErrorUri);
                builder.Append('\"');
            }

            Response.Headers.Append(HeaderNames.WWWAuthenticate, builder.ToString());
        }
    }

    private async Task VerifyUmaAccess()
    {
        var values = _resourceIdParameters.Select(x => Context.Request.RouteValues[x]).ToArray();
        var resourceId = _resourceSetIdFormat == null
            ? string.Join("", values.Select(v => (v ?? "").ToString()).ToArray())
            : string.Format(_resourceSetIdFormat, values);
        LogAttemptingToMapResourceid(Logger, resourceId);
        var resourceSetId = await _resourceMap.GetResourceSetId(resourceId).ConfigureAwait(false);
        if (resourceSetId == null)
        {
            LogFailedToMapResourceidToResourceSet(Logger, resourceId);
            await Results.Unauthorized().ExecuteAsync(Context).ConfigureAwait(false);
            return;
        }

        var requiredResourceSetScopes = await _permissionClient.GetResourceSetScopes(resourceSetId).ConfigureAwait(false);
        if (Context.User.CheckResourceAccess(resourceSetId, requiredResourceSetScopes))
        {
            var subject = Context.User.GetSubject();
            var scopes = string.Join(",", requiredResourceSetScopes);
            LogReceivedValidTokenForResourceIdScopesScopesFromSubject(Logger, resourceId, scopes, subject);
            return;
        }

        var serverToken = await HasServerAccessToken().ConfigureAwait(false);
        if (serverToken == null)
        {
            LogCouldNotRetrieveAccessTokenForServer(Logger);
            await new UmaServerUnreachableResult().ExecuteAsync(Context).ConfigureAwait(false);
            return;
        }

        var idToken = await GetIdToken(Context.Request).ConfigureAwait(false);
        if (idToken == null)
        {
            LogNoValidIdTokenToRequestPermissionForResourceid(Logger, resourceId);
            await new UmaServerUnreachableResult().ExecuteAsync(Context).ConfigureAwait(false);
            return;
        }

        var permission = await _permissionClient.RequestPermission(
                serverToken.AccessToken,
                CancellationToken.None,
                new PermissionRequest
                    { IdToken = idToken, ResourceSetId = resourceSetId, Scopes = requiredResourceSetScopes })
            .ConfigureAwait(false);
        switch (permission)
        {
            case Option<TicketResponse>.Error error:
                LogTitleTitleDetailsDetail(Logger, error.Details.Title, error.Details.Detail);
                await new UmaServerUnreachableResult().ExecuteAsync(Context).ConfigureAwait(false);
                break;
            case Option<TicketResponse>.Result result:
                LogTicketTicketidReceivedFromUri(Logger, result.Item.TicketId, _permissionClient.Authority.AbsoluteUri);
                await new UmaTicketResult(
                    new UmaTicketInfo(result.Item.TicketId, _permissionClient.Authority.AbsoluteUri, _realm))
                    .ExecuteAsync(Context).ConfigureAwait(false);
                break;
        }
    }

    private async Task<GrantedTokenResponse?> HasServerAccessToken()
    {
        var option = await _tokenClient.GetToken(TokenRequest.FromScopes(UmaConstants.UmaProtectionScope))
            .ConfigureAwait(false);
        return option is Option<GrantedTokenResponse>.Result accessToken ? accessToken.Item : null;
    }

    private async Task<string?> GetIdToken(HttpRequest request)
    {
        var idToken = await request.HttpContext.GetTokenAsync("id_token").ConfigureAwait(false);
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
            : null;
    }

    /// <inheritdoc />
    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        var forbiddenContext = new ForbiddenContext(Context, Scheme, Options);

        if (Response.StatusCode == 403)
        {
            // No-op
        }
        else if (Response.HasStarted)
        {
            Logger.LogDebug("Unable to reject the response as forbidden, it has already started");
        }
        else
        {
            Response.StatusCode = 403;
        }

        return Events.OnForbidden(forbiddenContext);
    }

    private static string CreateErrorDescription(Exception authFailure)
    {
        IReadOnlyCollection<Exception> exceptions;
        if (authFailure is AggregateException agEx)
        {
            exceptions = agEx.InnerExceptions;
        }
        else
        {
            exceptions = [authFailure];
        }

        var messages = new List<string>(exceptions.Count);

        foreach (var ex in exceptions)
        {
            // Order sensitive, some of these exceptions derive from others
            // and we want to display the most specific message possible.
            var message = ex switch
            {
                SecurityTokenInvalidAudienceException stia =>
                    $"The audience '{stia.InvalidAudience ?? "(null)"}' is invalid",
                SecurityTokenInvalidIssuerException stii => $"The issuer '{stii.InvalidIssuer ?? "(null)"}' is invalid",
                SecurityTokenNoExpirationException _ => "The token has no expiration",
                SecurityTokenInvalidLifetimeException stil => "The token lifetime is invalid; NotBefore: "
                  + $"'{stil.NotBefore?.ToString(CultureInfo.InvariantCulture) ?? "(null)"}'"
                  + $", Expires: '{stil.Expires?.ToString(CultureInfo.InvariantCulture) ?? "(null)"}'",
                SecurityTokenNotYetValidException stnyv =>
                    $"The token is not valid before '{stnyv.NotBefore.ToString(CultureInfo.InvariantCulture)}'",
                SecurityTokenExpiredException ste =>
                    $"The token expired at '{ste.Expires.ToString(CultureInfo.InvariantCulture)}'",
                SecurityTokenSignatureKeyNotFoundException _ => "The signature key was not found",
                SecurityTokenInvalidSignatureException _ => "The signature is invalid",
                _ => null,
            };

            if (message is not null)
            {
                messages.Add(message);
            }
        }

        return string.Join("; ", messages);
    }

    [LoggerMessage(LogLevel.Debug, "Attempting to map {ResourceId}")]
    static partial void LogAttemptingToMapResourceid(ILogger logger, string resourceId);

    [LoggerMessage(LogLevel.Error, "Failed to map {ResourceId} to resource set")]
    static partial void LogFailedToMapResourceidToResourceSet(ILogger logger, string resourceId);

    [LoggerMessage(LogLevel.Debug, "Received valid token for {ResourceId}, scopes {Scopes} from {Subject}")]
    static partial void LogReceivedValidTokenForResourceIdScopesScopesFromSubject(ILogger logger, string resourceId, string scopes, string? subject = "");

    [LoggerMessage(LogLevel.Error, "Could not retrieve access token for server")]
    static partial void LogCouldNotRetrieveAccessTokenForServer(ILogger logger);

    [LoggerMessage(LogLevel.Error, "No valid id token to request permission for {ResourceId}")]
    static partial void LogNoValidIdTokenToRequestPermissionForResourceid(ILogger logger, string resourceId);

    [LoggerMessage(LogLevel.Error, "Title: {Title}, Details: {Detail}")]
    static partial void LogTitleTitleDetailsDetail(ILogger logger, string title, string detail);

    [LoggerMessage(LogLevel.Debug, "Ticket {TicketId} received from {Uri}")]
    static partial void LogTicketTicketidReceivedFromUri(ILogger logger, string ticketId, string uri);
}
