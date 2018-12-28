namespace SimpleAuth.Tests.WebSite.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Errors;
    using Exceptions;
    using Moq;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.WebSite.User.Actions;
    using Xunit;

    public class UpdateUserClaimsOperationFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private Mock<IClaimRepository> _claimRepositoryStub;
        private IUpdateUserClaimsOperation _updateUserClaimsOperation;

        [Fact]
        public async Task When_Pass_Null_Parameters_Then_Exceptions_Are_Thrown()
        {            InitializeFakeObjects();

            
            await Assert.ThrowsAsync<ArgumentNullException>(() => _updateUserClaimsOperation.Execute(null, null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _updateUserClaimsOperation.Execute("subject", null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_ResourceOwner_DoesntExist_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((ResourceOwner)null));

                        var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _updateUserClaimsOperation.Execute("subject", new List<ClaimAggregate>())).ConfigureAwait(false);

                        Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InternalError);
            Assert.True(exception.Message == ErrorDescriptions.TheRoDoesntExist);
        }

        [Fact]
        public async Task When_Claims_Are_Updated_Then_Operation_Is_Called()
        {            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ResourceOwner
                {
                    Claims = new List<Claim>
                    {
                        new Claim("type", "value"),
                        new Claim("type1", "value")
                    }
                }));
            _claimRepositoryStub.Setup(r => r.GetAllAsync()).Returns(Task.FromResult((IEnumerable<ClaimAggregate>)new List<ClaimAggregate>
            {
                new ClaimAggregate
                {
                    Code = "type"
                }
            }));

                        await _updateUserClaimsOperation.Execute("subjet", new List<ClaimAggregate>
            {
                new ClaimAggregate("type", "value1")
            }).ConfigureAwait(false);

                        _resourceOwnerRepositoryStub.Verify(p => p.UpdateAsync(It.Is<ResourceOwner>(r => r.Claims.Any(c => c.Type == "type" && c.Value == "value1"))));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _claimRepositoryStub = new Mock<IClaimRepository>();
            _updateUserClaimsOperation = new UpdateUserClaimsOperation(_resourceOwnerRepositoryStub.Object,
                _claimRepositoryStub.Object);
        }
    }
}
