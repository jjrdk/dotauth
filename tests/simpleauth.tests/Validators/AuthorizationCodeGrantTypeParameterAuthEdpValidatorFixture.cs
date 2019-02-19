namespace SimpleAuth.Tests.Validators
{
    using Exceptions;
    using Moq;
    using Parameters;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth;
    using SimpleAuth.Shared.Errors;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Extensions;
    using Xunit;

    public sealed class AuthorizationCodeGrantTypeParameterAuthEdpValidatorFixture
    {
        private readonly Mock<IClientStore> _clientRepository;

        private readonly AuthorizationCodeGrantTypeParameterAuthEdpValidator
            _authorizationCodeGrantTypeParameterAuthEdpValidator;

        public AuthorizationCodeGrantTypeParameterAuthEdpValidatorFixture()
        {
            _clientRepository = new Mock<IClientStore>();
            _authorizationCodeGrantTypeParameterAuthEdpValidator =
                new AuthorizationCodeGrantTypeParameterAuthEdpValidator(_clientRepository.Object);
        }

        [Fact]
        public async Task When_Validating_Authorization_Parameter_With_Empty_Scope_Then_Exception_Is_Thrown()
        {
            const string state = "state";
            var authorizationParameter = new AuthorizationParameter { State = state };

            var exception = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _authorizationCodeGrantTypeParameterAuthEdpValidator.Validate(
                        authorizationParameter,
                        CancellationToken.None))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.Equal(
                string.Format(
                    ErrorDescriptions.MissingParameter,
                    CoreConstants.StandardAuthorizationRequestParameterNames.ScopeName),
                exception.Message);
            Assert.Equal(state, exception.State);
        }

        [Fact]
        public async Task When_Validating_Authorization_Parameter_With_Empty_ClientId_Then_Exception_Is_Thrown()
        {
            const string state = "state";
            var authorizationParameter = new AuthorizationParameter { State = state, Scope = "scope" };

            var exception = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _authorizationCodeGrantTypeParameterAuthEdpValidator.Validate(
                        authorizationParameter,
                        CancellationToken.None))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.Equal(
                string.Format(
                    ErrorDescriptions.MissingParameter,
                    CoreConstants.StandardAuthorizationRequestParameterNames.ClientIdName),
                exception.Message);
            Assert.Equal(state, exception.State);
        }

        [Fact]
        public async Task When_Validating_Authorization_Parameter_With_Empty_RedirectUri_Then_Exception_Is_Thrown()
        {
            const string state = "state";
            var authorizationParameter = new AuthorizationParameter
            {
                State = state,
                Scope = "scope",
                ClientId = "clientId"
            };

            var exception = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _authorizationCodeGrantTypeParameterAuthEdpValidator.Validate(
                        authorizationParameter,
                        CancellationToken.None))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.True(
                exception.Message
                == string.Format(
                    ErrorDescriptions.MissingParameter,
                    CoreConstants.StandardAuthorizationRequestParameterNames.RedirectUriName));
            Assert.True(exception.State == state);
        }

        [Fact]
        public async Task When_Validating_Authorization_Parameter_With_Empty_ResponseType_Then_Exception_Is_Thrown()
        {
            const string state = "state";
            var authorizationParameter = new AuthorizationParameter
            {
                State = state,
                Scope = "scope",
                ClientId = "clientId",
                RedirectUrl = new Uri("https://redirectUrl")
            };

            var exception = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _authorizationCodeGrantTypeParameterAuthEdpValidator.Validate(
                        authorizationParameter,
                        CancellationToken.None))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.True(
                exception.Message
                == string.Format(
                    ErrorDescriptions.MissingParameter,
                    CoreConstants.StandardAuthorizationRequestParameterNames.ResponseTypeName));
            Assert.True(exception.State == state);
        }

        [Fact]
        public async Task When_Vadidating_Authorization_Parameter_With_InvalidResponseType_Then_Exception_Is_Thrown()
        {
            const string state = "state";
            var authorizationParameter = new AuthorizationParameter
            {
                State = state,
                Scope = "scope",
                ClientId = "clientId",
                RedirectUrl = new Uri("https://redirectUrl"),
                ResponseType = "invalid_response_type"
            };

            var exception = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _authorizationCodeGrantTypeParameterAuthEdpValidator.Validate(
                        authorizationParameter,
                        CancellationToken.None))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.True(exception.Message == ErrorDescriptions.AtLeastOneResponseTypeIsNotSupported);
            Assert.True(exception.State == state);
        }

        [Fact]
        public async Task When_Validating_Authorization_Parameter_With_Invalid_Prompt_Then_Exception_Is_Thrown()
        {
            const string state = "state";
            var authorizationParameter = new AuthorizationParameter
            {
                State = state,
                Scope = "scope",
                ClientId = "clientId",
                RedirectUrl = new Uri("https://redirectUrl"),
                ResponseType = "code",
                Prompt = "invalid_prompt"
            };

            var exception = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _authorizationCodeGrantTypeParameterAuthEdpValidator.Validate(
                        authorizationParameter,
                        CancellationToken.None))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.True(exception.Message == ErrorDescriptions.AtLeastOnePromptIsNotSupported);
            Assert.True(exception.State == state);
        }

        [Fact]
        public async Task
            When_Validating_Authorization_Parameter_With_NoneLoginPromptParameter_Then_Exception_Is_Thrown()
        {
            const string state = "state";
            var authorizationParameter = new AuthorizationParameter
            {
                State = state,
                Scope = "scope",
                ClientId = "clientId",
                RedirectUrl = new Uri("https://redirectUrl"),
                ResponseType = "code",
                Prompt = "none login"
            };

            var exception = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _authorizationCodeGrantTypeParameterAuthEdpValidator.Validate(
                        authorizationParameter,
                        CancellationToken.None))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.True(exception.Message == ErrorDescriptions.PromptParameterShouldHaveOnlyNoneValue);
            Assert.True(exception.State == state);
        }

        [Fact]
        public async Task When_Validating_Authorization_Parameter_With_Invalid_ClientId_Then_Exception_Is_Thrown()
        {
            const string state = "state";
            const string clientId = "clientId";
            var authorizationParameter = new AuthorizationParameter
            {
                State = state,
                Scope = "scope",
                ClientId = clientId,
                RedirectUrl = new Uri("http://localhost"),
                ResponseType = "code",
                Prompt = "none"
            };

            _clientRepository.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult((Client)null));

            var exception = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _authorizationCodeGrantTypeParameterAuthEdpValidator.Validate(
                        authorizationParameter,
                        CancellationToken.None))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.Equal(string.Format(ErrorDescriptions.ClientIsNotValid, clientId), exception.Message);
            Assert.Equal(state, exception.State);
        }

        [Fact]
        public async Task
            When_Validating_Authorization_Parameter_With_RedirectUri_Not_Known_By_The_Client_Then_Exception_Is_Thrown()
        {
            const string state = "state";
            const string clientId = "clientId";
            const string redirectUri = "http://localhost/";
            var authorizationParameter = new AuthorizationParameter
            {
                State = state,
                Scope = "scope",
                ClientId = clientId,
                RedirectUrl = new Uri(redirectUri),
                ResponseType = "code",
                Prompt = "none"
            };
            var client = new Client();

            _clientRepository.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            var exception = await Assert.ThrowsAsync<SimpleAuthExceptionWithState>(
                    () => _authorizationCodeGrantTypeParameterAuthEdpValidator.Validate(
                        authorizationParameter,
                        CancellationToken.None))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.Equal(string.Format(ErrorDescriptions.RedirectUrlIsNotValid, redirectUri), exception.Message);
            Assert.Equal(state, exception.State);
        }
    }
}
