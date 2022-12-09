namespace DotAuth.AcceptanceTests.Features;

using System;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using Microsoft.IdentityModel.Tokens;
using TechTalk.SpecFlow;
using Xunit;
using Xunit.Abstractions;

[Binding]
[Scope(Feature = "Authorization Code Flow")]
public class AuthorizationCodeFlow : AuthFlowFeature
{
    private TokenClient _client = null!;
    private Option<Uri> _response;

    /// <inheritdoc />
    public AuthorizationCodeFlow(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Given(@"a running auth server")]
    public void GivenARunningAuthServer()
    {
        Fixture = new TestServerFixture(_outputHelper, BaseUrl);
    }

    [Given(@"the server's signing key")]
    public async Task GivenTheServersSigningKey()
    {
        var json = await Fixture.Client().GetStringAsync(BaseUrl + "/jwks").ConfigureAwait(false);
        var jwks = new JsonWebKeySet(json);

        Assert.NotEmpty(jwks.Keys);
    }

    [Given(@"a properly configured auth client")]
    public void GivenAProperlyConfiguredAuthClient()
    {
        _client = new TokenClient(
            TokenCredentials.FromClientCredentials(string.Empty, string.Empty),
            Fixture.Client,
            new Uri(WellKnownOpenidConfiguration));
    }

    [When(@"requesting authorization for scope (.*)")]
    public async Task WhenRequestingAuthorizationForScope(string scope)
    {
        var pkce = CodeChallengeMethods.S256.BuildPkce();
        var authorizationRequest = new AuthorizationRequest(
            new[] { scope },
            new[] { ResponseTypeNames.Code },
            "authcode_client",
            new Uri("http://localhost:5000/callback"),
            pkce.CodeChallenge,
            CodeChallengeMethods.S256,
            "abc")
        {
            code_challenge_method = CodeChallengeMethods.S256,
            code_challenge = CodeChallengeMethods.S256.BuildPkce().CodeChallenge,
            prompt = PromptNames.Login
        };
        _response = await _client.GetAuthorization(
                authorizationRequest)
            .ConfigureAwait(false);
    }

    [Then(@"has authorization uri")]
    public void ThenHasAuthorizationUri()
    {
        var result = Assert.IsType<Option<Uri>.Result>(_response);
        Assert.NotNull(result.Item);
    }

    [Then(@"has invalid scope error message")]
    public void ThenHasInvalidScopeErrorMessage()
    {
        var result = Assert.IsType<Option<Uri>.Error>(_response);
        Assert.Equal(ErrorCodes.InvalidScope, result.Details.Title);
    }

    [When(@"requesting authorization for wrong callback")]
    public async Task WhenRequestingAuthorizationForWrongCallback()
    {
        var pkce = CodeChallengeMethods.S256.BuildPkce();
        _response = await _client.GetAuthorization(
                new AuthorizationRequest(
                    new[] { "api1" },
                    new[] { ResponseTypeNames.Code },
                    "authcode_client",
                    new Uri("http://localhost:1000/callback"),
                    pkce.CodeChallenge,
                    CodeChallengeMethods.S256,
                    "abc"))
            .ConfigureAwait(false);
    }

    [Then(@"has invalid request error message")]
    public void ThenHasInvalidRequestErrorMessage()
    {
        var result = Assert.IsType<Option<Uri>.Error>(_response);
        Assert.Equal(ErrorCodes.InvalidRequest, result.Details.Title);
    }
}