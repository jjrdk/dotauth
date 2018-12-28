namespace SimpleAuth.Tests.WebSite.Consent
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Errors;
    using Exceptions;
    using Factories;
    using Logging;
    using Moq;
    using Parameters;
    using Results;
    using Services;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth;
    using SimpleAuth.Common;
    using SimpleAuth.Helpers;
    using SimpleAuth.WebSite.Consent.Actions;
    using Xunit;

    public sealed class ConfirmConsentFixture
    {
        private Mock<IConsentRepository> _consentRepositoryFake;
        private Mock<IClientStore> _clientRepositoryFake;
        private Mock<IScopeRepository> _scopeRepositoryFake;
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryFake;
        private Mock<IParameterParserHelper> _parameterParserHelperFake;
        private Mock<IActionResultFactory> _actionResultFactoryFake;
        private Mock<IGenerateAuthorizationResponse> _generateAuthorizationResponseFake;
        private Mock<IConsentHelper> _consentHelperFake;
        private Mock<IOpenIdEventSource> _openIdEventSource;
        private Mock<IAuthenticateResourceOwnerService> _authenticateResourceOwnerServiceStub;

        private IConfirmConsentAction _confirmConsentAction;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter();

                        await Assert.ThrowsAsync<ArgumentNullException>(() => _confirmConsentAction.Execute(null, null, null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _confirmConsentAction.Execute(authorizationParameter, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_No_Consent_Has_Been_Given_And_ResponseMode_Is_No_Correct_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
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
            var claimsIdentity = new ClaimsIdentity(claims, "SimpleIdentityServer");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var client = new Client
            {
                ClientId = "clientId"
            };
            var resourceOwner = new ResourceOwner
            {
                Id = subject
            };
            var actionResult = new EndpointResult
            {
                Type = TypeActionResult.RedirectToCallBackUrl,
                RedirectInstruction = new RedirectInstruction()
            };
            ICollection<string> scopeNames = new List<string>();
            ICollection<Scope> scopes = new List<Scope>();
            _consentHelperFake.Setup(c => c.GetConfirmedConsentsAsync(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult((Consent)null));
            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _parameterParserHelperFake.Setup(p => p.ParseScopes(It.IsAny<string>()))
                .Returns(scopeNames);
            _resourceOwnerRepositoryFake.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(resourceOwner));
            _actionResultFactoryFake.Setup(a => a.CreateAnEmptyActionResultWithRedirectionToCallBackUrl())
                .Returns(actionResult);
            _scopeRepositoryFake.Setup(s => s.SearchByNames(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(scopes));
            _parameterParserHelperFake.Setup(p => p.ParseResponseTypes(It.IsAny<string>()))
                .Returns(new List<ResponseType> { ResponseType.id_token, ResponseType.id_token });

                        var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(() => _confirmConsentAction.Execute(authorizationParameter, claimsPrincipal, null)).ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == ErrorDescriptions.TheAuthorizationFlowIsNotSupported);
            Assert.True(exception.State == state);
        }

        [Fact]
        public async Task When_No_Consent_Has_Been_Given_For_The_Claims_Then_Create_And_Insert_A_New_One()
        {            InitializeFakeObjects();
            const string subject = "subject";
            const string clientId = "clientId";
            var authorizationParameter = new AuthorizationParameter
            {
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
            var claimsIdentity = new ClaimsIdentity(claims, "SimpleIdentityServer");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var client = new Client
            {
                ClientId = clientId
            };
            var resourceOwner = new ResourceOwner
            {
                Id = subject
            };
            var actionResult = new EndpointResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            ICollection<Scope> scopes = new List<Scope>();
            _consentHelperFake.Setup(c => c.GetConfirmedConsentsAsync(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult((Consent)null));
            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _parameterParserHelperFake.Setup(p => p.ParseScopes(It.IsAny<string>()))
                .Returns(new List<string>());
            _scopeRepositoryFake.Setup(s => s.SearchByNames(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(scopes));
            _resourceOwnerRepositoryFake.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(resourceOwner));
            _actionResultFactoryFake.Setup(a => a.CreateAnEmptyActionResultWithRedirectionToCallBackUrl())
                .Returns(actionResult);
            Consent insertedConsent = null;
            _consentRepositoryFake.Setup(co => co.InsertAsync(It.IsAny<Consent>()))
                .Callback<Consent>(consent => insertedConsent = consent)
                .Returns(Task.FromResult(true));

                        await _confirmConsentAction.Execute(authorizationParameter, claimsPrincipal, null).ConfigureAwait(false);

                        Assert.NotNull(insertedConsent);
            Assert.Contains(JwtConstants.StandardResourceOwnerClaimNames.Subject, insertedConsent.Claims);
            Assert.True(insertedConsent.ResourceOwner.Id == subject);
            Assert.True(insertedConsent.Client.ClientId == clientId);
            _actionResultFactoryFake.Verify(a => a.CreateAnEmptyActionResultWithRedirectionToCallBackUrl());
        }

        [Fact]
        public async Task When_No_Consent_Has_Been_Given_Then_Create_And_Insert_A_New_One()
        {            InitializeFakeObjects();
            const string subject = "subject";
            var authorizationParameter = new AuthorizationParameter
            {
                Claims = null,
                Scope = "profile",
                ResponseMode = ResponseMode.None
            };
            var claims = new List<Claim>
            {
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimsIdentity = new ClaimsIdentity(claims, "SimpleIdentityServer");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var client = new Client
            {
                ClientId = "clientId"
            };
            var resourceOwner = new ResourceOwner
            {
                Id = subject
            };
            var actionResult = new EndpointResult
            {
                Type = TypeActionResult.RedirectToCallBackUrl,
                RedirectInstruction = new RedirectInstruction
                {
                    
                }
            };
            ICollection<Scope> scopes = new List<Scope>();
            _consentHelperFake.Setup(c => c.GetConfirmedConsentsAsync(It.IsAny<string>(),
                It.IsAny<AuthorizationParameter>()))
                .Returns(Task.FromResult((Consent)null));
            _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _parameterParserHelperFake.Setup(p => p.ParseScopes(It.IsAny<string>()))
                .Returns(new List<string>());
            _scopeRepositoryFake.Setup(s => s.SearchByNames(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(scopes));
            _resourceOwnerRepositoryFake.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(resourceOwner));
            _actionResultFactoryFake.Setup(a => a.CreateAnEmptyActionResultWithRedirectionToCallBackUrl())
                .Returns(actionResult);
            _parameterParserHelperFake.Setup(p => p.ParseResponseTypes(It.IsAny<string>()))
                .Returns(new List<ResponseType> {  ResponseType.code });

                        var result = await _confirmConsentAction.Execute(authorizationParameter, claimsPrincipal, null).ConfigureAwait(false);

                        _consentRepositoryFake.Verify(c => c.InsertAsync(It.IsAny<Consent>()));
            _actionResultFactoryFake.Verify(a => a.CreateAnEmptyActionResultWithRedirectionToCallBackUrl());
            Assert.True(result.RedirectInstruction.ResponseMode == ResponseMode.query);
        }

        private void InitializeFakeObjects()
        {
            _consentRepositoryFake = new Mock<IConsentRepository>();
            _clientRepositoryFake = new Mock<IClientStore>();
            _scopeRepositoryFake = new Mock<IScopeRepository>();
            _resourceOwnerRepositoryFake = new Mock<IResourceOwnerRepository>();
            _parameterParserHelperFake = new Mock<IParameterParserHelper>();
            _actionResultFactoryFake = new Mock<IActionResultFactory>();
            _generateAuthorizationResponseFake = new Mock<IGenerateAuthorizationResponse>();
            _consentHelperFake = new Mock<IConsentHelper>();
            _openIdEventSource = new Mock<IOpenIdEventSource>();
            _authenticateResourceOwnerServiceStub = new Mock<IAuthenticateResourceOwnerService>();
            _confirmConsentAction = new ConfirmConsentAction(
                _consentRepositoryFake.Object,
                _clientRepositoryFake.Object,
                _scopeRepositoryFake.Object,
                _resourceOwnerRepositoryFake.Object,
                _parameterParserHelperFake.Object,
                _actionResultFactoryFake.Object,
                _generateAuthorizationResponseFake.Object,
                _consentHelperFake.Object,
                _openIdEventSource.Object);
        }
    }
}
