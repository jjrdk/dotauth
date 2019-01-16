namespace SimpleAuth.Tests.Helpers
{
    using System;
    using System.Threading.Tasks;
    using Moq;
    using SimpleAuth.Helpers;
    using Xunit;

    public class ResourceOwnerAuthenticateHelperFixture
    {
        private Mock<IAmrHelper> _amrHelperStub;
        private IResourceOwnerAuthenticateHelper _resourceOwnerAuthenticateHelper;

        [Fact]
        public async Task When_Pass_Null_Parameters_Then_Exceptions_Are_Thrown()
        {            InitializeFakeObjects();

            
            await Assert.ThrowsAsync<ArgumentNullException>(() => _resourceOwnerAuthenticateHelper.Authenticate(null, null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _resourceOwnerAuthenticateHelper.Authenticate("login", null)).ConfigureAwait(false);
        }

        private void InitializeFakeObjects()
        {
            _amrHelperStub = new Mock<IAmrHelper>();
            _resourceOwnerAuthenticateHelper = new ResourceOwnerAuthenticateHelper(null, _amrHelperStub.Object);
        }
    }
}
