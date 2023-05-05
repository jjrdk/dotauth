namespace dotauth.authentication;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Defines the DotAuth authentication handler.
/// </summary>
/// <typeparam name="T">The type of <see cref="DotAuthOptions"/>.</typeparam>
public class DotAuthHandler<T> : RemoteAuthenticationHandler<T>
    where T : DotAuthOptions, new()
{
    private readonly TokenClient _client;
    private Pkce? _pkce;

    /// <summary>
    /// Initializes a new instance of the <see cref="DotAuthHandler{T}"/> class.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    /// <param name="encoder"></param>
    /// <param name="clock"></param>
    public DotAuthHandler(
        IOptionsMonitor<T> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock)
    {
        _client = new TokenClient(
            TokenCredentials.FromClientCredentials(options.CurrentValue.ClientId, options.CurrentValue.ClientSecret),
            () => options.CurrentValue.Backchannel,
            options.CurrentValue.Authority);
        _pkce = Options.UsePkce ? Options.CodeChallengeMethod.BuildPkce() : default;
    }

    /// <inheritdoc />
    protected override async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
    {
        var query = Request.Query;
        var code = query["code"];
        if (!string.IsNullOrEmpty(code.ToString()))
        {
            return HandleRequestResult.Fail("No code received.");
        }

        var option = await _client.GetToken(TokenRequest.FromAuthorizationCode(
            code!,
            BuildRedirectUri(Options.CallbackPath),
            _pkce?.CodeVerifier));
        switch (option)
        {
            case Option<GrantedTokenResponse>.Result result:
                var handler = new JwtSecurityTokenHandler();
                var v = await handler.ValidateTokenAsync(result.Item.AccessToken, Options.TokenValidationParameters);
                return HandleRequestResult.Success(
                    new AuthenticationTicket(new ClaimsPrincipal(v.ClaimsIdentity),
                        DotAuthDefaults.AuthenticationScheme));
            case Option<GrantedTokenResponse>.Error error:
                return HandleRequestResult.Fail(error.Details.Title);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    /// <inheritdoc />
    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var authorizationUriOption = await _client.GetAuthorization(
            new AuthorizationRequest(
                Options.Scopes,
                Options.ResponseTypes,
                Options.ClientId,
                new Uri(BuildRedirectUri(Options.CallbackPath)),
                _pkce?.CodeChallenge,
                Options.UsePkce ? Options.CodeChallengeMethod : null,
                Guid.NewGuid().ToString("N")));

        var authenticationScheme = new AuthenticationScheme(DotAuthDefaults.AuthenticationScheme,
            DotAuthDefaults.AuthenticationScheme,
            GetType());
        switch (authorizationUriOption)
        {
            case Option<Uri>.Result result:
                await Options.Events.RedirectToAuthorizationEndpoint(new RedirectContext<DotAuthOptions>(
                    Request.HttpContext,
                    authenticationScheme,
                    Options,
                    new AuthenticationProperties(),
                    result.Item.AbsoluteUri
                ));
                break;
            case Option<Uri>.Error error:
            {
                var context = new RemoteFailureContext(Context,
                    authenticationScheme,
                    Options,
                    new Exception(error.Details.Title));
                await Options.Events.OnRemoteFailure(
                    context);
                break;
            }
        }
    }
}
