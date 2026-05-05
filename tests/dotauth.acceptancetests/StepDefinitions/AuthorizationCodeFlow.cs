namespace DotAuth.AcceptanceTests.StepDefinitions;

using System;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using Reqnroll;
using Xunit;

public partial class FeatureTest
{
    private Option<Uri>? _response;
    // Stores the last PKCE code verifier generated during an authorization request
    private string? _lastPkceCodeVerifier;

    [Given(@"a properly configured auth client")]
    public void GivenAProperlyConfiguredAuthClient()
    {
        _tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials(string.Empty, string.Empty),
            _fixture!.Client,
            new Uri(WellKnownOpenidConfiguration));
    }

    [When(@"requesting authorization for scope (.*)")]
    public async Task WhenRequestingAuthorizationForScope(string scope)
    {
        var pkce = CodeChallengeMethods.S256.BuildPkce();
        // keep the code verifier so that subsequent steps can perform the token exchange
        _lastPkceCodeVerifier = pkce.CodeVerifier;
        var authorizationRequest = new AuthorizationRequest(
            [scope],
            [ResponseTypeNames.Code],
            "authcode_client",
            new Uri("http://localhost:5000/callback"),
            pkce.CodeChallenge,
            CodeChallengeMethods.S256,
            "abc")
        {
            code_challenge_method = CodeChallengeMethods.S256,
            code_challenge = pkce.CodeChallenge,
            prompt = PromptNames.Login
        };
        _response = await _tokenClient.GetAuthorization(
                authorizationRequest)
            ;
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
        _lastPkceCodeVerifier = pkce.CodeVerifier;
        _response = await _tokenClient.GetAuthorization(
                new AuthorizationRequest(
                    ["api1"],
                    [ResponseTypeNames.Code],
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
