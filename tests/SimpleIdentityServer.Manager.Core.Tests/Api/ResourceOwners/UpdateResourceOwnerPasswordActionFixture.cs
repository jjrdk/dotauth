using Moq;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Manager.Core.Api.ResourceOwners.Actions;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using SimpleIdentityServer.Manager.Core.Parameters;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.ResourceOwners
{
    public class UpdateResourceOwnerPasswordActionFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private IUpdateResourceOwnerPasswordAction _updateResourceOwnerPasswordAction;
                
        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _updateResourceOwnerPasswordAction.Execute(null));
        }

        [Fact]
        public async Task When_Resource_Owner_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string subject = "invalid_subject";
            var request = new UpdateResourceOwnerPasswordParameter
            {
                Login = subject
            };
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult((ResourceOwner)null));

            // ACT
            var exception = await Assert.ThrowsAsync<IdentityServerManagerException>(() => _updateResourceOwnerPasswordAction.Execute(request));

            // ASSERT
            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.InvalidParameterCode, exception.Code);
            Assert.Equal(string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, subject), exception.Message);
        }

        [Fact]
        public async Task When_Resource_Owner_Cannot_Be_Updated_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            var request = new UpdateResourceOwnerPasswordParameter
            {
                Login = "subject",
                Password = "password"
            };
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new ResourceOwner()));
            _resourceOwnerRepositoryStub.Setup(r => r.UpdateAsync(It.IsAny<ResourceOwner>())).Returns(Task.FromResult(false));

            // ACT
            var result = await Assert.ThrowsAsync<IdentityServerManagerException>(() => _updateResourceOwnerPasswordAction.Execute(request));

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal("internal_error", result.Code);
            Assert.Equal("the password cannot be updated", result.Message);
        }

        [Fact]
        public async Task When_Update_Resource_Owner_Password_Then_Operation_Is_Called()
        {
            // ARRANGE
            var request = new UpdateResourceOwnerPasswordParameter
            {
                Login = "subject",
                Password = "password"
            };
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new ResourceOwner()));
            _resourceOwnerRepositoryStub.Setup(r => r.UpdateAsync(It.IsAny<ResourceOwner>())).Returns(Task.FromResult(true));

            // ACT
            await _updateResourceOwnerPasswordAction.Execute(request);

            // ASSERT
            _resourceOwnerRepositoryStub.Verify(r => r.UpdateAsync(It.IsAny<ResourceOwner>()));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _updateResourceOwnerPasswordAction = new UpdateResourceOwnerPasswordAction(
                _resourceOwnerRepositoryStub.Object);
        }
    }
}