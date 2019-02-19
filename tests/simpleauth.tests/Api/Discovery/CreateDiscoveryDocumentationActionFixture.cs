namespace SimpleAuth.Tests.Api.Discovery
{
    using Moq;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.Api.Discovery;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class CreateDiscoveryDocumentationActionFixture
    {
        private readonly Mock<IScopeRepository> _scopeRepositoryStub;
        private readonly DiscoveryActions _createDiscoveryDocumentationAction;

        public CreateDiscoveryDocumentationActionFixture()
        {
            _scopeRepositoryStub = new Mock<IScopeRepository>();
            _createDiscoveryDocumentationAction = new DiscoveryActions(_scopeRepositoryStub.Object);
        }

        [Fact]
        public async Task When_Expose_Two_Scopes_Then_DiscoveryDocument_Is_Correct()
        {
            const string firstScopeName = "firstScopeName";
            const string secondScopeName = "secondScopeName";
            const string notExposedScopeName = "notExposedScopeName";
            var scopes = new[]
            {
                new Scope {IsExposed = true, Name = firstScopeName},
                new Scope {IsExposed = true, Name = secondScopeName},
                new Scope {IsExposed = false, Name = secondScopeName}
            };

            _scopeRepositoryStub.Setup(s => s.GetAll(It.IsAny<CancellationToken>())).ReturnsAsync(scopes);

            var discoveryInformation = await _createDiscoveryDocumentationAction
                .CreateDiscoveryInformation("http://test", CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Equal(2, discoveryInformation.ScopesSupported.Length);
            Assert.Contains(firstScopeName, discoveryInformation.ScopesSupported);
            Assert.Contains(secondScopeName, discoveryInformation.ScopesSupported);
            Assert.DoesNotContain(notExposedScopeName, discoveryInformation.ScopesSupported);
        }
    }
}
