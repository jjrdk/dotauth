using Moq;
using SimpleIdentityServer.Core.Api.Profile.Actions;
using SimpleIdentityServer.Core.Exceptions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Profile.Actions
{
    using System.Threading;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Parameters;
    using SimpleAuth.Shared.Repositories;

    public class GetUserProfilesActionFixture
    {
        private Mock<IProfileRepository> _profileRepositoryStub;
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private IGetUserProfilesAction _getProfileAction;

        [Fact]
        public async Task WhenPassingNullParameterThenExceptionIsThrown()
        {
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getProfileAction.Execute(null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getProfileAction.Execute(string.Empty)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_User_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            var ex = await Assert.ThrowsAsync<IdentityServerException>(() => _getProfileAction.Execute("subject")).ConfigureAwait(false);
            
            Assert.Equal("internal_error", ex.Code);
            Assert.Equal("The resource owner subject doesn't exist", ex.Message);
        }

        [Fact]
        public async Task WhenGetProfileThenOperationIsCalled()
        {
            const string subject = "subject"; InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new ResourceOwner()));

            await _getProfileAction.Execute(subject).ConfigureAwait(false);

            _profileRepositoryStub.Verify(p => p.Search(It.Is<SearchProfileParameter>(r => r.ResourceOwnerIds.Contains(subject))));
        }

        private void InitializeFakeObjects()
        {
            _profileRepositoryStub = new Mock<IProfileRepository>();
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _getProfileAction = new GetUserProfilesAction(_profileRepositoryStub.Object, _resourceOwnerRepositoryStub.Object);
        }
    }
}
