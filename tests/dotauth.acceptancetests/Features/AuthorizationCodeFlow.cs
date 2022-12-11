namespace DotAuth.AcceptanceTests.Features;

using System;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using TechTalk.SpecFlow;
using Xunit;

public partial class FeatureTest
{
    private Option<Uri> _response;

    [Given(@"a properly configured auth client")]
    public void GivenAProperlyConfiguredAuthClient()
    {
        _tokenClient = new TokenClient(
            TokenCredentials.FromClientCredentials(string.Empty, string.Empty),
            _fixture.Client,
            new Uri(FeatureTest.WellKnownOpenidConfiguration));
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
        _response = await _tokenClient.GetAuthorization(
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
        _response = await _tokenClient.GetAuthorization(
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