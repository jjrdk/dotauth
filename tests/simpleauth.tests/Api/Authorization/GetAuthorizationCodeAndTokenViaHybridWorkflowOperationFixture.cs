namespace SimpleAuth.Tests.Api.Authorization
{
    using Exceptions;
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth;
    using SimpleAuth.Api.Authorization;
    using SimpleAuth.Shared.Errors;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Repositories;
    using Xunit;

    //using Client = Shared.Models.Client;

    public sealed class GetAuthorizationCodeAndTokenViaHybridWorkflowOperationFixture
    {
        private readonly GetAuthorizationCodeAndTokenViaHybridWorkflowOperation
            _getAuthorizationCodeAndTokenViaHybridWorkflowOperation;

        public GetAuthorizationCodeAndTokenViaHybridWorkflowOperationFixture()
        {
            _getAuthorizationCodeAndTokenViaHybridWorkflowOperation =
                new GetAuthorizationCodeAndTokenViaHybridWorkflowOperation(
                    new Mock<IClientStore>().Object,
                    new Mock<IConsentRepository>().Object,
                    new Mock<IAuthorizationCodeStore>().Object,
                    new Mock<ITokenStore>().Object,
                    new Mock<IScopeRepository>().Object,
                    new InMemoryJwksRepository(),
                    new NoOpPublisher());
        }

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            await Assert.ThrowsAsync<NullReferenceException>(
                    () => _getAuthorizationCodeAndTokenViaHybridWorkflowOperation.Execute(
                        null,
                        null,
                        null,
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_Empty_Parameters_Then_Exceptions_Are_Thrown()
        {
            await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _getAuthorizationCodeAndTokenViaHybridWorkflowOperation.Execute(
                        new AuthorizationParameter(),
                        null,
                        null,
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Nonce_Parameter_Is_Not_Set_Then_Exception_Is_Thrown()
        {
            var authorizationParameter = new AuthorizationParameter {State = "state"};

            var ex = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _getAuthorizationCodeAndTokenViaHybridWorkflowOperation.Execute(
                        authorizationParameter,
                        null,
                        new Client(),
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequest, ex.Code);
            Assert.Equal(
                string.Format(
                    ErrorMessages.MissingParameter,
                    CoreConstants.StandardAuthorizationRequestParameterNames.NonceName),
                ex.Message);
            Assert.Equal(authorizationParameter.State, ex.State);
        }

        [Fact]
        public async Task When_Grant_Type_Is_Not_Supported_Then_Exception_Is_Thrown()
        {
            var redirectUrl = new Uri("https://localhost");
            var authorizationParameter = new AuthorizationParameter
            {
                RedirectUrl = redirectUrl,
                State = "state",
                Nonce = "nonce",
                Scope = "openid",
                ResponseType = ResponseTypeNames.Code,
            };

            var client = new Client {RedirectionUrls = new[] {redirectUrl}, AllowedScopes = new[] {"openid"},};
            var ex = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _getAuthorizationCodeAndTokenViaHybridWorkflowOperation.Execute(
                        authorizationParameter,
                        null,
                        client,
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequest, ex.Code);
            Assert.Equal(
                string.Format(
                    ErrorMessages.TheClientDoesntSupportTheGrantType,
                    authorizationParameter.ClientId,
                    "implicit and authorization_code"),
                ex.Message);
            Assert.Equal(authorizationParameter.State, ex.State);
        }
    }
}
