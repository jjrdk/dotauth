namespace SimpleAuth.AcceptanceTests.Features
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Client;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Responses;
    using Xbehave;
    using Xunit;

    public class AddPermissionFeature
    {
        private const string BaseUrl = "http://localhost:5000";
        private const string WellKnownUmaConfiguration = "https://localhost/.well-known/uma2-configuration";

        public AddPermissionFeature()
        {
            IdentityModelEventSource.ShowPII = true;
        }

        [Scenario]
        public void SuccessfulPermissionCreation()
        {
            TestServerFixture fixture = null;
            GrantedTokenResponse grantedToken = null;
            UmaClient client = null;
            string resourceId = null;
            string ticketId = null;

            "Given a running auth server".x(() => fixture = new TestServerFixture(BaseUrl))
                .Teardown(() => fixture.Dispose());

            "and the server's signing key".x(
                async () =>
                {
                    var json = await fixture.Client.GetStringAsync(BaseUrl + "/jwks").ConfigureAwait(false);
                    var jwks = new JsonWebKeySet(json);

                    Assert.NotEmpty(jwks.Keys);
                });

            "and a valid UMA token".x(
                async () =>
                {
                    var tokenClient = new TokenClient(
                        TokenCredentials.FromClientCredentials("clientCredentials", "clientCredentials"),
                        fixture.Client,
                        new Uri(WellKnownUmaConfiguration));
                    var token = await tokenClient.GetToken(TokenRequest.FromScopes("uma_protection"))
                        .ConfigureAwait(false);
                    var handler = new JwtSecurityTokenHandler();
                    var principal = handler.ReadJwtToken(token.Content.AccessToken);
                    Assert.NotNull(principal.Issuer);
                    grantedToken = token.Content;
                });

            "and a properly configured uma client".x(
                () => client = new UmaClient(fixture.Client, new Uri(WellKnownUmaConfiguration)));

            "when registering resource".x(
                async () =>
                {
                    var resource = await client.AddResource(
                            new ResourceSet {Name = "picture", Scopes = new[] {"read"}},
                            grantedToken.AccessToken)
                        .ConfigureAwait(false);
                    resourceId = resource.Content.Id;
                });

            "and adding permission".x(
                async () =>
                {
                    var response = await client.RequestPermission(
                            new PermissionRequest {ResourceSetId = resourceId, Scopes = new[] {"read"}},
                            grantedToken.AccessToken)
                        .ConfigureAwait(false);

                    Assert.False(response.ContainsError);

                    ticketId = response.Content.TicketId;
                });

            "then returns ticket id".x(() => { Assert.NotNull(ticketId); });
        }


        [Scenario]
        public void SuccessfulPermissionsCreation()
        {
            TestServerFixture fixture = null;
            GrantedTokenResponse grantedToken = null;
            UmaClient client = null;
            string resourceId = null;
            string ticketId = null;

            "Given a running auth server".x(() => fixture = new TestServerFixture(BaseUrl))
                .Teardown(() => fixture.Dispose());

            "and a valid UMA token".x(
                async () =>
                {
                    var tokenClient = new TokenClient(
                        TokenCredentials.FromClientCredentials("clientCredentials", "clientCredentials"),
                        fixture.Client,
                        new Uri(WellKnownUmaConfiguration));
                    var token = await tokenClient.GetToken(TokenRequest.FromScopes("uma_protection"))
                        .ConfigureAwait(false);
                    grantedToken = token.Content;
                });

            "and a properly configured uma client".x(
                () => client = new UmaClient(fixture.Client, new Uri(WellKnownUmaConfiguration)));

            "when registering resource".x(
                async () =>
                {
                    var resource = await client.AddResource(
                            new ResourceSet {Name = "picture", Scopes = new[] {"read", "write"}},
                            grantedToken.AccessToken)
                        .ConfigureAwait(false);
                    resourceId = resource.Content.Id;
                });

            "and adding permission".x(
                async () =>
                {
                    var response = await client.RequestPermissions(
                            grantedToken.AccessToken,
                            new PermissionRequest {ResourceSetId = resourceId, Scopes = new[] {"write"}},
                            new PermissionRequest {ResourceSetId = resourceId, Scopes = new[] {"read"}})
                        .ConfigureAwait(false);

                    Assert.False(response.ContainsError);

                    ticketId = response.Content.TicketId;
                });

            "then returns ticket id".x(() => { Assert.NotNull(ticketId); });
        }
    }
}
