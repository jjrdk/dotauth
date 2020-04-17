namespace SimpleAuth.AcceptanceTests.Features
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Newtonsoft.Json;
    using SimpleAuth.Client;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Responses;
    using Xbehave;
    using Xunit;

    public class ManageResourceSetFeature : AuthFlowFeature
    {
        [Scenario(DisplayName = "Can register a resource for a user and manage policies")]
        public void CanRegisterAResourceForUserAndManagePolicies()
        {
            TokenClient client = null;
            UmaClient umaClient = null;
            GrantedTokenResponse token = null;
            AddResourceSetResponse resourceSetResponse = null;
            EditPolicyResponse policyRules = null;

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
                        .ConfigureAwait(false);
                    token = response.Content;

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
                    var response = await umaClient.AddResource(resource, token.AccessToken).ConfigureAwait(false);

                    Assert.False(response.HasError);

                    resourceSetResponse = response.Content;
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

                    var policyResponse = await _fixture.Client.SendAsync(msg).ConfigureAwait(false);

                    Assert.True(policyResponse.IsSuccessStatusCode);

                    var content = await policyResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    policyRules = JsonConvert.DeserializeObject<EditPolicyResponse>(content);

                    Assert.Single(policyRules.Rules);
                });

            "And can update resource policies".x(
                async () =>
                {
                    policyRules.Rules[0].IsResourceOwnerConsentNeeded = false;

                    var msg = new HttpRequestMessage
                    {
                        Method = HttpMethod.Put,
                        RequestUri = new Uri(resourceSetResponse.UserAccessPolicyUri),
                        Content = new StringContent(JsonConvert.SerializeObject(policyRules))
                    };
                    msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

                    var policyResponse = await _fixture.Client.SendAsync(msg).ConfigureAwait(false);

                    Assert.True(policyResponse.IsSuccessStatusCode);
                });
        }
    }
}
