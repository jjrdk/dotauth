namespace DotAuth.AcceptanceTests.Features;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Xbehave;
using Xunit;
using Xunit.Abstractions;

public sealed class AddPermissionFeature
{
    private readonly ITestOutputHelper _outputHelper;
    private const string BaseUrl = "http://localhost:5000";
    private const string WellKnownUmaConfiguration = "https://localhost/.well-known/uma2-configuration";

    public AddPermissionFeature(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        IdentityModelEventSource.ShowPII = true;
    }

    [Scenario]
    public void SuccessfulPermissionCreation()
    {
        TestServerFixture fixture = null!;
        GrantedTokenResponse grantedToken = null!;
        UmaClient client = null!;
        string resourceId = null!;
        string ticketId = null!;

        "Given a running auth server".x(() => fixture = new TestServerFixture(_outputHelper, BaseUrl))
            .Teardown(() => fixture.Dispose());

        "and the server's signing key".x(
            async () =>
            {
                var json = await fixture.Client().GetStringAsync(BaseUrl + "/jwks").ConfigureAwait(false);
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
                    .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;
                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ReadJwtToken(token.Item.AccessToken);
                Assert.NotNull(principal.Issuer);
                grantedToken = token.Item;
            });

        "and a properly configured uma client".x(
            () => client = new UmaClient(fixture.Client, new Uri(WellKnownUmaConfiguration)));

        "when registering resource".x(
            async () =>
            {
                var resource = await client.AddResource(
                        new ResourceSet { Name = "picture", Scopes = new[] { "read" } },
                        grantedToken.AccessToken)
                    .ConfigureAwait(false) as Option<AddResourceSetResponse>.Result;
                resourceId = resource.Item.Id;
            });

        "and adding permission".x(
            async () =>
            {
                var response = await client.RequestPermission(
                        grantedToken.AccessToken,
                        requests: new PermissionRequest { IdToken = grantedToken.IdToken, ResourceSetId = resourceId, Scopes = new[] { "read" } })
                    .ConfigureAwait(false) as Option<TicketResponse>.Result;

                Assert.NotNull(response);

                ticketId = response.Item.TicketId;
            });

        "then returns ticket id".x(() => { Assert.NotNull(ticketId); });
    }

    [Scenario]
    public void SuccessfulPermissionsCreation()
    {
        TestServerFixture fixture = null!;
        GrantedTokenResponse grantedToken = null!;
        UmaClient client = null!;
        string resourceId = null!;
        string ticketId = null!;

        "Given a running auth server".x(() => fixture = new TestServerFixture(_outputHelper, BaseUrl))
            .Teardown(() => fixture.Dispose());

        "and a valid UMA token".x(
            async () =>
            {
                var tokenClient = new TokenClient(
                    TokenCredentials.FromClientCredentials("clientCredentials", "clientCredentials"),
                    fixture.Client,
                    new Uri(WellKnownUmaConfiguration));
                var token = await tokenClient.GetToken(TokenRequest.FromScopes("uma_protection"))
                    .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;
                grantedToken = token.Item;
            });

        "and a properly configured uma client".x(
            () => client = new UmaClient(fixture.Client, new Uri(WellKnownUmaConfiguration)));

        "when registering resource".x(
            async () =>
            {
                var resource = await client.AddResource(
                        new ResourceSet { Name = "picture", Scopes = new[] { "read", "write" } },
                        grantedToken.AccessToken)
                    .ConfigureAwait(false) as Option<AddResourceSetResponse>.Result;
                resourceId = resource.Item.Id;
            });

        "and adding permission".x(
            async () =>
            {
                var response = await client.RequestPermission(
                        grantedToken.AccessToken,
                        CancellationToken.None,
                        new PermissionRequest { ResourceSetId = resourceId, Scopes = new[] { "write" } },
                        new PermissionRequest { ResourceSetId = resourceId, Scopes = new[] { "read" } })
                    .ConfigureAwait(false) as Option<TicketResponse>.Result;

                Assert.NotNull(response);

                ticketId = response.Item.TicketId;
            });

        "then returns ticket id".x(() => { Assert.NotNull(ticketId); });
    }
}