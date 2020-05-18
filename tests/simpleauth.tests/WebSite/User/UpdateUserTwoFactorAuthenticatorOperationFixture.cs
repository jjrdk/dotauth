namespace SimpleAuth.Tests.WebSite.User
{
    using Moq;
    using Shared.Models;
    using Shared.Repositories;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Properties;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.WebSite.User;
    using Xunit;

    public class UpdateUserTwoFactorAuthenticatorOperationFixture
    {
        private readonly Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private readonly UpdateUserTwoFactorAuthenticatorOperation _updateUserTwoFactorAuthenticatorOperation;

        public UpdateUserTwoFactorAuthenticatorOperationFixture()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _updateUserTwoFactorAuthenticatorOperation = new UpdateUserTwoFactorAuthenticatorOperation(
                _resourceOwnerRepositoryStub.Object);
        }

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _updateUserTwoFactorAuthenticatorOperation.Execute(null, null, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_ResourceOwner_Does_not_Exist_Then_Exception_Is_Thrown()
        {
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ResourceOwner) null);

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _updateUserTwoFactorAuthenticatorOperation.Execute(
                        "subject",
                        "two_factor",
                        CancellationToken.None))
                .ConfigureAwait(false);

            Assert.Equal(ErrorCodes.InternalError, exception.Code);
            Assert.Equal(Strings.TheRoDoesntExist, exception.Message);
        }

        [Fact]
        public async Task When_Passing_Correct_Parameters_Then_ResourceOwnerIs_Updated()
        {
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ResourceOwner());

            await _updateUserTwoFactorAuthenticatorOperation.Execute("subject", "two_factor", CancellationToken.None)
                .ConfigureAwait(false);

            _resourceOwnerRepositoryStub.Verify(
                r => r.Update(It.IsAny<ResourceOwner>(), It.IsAny<CancellationToken>()));
        }
    }
}
