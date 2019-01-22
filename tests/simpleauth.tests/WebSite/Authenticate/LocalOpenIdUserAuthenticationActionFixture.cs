using SimpleAuth.Services;

namespace SimpleAuth.Tests.WebSite.Authenticate
{
    using Exceptions;
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Models;
    using SimpleAuth.WebSite.Authenticate.Actions;
    using SimpleAuth.WebSite.Authenticate.Common;
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class LocalOpenIdUserAuthenticationActionFixture
    {
        private Mock<IAuthenticateHelper> _authenticateHelperFake;
        private ILocalOpenIdUserAuthenticationAction _localUserAuthenticationAction;

        public LocalOpenIdUserAuthenticationActionFixture()
        {
            InitializeFakeObjects();
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exceptions_Are_Thrown()
        {
            var localAuthenticationParameter = new LocalAuthenticationParameter();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _localUserAuthenticationAction.Execute(null, null, null, null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _localUserAuthenticationAction.Execute(localAuthenticationParameter, null, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Resource_Owner_Cannot_Be_Authenticated_Then_Exception_Is_Thrown()
        {
            var authenticateService = new Mock<IAuthenticateResourceOwnerService>();
            authenticateService.SetupGet(x => x.Amr).Returns("pwd");
            authenticateService.Setup(x => x.AuthenticateResourceOwnerAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((ResourceOwner) null);
            InitializeFakeObjects(authenticateService.Object);
            var localAuthenticationParameter = new LocalAuthenticationParameter();
            var authorizationParameter = new AuthorizationParameter();

            await Assert.ThrowsAsync<AuthServerAuthenticationException>(
                    () => _localUserAuthenticationAction.Execute(
                        localAuthenticationParameter,
                        authorizationParameter,
                        null,
                        null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Resource_Owner_Credentials_Are_Correct_Then_Event_Is_Logged_And_Claims_Are_Returned()
        {
            const string subject = "subject";
            var localAuthenticationParameter = new LocalAuthenticationParameter();
            var authorizationParameter = new AuthorizationParameter();
            var resourceOwner = new ResourceOwner { Id = subject };
            var authenticateService = new Mock<IAuthenticateResourceOwnerService>();
            authenticateService.SetupGet(x => x.Amr).Returns("pwd");
            authenticateService.Setup(x => x.AuthenticateResourceOwnerAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(resourceOwner);
            InitializeFakeObjects(authenticateService.Object);

            var result = await _localUserAuthenticationAction
                .Execute(localAuthenticationParameter, authorizationParameter, null, null)
                .ConfigureAwait(false);

            // Specify the resource owner authentication date
            Assert.NotNull(result);
            Assert.NotNull(result.Claims);
            Assert.Contains(
                result.Claims,
                r => r.Type == ClaimTypes.AuthenticationInstant
                     || r.Type == JwtConstants.StandardResourceOwnerClaimNames.Subject);
        }

        private void InitializeFakeObjects(params IAuthenticateResourceOwnerService[] services)
        {
            _authenticateHelperFake = new Mock<IAuthenticateHelper>();
            _localUserAuthenticationAction = new LocalOpenIdUserAuthenticationAction(
                services,
                _authenticateHelperFake.Object);
        }
    }
}
