using SimpleAuth.Shared;

namespace SimpleAuth.Tests.WebSite.Consent
{
    using Errors;
    using Exceptions;
    using Moq;
    using Parameters;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.Common;
    using SimpleAuth.WebSite.Consent.Actions;
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class DisplayConsentActionFixture
    {
        private Mock<IScopeRepository> _scopeRepositoryFake;
        private Mock<IClientStore> _clientRepositoryFake;
        private Mock<IGenerateAuthorizationResponse> _generateAuthorizationResponseFake;
        private IDisplayConsentAction _displayConsentAction;
        private Mock<IConsentRepository> _consentRepository;

        public DisplayConsentActionFixture()
        {
            InitializeFakeObjects();
        }

        [Fact]
        public async Task When_Parameter_Is_Null_Then_Exception_Is_Thrown()
        {
            var authorizationParameter = new AuthorizationParameter();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _displayConsentAction.Execute(null, null, null))
                .ConfigureAwait(false);
            await Assert
                .ThrowsAsync<ArgumentNullException>(
                    () => _displayConsentAction.Execute(authorizationParameter, null, null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_A_Consent_Has_Been_Given_Then_Redirect_To_Callback()
        {
            var scope = "scope";
            var clientid = "client";
            var client = new Client
            {
                ClientId = clientid,
                AllowedScopes = new List<Scope> { new Scope { Name = scope, IsDisplayedInConsent = true } }
            };
            var consent = new Consent
            {
                Client = client,
                GrantedScopes = new List<Scope> { new Scope { Name = scope } }
            };
            _consentRepository.Setup(x => x.GetConsentsForGivenUser(It.IsAny<string>()))
                .ReturnsAsync(new List<Consent> { consent });
            _scopeRepositoryFake.Setup(x => x.SearchByNames(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new List<Scope> { new Scope { Name = scope, IsDisplayedInConsent = true } });
            var claimsIdentity = new ClaimsIdentity(new[] { new Claim("sub", "test"), });
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientid,
                Scope = scope,
                ResponseMode = ResponseMode.fragment
            };

            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>())).Returns(Task.FromResult(client));
            var result = await _displayConsentAction.Execute(authorizationParameter, claimsPrincipal, null)
                .ConfigureAwait(false);

            Assert.Equal(ResponseMode.fragment, result.EndpointResult.RedirectInstruction.ResponseMode);
        }

        [Fact]
        public async Task
            When_A_Consent_Has_Been_Given_And_The_AuthorizationFlow_Is_Not_Supported_Then_Exception_Is_Thrown()
        {
            const string clientId = "clientId";
            const string state = "state";
            var claimsIdentity = new ClaimsIdentity(new[] { new Claim("sub", "test") });
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var authorizationParameter = new AuthorizationParameter
            {
                ResponseType = ResponseTypeNames.Token,
                Scope = "scope",
                ClientId = clientId,
                State = state,
                ResponseMode = ResponseMode.None // No response mode is defined
            };
            var consent = new Consent
            {
                GrantedScopes = new List<Scope> { new Scope { Name = "scope" } },
                Client = new Client
                {
                    ClientId = clientId,
                    AllowedScopes = new List<Scope> { new Scope { Name = "scope" } }
                }
            };
            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>())).Returns(Task.FromResult(new Client()));
            _consentRepository.Setup(x => x.GetConsentsForGivenUser(It.IsAny<string>())).ReturnsAsync(new[] { consent });
            var exception = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _displayConsentAction.Execute(authorizationParameter, claimsPrincipal, null))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.Equal(ErrorDescriptions.TheAuthorizationFlowIsNotSupported, exception.Message);
            Assert.Equal(state, exception.State);

        }

        [Fact]
        public async Task When_No_Consent_Has_Been_Given_And_Client_Does_Not_Exist_Then_Exception_Is_Thrown()
        {
            const string clientId = "clientId";
            const string state = "state";
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var authorizationParameter = new AuthorizationParameter { ClientId = clientId, State = state };

            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>())).Returns(Task.FromResult((Client)null));

            var exception = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _displayConsentAction.Execute(authorizationParameter, claimsPrincipal, null))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.Equal(string.Format(ErrorDescriptions.ClientIsNotValid, clientId), exception.Message);
            Assert.Equal(state, exception.State);
        }

        [Fact]
        public async Task When_No_Consent_Has_Been_Given_Then_Redirect_To_Consent_Screen()
        {
            const string clientId = "clientId";
            const string state = "state";
            const string scopeName = "profile";
            var claimsIdentity = new ClaimsIdentity(new[] {new Claim("sub", "test")});
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var client = new Client();
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                State = state,
                Claims = null,
                Scope = scopeName
            };
            ICollection<Scope> scopes = new List<Scope> { new Scope { IsDisplayedInConsent = true, Name = scopeName } };

            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>())).Returns(Task.FromResult(client));
            _scopeRepositoryFake.Setup(s => s.SearchByNames(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(scopes));

            await _displayConsentAction.Execute(authorizationParameter, claimsPrincipal, null).ConfigureAwait(false);

            Assert.Contains(scopes, s => s.Name == scopeName);
        }

        private void InitializeFakeObjects()
        {
            _scopeRepositoryFake = new Mock<IScopeRepository>();
            _clientRepositoryFake = new Mock<IClientStore>();
            _generateAuthorizationResponseFake = new Mock<IGenerateAuthorizationResponse>();
            _consentRepository = new Mock<IConsentRepository>();
            _displayConsentAction = new DisplayConsentAction(
                _scopeRepositoryFake.Object,
                _clientRepositoryFake.Object,
                _consentRepository.Object,
                _generateAuthorizationResponseFake.Object);
        }
    }
}
