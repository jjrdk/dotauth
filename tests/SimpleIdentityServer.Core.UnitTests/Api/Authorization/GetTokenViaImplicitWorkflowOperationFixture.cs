namespace SimpleAuth.Tests.Api.Authorization
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Errors;
    using Exceptions;
    using Logging;
    using Moq;
    using Parameters;
    using Results;
    using Shared.Models;
    using SimpleAuth;
    using SimpleAuth.Api.Authorization.Actions;
    using SimpleAuth.Api.Authorization.Common;
    using SimpleAuth.Common;
    using SimpleAuth.Validators;
    using Xunit;
    using Client = Shared.Models.Client;

    public class GetTokenViaImplicitWorkflowOperationFixture
    {
        private Mock<IProcessAuthorizationRequest> _processAuthorizationRequestFake;
        private Mock<IGenerateAuthorizationResponse> _generateAuthorizationResponseFake;
        private Mock<IClientValidator> _clientValidatorFake;
        private Mock<IOAuthEventSource> _oauthEventSource;
        private IGetTokenViaImplicitWorkflowOperation _getTokenViaImplicitWorkflowOperation;

        [Fact]
        public async Task When_Passing_No_Authorization_Request_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            
                        await Assert.ThrowsAsync<ArgumentNullException>(() => _getTokenViaImplicitWorkflowOperation.Execute(null, null, null, null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getTokenViaImplicitWorkflowOperation.Execute(new AuthorizationParameter(), null, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_No_Nonce_Parameter_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter
            {
                State = "state"
            };

                        var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(() => _getTokenViaImplicitWorkflowOperation.Execute(authorizationParameter, null, new Client(), null)).ConfigureAwait(false);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.MissingParameter, CoreConstants.StandardAuthorizationRequestParameterNames.NonceName));
            Assert.True(exception.State == authorizationParameter.State);
        }

        [Fact]
        public async Task When_Implicit_Flow_Is_Not_Supported_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter
            {
                Nonce = "nonce",
                State = "state"
            };

            _clientValidatorFake.Setup(c => c.CheckGrantTypes(It.IsAny<Client>(), It.IsAny<GrantType[]>()))
                .Returns(false);

                        var ex = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(() => _getTokenViaImplicitWorkflowOperation.Execute(authorizationParameter, null, new Client(), null)).ConfigureAwait(false);
            Assert.True(ex.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(ex.Message == string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType,
                        authorizationParameter.ClientId,
                        "implicit"));
            Assert.True(ex.State == authorizationParameter.State);
        }

        [Fact]
        public async Task When_Requesting_Authorization_With_Valid_Request_Then_Events_Are_Logged()
        {            InitializeFakeObjects();
            const string clientId = "clientId";
            const string scope = "openid";
            var authorizationParameter = new AuthorizationParameter
            {
                State = "state",
                Nonce =  "nonce",
                ClientId =  clientId,
                Scope = scope,
                Claims = null
            };
            var actionResult = new EndpointResult()
            {
                Type = TypeActionResult.RedirectToAction,
                RedirectInstruction = new RedirectInstruction
                {
                    Action = IdentityServerEndPoints.ConsentIndex
                }
            };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));
            _processAuthorizationRequestFake.Setup(p => p.ProcessAsync(It.IsAny<AuthorizationParameter>(),
                It.IsAny<ClaimsPrincipal>(), It.IsAny<Client>(), null)).Returns(Task.FromResult(actionResult));
            _clientValidatorFake.Setup(c => c.CheckGrantTypes(It.IsAny<Client>(), It.IsAny<GrantType[]>()))
                .Returns(true);

                        await _getTokenViaImplicitWorkflowOperation.Execute(authorizationParameter, claimsPrincipal, new Client(), null).ConfigureAwait(false);

                        _oauthEventSource.Verify(s => s.StartImplicitFlow(clientId, scope, string.Empty));
            _oauthEventSource.Verify(s => s.EndImplicitFlow(clientId, "RedirectToAction", "ConsentIndex"));
        }

        private void InitializeFakeObjects()
        {
            _processAuthorizationRequestFake = new Mock<IProcessAuthorizationRequest>();
            _generateAuthorizationResponseFake = new Mock<IGenerateAuthorizationResponse>();
            _clientValidatorFake = new Mock<IClientValidator>();
            _oauthEventSource = new Mock<IOAuthEventSource>();
            _getTokenViaImplicitWorkflowOperation = new GetTokenViaImplicitWorkflowOperation(
                _processAuthorizationRequestFake.Object,
                _generateAuthorizationResponseFake.Object,
                _clientValidatorFake.Object,
                _oauthEventSource.Object);
        }
    }
}
