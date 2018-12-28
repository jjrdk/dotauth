namespace SimpleAuth.Tests.Api.Profile.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Errors;
    using Exceptions;
    using Moq;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.Api.Profile.Actions;
    using Xunit;

    public class UnlinkProfileActionFixture
    {
        private const string LocalSubject = "localSubject";
        private const string ExternalSubject = "externalSubject";
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private Mock<IProfileRepository> _profileRepositoryStub;
        private IUnlinkProfileAction _unlinkProfileAction;

        [Fact]
        public async Task WhenNullParametersArePassedThenExceptionsAreThrown()
        {            InitializeFakeObjects();

            
            await Assert.ThrowsAsync<ArgumentNullException>(() => _unlinkProfileAction.Execute(null, null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _unlinkProfileAction.Execute(LocalSubject, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task WhenResourceOwnerDoesntExistThenExceptionIsThrown()
        {            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult((ResourceOwner)null));

                        var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _unlinkProfileAction.Execute(LocalSubject, ExternalSubject)).ConfigureAwait(false);

                        Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.InternalError, exception.Code);
            Assert.Equal(string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, LocalSubject), exception.Message);
        }

        [Fact]
        public async Task WhenUserNotAuthorizedToUnlinkProfileThenExceptionIsThrown()
        {            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new ResourceOwner()));
            _profileRepositoryStub.Setup(r => r.Get(It.IsAny<string>())).Returns(Task.FromResult(new ResourceOwnerProfile
            {
                ResourceOwnerId = "otherSubject"
            }));

                        var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _unlinkProfileAction.Execute(LocalSubject, ExternalSubject)).ConfigureAwait(false);

                        Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.InternalError, exception.Code);
            Assert.Equal(ErrorDescriptions.NotAuthorizedToRemoveTheProfile, exception.Message);

        }

        [Fact]
        public async Task WhenUnlinkProfileThenOperationIsCalled()
        {            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new ResourceOwner()));
            _profileRepositoryStub.Setup(r => r.Get(It.IsAny<string>())).Returns(Task.FromResult(new ResourceOwnerProfile
            {
                ResourceOwnerId = LocalSubject
            }));

                        await _unlinkProfileAction.Execute(LocalSubject, ExternalSubject).ConfigureAwait(false);

                        _profileRepositoryStub.Verify(p => p.Remove(It.Is<IEnumerable<string>>(r => r.Contains(ExternalSubject))));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _profileRepositoryStub = new Mock<IProfileRepository>();
            _unlinkProfileAction = new UnlinkProfileAction(_resourceOwnerRepositoryStub.Object,
                _profileRepositoryStub.Object);
        }
    }
}
