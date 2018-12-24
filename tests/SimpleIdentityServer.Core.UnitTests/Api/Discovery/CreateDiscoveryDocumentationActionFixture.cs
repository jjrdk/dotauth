using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Discovery
{
    using Core.Api.Discovery;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    public class CreateDiscoveryDocumentationActionFixture
    {
        private Mock<IScopeRepository> _scopeRepositoryStub;
        private Mock<IClaimRepository> _claimRepositoryStub;
        private IDiscoveryActions _createDiscoveryDocumentationAction;

        [Fact]
        public async Task When_Expose_Two_Scopes_Then_DiscoveryDocument_Is_Correct()
        {            InitializeFakeObjects();
            const string firstScopeName = "firstScopeName";
            const string secondScopeName = "secondScopeName";
            const string notExposedScopeName = "notExposedScopeName";
            ICollection<Scope> scopes = new List<Scope>
            {
                new Scope
                {
                    IsExposed = true,
                    Name = firstScopeName
                },
                new Scope
                {
                    IsExposed = true,
                    Name = secondScopeName
                },
                new Scope
                {
                    IsExposed = false,
                    Name = secondScopeName
                }
            };
            IEnumerable<ClaimAggregate> claims = new List<ClaimAggregate>
            {
                new ClaimAggregate
                {
                    Code = "claim"
                }
            };
            _scopeRepositoryStub.Setup(s => s.GetAll())
                .Returns(Task.FromResult(scopes));
            _claimRepositoryStub.Setup(c => c.GetAllAsync())
                .Returns(() => Task.FromResult(claims));

                        var discoveryInformation = await _createDiscoveryDocumentationAction.CreateDiscoveryInformation("http://test").ConfigureAwait(false);

                        Assert.NotNull(discoveryInformation);
            Assert.True(discoveryInformation.ScopesSupported.Length == 2);
            Assert.Contains(firstScopeName, discoveryInformation.ScopesSupported);
            Assert.Contains(secondScopeName, discoveryInformation.ScopesSupported);
            Assert.DoesNotContain(notExposedScopeName, discoveryInformation.ScopesSupported);
        }

        private void InitializeFakeObjects()
        {
            _scopeRepositoryStub = new Mock<IScopeRepository>();
            _claimRepositoryStub = new Mock<IClaimRepository>();
            _createDiscoveryDocumentationAction = new DiscoveryActions(
                _scopeRepositoryStub.Object,
                _claimRepositoryStub.Object);
        }
    }
}
