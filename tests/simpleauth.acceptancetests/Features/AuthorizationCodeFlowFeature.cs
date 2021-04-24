namespace SimpleAuth.AcceptanceTests.Features
{
    using SimpleAuth.Client;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Requests;
    using System;
    using SimpleAuth.Shared.Errors;
    using Xbehave;
    using Xunit;
    using Xunit.Abstractions;

    public class AuthorizationCodeFlowFeature : AuthFlowFeature
    {
        /// <inheritdoc />
        public AuthorizationCodeFlowFeature(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Scenario]
        public void SuccessfulAuthorizationCodeGrant()
        {
            TokenClient client = null;
            Uri result = null;

            "and a properly configured auth client".x(
                () => client = new TokenClient(
                    TokenCredentials.FromClientCredentials(string.Empty, string.Empty),
                    _fixture.Client,
                    new Uri(WellKnownOpenidConfiguration)));

            "when requesting authorization".x(
                async () =>
                {
                    var response = await client.GetAuthorization(
                            new AuthorizationRequest(
                                new[] { "api1" },
                                new[] { ResponseTypeNames.Code },
                                "authcode_client",
                                new Uri("http://localhost:5000/callback"),
                                "abc"))
                        .ConfigureAwait(false);

                    Assert.False(response.HasError);

                    result = response.Content;
                });

            "then has authorization uri".x(() => { Assert.NotNull(result); });
        }

        [Scenario(DisplayName = "Scope does not match client registration")]
        public void InvalidScope()
        {
            TokenClient client = null;
            GenericResponse<Uri> result = null;

            "and an improperly configured authorization client".x(
                () => client = new TokenClient(
                    TokenCredentials.FromClientCredentials(string.Empty, string.Empty),
                    _fixture.Client,
                    new Uri(WellKnownOpenidConfiguration)));

            "when requesting authorization".x(
                async () =>
                {
                    result = await client.GetAuthorization(
                            new AuthorizationRequest(
                                new[] { "cheese" },
                                new[] { ResponseTypeNames.Code },
                                "authcode_client",
                                new Uri("http://localhost:5000/callback"),
                                "abc"))
                        .ConfigureAwait(false);
                });

            "then has expected error message".x(() => { Assert.Equal(ErrorCodes.InvalidScope, result.Error.Title); });
        }

        [Scenario(DisplayName = "Redirect uri does not match client registration")]
        public void InvalidRedirectUri()
        {
            TokenClient client = null;
            GenericResponse<Uri> result = null;

            "and an improperly configured authorization client".x(
                () => client = new TokenClient(
                    TokenCredentials.FromClientCredentials(string.Empty, string.Empty),
                    _fixture.Client,
                    new Uri(WellKnownOpenidConfiguration)));

            "when requesting authorization".x(
                async () =>
                {
                    result = await client.GetAuthorization(
                            new AuthorizationRequest(
                                new[] { "api1" },
                                new[] { ResponseTypeNames.Code },
                                "authcode_client",
                                new Uri("http://localhost:1000/callback"),
                                "abc"))
                        .ConfigureAwait(false);
                });

            "then has expected error message".x(() => { Assert.Equal(ErrorCodes.InvalidRequest, result.Error.Title); });
        }
    }
}
