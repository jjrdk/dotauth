namespace SimpleAuth.Tests.Api.Profile.Actions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.Api.Profile;
    using Xunit;

    public class GetResourceOwnerClaimsActionFixture
    {
        private Mock<IProfileRepository> _profileRepositoryStub;
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private GetResourceOwnerClaimsAction _getResourceOwnerClaimsAction;

        [Fact]
        public async Task WhenPassNullParameterThenExceptionIsThrown()
        {            InitializeFakeObjects();

            
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getResourceOwnerClaimsAction.Execute(null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getResourceOwnerClaimsAction.Execute(string.Empty)).ConfigureAwait(false);
        }

        [Fact]
        public async Task WhenProfileDoesntExistThenNullIsReturned()
        {
            // INITIALIZE
            InitializeFakeObjects();
            _profileRepositoryStub.Setup(p => p.Get(It.IsAny<string>())).Returns(Task.FromResult((ResourceOwnerProfile)null));

                        var result = await _getResourceOwnerClaimsAction.Execute("externalSubject").ConfigureAwait(false);

                        Assert.Null(result);
        }

        [Fact]
        public async Task WhenProfileExistsThenResourceOwnerIsReturned()
        {
            // INITIALIZE
            InitializeFakeObjects();
            _profileRepositoryStub.Setup(p => p.Get(It.IsAny<string>())).Returns(Task.FromResult(new ResourceOwnerProfile()));
            _resourceOwnerRepositoryStub.Setup(p => p.Get(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new ResourceOwner
            {
                Id = "id"
            }));

                        var result = await _getResourceOwnerClaimsAction.Execute("externalSubject").ConfigureAwait(false);

                        Assert.NotNull(result);
            Assert.Equal("id", result.Id);
        }

        private void InitializeFakeObjects()
        {
            _profileRepositoryStub = new Mock<IProfileRepository>();
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _getResourceOwnerClaimsAction = new GetResourceOwnerClaimsAction(_profileRepositoryStub.Object,
                _resourceOwnerRepositoryStub.Object);
        }
    }
}
