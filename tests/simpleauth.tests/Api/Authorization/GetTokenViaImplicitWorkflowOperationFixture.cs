namespace SimpleAuth.Tests.Api.Authorization
{
    using Exceptions;
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Repositories;
    using SimpleAuth;
    using SimpleAuth.Api.Authorization;
    using System;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Properties;
    using SimpleAuth.Repositories;
    using SimpleAuth.Shared.Errors;
    using Xunit;
    using Client = Shared.Models.Client;

    public class GetTokenViaImplicitWorkflowOperationFixture
    {
        private readonly GetTokenViaImplicitWorkflowOperation _getTokenViaImplicitWorkflowOperation;

        public GetTokenViaImplicitWorkflowOperationFixture()
        {
            _getTokenViaImplicitWorkflowOperation = new GetTokenViaImplicitWorkflowOperation(
                new Mock<IClientStore>().Object,
                new Mock<IConsentRepository>().Object,
                new Mock<IAuthorizationCodeStore>().Object,
                new Mock<ITokenStore>().Object,
                new Mock<IScopeRepository>().Object,
                new InMemoryJwksRepository(),
                new NoOpPublisher());
        }

        [Fact]
        public async Task When_Passing_No_Authorization_Request_Then_Exception_Is_Thrown()
        {
            await Assert.ThrowsAsync<NullReferenceException>(
                    () => _getTokenViaImplicitWorkflowOperation.Execute(null, null, null, null, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_Empty_Authorization_Request_Then_Exception_Is_Thrown()
        {
            await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _getTokenViaImplicitWorkflowOperation.Execute(
                        new AuthorizationParameter(),
                        null,
                        null,
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_No_Nonce_Parameter_Then_Exception_Is_Thrown()
        {
            var authorizationParameter = new AuthorizationParameter {State = "state"};

            var exception = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _getTokenViaImplicitWorkflowOperation.Execute(
                        authorizationParameter,
                        null,
                        new Client(),
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequest, exception.Code);
            Assert.Equal(
                string.Format(
                    Strings.MissingParameter,
                    CoreConstants.StandardAuthorizationRequestParameterNames.NonceName),
                exception.Message);
            Assert.Equal(authorizationParameter.State, exception.State);
        }

        [Fact]
        public async Task When_Implicit_Flow_Is_Not_Supported_Then_Exception_Is_Thrown()
        {
            var authorizationParameter = new AuthorizationParameter {Nonce = "nonce", State = "state"};

            var ex = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _getTokenViaImplicitWorkflowOperation.Execute(
                        authorizationParameter,
                        null,
                        new Client(),
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequest, ex.Code);
            Assert.Equal(
                string.Format(
                    Strings.TheClientDoesntSupportTheGrantType,
                    authorizationParameter.ClientId,
                    "implicit"),
                ex.Message);
            Assert.Equal(authorizationParameter.State, ex.State);
        }

        [Fact]
        public async Task When_Requesting_Authorization_With_Valid_Request_Then_ReturnsRedirectInstruction()
        {
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
                RedirectionUrls = new[] {new Uri("https://localhost"),},
                GrantTypes = new[] {GrantTypes.Implicit},
                AllowedScopes = new[] {"openid"}
            };
            var result = await _getTokenViaImplicitWorkflowOperation.Execute(
                    authorizationParameter,
                    claimsPrincipal,
                    client,
                    null,
                    CancellationToken.None)
                .ConfigureAwait(false);

            Assert.NotNull(result.RedirectInstruction);
        }
    }
}
