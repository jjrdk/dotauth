namespace SimpleAuth.Tests.Api.Discovery
{
    using Moq;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.Api.Discovery;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public class CreateDiscoveryDocumentationActionFixture
    {
        private Mock<IScopeRepository> _scopeRepositoryStub;
        private IDiscoveryActions _createDiscoveryDocumentationAction;

        [Fact]
        public async Task When_Expose_Two_Scopes_Then_DiscoveryDocument_Is_Correct()
        {
            InitializeFakeObjects();
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
            //IEnumerable<Claim> claims = new List<Claim>
            //{
            //    new Claim
            //    {
            //        Code = "claim"
            //    }
            //};
            _scopeRepositoryStub.Setup(s => s.GetAll())
                .Returns(Task.FromResult(scopes));
            //_claimRepositoryStub.Setup(c => c.GetAllAsync())
            //    .Returns(() => Task.FromResult(claims));

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
            _createDiscoveryDocumentationAction = new DiscoveryActions(_scopeRepositoryStub.Object);
        }
    }
}
