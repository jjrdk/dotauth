namespace SimpleAuth.Tests.WebSite.User
{
    using Moq;
    using Shared.Models;
    using Shared.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.WebSite.User;
    using Xunit;

    public class UpdateUserClaimsOperationFixture
    {
        private readonly Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private readonly UpdateUserClaimsOperation _updateUserClaimsOperation;

        public UpdateUserClaimsOperationFixture()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _updateUserClaimsOperation = new UpdateUserClaimsOperation(_resourceOwnerRepositoryStub.Object);
        }

        [Fact]
        public async Task When_Pass_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            await Assert
                .ThrowsAsync<SimpleAuthException>(
                    () => _updateUserClaimsOperation.Execute(null, null, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Pass_Bad_Parameters_Then_Exceptions_Are_Thrown()
        {
            await Assert
                .ThrowsAsync<SimpleAuthException>(
                    () => _updateUserClaimsOperation.Execute("subject", null, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_ResourceOwner_DoesntExist_Then_Exception_Is_Thrown()
        {
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ResourceOwner) null);

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(
                    () => _updateUserClaimsOperation.Execute("subject", new List<Claim>(), CancellationToken.None))
                .ConfigureAwait(false);

            Assert.Equal(ErrorCodes.InternalError, exception.Code);
            Assert.Equal(ErrorDescriptions.TheRoDoesntExist, exception.Message);
        }

        [Fact]
        public async Task When_Claims_Are_Updated_Then_Operation_Is_Called()
        {
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new ResourceOwner {Claims = new[] {new Claim("type", "value"), new Claim("type1", "value")}});

            await _updateUserClaimsOperation.Execute(
                    "subjet",
                    new List<Claim> {new Claim("type", "value1")},
                    CancellationToken.None)
                .ConfigureAwait(false);

            _resourceOwnerRepositoryStub.Verify(
                p => p.Update(
                    It.Is<ResourceOwner>(r => r.Claims.Any(c => c.Type == "type" && c.Value == "value1")),
                    It.IsAny<CancellationToken>()));
        }
    }
}
