namespace SimpleAuth.Tests.Api.Sms.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Sms.Services;
    using Xunit;

    public class SmsAuthenticateResourceOwnerServiceFixture
    {
        private readonly Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private readonly Mock<IConfirmationCodeStore> _confirmationCodeStoreStub;
        private readonly IAuthenticateResourceOwnerService _authenticateResourceOwnerService;

        public SmsAuthenticateResourceOwnerServiceFixture()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _confirmationCodeStoreStub = new Mock<IConfirmationCodeStore>();
            _authenticateResourceOwnerService = new SmsAuthenticateResourceOwnerService(
                _resourceOwnerRepositoryStub.Object,
                _confirmationCodeStoreStub.Object);
        }

        [Fact]
        public async Task When_Pass_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _authenticateResourceOwnerService.AuthenticateResourceOwner(
                        null,
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _authenticateResourceOwnerService.AuthenticateResourceOwner(
                        "login",
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_ConfirmationCode_Does_Not_Exist_Then_Null_Is_Returned()
        {
            _confirmationCodeStoreStub.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult((ConfirmationCode) null));

            var result = await _authenticateResourceOwnerService
                .AuthenticateResourceOwner("login", "password", CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Null(result);
        }

        [Fact]
        public async Task When_Subject_Is_Different_Then_Null_Is_Returned()
        {
            _confirmationCodeStoreStub.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(new ConfirmationCode {Subject = "sub"}));

            var result = await _authenticateResourceOwnerService
                .AuthenticateResourceOwner("login", "password", CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Null(result);
        }

        [Fact]
        public async Task When_ConfirmationCode_Is_Expired_Then_Null_Is_Returned()
        {
            const string login = "login";
            _confirmationCodeStoreStub.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(
                    () => Task.FromResult(
                        new ConfirmationCode
                        {
                            Subject = login, IssueAt = DateTime.UtcNow.AddDays(-1), ExpiresIn = 100
                        }));

            var result = await _authenticateResourceOwnerService
                .AuthenticateResourceOwner(login, "password", CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Null(result);
        }

        [Fact]
        public async Task When_ConfirmationCode_Is_Correct_And_PhoneNumber_Correct_Then_Operation_Is_Called()
        {
            const string login = "login";
            _confirmationCodeStoreStub.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(
                    () => Task.FromResult(
                        new ConfirmationCode {Subject = login, IssueAt = DateTime.UtcNow, ExpiresIn = 100}));
            _resourceOwnerRepositoryStub
                .Setup(
                    r => r.GetResourceOwnerByClaim(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ResourceOwner());

            await _authenticateResourceOwnerService.AuthenticateResourceOwner(login, "password", CancellationToken.None)
                .ConfigureAwait(false);

            _resourceOwnerRepositoryStub.Verify(
                r => r.GetResourceOwnerByClaim(
                    OpenIdClaimTypes.PhoneNumber,
                    login,
                    CancellationToken.None));
            _confirmationCodeStoreStub.Verify(c => c.Remove("password", It.IsAny<CancellationToken>()));
        }
    }
}
