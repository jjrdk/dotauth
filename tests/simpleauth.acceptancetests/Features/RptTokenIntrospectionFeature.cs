namespace SimpleAuth.AcceptanceTests.Features;

using System;
using SimpleAuth.Client;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Requests;
using SimpleAuth.Shared.Responses;
using Xbehave;
using Xunit;
using Xunit.Abstractions;
using IntrospectionRequest = Client.IntrospectionRequest;

public sealed class RptTokenIntrospectionFeature : AuthFlowFeature
{
    /// <inheritdoc />
    public RptTokenIntrospectionFeature(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Scenario(DisplayName = "Can use PAT token to introspect RPT token")]
    public void CanGetUserInfoFromPatToken()
    {
        TokenClient client = null!;
        UmaClient umaClient = null!;
        string patToken = null!;
        string idToken = null!;
        string ticketId = null!;
        string rptToken = null!;
        string resourceId = null!;

        "Given a token client".x(
            () =>
            {
                client = new TokenClient(
                    TokenCredentials.FromClientCredentials("clientCredentials", "clientCredentials"),
                    _fixture.Client,
                    new Uri(WellKnownOpenidConfiguration));
            });

        "and a UMA client".x(() => { umaClient = new UmaClient(_fixture.Client, new Uri(BaseUrl)); });

        "and a PAT token".x(
            async () =>
            {
                var response = await client.GetToken(
                        TokenRequest.FromPassword("administrator", "password", new[] {"uma_protection"}))
                    .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;

                Assert.NotNull(response);
                Assert.NotNull(response.Item.AccessToken);
                Assert.NotNull(response.Item.IdToken);

                patToken = response.Item.AccessToken;
                idToken = response.Item.IdToken;
            });

        "and a registered resource".x(
            async () =>
            {
                var resourceSet = new ResourceSet
                {
                    Description = "Test resource",
                    Name = "Test resource",
                    Scopes = new[] {"read"},
                    Type = "Test resource"
                };
                var response =
                    await umaClient.AddResource(resourceSet, patToken).ConfigureAwait(false) as
                        Option<AddResourceSetResponse>.Result;

                Assert.NotNull(response);

                resourceId = response.Item.Id;
            });

        "and an updated authorization policy".x(
            async () =>
            {
                var resourceSet = new ResourceSet
                {
                    Id = resourceId,
                    AuthorizationPolicies =
                        new[]
                        {
                            new PolicyRule
                            {
                                ClientIdsAllowed = new[] {"clientCredentials"}, Scopes = new[] {"read"}
                            }
                        },
                    Description = "Test resource",
                    Name = "Test resource",
                    Scopes = new[] {"read"},
                    Type = "Test resource"
                };
                var response =
                    await umaClient.UpdateResource(resourceSet, patToken).ConfigureAwait(false) as
                        Option<UpdateResourceSetResponse>.Result;

                Assert.NotNull(response);

                resourceId = response.Item.Id;
            });

        "When getting a ticket".x(
            async () =>
            {
                var ticketResponse = await umaClient.RequestPermission(
                        patToken,
                        requests: new PermissionRequest {ResourceSetId = resourceId, Scopes = new[] {"read"}})
                    .ConfigureAwait(false) as Option<TicketResponse>.Result;

                Assert.NotNull(ticketResponse);

                ticketId = ticketResponse.Item.TicketId;
            });

        "and getting an RPT token".x(
            async () =>
            {
                var rptResponse = await client.GetToken(TokenRequest.FromTicketId(ticketId, idToken))
                    .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;

                Assert.NotNull(rptResponse);

                rptToken = rptResponse.Item.AccessToken;
            });

        "then can introspect RPT token using PAT token as authentication".x(
            async () =>
            {
                var introspectResult = await umaClient
                    .Introspect(IntrospectionRequest.Create(rptToken, "access_token", patToken))
                    .ConfigureAwait(false);

                Assert.IsType<Option<UmaIntrospectionResponse>.Result>(introspectResult);
            });
    }
}