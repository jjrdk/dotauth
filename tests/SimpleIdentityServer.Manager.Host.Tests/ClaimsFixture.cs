namespace SimpleIdentityServer.Manager.Host.Tests
{
    using Client.Claims;
    using Client.Configuration;
    using Shared.Responses;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class ClaimsFixture : IClassFixture<TestManagerServerFixture>
    {
        private const string WellKnownOpenidConfiguration = "http://localhost:5000/.well-known/openid-configuration";
        private readonly TestManagerServerFixture _server;
        private IClaimsClient _claimsClient;

        public ClaimsFixture(TestManagerServerFixture server)
        {
            _server = server;
        }

        [Fact]
        public async Task When_Add_Claim_Then_Several_Claims_Are_Returned()
        {
            InitializeFakeObjects();
            var result = await _claimsClient.Add(
                new Uri(WellKnownOpenidConfiguration),
                new ClaimResponse
                {
                    Code = "code"
                }).ConfigureAwait(false);

            var getAllResult = await _claimsClient.GetAll(new Uri(WellKnownOpenidConfiguration)).ConfigureAwait(false);

            Assert.False(getAllResult.ContainsError);
            Assert.True(getAllResult.Content.Any());
        }

        private void InitializeFakeObjects()
        {
            var addClaimOperation = new AddClaimOperation(_server.Client);
            var deleteClaimOperation = new DeleteClaimOperation(_server.Client);
            var getClaimOperation = new GetClaimOperation(_server.Client);
            var searchClaimsOperation = new SearchClaimsOperation(_server.Client);
            var configurationClient = new GetConfigurationOperation(_server.Client);
            var getAllClaimsOperation = new GetAllClaimsOperation(_server.Client);
            _claimsClient = new ClaimsClient(
                addClaimOperation, deleteClaimOperation, getClaimOperation, searchClaimsOperation,
                configurationClient, getAllClaimsOperation);
        }
    }
}
