namespace DotAuth.AcceptanceTests.Features;

using System;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Microsoft.AspNetCore.WebUtilities;
using Xbehave;
using Xunit;
using Xunit.Abstractions;

public sealed class AuthorizationCodeFlowFeature : AuthFlowFeature
{
    /// <inheritdoc />
    public AuthorizationCodeFlowFeature(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Scenario(DisplayName = "Successful authorization code grant")]
    public void SuccessfulAuthorizationCodeGrant()
    {
        TokenClient client = null!;
        Uri result = null!;

        "and a properly configured auth client".x(
            () => client = new TokenClient(
                TokenCredentials.FromClientCredentials(string.Empty, string.Empty),
                _fixture.Client,
                new Uri(WellKnownOpenidConfiguration)));

        "when requesting authorization".x(
            async () =>
            {
                var pkce = CodeChallengeMethods.S256.BuildPkce();
                var authorizationRequest = new AuthorizationRequest(
                    new[] { "api1" },
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
                var response = await client.GetAuthorization(
                        authorizationRequest)
                    .ConfigureAwait(false) as Option<Uri>.Result;

                Assert.NotNull(response);

                result = response.Item;
            });

        "then has authorization uri".x(() => { Assert.NotNull(result); });
    }
    
    [Scenario(DisplayName = "Scope does not match client registration")]
    public void InvalidScope()
    {
        TokenClient client = null!;
        Option<Uri>.Error result = null!;

        "and an improperly configured authorization client".x(
            () => client = new TokenClient(
                TokenCredentials.FromClientCredentials(string.Empty, string.Empty),
                _fixture.Client,
                new Uri(WellKnownOpenidConfiguration)));

        "when requesting authorization".x(
            async () =>
            {
                var pkce = CodeChallengeMethods.S256.BuildPkce();
                result = await client.GetAuthorization(
                        new AuthorizationRequest(
                            new[] { "cheese" },
                            new[] { ResponseTypeNames.Code },
                            "authcode_client",
                            new Uri("http://localhost:5000/callback"),
                            pkce.CodeChallenge,
                            CodeChallengeMethods.S256,
                            "abc"))
                    .ConfigureAwait(false) as Option<Uri>.Error;
            });

        "then has expected error message".x(() => { Assert.Equal(ErrorCodes.InvalidScope, result.Details.Title); });
    }

    [Scenario(DisplayName = "Redirect uri does not match client registration")]
    public void InvalidRedirectUri()
    {
        TokenClient client = null!;
        Option<Uri>.Error result = null!;

        "and an improperly configured authorization client".x(
            () => client = new TokenClient(
                TokenCredentials.FromClientCredentials(string.Empty, string.Empty),
                _fixture.Client,
                new Uri(WellKnownOpenidConfiguration)));

        "when requesting authorization".x(
            async () =>
            {
                var pkce = CodeChallengeMethods.S256.BuildPkce();
                result = await client.GetAuthorization(
                        new AuthorizationRequest(
                            new[] { "api1" },
                            new[] { ResponseTypeNames.Code },
                            "authcode_client",
                            new Uri("http://localhost:1000/callback"),
                            pkce.CodeChallenge,
                            CodeChallengeMethods.S256,
                            "abc"))
                    .ConfigureAwait(false) as Option<Uri>.Error;
            });

        "then has expected error message".x(() => { Assert.Equal(ErrorCodes.InvalidRequest, result.Details.Title); });
    }
}