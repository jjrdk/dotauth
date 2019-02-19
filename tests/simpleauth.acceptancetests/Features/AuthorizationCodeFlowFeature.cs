namespace SimpleAuth.AcceptanceTests.Features
{
    using SimpleAuth.Client;
    using SimpleAuth.Client.Results;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Requests;
    using System;
    using Xbehave;
    using Xunit;

    public class AuthorizationCodeFlowFeature : AuthFlowFeature
    {
        [Scenario]
        public void SuccessfulAuthorizationCodeGrant()
        {
            AuthorizationClient client = null;
            Uri result = null;

            "and a properly configured auth client".x(
                async () => client = await AuthorizationClient.Create(
                        fixture.Client,
                        new Uri(WellKnownOpenidConfiguration))
                    .ConfigureAwait(false));

            "when requesting authorization".x(
                async () =>
                {
                    var response = await client.GetAuthorization(
                            new AuthorizationRequest(
                                new[] {"api1"},
                                new[] {ResponseTypeNames.Code},
                                "authcode_client",
                                new Uri("http://localhost:5000/callback"),
                                "abc"))
                        .ConfigureAwait(false);

                    Assert.False(response.ContainsError);

                    result = response.Location;
                });

            "then has authorization uri".x(() => { Assert.NotNull(result); });
        }

        [Scenario(DisplayName = "Scope does not match client registration")]
        public void InvalidScope()
        {
            AuthorizationClient client = null;
            GetAuthorizationResult result = null;

            "and an improperly configured authorization client".x(
                async () => client = await AuthorizationClient.Create(
                        fixture.Client,
                        new Uri(WellKnownOpenidConfiguration))
                    .ConfigureAwait(false));

            "when requesting authorization".x(
                async () =>
                {
                    result = await client.GetAuthorization(
                            new AuthorizationRequest(
                                new[] {"cheese"},
                                new[] {ResponseTypeNames.Code},
                                "authcode_client",
                                new Uri("http://localhost:5000/callback"),
                                "abc"))
                        .ConfigureAwait(false);
                });

            "then has expected error message".x(() => { Assert.Equal("invalid_scope", result.Error.Error); });
        }

        [Scenario(DisplayName = "Redirect uri does not match client registration")]
        public void InvalidRedirectUri()
        {
            AuthorizationClient client = null;
            GetAuthorizationResult result = null;

            "and an improperly configured authorization client".x(
                async () => client = await AuthorizationClient.Create(
                        fixture.Client,
                        new Uri(WellKnownOpenidConfiguration))
                    .ConfigureAwait(false));

            "when requesting authorization".x(
                async () =>
                {
                    result = await client.GetAuthorization(
                            new AuthorizationRequest(
                                new[] {"api1"},
                                new[] {ResponseTypeNames.Code},
                                "authcode_client",
                                new Uri("http://localhost:1000/callback"),
                                "abc"))
                        .ConfigureAwait(false);
                });

            "then has expected error message".x(() => { Assert.Equal("invalid_request", result.Error.Error); });
        }
    }
}
