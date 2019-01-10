namespace SimpleAuth.Tests.WebSite.User
{
    using Errors;
    using Exceptions;
    using Moq;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.WebSite.User.Actions;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class UpdateUserTwoFactorAuthenticatorOperationFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private UpdateUserTwoFactorAuthenticatorOperation _updateUserTwoFactorAuthenticatorOperation;

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            InitializeFakeObjects();
            
            await Assert.ThrowsAsync<ArgumentNullException>(() => _updateUserTwoFactorAuthenticatorOperation.Execute(null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_ResourceOwner_Does_not_Exist_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((ResourceOwner)null));

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(() => _updateUserTwoFactorAuthenticatorOperation.Execute("subject", "two_factor")).ConfigureAwait(false);

            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.InternalError, exception.Code);
            Assert.True(exception.Message == ErrorDescriptions.TheRoDoesntExist);
        }

        [Fact]
        public async Task When_Passing_Correct_Parameters_Then_ResourceOwnerIs_Updated()
        {
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ResourceOwner()));

            await _updateUserTwoFactorAuthenticatorOperation.Execute("subject", "two_factor").ConfigureAwait(false);

            _resourceOwnerRepositoryStub.Setup(r => r.UpdateAsync(It.IsAny<ResourceOwner>()));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _updateUserTwoFactorAuthenticatorOperation = new UpdateUserTwoFactorAuthenticatorOperation(
                _resourceOwnerRepositoryStub.Object);
        }
    }
}
