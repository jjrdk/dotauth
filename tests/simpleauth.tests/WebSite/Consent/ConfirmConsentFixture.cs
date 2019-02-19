namespace SimpleAuth.Tests.WebSite.Consent
{
    using Exceptions;
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.MiddleWare;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.WebSite.Consent.Actions;
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Errors;
    using Xunit;

    public sealed class ConfirmConsentFixture
    {
        private readonly Mock<IConsentRepository> _consentRepositoryFake;
        private readonly Mock<IClientStore> _clientRepositoryFake;
        private readonly Mock<IScopeRepository> _scopeRepositoryFake;
        private readonly Mock<IResourceOwnerRepository> _resourceOwnerRepositoryFake;
        private readonly ConfirmConsentAction _confirmConsentAction;

        public ConfirmConsentFixture()
        {
            _consentRepositoryFake = new Mock<IConsentRepository>();
            _clientRepositoryFake = new Mock<IClientStore>();
            _scopeRepositoryFake = new Mock<IScopeRepository>();
            _resourceOwnerRepositoryFake = new Mock<IResourceOwnerRepository>();
            _confirmConsentAction = new ConfirmConsentAction(
                new Mock<IAuthorizationCodeStore>().Object,
                new Mock<ITokenStore>().Object,
                _consentRepositoryFake.Object,
                _clientRepositoryFake.Object,
                _scopeRepositoryFake.Object,
                _resourceOwnerRepositoryFake.Object,
                new NoOpPublisher());
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            var authorizationParameter = new AuthorizationParameter();

            await Assert
                .ThrowsAsync<ArgumentNullException>(
                    () => _confirmConsentAction.Execute(null, null, null, CancellationToken.None))
                .ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _confirmConsentAction.Execute(authorizationParameter, null, null, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_No_Consent_Has_Been_Given_And_ResponseMode_Is_No_Correct_Then_Exception_Is_Thrown()
        {
            const string subject = "subject";
            const string state = "state";
            var authorizationParameter = new AuthorizationParameter
            {
                Claims = null, Scope = "profile", ResponseMode = ResponseModes.None, State = state
            };
            var claims = new List<Claim> {new Claim(JwtConstants.OpenIdClaimTypes.Subject, subject)};
            var claimsIdentity = new ClaimsIdentity(claims, "SimpleAuthServer");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var client = new Client {ClientId = "clientId"};
            var resourceOwner = new ResourceOwner {Id = subject};

            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _clientRepositoryFake.Setup(x => x.GetAll(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<Client>());
            _resourceOwnerRepositoryFake.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resourceOwner);
            _scopeRepositoryFake.Setup(s => s.SearchByNames(It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
                .ReturnsAsync(Array.Empty<Scope>());
            var exception = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _confirmConsentAction.Execute(
                        authorizationParameter,
                        claimsPrincipal,
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);

            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.Equal(ErrorDescriptions.TheAuthorizationFlowIsNotSupported, exception.Message);
            Assert.Equal(state, exception.State);
        }

        [Fact]
        public async Task When_No_Consent_Has_Been_Given_For_The_Claims_Then_Create_And_Insert_A_New_One()
        {
            const string subject = "subject";
            const string clientId = "clientId";
            var authorizationParameter = new AuthorizationParameter
            {
                ResponseType = "code",
                Claims = new ClaimsParameter
                {
                    UserInfo = new List<ClaimParameter>
                    {
                        new ClaimParameter {Name = JwtConstants.OpenIdClaimTypes.Subject}
                    }
                },
                Scope = "profile"
            };
            var claims = new List<Claim> {new Claim(JwtConstants.OpenIdClaimTypes.Subject, subject)};
            var claimsIdentity = new ClaimsIdentity(claims, "SimpleAuthServer");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var client = new Client {ClientId = clientId};
            var resourceOwner = new ResourceOwner {Id = subject};

            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _clientRepositoryFake.Setup(x => x.GetAll(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<Client>());
            _scopeRepositoryFake.Setup(s => s.SearchByNames(It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
                .ReturnsAsync(Array.Empty<Scope>());
            _resourceOwnerRepositoryFake.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resourceOwner);
            Consent insertedConsent = null;
            _consentRepositoryFake.Setup(co => co.Insert(It.IsAny<Consent>(), It.IsAny<CancellationToken>()))
                .Callback<Consent, CancellationToken>((consent, token) => insertedConsent = consent)
                .ReturnsAsync(true);

            await _confirmConsentAction.Execute(authorizationParameter, claimsPrincipal, null, CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Contains(JwtConstants.OpenIdClaimTypes.Subject, insertedConsent.Claims);
            Assert.Equal(subject, insertedConsent.ResourceOwner.Id);
            Assert.Equal(clientId, insertedConsent.Client.ClientId);
        }

        [Fact]
        public async Task When_No_Consent_Has_Been_Given_Then_Create_And_Insert_A_New_One()
        {
            const string subject = "subject";
            var authorizationParameter = new AuthorizationParameter
            {
                ResponseType = "code", Claims = null, Scope = "profile", ResponseMode = ResponseModes.None
            };
            var claims = new List<Claim> {new Claim(JwtConstants.OpenIdClaimTypes.Subject, subject)};
            var claimsIdentity = new ClaimsIdentity(claims, "SimpleAuthServer");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var client = new Client {ClientId = "clientId"};
            var resourceOwner = new ResourceOwner {Id = subject};
            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _clientRepositoryFake.Setup(x => x.GetAll(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<Client>());
            _scopeRepositoryFake.Setup(s => s.SearchByNames(It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
                .ReturnsAsync(Array.Empty<Scope>());
            _resourceOwnerRepositoryFake.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resourceOwner);

            var result = await _confirmConsentAction
                .Execute(authorizationParameter, claimsPrincipal, null, CancellationToken.None)
                .ConfigureAwait(false);

            _consentRepositoryFake.Verify(c => c.Insert(It.IsAny<Consent>(), It.IsAny<CancellationToken>()));
            Assert.Equal(ResponseModes.Query, result.RedirectInstruction.ResponseMode);
        }
    }
}
