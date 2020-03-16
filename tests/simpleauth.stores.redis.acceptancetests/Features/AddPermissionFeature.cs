namespace SimpleAuth.Stores.Redis.AcceptanceTests.Features
{
    using System;
    using System.Threading;
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
                        var tokenClient = new TokenClient(
                            TokenCredentials.FromClientCredentials("clientCredentials", "clientCredentials"),
                            _fixture.Client,
                            new Uri(WellKnownUmaConfiguration));
                        var token = await tokenClient.GetToken(TokenRequest.FromScopes("uma_protection"))
                                        .ConfigureAwait(false);
                        grantedToken = token.Content;
                    });

            "and a properly configured uma client".x(
                () => client = new UmaClient(_fixture.Client, new Uri(WellKnownUmaConfiguration)));

            "when registering resource".x(
                async () =>
                    {
                        var resource = await client.AddResource(
                                               new ResourceSet { Name = "picture", Scopes = new[] { "read" } },
                                               grantedToken.AccessToken)
                                           .ConfigureAwait(false);
                        resourceId = resource.Content.Id;
                    });

            "and adding permission".x(
                async () =>
                    {
                        var response = await client.RequestPermission(grantedToken.AccessToken,
                                new PermissionRequest
                                {
                                    ResourceSetId = resourceId,
                                    Scopes = new[] { "read" }
                                })
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
                        var tokenClient = new TokenClient(
                            TokenCredentials.FromClientCredentials("clientCredentials", "clientCredentials"),
                            _fixture.Client,
                            new Uri(WellKnownUmaConfiguration));
                        var token = await tokenClient.GetToken(TokenRequest.FromScopes("uma_protection"))
                                        .ConfigureAwait(false);
                        grantedToken = token.Content;

                        Assert.NotNull(grantedToken);
                    });

            "and a properly configured uma client".x(
                () => client = new UmaClient(_fixture.Client, new Uri(WellKnownUmaConfiguration)));

            "when registering resource".x(
                async () =>
                    {
                        var resource = await client.AddResource(
                                               new ResourceSet
                                               {
                                                   Name = "picture",
                                                   Scopes = new[] { "read", "write" }
                                               },
                                               grantedToken.AccessToken)
                                           .ConfigureAwait(false);
                        resourceId = resource.Content.Id;

                        Assert.NotNull(resourceId);
                    });

            "and adding permission".x(
                async () =>
                    {
                        var response = await client.RequestPermissions(
                                               grantedToken.AccessToken,
                                               CancellationToken.None,
                                               new PermissionRequest
                                               {
                                                   ResourceSetId = resourceId,
                                                   Scopes = new[] { "write" }
                                               },
                                               new PermissionRequest
                                               {
                                                   ResourceSetId = resourceId,
                                                   Scopes = new[] { "read" }
                                               })
                                           .ConfigureAwait(false);

                        Assert.False(response.ContainsError);

                        ticketId = response.Content.TicketId;

                        Assert.NotNull(ticketId);
                    });

            "then returns ticket id".x(() => { Assert.NotNull(ticketId); });
        }
    }
}
