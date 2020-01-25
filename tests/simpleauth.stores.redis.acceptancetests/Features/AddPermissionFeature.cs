namespace SimpleAuth.Stores.Redis.AcceptanceTests.Features
{
    using System;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Client;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Responses;
    using Xbehave;
    using Xunit;

    public class AddPermissionFeature : AuthorizedManagementFeatureBase
    {
        private const string WellKnownUmaConfiguration = "https://localhost/.well-known/uma2-configuration";

        public AddPermissionFeature()
        {
            IdentityModelEventSource.ShowPII = true;
        }

        [Scenario(DisplayName = "Successful Permission Creation")]
        public void SuccessfulPermissionCreation()
        {
            GrantedTokenResponse grantedToken = null;
            UmaClient client = null;
            JsonWebKeySet jwks = null;
            string resourceId = null;
            string ticketId = null;

            "and the server's signing key".x(
                async () =>
                {
                    var json = await _fixture.Client.GetStringAsync(BaseUrl + "/jwks").ConfigureAwait(false);
                    jwks = new JsonWebKeySet(json);

                    Assert.NotEmpty(jwks.Keys);
                });

            "and a valid UMA token".x(
                async () =>
                {
                    var tokenClient = await TokenClient.Create(
                            TokenCredentials.FromClientCredentials("clientCredentials", "clientCredentials"),
                            _fixture.Client,
                            new Uri(WellKnownUmaConfiguration))
                        .ConfigureAwait(false);
                    var token = await tokenClient.GetToken(TokenRequest.FromScopes("uma_protection"))
                        .ConfigureAwait(false);
                    grantedToken = token.Content;
                });

            "and a properly configured uma client".x(
                async () => client = await UmaClient.Create(_fixture.Client, new Uri(WellKnownUmaConfiguration))
                    .ConfigureAwait(false));

            "when registering resource".x(
                async () =>
                {
                    var resource = await client.AddResource(
                            new PostResourceSet { Name = "picture", Scopes = new[] { "read" } },
                            grantedToken.AccessToken)
                        .ConfigureAwait(false);
                    resourceId = resource.Content.Id;
                });

            "and adding permission".x(
                async () =>
                {
                    var response = await client.AddPermission(
                            new PostPermission { ResourceSetId = resourceId, Scopes = new[] { "read" } },
                            grantedToken.AccessToken)
                        .ConfigureAwait(false);

                    Assert.False(response.ContainsError);

                    ticketId = response.Content.TicketId;
                });

            "then returns ticket id".x(() => { Assert.NotNull(ticketId); });
        }


        [Scenario(DisplayName = "Successful Multiple Permissions Creation")]
        public void SuccessfulMultiplePermissionsCreation()
        {
            GrantedTokenResponse grantedToken = null;
            UmaClient client = null;
            string resourceId = null;
            string ticketId = null;

            "and a valid UMA token".x(
                async () =>
                {
                    var tokenClient = await TokenClient.Create(
                            TokenCredentials.FromClientCredentials("clientCredentials", "clientCredentials"),
                            _fixture.Client,
                            new Uri(WellKnownUmaConfiguration))
                        .ConfigureAwait(false);
                    var token = await tokenClient.GetToken(TokenRequest.FromScopes("uma_protection"))
                        .ConfigureAwait(false);
                    grantedToken = token.Content;

                    Assert.NotNull(grantedToken);
                });

            "and a properly configured uma client".x(
                async () => client = await UmaClient.Create(_fixture.Client, new Uri(WellKnownUmaConfiguration))
                    .ConfigureAwait(false));

            "when registering resource".x(
                async () =>
                {
                    var resource = await client.AddResource(
                            new PostResourceSet { Name = "picture", Scopes = new[] { "read", "write" } },
                            grantedToken.AccessToken)
                        .ConfigureAwait(false);
                    resourceId = resource.Content.Id;

                    Assert.NotNull(resourceId);
                });

            "and adding permission".x(
                async () =>
                {
                    var response = await client.AddPermissions(
                            grantedToken.AccessToken,
                            new PostPermission { ResourceSetId = resourceId, Scopes = new[] { "write" } },
                            new PostPermission { ResourceSetId = resourceId, Scopes = new[] { "read" } })
                        .ConfigureAwait(false);

                    Assert.False(response.ContainsError);

                    ticketId = response.Content.TicketId;

                    Assert.NotNull(ticketId);
                });

            "then returns ticket id".x(() => { Assert.NotNull(ticketId); });
        }
    }
}
