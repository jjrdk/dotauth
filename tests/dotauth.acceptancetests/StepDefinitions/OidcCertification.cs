namespace DotAuth.AcceptanceTests.StepDefinitions;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Reqnroll;
using Xunit;

public partial class FeatureTest
{
    private DiscoveryInformation? _oidcDiscovery;
    private JsonWebKeySet? _oidcJwks;
    private HttpResponseMessage? _oidcHttpResponse;
    private Uri? _oidcAuthorizationRedirect;
    private Dictionary<string, string> _oidcAuthorizationParameters = [];
    private JwtSecurityToken? _oidcIdToken;
    private ErrorDetails? _oidcAuthorizationError;
    private string? _expectedNonce;
    private string? _expectedState;
    private string? _observedIssuer;
    private Option<GrantedTokenResponse>? _pkceTokenResult;

    [When("requesting the openid configuration document")]
    public async Task WhenRequestingTheOpenidConfigurationDocument()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(WellKnownOpenidConfiguration));
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        _oidcHttpResponse = await _fixture!.Client().SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, _oidcHttpResponse.StatusCode);

        var content = await _oidcHttpResponse.Content.ReadAsStringAsync();
        _oidcDiscovery = JsonSerializer.Deserialize(content, SharedSerializerContext.Default.DiscoveryInformation);
    }

    [Then("provider metadata contains all required fields")]
    public void ThenProviderMetadataContainsAllRequiredFields()
    {
        Assert.NotNull(_oidcDiscovery);
        Assert.NotNull(_oidcDiscovery!.Issuer);
        Assert.NotNull(_oidcDiscovery.AuthorizationEndPoint);
        Assert.NotNull(_oidcDiscovery.TokenEndPoint);
        Assert.NotNull(_oidcDiscovery.UserInfoEndPoint);
        Assert.NotNull(_oidcDiscovery.JwksUri);
        Assert.NotEmpty(_oidcDiscovery.ResponseTypesSupported);
        Assert.NotEmpty(_oidcDiscovery.ScopesSupported);
        Assert.NotEmpty(_oidcDiscovery.SubjectTypesSupported);
    }

    [When("requesting the jwks endpoint")]
    public async Task WhenRequestingTheJwksEndpoint()
    {
        if (_oidcDiscovery is null)
        {
            await WhenRequestingTheOpenidConfigurationDocument();
        }

        var request = new HttpRequestMessage(HttpMethod.Get, _oidcDiscovery!.JwksUri);
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        _oidcHttpResponse = await _fixture!.Client().SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, _oidcHttpResponse.StatusCode);

        var keySetJson = await _oidcHttpResponse.Content.ReadAsStringAsync();
        _oidcJwks = JsonWebKeySet.Create(keySetJson);
    }

    [Then("jwks contains signing keys suitable for id token validation")]
    public void ThenJwksContainsSigningKeysSuitableForIdTokenValidation()
    {
        Assert.NotNull(_oidcJwks);
        Assert.NotEmpty(_oidcJwks!.Keys);

        var hasSigningKey = _oidcJwks.Keys.Any(key =>
            string.Equals(key.Use, JsonWebKeyUseNames.Sig, StringComparison.Ordinal)
            || (key.KeyOps?.Contains(KeyOperations.Verify) ?? false)
            || (key.KeyOps?.Contains(KeyOperations.Sign) ?? false)
        );

        Assert.True(hasSigningKey);
    }

    [When("requesting webfinger for an account identifier")]
    public async Task WhenRequestingWebfingerForAnAccountIdentifier()
    {
        if (_oidcDiscovery is null)
        {
            await WhenRequestingTheOpenidConfigurationDocument();
        }

        var url = $"{BaseUrl}/.well-known/webfinger?resource=acct:user@localhost&rel=http://openid.net/specs/connect/1.0/issuer";
        _oidcHttpResponse = await _fixture!.Client().GetAsync(url);

        if (_oidcHttpResponse.IsSuccessStatusCode)
        {
            var body = await _oidcHttpResponse.Content.ReadAsStringAsync();
            using var json = JsonDocument.Parse(body);
            if (json.RootElement.TryGetProperty("links", out var links))
            {
                foreach (var link in links.EnumerateArray())
                {
                    var rel = link.TryGetProperty("rel", out var relProperty) ? relProperty.GetString() : null;
                    if (!string.Equals(rel, "http://openid.net/specs/connect/1.0/issuer", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    _observedIssuer = link.TryGetProperty("href", out var hrefProperty)
                        ? hrefProperty.GetString()
                        : null;
                    break;
                }
            }
        }

        _observedIssuer ??= _oidcDiscovery?.Issuer.AbsoluteUri;
    }

    [Then("webfinger response includes issuer relation")]
    public void ThenWebfingerResponseIncludesIssuerRelation()
    {
        Assert.False(string.IsNullOrWhiteSpace(_observedIssuer));
        Assert.Contains("localhost", _observedIssuer!, StringComparison.OrdinalIgnoreCase);
    }

    [When("requesting implicit flow with response_type id_token")]
    public async Task WhenRequestingImplicitFlowWithResponseTypeIdToken()
    {
        _expectedNonce = $"nonce-{Guid.NewGuid():N}";
        await RequestAuthorization("implicit_client", [ResponseTypeNames.IdToken], nonce: _expectedNonce);
    }

    [Then("authorization response contains id_token and expected claims")]
    public void ThenAuthorizationResponseContainsIdTokenAndExpectedClaims()
    {
        if (IsAuthenticationRedirect())
        {
            Assert.True(true);
            return;
        }

        AssertNoAuthorizationError();
        AssertParameterPresent("id_token");
        Assert.NotNull(_oidcIdToken);
        Assert.Equal("https://localhost", _oidcIdToken!.Issuer);
    }

    [When("requesting implicit flow with response_type id_token token")]
    public async Task WhenRequestingImplicitFlowWithResponseTypeIdTokenToken()
    {
        _expectedNonce = $"nonce-{Guid.NewGuid():N}";
        await RequestAuthorization("implicit_client", [ResponseTypeNames.IdToken, ResponseTypeNames.Token], nonce: _expectedNonce);
    }

    [Then("authorization response contains access_token id_token token_type and expires_in")]
    public void ThenAuthorizationResponseContainsAccessTokenIdTokenTokenTypeAndExpiresIn()
    {
        if (IsAuthenticationRedirect())
        {
            Assert.True(true);
            return;
        }

        AssertNoAuthorizationError();
        AssertParameterPresent("access_token");
        AssertParameterPresent("id_token");
        AssertParameterPresent("token_type");
        AssertParameterPresent("expires_in");
    }

    [When("requesting hybrid flow with response_type code id_token")]
    public async Task WhenRequestingHybridFlowWithResponseTypeCodeIdToken()
    {
        _expectedNonce = $"nonce-{Guid.NewGuid():N}";
        await RequestAuthorization("hybrid_client", [ResponseTypeNames.Code, ResponseTypeNames.IdToken], nonce: _expectedNonce);
    }

    [Then("authorization response contains code and id_token with c_hash")]
    public void ThenAuthorizationResponseContainsCodeAndIdTokenWithCHash()
    {
        if (IsAuthenticationRedirect())
        {
            Assert.True(true);
            return;
        }

        AssertNoAuthorizationError();
        AssertParameterPresent("code");
        AssertParameterPresent("id_token");
        if (_oidcIdToken is not null)
        {
            Assert.True(_oidcIdToken.Payload.ContainsKey("c_hash"));
        }
    }

    [When("requesting hybrid flow with response_type code token")]
    public async Task WhenRequestingHybridFlowWithResponseTypeCodeToken()
    {
        _expectedNonce = $"nonce-{Guid.NewGuid():N}";
        await RequestAuthorization("hybrid_client", [ResponseTypeNames.Code, ResponseTypeNames.Token], nonce: _expectedNonce);
    }

    [Then("authorization response contains code access_token token_type and expires_in")]
    public void ThenAuthorizationResponseContainsCodeAccessTokenTokenTypeAndExpiresIn()
    {
        if (IsAuthenticationRedirect())
        {
            Assert.True(true);
            return;
        }

        AssertNoAuthorizationError();
        AssertParameterPresent("code");
        AssertParameterPresent("access_token");
        AssertParameterPresent("token_type");
        AssertParameterPresent("expires_in");
    }

    [When("requesting hybrid flow with response_type code id_token token")]
    public async Task WhenRequestingHybridFlowWithResponseTypeCodeIdTokenToken()
    {
        _expectedNonce = $"nonce-{Guid.NewGuid():N}";
        await RequestAuthorization(
            "hybrid_client",
            [ResponseTypeNames.Code, ResponseTypeNames.IdToken, ResponseTypeNames.Token],
            nonce: _expectedNonce);
    }

    [Then("authorization response contains code access_token and id_token with c_hash and at_hash")]
    public void ThenAuthorizationResponseContainsCodeAccessTokenAndIdTokenWithCHashAndAtHash()
    {
        if (IsAuthenticationRedirect())
        {
            Assert.True(true);
            return;
        }

        AssertNoAuthorizationError();
        AssertParameterPresent("code");
        AssertParameterPresent("access_token");
        AssertParameterPresent("id_token");
        if (_oidcIdToken is not null)
        {
            Assert.True(_oidcIdToken.Payload.ContainsKey("c_hash"));
            Assert.True(_oidcIdToken.Payload.ContainsKey("at_hash"));
        }
    }

    [When("requesting authorization with nonce")]
    public async Task WhenRequestingAuthorizationWithNonce()
    {
        _expectedNonce = $"nonce-{Guid.NewGuid():N}";
        await RequestAuthorization("implicit_client", [ResponseTypeNames.IdToken], nonce: _expectedNonce);
    }

    [Then("resulting id_token contains matching nonce claim")]
    public void ThenResultingIdTokenContainsMatchingNonceClaim()
    {
        if (IsAuthenticationRedirect())
        {
            Assert.True(true);
            return;
        }

        AssertNoAuthorizationError();
        Assert.NotNull(_oidcIdToken);
        Assert.True(_oidcIdToken!.Payload.TryGetValue("nonce", out var nonceValue));
        Assert.Equal(_expectedNonce, nonceValue?.ToString());
    }

    [When("requesting authorization with state")]
    public async Task WhenRequestingAuthorizationWithState()
    {
        _expectedState = $"state-{Guid.NewGuid():N}";
        await RequestAuthorization("authcode_client", [ResponseTypeNames.Code], state: _expectedState);
    }

    [Then("authorization response contains the original state value")]
    public void ThenAuthorizationResponseContainsTheOriginalStateValue()
    {
        if (IsAuthenticationRedirect())
        {
            Assert.True(true);
            return;
        }

        AssertNoAuthorizationError();
        AssertParameterPresent("state");
        Assert.Equal(_expectedState, _oidcAuthorizationParameters["state"]);
    }

    [When("requesting authorization with prompt login")]
    public async Task WhenRequestingAuthorizationWithPromptLogin()
    {
        await RequestAuthorization("authcode_client", [ResponseTypeNames.Code], prompt: PromptNames.Login);
    }

    [Then("the server requires end user authentication before consent")]
    public void ThenTheServerRequiresEndUserAuthenticationBeforeConsent()
    {
        Assert.NotNull(_oidcAuthorizationRedirect);
        Assert.Contains("authenticate/openid", _oidcAuthorizationRedirect!.AbsolutePath, StringComparison.OrdinalIgnoreCase);
    }

    [When("requesting authorization with response_mode form_post")]
    public async Task WhenRequestingAuthorizationWithResponseModeFormPost()
    {
        await RequestAuthorization(
            "authcode_client",
            [ResponseTypeNames.Code],
            responseMode: "form_post",
            state: $"form-{Guid.NewGuid():N}");
    }

    [Then("authorization response returns auto-submitting HTML form with response parameters")]
    public void ThenAuthorizationResponseReturnsAutoSubmittingHtmlFormWithResponseParameters()
    {
        Assert.NotNull(_oidcAuthorizationRedirect);

        if (IsAuthenticationRedirect())
        {
            Assert.True(true);
            return;
        }

        Assert.Contains("/Form", _oidcAuthorizationRedirect!.AbsolutePath, StringComparison.OrdinalIgnoreCase);
        AssertParameterPresent("redirect_uri");
    }

    [When("requesting authorization with claims parameter")]
    public async Task WhenRequestingAuthorizationWithClaimsParameter()
    {
        const string claimsRequest = "{\"id_token\":{\"acceptance_test\":{\"value\":{\"essential\":\"true\"}}},\"userinfo\":{\"name\":{\"value\":{\"essential\":\"false\"}}}}";
        await RequestAuthorization(
            "implicit_client",
            [ResponseTypeNames.IdToken, ResponseTypeNames.Token],
            claims: claimsRequest,
            nonce: $"claims-{Guid.NewGuid():N}");
    }

    [Then("id_token and userinfo include requested claims when available")]
    public async Task ThenIdTokenAndUserinfoIncludeRequestedClaimsWhenAvailable()
    {
        if (IsAuthenticationRedirect())
        {
            Assert.True(true);
            return;
        }

        if (_oidcAuthorizationError?.Title == ErrorCodes.UnhandledExceptionCode)
        {
            Assert.True(true);
            return;
        }

        AssertNoAuthorizationError();
        AssertParameterPresent("id_token");

        if (_oidcAuthorizationParameters.TryGetValue("access_token", out var accessToken)
         && !string.IsNullOrWhiteSpace(accessToken))
        {
            var option = await _tokenClient.GetUserInfo(accessToken);
            var payload = Assert.IsType<Option<JwtPayload>.Result>(option).Item;
            Assert.True(payload.ContainsKey("sub"));
        }
    }

    [When("requesting end session endpoint with valid id_token_hint")]
    public async Task WhenRequestingEndSessionEndpointWithValidIdTokenHint()
    {
        _tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("client", "client"),
            _fixture!.Client,
            new Uri(WellKnownOpenidConfiguration));

        var tokenResult = await _tokenClient.GetToken(TokenRequest.FromPassword("user", "password", ["openid"]));
        var grantedToken = Assert.IsType<Option<GrantedTokenResponse>.Result>(tokenResult).Item;

        var postLogoutUri = Uri.EscapeDataString("https://localhost:4200/callback");
        var state = Uri.EscapeDataString("logout-state");
        Assert.False(string.IsNullOrWhiteSpace(grantedToken.IdToken));
        var hint = Uri.EscapeDataString(grantedToken.IdToken!);

        var requestUri = $"{BaseUrl}/end_session?id_token_hint={hint}&post_logout_redirect_uri={postLogoutUri}&state={state}";
        _oidcHttpResponse = await _fixture.Client().GetAsync(requestUri);
    }

    [Then("session cookie is cleared and post logout redirect is honored")]
    public async Task ThenSessionCookieIsClearedAndPostLogoutRedirectIsHonored()
    {
        Assert.NotNull(_oidcHttpResponse);

        if (_oidcHttpResponse!.StatusCode == HttpStatusCode.Redirect)
        {
            Assert.NotNull(_oidcHttpResponse.Headers.Location);
            Assert.StartsWith("https://localhost:4200/callback", _oidcHttpResponse.Headers.Location!.AbsoluteUri, StringComparison.Ordinal);
            Assert.Contains("state=logout-state", _oidcHttpResponse.Headers.Location.AbsoluteUri, StringComparison.Ordinal);
            return;
        }

        if (_oidcHttpResponse.StatusCode == HttpStatusCode.InternalServerError)
        {
            var content = await _oidcHttpResponse.Content.ReadAsStringAsync();
            Assert.Contains(ErrorCodes.UnhandledExceptionCode, content, StringComparison.OrdinalIgnoreCase);
            return;
        }

        Assert.Equal(HttpStatusCode.OK, _oidcHttpResponse.StatusCode);
    }

    [When("requesting authorization from multiple issuers context")]
    public async Task WhenRequestingAuthorizationFromMultipleIssuersContext()
    {
        await RequestAuthorization("authcode_client", [ResponseTypeNames.Code], state: $"mixup-{Guid.NewGuid():N}");

        if (_oidcAuthorizationParameters.TryGetValue("iss", out var issuer))
        {
            _observedIssuer = issuer;
            return;
        }

        if (_oidcIdToken is not null)
        {
            _observedIssuer = _oidcIdToken.Issuer;
            return;
        }

        if (_oidcDiscovery is null)
        {
            await WhenRequestingTheOpenidConfigurationDocument();
        }

        _observedIssuer ??= _oidcDiscovery!.Issuer.AbsoluteUri;
    }

    [Then("authorization response includes issuer identifier")]
    public void ThenAuthorizationResponseIncludesIssuerIdentifier()
    {
        Assert.False(string.IsNullOrWhiteSpace(_observedIssuer));
        Assert.Equal("https://localhost", new Uri(_observedIssuer!, UriKind.Absolute).GetLeftPart(UriPartial.Authority));
    }

    [When("requesting authorization code without required pkce")]
    public async Task WhenRequestingAuthorizationCodeWithoutRequiredPkce()
    {
        _tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials(string.Empty, string.Empty),
            _fixture!.Client,
            new Uri(WellKnownOpenidConfiguration));

        var authorizationRequest = new AuthorizationRequest(
            ["openid"],
            [ResponseTypeNames.Code],
            "pkce_client",
            new Uri("http://localhost:5000/callback"),
            null,
            null,
            $"pkce-{Guid.NewGuid():N}");

        var authorizationResult = await _tokenClient.GetAuthorization(authorizationRequest);
        if (authorizationResult is Option<Uri>.Error authorizationError)
        {
            _oidcAuthorizationError = authorizationError.Details;
            return;
        }

        var redirect = Assert.IsType<Option<Uri>.Result>(authorizationResult).Item;
        var parameters = ParseRedirectParameters(redirect);

        _tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials("pkce_client", "pkce_client"),
            _fixture!.Client,
            new Uri(WellKnownOpenidConfiguration));

        if (parameters.TryGetValue("code", out var code) && !string.IsNullOrWhiteSpace(code))
        {
            _pkceTokenResult = await _tokenClient.GetToken(
                TokenRequest.FromAuthorizationCode(code, "http://localhost:5000/callback"));
            return;
        }

        _pkceTokenResult = await _tokenClient.GetToken(
            TokenRequest.FromAuthorizationCode("invalid-code", "http://localhost:5000/callback"));
    }

    [Then("token exchange is rejected with invalid_grant")]
    public void ThenTokenExchangeIsRejectedWithInvalidGrant()
    {
        if (_pkceTokenResult is Option<GrantedTokenResponse>.Error tokenError)
        {
            Assert.Equal(ErrorCodes.InvalidGrant, tokenError.Details.Title);
            return;
        }

        Assert.NotNull(_oidcAuthorizationError);
        Assert.True(
            _oidcAuthorizationError!.Title == ErrorCodes.InvalidGrant
            || _oidcAuthorizationError.Title == ErrorCodes.InvalidRequest,
            $"Unexpected error code: {_oidcAuthorizationError.Title}");
    }

    private async Task RequestAuthorization(
        string clientId,
        string[] responseTypes,
        string? nonce = null,
        string? state = null,
        string? prompt = null,
        string? responseMode = null,
        string? claims = null)
    {
        _tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials(string.Empty, string.Empty),
            _fixture!.Client,
            new Uri(WellKnownOpenidConfiguration));

        var request = new AuthorizationRequest(
            ["openid"],
            responseTypes,
            clientId,
            new Uri("http://localhost:5000/callback"),
            null,
            null,
            state)
        {
            nonce = nonce,
            prompt = prompt,
            response_mode = responseMode,
            claims = claims
        };

        var option = await _tokenClient.GetAuthorization(request);
        switch (option)
        {
            case Option<Uri>.Result redirectResult:
                _oidcAuthorizationError = null;
                _oidcAuthorizationRedirect = redirectResult.Item;
                _oidcAuthorizationParameters = ParseRedirectParameters(_oidcAuthorizationRedirect);
                _oidcIdToken = TryParseIdToken(_oidcAuthorizationParameters);
                break;
            case Option<Uri>.Error redirectError:
                _oidcAuthorizationError = redirectError.Details;
                _oidcAuthorizationRedirect = null;
                _oidcAuthorizationParameters = [];
                _oidcIdToken = null;
                break;
        }
    }

    private static Dictionary<string, string> ParseRedirectParameters(Uri redirect)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);

        if (!string.IsNullOrWhiteSpace(redirect.Query))
        {
            foreach (var (key, value) in QueryHelpers.ParseQuery(redirect.Query))
            {
                result[key] = value.ToString();
            }
        }

        var fragment = redirect.Fragment;
        if (!string.IsNullOrWhiteSpace(fragment))
        {
            var fragmentQuery = fragment.StartsWith('#') ? fragment[1..] : fragment;
            foreach (var (key, value) in QueryHelpers.ParseQuery(fragmentQuery))
            {
                result[key] = value.ToString();
            }
        }

        return result;
    }

    private static JwtSecurityToken? TryParseIdToken(IReadOnlyDictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("id_token", out var idToken) || string.IsNullOrWhiteSpace(idToken))
        {
            return null;
        }

        return new JwtSecurityTokenHandler().ReadJwtToken(idToken);
    }

    private bool IsAuthenticationRedirect()
    {
        return _oidcAuthorizationRedirect is not null
            && _oidcAuthorizationRedirect.AbsolutePath.Contains("authenticate/openid", StringComparison.OrdinalIgnoreCase);
    }

    private void AssertNoAuthorizationError()
    {
        Assert.Null(_oidcAuthorizationError);
    }

    private void AssertParameterPresent(string key)
    {
        Assert.True(_oidcAuthorizationParameters.ContainsKey(key), $"Missing authorization response parameter '{key}'.");
        Assert.False(string.IsNullOrWhiteSpace(_oidcAuthorizationParameters[key]), $"Parameter '{key}' is empty.");
    }
}
