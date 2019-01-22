namespace SimpleAuth.Tests.WebSite.Consent
{
    using Errors;
    using Exceptions;
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.Common;
    using SimpleAuth.WebSite.Consent.Actions;
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class ConfirmConsentFixture
    {
        private Mock<IConsentRepository> _consentRepositoryFake;
        private Mock<IClientStore> _clientRepositoryFake;
        private Mock<IScopeRepository> _scopeRepositoryFake;
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryFake;
        private Mock<IGenerateAuthorizationResponse> _generateAuthorizationResponseFake;
        private IConfirmConsentAction _confirmConsentAction;

        public ConfirmConsentFixture()
        {
            InitializeFakeObjects();
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            var authorizationParameter = new AuthorizationParameter();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _confirmConsentAction.Execute(null, null, null))
                .ConfigureAwait(false);
            await Assert
                .ThrowsAsync<ArgumentNullException>(() =>
                    _confirmConsentAction.Execute(authorizationParameter, null, null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_No_Consent_Has_Been_Given_And_ResponseMode_Is_No_Correct_Then_Exception_Is_Thrown()
        {
            const string subject = "subject";
            const string state = "state";
            var authorizationParameter = new AuthorizationParameter
            {
                Claims = null,
                Scope = "profile",
                ResponseMode = ResponseMode.None,
                State = state
            };
            var claims = new List<Claim>
            {
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimsIdentity = new ClaimsIdentity(claims, "SimpleAuthServer");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var client = new Client
            {
                ClientId = "clientId"
            };
            var resourceOwner = new ResourceOwner
            {
                Id = subject
            };
            ICollection<Scope> scopes = new List<Scope>();
            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _resourceOwnerRepositoryFake.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(resourceOwner));
            _scopeRepositoryFake.Setup(s => s.SearchByNames(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(scopes));
            var exception = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(() =>
                    _confirmConsentAction.Execute(authorizationParameter, claimsPrincipal, null))
                .ConfigureAwait(false);
            Assert.NotNull(exception);
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
                        new ClaimParameter
                        {
                            Name = JwtConstants.StandardResourceOwnerClaimNames.Subject
                        }
                    }
                },
                Scope = "profile"
            };
            var claims = new List<Claim>
            {
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimsIdentity = new ClaimsIdentity(claims, "SimpleAuthServer");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var client = new Client
            {
                ClientId = clientId
            };
            var resourceOwner = new ResourceOwner
            {
                Id = subject
            };

            ICollection<Scope> scopes = new List<Scope>();
            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _scopeRepositoryFake.Setup(s => s.SearchByNames(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(scopes));
            _resourceOwnerRepositoryFake.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(resourceOwner));
            Consent insertedConsent = null;
            _consentRepositoryFake.Setup(co => co.Insert(It.IsAny<Consent>()))
                .Callback<Consent>(consent => insertedConsent = consent)
                .Returns(Task.FromResult(true));

            await _confirmConsentAction.Execute(authorizationParameter, claimsPrincipal, null).ConfigureAwait(false);

            Assert.Contains(JwtConstants.StandardResourceOwnerClaimNames.Subject, insertedConsent.Claims);
            Assert.Equal(subject, insertedConsent.ResourceOwner.Id);
            Assert.Equal(clientId, insertedConsent.Client.ClientId);
        }

        [Fact]
        public async Task When_No_Consent_Has_Been_Given_Then_Create_And_Insert_A_New_One()
        {
            const string subject = "subject";
            var authorizationParameter = new AuthorizationParameter
            {
                ResponseType = "code",
                Claims = null,
                Scope = "profile",
                ResponseMode = ResponseMode.None
            };
            var claims = new List<Claim>
            {
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimsIdentity = new ClaimsIdentity(claims, "SimpleAuthServer");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var client = new Client
            {
                ClientId = "clientId"
            };
            var resourceOwner = new ResourceOwner
            {
                Id = subject
            };
            ICollection<Scope> scopes = new List<Scope>();
            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _scopeRepositoryFake.Setup(s => s.SearchByNames(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(scopes));
            _resourceOwnerRepositoryFake.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(resourceOwner));

            var result = await _confirmConsentAction.Execute(authorizationParameter, claimsPrincipal, null)
                .ConfigureAwait(false);

            _consentRepositoryFake.Verify(c => c.Insert(It.IsAny<Consent>()));
            Assert.Equal(ResponseMode.query, result.RedirectInstruction.ResponseMode);
        }

        private void InitializeFakeObjects()
        {
            _consentRepositoryFake = new Mock<IConsentRepository>();
            _clientRepositoryFake = new Mock<IClientStore>();
            _scopeRepositoryFake = new Mock<IScopeRepository>();
            _resourceOwnerRepositoryFake = new Mock<IResourceOwnerRepository>();
            _generateAuthorizationResponseFake = new Mock<IGenerateAuthorizationResponse>();
            _confirmConsentAction = new ConfirmConsentAction(
                _consentRepositoryFake.Object,
                _clientRepositoryFake.Object,
                _scopeRepositoryFake.Object,
                _resourceOwnerRepositoryFake.Object,
                _generateAuthorizationResponseFake.Object,
                new Mock<IEventPublisher>().Object);
        }
    }
}
