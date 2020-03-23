namespace SimpleAuth.Stores.Marten.AcceptanceTests.Features
{
    using System;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using Xbehave;
    using Xunit;

    public class ClientManagementFeature : AuthorizedManagementFeatureBase
    {
        [Scenario]
        public void SuccessfulClientListing()
        {
            Client[] clients = null;

            "When getting all clients".x(
                async () =>
                {
                    var response = await _managerClient.GetAllClients(_grantedToken.AccessToken).ConfigureAwait(false);

                    Assert.False(response.HasError);

                    clients = response.Content;
                });

            "Then contains list of clients".x(() => { Assert.All(clients, x => { Assert.NotNull(x.ClientId); }); });
        }

        [Scenario]
        public void SuccessfulAddClient()
        {
            "When adding client".x(
                async () =>
                {
                    var client = new Client
                    {
                        ClientId = "test_client",
                        ClientName = "Test Client",
                        Secrets =
                            new[] {new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = "secret"}},
                        AllowedScopes = new[] {"api"},
                        RedirectionUrls = new[] {new Uri("http://localhost/callback"),},
                        ApplicationType = ApplicationTypes.Native,
                        GrantTypes = new[] {GrantTypes.ClientCredentials},
                        JsonWebKeys = TestKeys.SuperSecretKey.CreateSignatureJwk().ToSet()
                    };
                    var response = await _managerClient.AddClient(client, _grantedToken.AccessToken).ConfigureAwait(false);
                });
        }
    }
}
