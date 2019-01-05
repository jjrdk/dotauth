namespace SimpleAuth.Tests.Api.Authorization
{
    using Errors;
    using Exceptions;
    using Logging;
    using Moq;
    using Parameters;
    using Results;
    using Shared;
    using Shared.Models;
    using SimpleAuth;
    using SimpleAuth.Api.Authorization.Actions;
    using SimpleAuth.Api.Authorization.Common;
    using SimpleAuth.Common;
    using SimpleAuth.Helpers;
    using SimpleAuth.JwtToken;
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Shared.Repositories;
    using Xunit;
    //using Client = Shared.Models.Client;

    public sealed class GetAuthorizationCodeAndTokenViaHybridWorkflowOperationFixture
    {
        private Mock<IOAuthEventSource> _oauthEventSource;
        private Mock<IGenerateAuthorizationResponse> _generateAuthorizationResponseFake;

        private GetAuthorizationCodeAndTokenViaHybridWorkflowOperation
            _getAuthorizationCodeAndTokenViaHybridWorkflowOperation;

        private Mock<IConsentHelper> _consentHelper;

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                    _getAuthorizationCodeAndTokenViaHybridWorkflowOperation.Execute(null, null, null, null))
                .ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                    _getAuthorizationCodeAndTokenViaHybridWorkflowOperation.Execute(new AuthorizationParameter(),
                        null,
                        null,
                        null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Nonce_Parameter_Is_Not_Set_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter
            {
                State = "state"
            };

            var ex = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(() =>
                    _getAuthorizationCodeAndTokenViaHybridWorkflowOperation.Execute(authorizationParameter,
                        null,
                        new Client(),
                        null))
                .ConfigureAwait(false);
            Assert.True(ex.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(ex.Message ==
                        string.Format(ErrorDescriptions.MissingParameter,
                            CoreConstants.StandardAuthorizationRequestParameterNames.NonceName));
            Assert.True(ex.State == authorizationParameter.State);
        }

        [Fact]
        public async Task When_Grant_Type_Is_Not_Supported_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var redirectUrl = new Uri("https://localhost");
            var authorizationParameter = new AuthorizationParameter
            {
                RedirectUrl = redirectUrl,
                State = "state",
                Nonce = "nonce",
                Scope = "openid",
                ResponseType = ResponseTypeNames.Code,
            };

            //_clientValidatorFake
            //    .Setup(c => c.CheckGrantTypes(It.IsAny<Client>(), It.IsAny<GrantType[]>()))
            //    .Returns(false);

            var client = new Client
            {
                RedirectionUrls = new[] { redirectUrl },
                AllowedScopes = new[] { new Scope { Name = "openid" } },

            };
            var ex = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _getAuthorizationCodeAndTokenViaHybridWorkflowOperation.Execute(authorizationParameter,
                        null,
                        client,
                        null))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestCode, ex.Code);
            Assert.Equal(
                string.Format(
                    ErrorDescriptions.TheClientDoesntSupportTheGrantType,
                    authorizationParameter.ClientId,
                    "implicit and authorization_code"),
                ex.Message);
            Assert.Equal(authorizationParameter.State, ex.State);
        }

        [Fact(Skip = "Invalid test")]
        public async Task When_Redirected_To_Callback_And_Resource_Owner_Is_Not_Authenticated_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var redirectUrl = new Uri("https://localhost");
            var authorizationParameter = new AuthorizationParameter
            {
                Prompt = PromptNames.None,
                ClientId = "test",
                State = "state",
                Nonce = "nonce",
                RedirectUrl = redirectUrl,
                Scope = "openid",
                ResponseType = ResponseTypeNames.IdToken
            };

            //var actionResult = new EndpointResult
            //{
            //    Type = TypeActionResult.RedirectToCallBackUrl
            //};

            //_processAuthorizationRequestFake.Setup(p => p.ProcessAsync(It.IsAny<AuthorizationParameter>(),
            //        It.IsAny<ClaimsPrincipal>(),
            //        It.IsAny<Client>(),
            //        null))
            //    .Returns(Task.FromResult(actionResult));
            //_clientValidatorFake.Setup(c =>
            //        c.CheckGrantTypes(It.IsAny<Client>(), It.IsAny<GrantType[]>()))
            //    .Returns(true);

            var client = new Client
            {
                ClientId = "test",
                GrantTypes = new[] { GrantType.@implicit, GrantType.authorization_code },
                ResponseTypes = new[] { ResponseTypeNames.IdToken },
                AllowedScopes = new[] { new Scope { Name = "openid", IsDisplayedInConsent = true } },
                RedirectionUrls = new[] { redirectUrl }
            };
            var ex = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _getAuthorizationCodeAndTokenViaHybridWorkflowOperation.Execute(
                        authorizationParameter,
                      null, // new ClaimsPrincipal(new ClaimsIdentity(new Claim[0], "")),
                        client,
                        null))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestCode, ex.Code);
            Assert.Equal(ErrorDescriptions.TheResponseCannotBeGeneratedBecauseResourceOwnerNeedsToBeAuthenticated, ex.Message);
            Assert.Equal(authorizationParameter.State, ex.State);
        }

        [Fact]
        public async Task
            When_Resource_Owner_Is_Authenticated_And_Pass_Correct_Authorization_Request_Then_Events_Are_Logged()
        {
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter
            {
                Prompt = PromptNames.None,
                ResponseType = ResponseTypeNames.Code,
                RedirectUrl = new Uri("https://localhost"),
                State = "state",
                ClientId = "client_id",
                Scope = "scope",
                Nonce = "nonce"
            };

            var actionResult = new EndpointResult
            {
                Type = TypeActionResult.RedirectToCallBackUrl,
                RedirectInstruction = new RedirectInstruction
                {
                    Action = SimpleAuthEndPoints.ConsentIndex
                }
            };

            var identity = new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.AuthenticationInstant, "1"), },
                "Cookies");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var client = new Client
            {
                GrantTypes = new[] { GrantType.@implicit, GrantType.authorization_code },
                ResponseTypes = ResponseTypeNames.All,
                RedirectionUrls = new[] { new Uri("https://localhost"), },
                AllowedScopes = new[] { new Scope { Name = "scope" } }
            };
            //_processAuthorizationRequestFake.Setup(p => p.ProcessAsync(It.IsAny<AuthorizationParameter>(),
            //        It.IsAny<ClaimsPrincipal>(),
            //        It.IsAny<Client>(),
            //        It.IsAny<string>()))
            //    .Returns(Task.FromResult(actionResult));
            //_clientValidatorFake.Setup(c =>
            //        c.CheckGrantTypes(It.IsAny<Client>(), It.IsAny<GrantType[]>()))
            //    .Returns(true);

            _consentHelper.Setup(x =>
                    x.GetConfirmedConsentsAsync(It.IsAny<string>(), It.IsAny<AuthorizationParameter>()))
                .ReturnsAsync(new Consent { });
            await _getAuthorizationCodeAndTokenViaHybridWorkflowOperation
                .Execute(authorizationParameter, claimsPrincipal, client, null)
                .ConfigureAwait(false);
            _oauthEventSource.Verify(s => s.StartHybridFlow(authorizationParameter.ClientId,
                authorizationParameter.Scope,
                string.Empty));
            _generateAuthorizationResponseFake.Verify(g => g.ExecuteAsync(
                It.IsAny<EndpointResult>(),
                authorizationParameter,
                claimsPrincipal,
                It.IsAny<Client>(),
                It.IsAny<string>()));
            _oauthEventSource.Verify(s => s.EndHybridFlow(authorizationParameter.ClientId,
                Enum.GetName(typeof(TypeActionResult), actionResult.Type),
                Enum.GetName(typeof(SimpleAuthEndPoints), actionResult.RedirectInstruction.Action)));
        }

        private void InitializeFakeObjects()
        {
            _oauthEventSource = new Mock<IOAuthEventSource>();
            _generateAuthorizationResponseFake = new Mock<IGenerateAuthorizationResponse>();
            _consentHelper = new Mock<IConsentHelper>();
            _getAuthorizationCodeAndTokenViaHybridWorkflowOperation =
                new GetAuthorizationCodeAndTokenViaHybridWorkflowOperation(
                    _oauthEventSource.Object,
                    new ProcessAuthorizationRequest(
                        new Mock<IClientStore>().Object,
                        _consentHelper.Object,
                        _oauthEventSource.Object),
                    _generateAuthorizationResponseFake.Object);
        }
    }
}
