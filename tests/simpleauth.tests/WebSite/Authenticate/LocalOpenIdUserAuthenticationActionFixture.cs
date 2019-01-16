namespace SimpleAuth.Tests.WebSite.Authenticate
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Exceptions;
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Models;
    using SimpleAuth.Helpers;
    using SimpleAuth.WebSite.Authenticate.Actions;
    using SimpleAuth.WebSite.Authenticate.Common;
    using Xunit;

    public sealed class LocalOpenIdUserAuthenticationActionFixture
    {
        private Mock<IResourceOwnerAuthenticateHelper> _resourceOwnerAuthenticateHelperStub;
        private Mock<IAuthenticateHelper> _authenticateHelperFake;
        private ILocalOpenIdUserAuthenticationAction _localUserAuthenticationAction;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exceptions_Are_Thrown()
        {            InitializeFakeObjects();
            var localAuthenticationParameter = new LocalAuthenticationParameter();

            
            await Assert.ThrowsAsync<ArgumentNullException>(() => _localUserAuthenticationAction.Execute(null, null, null, null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _localUserAuthenticationAction.Execute(localAuthenticationParameter, null, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Resource_Owner_Cannot_Be_Authenticated_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            var localAuthenticationParameter = new LocalAuthenticationParameter();
            var authorizationParameter = new AuthorizationParameter();
            _resourceOwnerAuthenticateHelperStub.Setup(r => r.Authenticate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult((ResourceOwner)null));

                        await Assert.ThrowsAsync<AuthServerAuthenticationException>(() => _localUserAuthenticationAction.Execute(localAuthenticationParameter, authorizationParameter, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Resource_Owner_Credentials_Are_Correct_Then_Event_Is_Logged_And_Claims_Are_Returned()
        {            const string subject = "subject";
            InitializeFakeObjects();
            var localAuthenticationParameter = new LocalAuthenticationParameter();
            var authorizationParameter = new AuthorizationParameter();
            var resourceOwner = new ResourceOwner
            {
                Id = subject
            };
            _resourceOwnerAuthenticateHelperStub.Setup(r => r.Authenticate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult(resourceOwner));

                        var result = await _localUserAuthenticationAction.Execute(localAuthenticationParameter,
                authorizationParameter, 
                null, null).ConfigureAwait(false);

            // Specify the resource owner authentication date
            Assert.NotNull(result);
            Assert.NotNull(result.Claims);
            Assert.Contains(result.Claims, r => r.Type == ClaimTypes.AuthenticationInstant ||
                r.Type == JwtConstants.StandardResourceOwnerClaimNames.Subject);
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerAuthenticateHelperStub = new Mock<IResourceOwnerAuthenticateHelper>();
            _authenticateHelperFake = new Mock<IAuthenticateHelper>();
            _localUserAuthenticationAction = new LocalOpenIdUserAuthenticationAction(
                _resourceOwnerAuthenticateHelperStub.Object,
                _authenticateHelperFake.Object);
        }
    }
}
