namespace SimpleAuth.Server.Tests.Apis
{
    using Controllers;
    using Exceptions;
    using Moq;
    using Shared.Models;
    using Shared.Parameters;
    using Shared.Repositories;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class GetUserProfilesActionFixture
    {
        private Mock<IProfileRepository> _profileRepositoryStub;
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private GetUserProfilesAction _getProfileAction;

        [Fact]
        public async Task WhenPassingNullParameterThenExceptionIsThrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _getProfileAction.Execute(null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getProfileAction.Execute(string.Empty)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_User_Does_Not_Exist_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            var ex = await Assert.ThrowsAsync<SimpleAuthException>(() => _getProfileAction.Execute("subject")).ConfigureAwait(false);

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
            _getProfileAction = new GetUserProfilesAction(_resourceOwnerRepositoryStub.Object, _profileRepositoryStub.Object);
        }
    }
}
