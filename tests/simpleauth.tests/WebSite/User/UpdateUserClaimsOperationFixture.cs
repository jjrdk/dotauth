namespace SimpleAuth.Tests.WebSite.User
{
    using Errors;
    using Exceptions;
    using Moq;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.WebSite.User.Actions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class UpdateUserClaimsOperationFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private UpdateUserClaimsOperation _updateUserClaimsOperation;

        [Fact]
        public async Task When_Pass_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _updateUserClaimsOperation.Execute(null, null))
                .ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _updateUserClaimsOperation.Execute("subject", null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_ResourceOwner_DoesntExist_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((ResourceOwner) null));

            var exception = await Assert
                .ThrowsAsync<SimpleAuthException>(
                    () => _updateUserClaimsOperation.Execute("subject", new List<Claim>()))
                .ConfigureAwait(false);

            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.InternalError, exception.Code);
            Assert.True(exception.Message == ErrorDescriptions.TheRoDoesntExist);
        }

        [Fact]
        public async Task When_Claims_Are_Updated_Then_Operation_Is_Called()
        {
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ResourceOwner
                {
                    Claims = new List<Claim>
                    {
                        new Claim("type", "value"),
                        new Claim("type1", "value")
                    }
                }));

            await _updateUserClaimsOperation.Execute("subjet",
                    new List<Claim>
                    {
                        new Claim("type", "value1")
                    })
                .ConfigureAwait(false);

            _resourceOwnerRepositoryStub.Verify(p =>
                p.UpdateAsync(It.Is<ResourceOwner>(r => r.Claims.Any(c => c.Type == "type" && c.Value == "value1"))));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _updateUserClaimsOperation = new UpdateUserClaimsOperation(_resourceOwnerRepositoryStub.Object);
        }
    }
}
