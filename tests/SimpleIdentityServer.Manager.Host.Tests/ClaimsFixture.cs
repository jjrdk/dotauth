using Moq;
using SimpleIdentityServer.Common.Client.Factories;
using SimpleIdentityServer.Manager.Client.Claims;
using SimpleIdentityServer.Manager.Client.Configuration;
using System;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

namespace SimpleIdentityServer.Manager.Host.Tests
{
    public class ClaimsFixture : IClassFixture<TestManagerServerFixture>
    {
        private TestManagerServerFixture _server;
        private Mock<IHttpClientFactory> _httpClientFactoryStub;
        private IClaimsClient _claimsClient;

        public ClaimsFixture(TestManagerServerFixture server)
        {
            _server = server;
        }

        #region Happy paths

        #region Get all

        [Fact]
        public async Task When_Add_Claim_Then_Several_Claims_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _httpClientFactoryStub.Setup(h => h.GetHttpClient()).Returns(_server.Client);
            var result = await _claimsClient.Add(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"), new Common.Responses.ClaimResponse
            {
                Code = "code"
            });


            // ACT
            var getAllResult = await _claimsClient.GetAll(new Uri("http://localhost:5000/.well-known/openidmanager-configuration"));

            // ASSERTS
            Assert.False(getAllResult.ContainsError);
            Assert.True(getAllResult.Content.Count() >= 1);
        }

        #endregion

        #endregion
        
        private void InitializeFakeObjects()
        {
            _httpClientFactoryStub = new Mock<IHttpClientFactory>();
            var addClaimOperation = new AddClaimOperation(_httpClientFactoryStub.Object);
            var deleteClaimOperation = new DeleteClaimOperation(_httpClientFactoryStub.Object);
            var getClaimOperation = new GetClaimOperation(_httpClientFactoryStub.Object);
            var searchClaimsOperation = new SearchClaimsOperation(_httpClientFactoryStub.Object);
            var configurationClient = new ConfigurationClient(new GetConfigurationOperation(_httpClientFactoryStub.Object));
            var getAllClaimsOperation = new GetAllClaimsOperation(_httpClientFactoryStub.Object);
            _claimsClient = new ClaimsClient(
                addClaimOperation, deleteClaimOperation, getClaimOperation, searchClaimsOperation,
                configurationClient, getAllClaimsOperation);
        }
    }
}
