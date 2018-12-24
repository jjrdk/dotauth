using Moq;
using SimpleIdentityServer.Core.Api.Profile.Actions;
using SimpleIdentityServer.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Profile.Actions
{
    using System.Threading;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    public class LinkProfileActionFixture
    {
        private const string LocalSubject = "localSubject";
        private const string ExternalSubject = "externalSubject";
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private Mock<IProfileRepository> _profileRepositoryStub;
        private ILinkProfileAction _linkProfileAction;

        [Fact]
        public async Task WhenPassingNullParametersThenExceptionAreThrown()
        {            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _linkProfileAction.Execute(null, null, null, false)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _linkProfileAction.Execute(LocalSubject, null, null, false)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _linkProfileAction.Execute(LocalSubject, ExternalSubject, null, false)).ConfigureAwait(false);
        }

        [Fact]
        public async Task WhenResourceOwnerDoesntExistThenExceptionIsThrown()
        {            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult((ResourceOwner)null));

                        var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _linkProfileAction.Execute(LocalSubject, ExternalSubject, "issuer", false)).ConfigureAwait(false);

                        Assert.NotNull(exception);
            Assert.Equal(Errors.ErrorCodes.InternalError, exception.Code);
            Assert.Equal(string.Format(Errors.ErrorDescriptions.TheResourceOwnerDoesntExist, LocalSubject), exception.Message);
        }

        [Fact]
        public async Task WhenLinkingExistingProfileThenExceptionIsThrown()
        {            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new ResourceOwner()));
            _profileRepositoryStub.Setup(p => p.Get(It.IsAny<string>())).Returns(Task.FromResult(new ResourceOwnerProfile
            {
                ResourceOwnerId = "otherSubject"
            }));

                        var exception = await Assert.ThrowsAsync<ProfileAssignedAnotherAccountException>(() => _linkProfileAction.Execute(LocalSubject, ExternalSubject, "issuer", false)).ConfigureAwait(false);

                        Assert.NotNull(exception);
        }

        [Fact]
        public async Task WhenProfileHasAlreadyBeenLinkedThenExceptionIsThrown()
        {            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new ResourceOwner()));
            _profileRepositoryStub.Setup(p => p.Get(It.IsAny<string>())).Returns(Task.FromResult(new ResourceOwnerProfile
            {
                ResourceOwnerId = LocalSubject
            }));

                        var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _linkProfileAction.Execute(LocalSubject, ExternalSubject, "issuer", false)).ConfigureAwait(false);

                        Assert.NotNull(exception);
            Assert.Equal(Errors.ErrorCodes.InternalError, exception.Code);
            Assert.Equal(Errors.ErrorDescriptions.TheProfileAlreadyLinked, exception.Message);
        }

        [Fact]
        public async Task WhenLinkProfileThenOperationIsCalled()
        {            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new ResourceOwner()));
            _profileRepositoryStub.Setup(p => p.Get(It.IsAny<string>())).Returns(Task.FromResult((ResourceOwnerProfile)null));

                        await _linkProfileAction.Execute(LocalSubject, ExternalSubject, "issuer", false).ConfigureAwait(false);

                        _profileRepositoryStub.Verify(p => p.Add(It.Is<IEnumerable<ResourceOwnerProfile>>(r => r.First().ResourceOwnerId == LocalSubject && r.First().Subject == ExternalSubject && r.First().Issuer == "issuer")));
        }

        [Fact]
        public async Task WhenForceLinkProfileThenOperationIsCalled()
        {            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new ResourceOwner()));
            _profileRepositoryStub.Setup(p => p.Get(It.IsAny<string>())).Returns(Task.FromResult(new ResourceOwnerProfile
            {
                ResourceOwnerId = "otherSubject"
            }));

                        await _linkProfileAction.Execute(LocalSubject, ExternalSubject, "issuer", true).ConfigureAwait(false);

                        _profileRepositoryStub.Verify(p => p.Remove(It.Is<IEnumerable<string>>(r => r.Contains(ExternalSubject))));
            _profileRepositoryStub.Verify(p => p.Add(It.Is<IEnumerable<ResourceOwnerProfile>>(r => r.First().ResourceOwnerId == LocalSubject && r.First().Subject == ExternalSubject && r.First().Issuer == "issuer")));

        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _profileRepositoryStub = new Mock<IProfileRepository>();
            _linkProfileAction = new LinkProfileAction(_resourceOwnerRepositoryStub.Object, _profileRepositoryStub.Object);
        }
    }
}