namespace SimpleAuth.Tests.Api.Authorization
{
    using Errors;
    using Exceptions;
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth;
    using SimpleAuth.Api.Authorization;
    using SimpleAuth.Common;
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Xunit;
    using Client = Shared.Models.Client;

    public class GetTokenViaImplicitWorkflowOperationFixture
    {
        //private Mock<IProcessAuthorizationRequest> _processAuthorizationRequestFake;
        private Mock<IGenerateAuthorizationResponse> _generateAuthorizationResponseFake;
        private GetTokenViaImplicitWorkflowOperation _getTokenViaImplicitWorkflowOperation;

        [Fact]
        public async Task When_Passing_No_Authorization_Request_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert
                .ThrowsAsync<ArgumentNullException>(() =>
                    _getTokenViaImplicitWorkflowOperation.Execute(null, null, null, null))
                .ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                    _getTokenViaImplicitWorkflowOperation.Execute(new AuthorizationParameter(), null, null, null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_No_Nonce_Parameter_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter { State = "state" };

            var exception = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _getTokenViaImplicitWorkflowOperation.Execute(
                        authorizationParameter,
                        null,
                        new Client(),
                        null))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.Equal(
                string.Format(
                    ErrorDescriptions.MissingParameter,
                    CoreConstants.StandardAuthorizationRequestParameterNames.NonceName),
                exception.Message);
            Assert.Equal(authorizationParameter.State, exception.State);
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

            var ex = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(() =>
                    _getTokenViaImplicitWorkflowOperation.Execute(authorizationParameter, null, new Client(), null))
                .ConfigureAwait(false);
            Assert.True(ex.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(ex.Message ==
                        string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType,
                            authorizationParameter.ClientId,
                            "implicit"));
            Assert.True(ex.State == authorizationParameter.State);
        }

        [Fact]
        public async Task When_Requesting_Authorization_With_Valid_Request_Then_ReturnsRedirectInstruction()
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

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] {new Claim("sub", "test")}, "fake"));

            var client = new Client
            {
                ResponseTypes = ResponseTypeNames.All,
                RedirectionUrls = new[] { new Uri("https://localhost"), },
                GrantTypes = new[] { GrantType.@implicit },
                AllowedScopes = new[] { new Scope { Name = "openid" } }
            };
            var result = await _getTokenViaImplicitWorkflowOperation
                .Execute(authorizationParameter, claimsPrincipal, client, null)
                .ConfigureAwait(false);

            Assert.NotNull(result.RedirectInstruction);
        }

        private void InitializeFakeObjects()
        {
            _generateAuthorizationResponseFake = new Mock<IGenerateAuthorizationResponse>();
            _getTokenViaImplicitWorkflowOperation = new GetTokenViaImplicitWorkflowOperation(
                new ProcessAuthorizationRequest(
                    new Mock<IClientStore>().Object,
                    new Mock<IConsentRepository>().Object),
                _generateAuthorizationResponseFake.Object);
        }
    }
}
