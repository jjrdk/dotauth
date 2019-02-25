using SimpleAuth.Shared;

namespace SimpleAuth.Tests.WebSite.Consent
{
    using Exceptions;
    using Microsoft.IdentityModel.Tokens;
    using Moq;
    using Parameters;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.WebSite.Consent.Actions;
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Errors;
    using Xunit;

    public sealed class DisplayConsentActionFixture
    {
        private readonly Mock<IScopeRepository> _scopeRepositoryFake;
        private readonly Mock<IClientStore> _clientRepositoryFake;
        private readonly DisplayConsentAction _displayConsentAction;
        private readonly Mock<IConsentRepository> _consentRepository;

        public DisplayConsentActionFixture()
        {
            _scopeRepositoryFake = new Mock<IScopeRepository>();
            _clientRepositoryFake = new Mock<IClientStore>();
            _consentRepository = new Mock<IConsentRepository>();
            _displayConsentAction = new DisplayConsentAction(
                _scopeRepositoryFake.Object,
                _clientRepositoryFake.Object,
                _consentRepository.Object,
                new Mock<IAuthorizationCodeStore>().Object,
                new Mock<ITokenStore>().Object,
                new InMemoryJwksRepository(),
                new Mock<IEventPublisher>().Object);
        }

        [Fact]
        public async Task When_Parameter_Is_Null_Then_Exception_Is_Thrown()
        {
            var authorizationParameter = new AuthorizationParameter();

            await Assert
                .ThrowsAsync<ArgumentNullException>(
                    () => _displayConsentAction.Execute(null, null, null, CancellationToken.None))
                .ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _displayConsentAction.Execute(authorizationParameter, null, null, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_A_Consent_Has_Been_Given_Then_Redirect_To_Callback()
        {
            var scope = "scope";
            var clientid = "client";
            var client = new Client
            {
                JsonWebKeys =
                    "verylongkeyfortesting".CreateJwk(
                            JsonWebKeyUseNames.Sig,
                            KeyOperations.Sign,
                            KeyOperations.Verify)
                        .ToSet(),
                IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                ClientId = clientid,
                AllowedScopes = new[] {scope}
            };
            var consent = new Consent {Client = client, GrantedScopes = new[] {scope}};
            _consentRepository.Setup(x => x.GetConsentsForGivenUser(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Consent> {consent});
            _scopeRepositoryFake.Setup(x => x.SearchByNames(It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
                .ReturnsAsync(new[] {new Scope {Name = scope, IsDisplayedInConsent = true}});
            var claimsIdentity = new ClaimsIdentity(new[] {new Claim("sub", "test"),}, "test");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientid, Scope = scope, ResponseMode = ResponseModes.Fragment
            };

            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _clientRepositoryFake.Setup(x => x.GetAll(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<Client>());
            var result = await _displayConsentAction
                .Execute(authorizationParameter, claimsPrincipal, null, CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Equal(ResponseModes.Fragment, result.EndpointResult.RedirectInstruction.ResponseMode);
        }

        [Fact]
        public async Task
            When_A_Consent_Has_Been_Given_And_The_AuthorizationFlow_Is_Not_Supported_Then_Exception_Is_Thrown()
        {
            const string clientId = "clientId";
            const string state = "state";
            var claimsIdentity = new ClaimsIdentity(new[] {new Claim("sub", "test")}, "test");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var authorizationParameter = new AuthorizationParameter
            {
                ResponseType = ResponseTypeNames.Token,
                Scope = "scope",
                ClientId = clientId,
                State = state,
                ResponseMode = ResponseModes.None // No response mode is defined
            };
            var consent = new Consent
            {
                GrantedScopes = new[] {"scope"},
                Client = new Client {ClientId = clientId, AllowedScopes = new[] {"scope"}}
            };
            var returnedClient = new Client
            {
                ClientId = clientId,
                JsonWebKeys = "verylongkeyfortesting".CreateJwk(
                        JsonWebKeyUseNames.Sig,
                        KeyOperations.Sign,
                        KeyOperations.Verify)
                    .ToSet(),
                IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256
            };
            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(returnedClient);
            _clientRepositoryFake.Setup(x => x.GetAll(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<Client>());
            _consentRepository.Setup(x => x.GetConsentsForGivenUser(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] {consent});
            var exception = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _displayConsentAction.Execute(
                        authorizationParameter,
                        claimsPrincipal,
                        "issuer",
                        CancellationToken.None))
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
            var authorizationParameter = new AuthorizationParameter {ClientId = clientId, State = state};

            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client) null);

            var exception = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _displayConsentAction.Execute(
                        authorizationParameter,
                        claimsPrincipal,
                        null,
                        CancellationToken.None))
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
                ClientId = clientId, State = state, Claims = null, Scope = scopeName
            };
            var scopes = new[] {new Scope {IsDisplayedInConsent = true, Name = scopeName}};

            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _scopeRepositoryFake.Setup(s => s.SearchByNames(It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
                .ReturnsAsync(scopes);

            await _displayConsentAction.Execute(authorizationParameter, claimsPrincipal, null, CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Contains(scopes, s => s.Name == scopeName);
        }
    }
}
