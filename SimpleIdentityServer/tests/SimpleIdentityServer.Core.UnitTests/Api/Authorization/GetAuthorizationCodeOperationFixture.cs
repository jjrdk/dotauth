using Moq;
using SimpleIdentityServer.Core.Api.Authorization.Actions;
using SimpleIdentityServer.Core.Api.Authorization.Common;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.OAuth.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Authorization
{
    using Client = Client;

    public sealed class GetAuthorizationCodeOperationFixture
    {
        private Mock<IProcessAuthorizationRequest> _processAuthorizationRequestFake;
        private Mock<IClientValidator> _clientValidatorFake;
        private Mock<IGenerateAuthorizationResponse> _generateAuthorizationResponseFake;
        private Mock<IOAuthEventSource> _oauthEventSource;
        private IGetAuthorizationCodeOperation _getAuthorizationCodeOperation;

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getAuthorizationCodeOperation.Execute(null, null, null, null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getAuthorizationCodeOperation.Execute(new AuthorizationParameter(), null, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_The_Client_Grant_Type_Is_Not_Supported_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "clientId";
            const string scope = "scope";
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Scope = scope,
                Claims = null
            };
            _clientValidatorFake.Setup(c => c.CheckGrantTypes(It.IsAny<Client>(), It.IsAny<GrantType[]>()))
                .Returns(false);

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(() => _getAuthorizationCodeOperation.Execute(authorizationParameter, null, new Client(), null)).ConfigureAwait(false);

            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.Equal(string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType,
                    clientId,
                    "authorization_code"),
                exception.Message);
        }

        [Fact]
        public async Task When_Redirected_To_Callback_And_Resource_Owner_Is_Not_Authenticated_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter
            {
                State = "state"
            };

            var actionResult = new ActionResult
            {
                Type = TypeActionResult.RedirectToCallBackUrl
            };

            _processAuthorizationRequestFake.Setup(p => p.ProcessAsync(It.IsAny<AuthorizationParameter>(),
                It.IsAny<ClaimsPrincipal>(), It.IsAny<Client>(), null))
                .Returns(Task.FromResult(actionResult));
            _clientValidatorFake.Setup(c => c.CheckGrantTypes(It.IsAny<Client>(), It.IsAny<GrantType[]>()))
                .Returns(true);

            // ACT & ASSERT
            var ex = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(
                () => _getAuthorizationCodeOperation.Execute(authorizationParameter, null, new Client(), null)).ConfigureAwait(false);
            Assert.True(ex.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(ex.Message ==
                          ErrorDescriptions.TheResponseCannotBeGeneratedBecauseResourceOwnerNeedsToBeAuthenticated);
            Assert.True(ex.State == authorizationParameter.State);
        }

        [Fact]
        public async Task When_Passing_Valid_Request_Then_Events_Are_Logged()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "clientId";
            const string scope = "scope";
            var actionResult = new ActionResult
            {
                Type = TypeActionResult.RedirectToAction,
                RedirectInstruction = new RedirectInstruction
                {
                    Action = IdentityServerEndPoints.FormIndex
                }
            };
            var client = new Client();
            var authorizationParameter = new AuthorizationParameter
            {
                ClientId = clientId,
                Scope = scope,
                Claims = null
            };
            var jsonAuthorizationParameter = authorizationParameter.SerializeWithJavascript();
            _processAuthorizationRequestFake.Setup(p => p.ProcessAsync(
                It.IsAny<AuthorizationParameter>(),
                It.IsAny<ClaimsPrincipal>(), It.IsAny<Client>(), null)).Returns(Task.FromResult(actionResult));
            _clientValidatorFake.Setup(c => c.CheckGrantTypes(It.IsAny<Client>(), It.IsAny<GrantType[]>()))
                .Returns(true);

            // ACT
            await _getAuthorizationCodeOperation.Execute(authorizationParameter, null, client, null).ConfigureAwait(false);

            // ASSERTS
            _oauthEventSource.Verify(s => s.StartAuthorizationCodeFlow(clientId, scope, string.Empty));
            _oauthEventSource.Verify(s => s.EndAuthorizationCodeFlow(clientId, "RedirectToAction", "FormIndex"));
        }

        private void InitializeFakeObjects()
        {
            _processAuthorizationRequestFake = new Mock<IProcessAuthorizationRequest>();
            _clientValidatorFake = new Mock<IClientValidator>();
            _generateAuthorizationResponseFake = new Mock<IGenerateAuthorizationResponse>();
            _oauthEventSource = new Mock<IOAuthEventSource>();
            _getAuthorizationCodeOperation = new GetAuthorizationCodeOperation(
                _processAuthorizationRequestFake.Object,
                _clientValidatorFake.Object,
                _generateAuthorizationResponseFake.Object,
                _oauthEventSource.Object);
        }
    }
}
