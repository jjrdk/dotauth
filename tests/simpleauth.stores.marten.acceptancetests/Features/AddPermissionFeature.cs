namespace SimpleAuth.Stores.Marten.AcceptanceTests.Features;

using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using SimpleAuth.Client;
using SimpleAuth.Shared.Responses;
using System;
using System.Threading;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Requests;
using Xbehave;
using Xunit;
using Xunit.Abstractions;

public sealed class AddPermissionFeature : AuthorizedManagementFeatureBase
{
    private const string WellKnownUmaConfiguration = "https://localhost/.well-known/uma2-configuration";

    public AddPermissionFeature(ITestOutputHelper output)
        : base(output)
    {
        IdentityModelEventSource.ShowPII = true;
    }

    [Scenario(DisplayName = "Successful Permission Creation")]
    public void SuccessfulPermissionCreation()
    {
        GrantedTokenResponse grantedToken = null!;
        UmaClient client = null!;
        JsonWebKeySet jwks = null!;
        string resourceId = null!;
        string ticketId = null!;

        "and the server's signing key".x(
            async () =>
            {
                var json = await Fixture.Client().GetStringAsync(BaseUrl + "/jwks").ConfigureAwait(false);
                jwks = new JsonWebKeySet(json);

                Assert.NotEmpty(jwks.Keys);
            });

        "and a valid UMA token".x(
            async () =>
            {
                var tokenClient = new TokenClient(
                    TokenCredentials.FromClientCredentials("clientCredentials", "clientCredentials"),
                    Fixture.Client,
                    new Uri(WellKnownUmaConfiguration));
                var token = await tokenClient.GetToken(TokenRequest.FromScopes("uma_protection"))
                    .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;
                grantedToken = token!.Item;

                Assert.NotNull(grantedToken.AccessToken);
            });

        "and a properly configured uma client".x(
            () => client = new UmaClient(Fixture.Client, new Uri(WellKnownUmaConfiguration)));

        "when registering resource".x(
            async () =>
            {
                var resource = await client.AddResource(
                        new ResourceSet {Name = "picture", Scopes = new[] {"read"}},
                        grantedToken.AccessToken)
                    .ConfigureAwait(false) as Option<AddResourceSetResponse>.Result;
                resourceId = resource!.Item.Id;
            });

        "and adding permission".x(
            async () =>
            {
                var response = await client.RequestPermission(
                        grantedToken.AccessToken,
                        requests: new PermissionRequest {ResourceSetId = resourceId, Scopes = new[] {"read"}})
                    .ConfigureAwait(false) as Option<TicketResponse>.Result;

                ticketId = response!.Item.TicketId;
            });

        "then returns ticket id".x(() => { Assert.NotNull(ticketId); });
    }


    [Scenario(DisplayName = "Successful Multiple Permissions Creation")]
    public void SuccessfulMultiplePermissionsCreation()
    {
        GrantedTokenResponse grantedToken = null!;
        UmaClient client = null!;
        string resourceId = null!;
        string ticketId = null!;

        "and a valid UMA token".x(
            async () =>
            {
                var tokenClient = new TokenClient(
                    TokenCredentials.FromClientCredentials("clientCredentials", "clientCredentials"),
                    Fixture.Client,
                    new Uri(WellKnownUmaConfiguration));
                var token = await tokenClient.GetToken(TokenRequest.FromScopes("uma_protection"))
                    .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;
                grantedToken = token!.Item;

                Assert.NotNull(grantedToken);
            });

        "and a properly configured uma client".x(
            () => client = new UmaClient(Fixture.Client, new Uri(WellKnownUmaConfiguration)));

        "when registering resource".x(
            async () =>
            {
                var resource = await client.AddResource(
                        new ResourceSet {Name = "picture", Scopes = new[] {"read", "write"}},
                        grantedToken.AccessToken)
                    .ConfigureAwait(false) as Option<AddResourceSetResponse>.Result;
                resourceId = resource!.Item.Id;

                Assert.NotNull(resourceId);
            });

        "and adding permission".x(
            async () =>
            {
                var response = await client.RequestPermission(
                        grantedToken.AccessToken,
                        CancellationToken.None,
                        new PermissionRequest {ResourceSetId = resourceId, Scopes = new[] {"write"}},
                        new PermissionRequest {ResourceSetId = resourceId, Scopes = new[] {"read"}})
                    .ConfigureAwait(false) as Option<TicketResponse>.Result;

                ticketId = response!.Item.TicketId;

                Assert.NotNull(ticketId);
            });

        "then returns ticket id".x(() => { Assert.NotNull(ticketId); });
    }
}