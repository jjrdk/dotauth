namespace DotAuth.AcceptanceTests.Features;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Responses;
using Newtonsoft.Json;
using Xbehave;
using Xunit;
using Xunit.Abstractions;

public sealed class ManageResourceSetFeature : AuthFlowFeature
{
    /// <inheritdoc />
    public ManageResourceSetFeature(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Scenario(DisplayName = "Can register a resource for a user and manage policies")]
    public void CanRegisterAResourceForUserAndManagePolicies()
    {
        TokenClient client = null!;
        UmaClient umaClient = null!;
        GrantedTokenResponse token = null!;
        AddResourceSetResponse resourceSetResponse = null!;
        EditPolicyResponse policyRules = null!;

        "Given a token client".x(
            () =>
            {
                client = new TokenClient(
                    TokenCredentials.FromClientCredentials("clientCredentials", "clientCredentials"),
                    _fixture.Client,
                    new Uri(WellKnownOpenidConfiguration));
            });

        "And a UMA client".x(() => { umaClient = new UmaClient(_fixture.Client, new Uri(BaseUrl)); });

        "When getting a PAT token".x(
            async () =>
            {
                var response = await client.GetToken(
                        TokenRequest.FromPassword("administrator", "password", new[] { "uma_protection" }))
                    .ConfigureAwait(false) as Option<GrantedTokenResponse>.Result;
                token = response.Item;

                Assert.NotNull(token);
            });

        "Then can register a resource".x(
            async () =>
            {
                var resource = new ResourceSet
                {
                    AuthorizationPolicies = new[]
                    {
                        new PolicyRule
                        {
                            ClientIdsAllowed = new[] {"clientCredentials"},
                            IsResourceOwnerConsentNeeded = true,
                            Scopes = new[] {"read"}
                        }
                    },
                    Name = "test resource",
                    Scopes = new[] { "read" },
                    Type = "test"
                };
                var response = await umaClient.AddResource(resource, token.AccessToken).ConfigureAwait(false) as Option<AddResourceSetResponse>.Result;

                Assert.NotNull(response);

                resourceSetResponse = response.Item;
            });

        "And can view resource policies".x(
            async () =>
            {
                var msg = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(resourceSetResponse.UserAccessPolicyUri)
                };
                msg.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

                var policyResponse = await _fixture.Client().SendAsync(msg).ConfigureAwait(false);

                Assert.True(policyResponse.IsSuccessStatusCode);

                var content = await policyResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                policyRules = JsonConvert.DeserializeObject<EditPolicyResponse>(content);

                Assert.Single(policyRules!.Rules);
            });

        "And can update resource policies".x(
            async () =>
            {
                policyRules.Rules[0] = policyRules.Rules[0] with { IsResourceOwnerConsentNeeded = false };

                var msg = new HttpRequestMessage
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri(resourceSetResponse.UserAccessPolicyUri),
                    Content = new StringContent(JsonConvert.SerializeObject(policyRules))
                };
                msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

                var policyResponse = await _fixture.Client().SendAsync(msg).ConfigureAwait(false);

                Assert.True(policyResponse.IsSuccessStatusCode);
            });
    }
}