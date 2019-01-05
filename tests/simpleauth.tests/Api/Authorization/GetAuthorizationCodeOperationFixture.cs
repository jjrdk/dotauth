namespace SimpleAuth.Tests.Api.Authorization
{
    using Errors;
    using Exceptions;
    using Logging;
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Models;
    using SimpleAuth.Api.Authorization.Actions;
    using SimpleAuth.Api.Authorization.Common;
    using SimpleAuth.Common;
    using SimpleAuth.Helpers;
    using SimpleAuth.JwtToken;
    using System;
    using System.Threading.Tasks;
    using Shared.Repositories;
    using Xunit;
    using Client = Shared.Models.Client;

    public sealed class GetAuthorizationCodeOperationFixture
    {
        private const string HttpsLocalhost = "https://localhost";
        private Mock<IGenerateAuthorizationResponse> _generateAuthorizationResponseFake;
        private Mock<IOAuthEventSource> _oauthEventSource;
        private GetAuthorizationCodeOperation _getAuthorizationCodeOperation;

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _getAuthorizationCodeOperation.Execute(null, null, null, null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getAuthorizationCodeOperation.Execute(new AuthorizationParameter(), null, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_The_Client_Grant_Type_Is_Not_Supported_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            const string clientId = "clientId";
            const string scope = "scope";
            var authorizationParameter = new AuthorizationParameter
            {
                ResponseType = ResponseTypeNames.Code,
                RedirectUrl = new Uri(HttpsLocalhost),
                ClientId = clientId,
                Scope = scope,
                Claims = null
            };
            //_clientValidatorFake.Setup(c => c.CheckGrantTypes(It.IsAny<Client>(), It.IsAny<GrantType[]>()))
            //    .Returns(false);
            var client = new Client
            {
                GrantTypes = new[] { GrantType.client_credentials },
                AllowedScopes = new[] { new Scope { Name = scope } },
                RedirectionUrls = new[] { new Uri(HttpsLocalhost), }
            };
            var exception = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(() =>
                    _getAuthorizationCodeOperation.Execute(authorizationParameter, null, client, null))
                .ConfigureAwait(false);

            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.Equal(string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType,
                    clientId,
                    "authorization_code"),
                exception.Message);
        }

        [Fact(Skip = "Invalid test")]
        public async Task When_Redirected_To_Callback_And_Resource_Owner_Is_Not_Authenticated_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter
            {
                Prompt = PromptNames.None,
                ResponseType = ResponseTypeNames.Code,
                Scope = "scope",
                RedirectUrl = new Uri(HttpsLocalhost),
                State = "state"
            };

            //var actionResult = new EndpointResult
            //{
            //    Type = TypeActionResult.RedirectToCallBackUrl
            //};

            //_processAuthorizationRequestFake.Setup(p => p.ProcessAsync(It.IsAny<AuthorizationParameter>(),
            //    It.IsAny<ClaimsPrincipal>(), It.IsAny<Client>(), null))
            //    .Returns(Task.FromResult(actionResult));
            //_clientValidatorFake.Setup(c => c.CheckGrantTypes(It.IsAny<Client>(), It.IsAny<GrantType[]>()))
            //    .Returns(true);
            var client = new Client
            {
                ResponseTypes = new[] { ResponseTypeNames.Code },
                AllowedScopes = new[] { new Scope { Name = "scope" } },
                RedirectionUrls = new[] { new Uri(HttpsLocalhost), }
            };
            var ex = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _getAuthorizationCodeOperation.Execute(authorizationParameter, null, client, null))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestCode, ex.Code);
            Assert.Equal(ErrorDescriptions.TheResponseCannotBeGeneratedBecauseResourceOwnerNeedsToBeAuthenticated, ex.Message);
            Assert.Equal(authorizationParameter.State, ex.State);
        }

        [Fact]
        public async Task When_Passing_Valid_Request_Then_Events_Are_Logged()
        {
            InitializeFakeObjects();
            const string clientId = "clientId";
            const string scope = "scope";
            //var actionResult = new EndpointResult
            //{
            //    Type = TypeActionResult.RedirectToAction,
            //    RedirectInstruction = new RedirectInstruction
            //    {
            //        Action = SimpleAuthEndPoints.FormIndex
            //    }
            //};
            var client = new Client
            {
                ResponseTypes = new[] { ResponseTypeNames.Code },
                AllowedScopes = new[] { new Scope { Name = scope } },
                RedirectionUrls = new[] { new Uri(HttpsLocalhost), }
            };
            var authorizationParameter = new AuthorizationParameter
            {
                ResponseType = ResponseTypeNames.Code,
                RedirectUrl = new Uri(HttpsLocalhost),
                ClientId = clientId,
                Scope = scope,
                Claims = null
            };
            //var jsonAuthorizationParameter = authorizationParameter.SerializeWithJavascript();
            //_processAuthorizationRequestFake.Setup(p => p.ProcessAsync(
            //    It.IsAny<AuthorizationParameter>(),
            //    It.IsAny<ClaimsPrincipal>(), It.IsAny<Client>(), null)).Returns(Task.FromResult(actionResult));
            //_clientValidatorFake.Setup(c => c.CheckGrantTypes(It.IsAny<Client>(), It.IsAny<GrantType[]>()))
            //    .Returns(true);

            var result = await _getAuthorizationCodeOperation.Execute(authorizationParameter, null, client, null).ConfigureAwait(false);

            _oauthEventSource.Verify(s => s.StartAuthorizationCodeFlow(clientId, scope, string.Empty));
            _oauthEventSource.Verify(s => s.EndAuthorizationCodeFlow(clientId, "RedirectToAction", "AuthenticateIndex"));
        }

        private void InitializeFakeObjects()
        {
            _generateAuthorizationResponseFake = new Mock<IGenerateAuthorizationResponse>();
            _oauthEventSource = new Mock<IOAuthEventSource>();
            _getAuthorizationCodeOperation = new GetAuthorizationCodeOperation(
                new ProcessAuthorizationRequest(
                    new Mock<IClientStore>().Object,
                    new Mock<IConsentHelper>().Object,
                    _oauthEventSource.Object),
                _generateAuthorizationResponseFake.Object,
                _oauthEventSource.Object);
        }
    }
}
