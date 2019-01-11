namespace SimpleAuth.Uma.Tests.Api.ResourceSetController.Actions
{
    using Errors;
    using Exceptions;
    using Moq;
    using System;
    using System.Threading.Tasks;
    using Repositories;
    using SimpleAuth.Api.ResourceSetController.Actions;
    using SimpleAuth.Shared.Models;
    using Xunit;

    public class RemoveResourceSetActionFixture
    {
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private DeleteResourceSetAction _deleteResourceSetAction;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _deleteResourceSetAction.Execute(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_ResourceSet_Does_Not_Exist_Then_False_Is_Returned()
        {
            const string resourceSetId = "resourceSetId";
            InitializeFakeObjects();
            _resourceSetRepositoryStub.Setup(r => r.Get(It.IsAny<string>()))
                .Returns(() => Task.FromResult((ResourceSet)null));

            var result = await _deleteResourceSetAction.Execute(resourceSetId).ConfigureAwait(false);

            Assert.False(result);
        }

        [Fact]
        public async Task When_ResourceSet_Cannot_Be_Updated_Then_Exception_Is_Thrown()
        {
            const string resourceSetId = "resourceSetId";
            InitializeFakeObjects();
            _resourceSetRepositoryStub.Setup(r => r.Get(It.IsAny<string>()))
                .Returns(Task.FromResult(new ResourceSet()));
            _resourceSetRepositoryStub.Setup(r => r.Delete(It.IsAny<string>()))
                .Returns(Task.FromResult(false));

            var exception = await Assert.ThrowsAsync<SimpleAuthException>(() => _deleteResourceSetAction.Execute(resourceSetId)).ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InternalError);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheResourceSetCannotBeRemoved, resourceSetId));
        }

        [Fact]
        public async Task When_ResourceSet_Is_Removed_Then_True_Is_Returned()
        {
            const string resourceSetId = "resourceSetId";
            InitializeFakeObjects();
            _resourceSetRepositoryStub.Setup(r => r.Get(It.IsAny<string>()))
                .Returns(Task.FromResult(new ResourceSet()));
            _resourceSetRepositoryStub.Setup(r => r.Delete(It.IsAny<string>()))
               .Returns(Task.FromResult(true));

            var result = await _deleteResourceSetAction.Execute(resourceSetId).ConfigureAwait(false);

            Assert.True(result);
        }

        private void InitializeFakeObjects()
        {
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _deleteResourceSetAction = new DeleteResourceSetAction(_resourceSetRepositoryStub.Object);
        }
    }
}
