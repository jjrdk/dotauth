namespace DotAuth.Client;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;

/// <summary>
/// Defines the abstract base class for authentication clients.
/// </summary>
public abstract class AuthenticateClientBase : IDisposable
{
    private readonly AuthenticateClientOptions _options;
    private readonly HttpClient _httpClient = new(new HttpClientHandler { AllowAutoRedirect = false });
    private readonly TokenClient _tokenClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticateClientBase"/> class.
    /// </summary>
    /// <param name="options">The <see cref="AuthenticateClientOptions"/>.</param>
    protected AuthenticateClientBase(AuthenticateClientOptions options)
    {
        _options = options;
        _tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials(options.ClientId, options.ClientSecret),
            () => _httpClient,
            options.Authority);
    }

    /// <summary>
    /// Logs the user in using an authentication code flow.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns>An <see cref="Option{T}"/> as an async operation.</returns>
    public async Task<Option<GrantedTokenResponse>> LogIn(CancellationToken cancellationToken)
    {
        var pkce = CodeChallengeMethods.Rs256.BuildPkce();
        var request = new AuthorizationRequest(
                _options.Scopes,
                new[] { ResponseTypeNames.Code },
                _options.ClientId,
                _options.Callback,
                pkce.CodeChallenge,
                CodeChallengeMethods.Rs256,
                Guid.NewGuid().ToString("N"))
        {
            nonce = Guid.NewGuid().ToString("N"),
            response_mode = _options.ResponseMode
        };
        var uriOption = await _tokenClient.GetAuthorization(request, cancellationToken);
        if (uriOption is Option<Uri>.Error error)
        {
            return error.Details;
        }

        var result = (Option<Uri>.Result)uriOption;
        var code = await Authenticate(result.Item, _options.Callback);
        var tokenOption = await _tokenClient.GetToken(
            TokenRequest.FromAuthorizationCode(
                code,
                _options.Callback.AbsoluteUri,
                pkce.CodeVerifier),
            cancellationToken);
        return tokenOption;
    }

    /// <summary>
    /// The platform specific authentication interaction flow.
    /// </summary>
    /// <param name="uri">The <see cref="Uri"/> of where the authentication code flow is initiated.</param>
    /// <param name="callback">The <see cref="Uri"/> to receive the code response.</param>
    /// <returns>The authentication code.</returns>
    protected abstract Task<string> Authenticate(Uri uri, Uri callback);

    /// <inheritdoc />
    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}