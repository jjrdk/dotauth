using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Consent
{
    using SimpleAuth.Common;
    using SimpleAuth.Errors;
    using SimpleAuth.Exceptions;
    using SimpleAuth.Factories;
    using SimpleAuth.Helpers;
    using SimpleAuth.Parameters;
    using SimpleAuth.Results;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.WebSite.Consent.Actions;

    public sealed class DisplayConsentActionFixture
    {
        private Mock<IScopeRepository> _scopeRepositoryFake;
        private Mock<IClientStore> _clientRepositoryFake;
        private Mock<IConsentHelper> _consentHelperFake;
        private Mock<IGenerateAuthorizationResponse> _generateAuthorizationResponseFake;
        private Mock<IParameterParserHelper> _parameterParserHelperFake;
        private Mock<IActionResultFactory> _actionResultFactoryFake;
        private IDisplayConsentAction _displayConsentAction;

        [Fact]
        public async Task When_Parameter_Is_Null_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter();

                        await Assert.ThrowsAsync<ArgumentNullException>(() => _displayConsentAction.Execute(
                null,
                null, null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _displayConsentAction.Execute(
                authorizationParameter,
                null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_A_Consent_Has_Been_Given_Then_Redirect_To_Callback()
        {            InitializeFakeObjects();
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var actionResult = new EndpointResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            var authorizationParameter = new AuthorizationParameter
            {
                ResponseMode = ResponseMode.fragment
            };
            var consent = new Consent();
            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult(new Client()));
            _consentHelperFake.Setup(c => c.GetConfirmedConsentsAsync(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult(consent));
            _actionResultFactoryFake.Setup(a => a.CreateAnEmptyActionResultWithRedirectionToCallBackUrl())
                .Returns(actionResult);

                        var result = await _displayConsentAction.Execute(authorizationParameter, claimsPrincipal, null).ConfigureAwait(false);

                        _actionResultFactoryFake.Verify(a => a.CreateAnEmptyActionResultWithRedirectionToCallBackUrl());
            Assert.True(result.EndpointResult.RedirectInstruction.ResponseMode == ResponseMode.fragment);
        }

        [Fact]
        public async Task When_A_Consent_Has_Been_Given_And_The_AuthorizationFlow_Is_Not_Supported_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            const string clientId = "clientId";
            const string state = "state";
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var responseTypes = new List<ResponseType>();
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                State = state,
                ResponseMode = ResponseMode.None // No response mode is defined
            };
            var consent = new Consent();
            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult(new Client()));
            _consentHelperFake.Setup(c => c.GetConfirmedConsentsAsync(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult(consent));
            _parameterParserHelperFake.Setup(p => p.ParseResponseTypes(It.IsAny<string>()))
                .Returns(responseTypes);

                        var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(() => _displayConsentAction.Execute(authorizationParameter,
                claimsPrincipal, null)).ConfigureAwait(false);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == ErrorDescriptions.TheAuthorizationFlowIsNotSupported);
            Assert.True(exception.State == state);

        }

        [Fact]
        public async Task When_No_Consent_Has_Been_Given_And_Client_Doesnt_Exist_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            const string clientId = "clientId";
            const string state = "state";
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                State = state
            };
            _consentHelperFake.Setup(c => c.GetConfirmedConsentsAsync(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult((Consent)null));
            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>())).
                Returns(Task.FromResult((Client)null));

                        var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(() => _displayConsentAction.Execute(authorizationParameter,
                claimsPrincipal, null)).ConfigureAwait(false);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.ClientIsNotValid, clientId));
            Assert.True(exception.State == state);
        }

        [Fact]
        public async Task When_No_Consent_Has_Been_Given_Then_Redirect_To_Consent_Screen()
        {            InitializeFakeObjects();
            const string clientId = "clientId";
            const string state = "state";
            const string scopeName = "profile";
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var client = new Client();
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                State = state,
                Claims = null,
                Scope = scopeName
            };
            ICollection<Scope> scopes = new List<Scope> { new Scope
            {
                IsDisplayedInConsent = true,
                Name = scopeName
            } };
            _consentHelperFake.Setup(c => c.GetConfirmedConsentsAsync(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult((Consent)null));
            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>())).
                Returns(Task.FromResult(client));
            _scopeRepositoryFake.Setup(s => s.SearchByNames(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(scopes));

                        await _displayConsentAction.Execute(authorizationParameter,
                claimsPrincipal, null).ConfigureAwait(false);

                        Assert.Contains(scopes, s => s.Name == scopeName);
            _actionResultFactoryFake.Verify(a => a.CreateAnEmptyActionResultWithOutput());
        }

        private void InitializeFakeObjects()
        {
            _scopeRepositoryFake = new Mock<IScopeRepository>();
            _clientRepositoryFake = new Mock<IClientStore>();
            _consentHelperFake = new Mock<IConsentHelper>();
            _generateAuthorizationResponseFake = new Mock<IGenerateAuthorizationResponse>();
            _parameterParserHelperFake = new Mock<IParameterParserHelper>();
            _actionResultFactoryFake = new Mock<IActionResultFactory>();
            _displayConsentAction = new DisplayConsentAction(
                _scopeRepositoryFake.Object,
                _clientRepositoryFake.Object,
                _consentHelperFake.Object,
                _generateAuthorizationResponseFake.Object,
                _parameterParserHelperFake.Object,
                _actionResultFactoryFake.Object);
        }
    }
}
