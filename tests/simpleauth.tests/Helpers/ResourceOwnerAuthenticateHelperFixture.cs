namespace SimpleAuth.Tests.Helpers
{
    using SimpleAuth.Helpers;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class ResourceOwnerAuthenticateHelperFixture
    {
        private IResourceOwnerAuthenticateHelper _resourceOwnerAuthenticateHelper;

        [Fact]
        public async Task When_Pass_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _resourceOwnerAuthenticateHelper.Authenticate(null, null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _resourceOwnerAuthenticateHelper.Authenticate("login", null)).ConfigureAwait(false);
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerAuthenticateHelper = new ResourceOwnerAuthenticateHelper(null);
        }
    }
}
