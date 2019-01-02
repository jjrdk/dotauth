namespace SimpleAuth.Tests.Api.Authorization
{
    using Errors;
    using Exceptions;
    using Logging;
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Models;
    using SimpleAuth;
    using SimpleAuth.Api.Authorization.Actions;
    using SimpleAuth.Api.Authorization.Common;
    using SimpleAuth.Common;
    using SimpleAuth.Helpers;
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Xunit;
    using Client = Shared.Models.Client;

    public class GetTokenViaImplicitWorkflowOperationFixture
    {
        //private Mock<IProcessAuthorizationRequest> _processAuthorizationRequestFake;
        private Mock<IGenerateAuthorizationResponse> _generateAuthorizationResponseFake;
        private Mock<IOAuthEventSource> _oauthEventSource;
        private GetTokenViaImplicitWorkflowOperation _getTokenViaImplicitWorkflowOperation;

        [Fact]
        public async Task When_Passing_No_Authorization_Request_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _getTokenViaImplicitWorkflowOperation.Execute(null, null, null, null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getTokenViaImplicitWorkflowOperation.Execute(new AuthorizationParameter(), null, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_No_Nonce_Parameter_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter
            {
                State = "state"
            };

            var exception = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(() => _getTokenViaImplicitWorkflowOperation.Execute(authorizationParameter, null, new Client(), null)).ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.MissingParameter, CoreConstants.StandardAuthorizationRequestParameterNames.NonceName));
            Assert.True(exception.State == authorizationParameter.State);
        }

        [Fact]
        public async Task When_Implicit_Flow_Is_Not_Supported_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter
            {
                Nonce = "nonce",
                State = "state"
            };

            //_clientValidatorFake.Setup(c => c.CheckGrantTypes(It.IsAny<Client>(), It.IsAny<GrantType[]>()))
            //    .Returns(false);

            var ex = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(() => _getTokenViaImplicitWorkflowOperation.Execute(authorizationParameter, null, new Client(), null)).ConfigureAwait(false);
            Assert.True(ex.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(ex.Message == string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType,
                        authorizationParameter.ClientId,
                        "implicit"));
            Assert.True(ex.State == authorizationParameter.State);
        }

        [Fact]
        public async Task When_Requesting_Authorization_With_Valid_Request_Then_Events_Are_Logged()
        {
            InitializeFakeObjects();

            const string clientId = "clientId";
            const string scope = "openid";
            var authorizationParameter = new AuthorizationParameter
            {
                ResponseType = ResponseTypeNames.Token,
                State = "state",
                Nonce = "nonce",
                ClientId = clientId,
                Scope = scope,
                Claims = null,
                RedirectUrl = new Uri("https://localhost")
            };
            //var actionResult = new EndpointResult()
            //{
            //    Type = TypeActionResult.RedirectToAction,
            //    RedirectInstruction = new RedirectInstruction
            //    {
            //        Action = SimpleAuthEndPoints.ConsentIndex
            //    }
            //};
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));
            //_processAuthorizationRequestFake.Setup(p => p.ProcessAsync(It.IsAny<AuthorizationParameter>(),
            //        It.IsAny<ClaimsPrincipal>(),
            //        It.IsAny<Client>(),
            //        null))
            //    .Returns(Task.FromResult(actionResult));
            //_parameterParser.Setup(x => x.ParseScopes(It.IsAny<string>())).Returns(new List<string> { "openid" });
            //_parameterParser.Setup(x => x.ParseResponseTypes(It.IsAny<string>())).Returns(new[] { ResponseType.code });
            //_clientValidatorFake.Setup(c => c.CheckGrantTypes(It.IsAny<Client>(), It.IsAny<GrantType[]>()))
            //    .Returns(true);
            //_clientValidatorFake.Setup(x => x.GetRedirectionUrls(It.IsAny<Client>(), It.IsAny<Uri[]>()))
            //    .Returns(new[] { new Uri("https://localhost") });
            //_clientValidatorFake.Setup(x => x.CheckResponseTypes(It.IsAny<Client>(), It.IsAny<ResponseType[]>()))
            //    .Returns(true);
            var client = new Client
            {
                ResponseTypes = ResponseTypeNames.All,
                RedirectionUrls = new[] { new Uri("https://localhost"), },
                GrantTypes = new[] { GrantType.@implicit },
                AllowedScopes = new[] { new Scope { Name = "openid" } }
            };
            await _getTokenViaImplicitWorkflowOperation.Execute(authorizationParameter, claimsPrincipal, client, null).ConfigureAwait(false);

            _oauthEventSource.Verify(s => s.StartImplicitFlow(clientId, scope, string.Empty));
            _oauthEventSource.Verify(s => s.EndImplicitFlow(clientId, "RedirectToAction", "ConsentIndex"));
        }

        private void InitializeFakeObjects()
        {
            _generateAuthorizationResponseFake = new Mock<IGenerateAuthorizationResponse>();
            _oauthEventSource = new Mock<IOAuthEventSource>();
            _getTokenViaImplicitWorkflowOperation = new GetTokenViaImplicitWorkflowOperation(
                new ProcessAuthorizationRequest(
                    new Mock<IConsentHelper>().Object,
                    null,
                    _oauthEventSource.Object),
                _generateAuthorizationResponseFake.Object,
                _oauthEventSource.Object);
        }
    }
}
