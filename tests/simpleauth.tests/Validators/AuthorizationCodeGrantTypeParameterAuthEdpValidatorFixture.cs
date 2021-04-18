namespace SimpleAuth.Tests.Validators
{
    using Moq;
    using Parameters;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth;
    using SimpleAuth.Shared.Errors;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Divergic.Logging.Xunit;
    using SimpleAuth.Extensions;
    using SimpleAuth.Properties;
    using SimpleAuth.Shared;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class AuthorizationCodeGrantTypeParameterAuthEdpValidatorFixture
    {
        private readonly Mock<IClientStore> _clientRepository;

        private readonly AuthorizationCodeGrantTypeParameterAuthEdpValidator
            _authorizationCodeGrantTypeParameterAuthEdpValidator;

        public AuthorizationCodeGrantTypeParameterAuthEdpValidatorFixture(ITestOutputHelper outputHelper)
        {
            _clientRepository = new Mock<IClientStore>();
            _authorizationCodeGrantTypeParameterAuthEdpValidator =
                new AuthorizationCodeGrantTypeParameterAuthEdpValidator(
                    _clientRepository.Object,
                    new TestOutputLogger("test", outputHelper));
        }

        [Fact]
        public async Task When_Validating_Authorization_Parameter_With_Empty_Scope_Then_Exception_Is_Thrown()
        {
            const string state = "state";
            var authorizationParameter = new AuthorizationParameter {State = state};

            var exception = await _authorizationCodeGrantTypeParameterAuthEdpValidator.Validate(
                    authorizationParameter,
                    CancellationToken.None)
                .ConfigureAwait(false) as Option<Client>.Error;

            Assert.Equal(
                new Option<Client>.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidRequest,
                        Detail = string.Format(
                            Strings.MissingParameter,
                            CoreConstants.StandardAuthorizationRequestParameterNames.ScopeName)
                    },
                    state),
                exception);
        }

        [Fact]
        public async Task When_Validating_Authorization_Parameter_With_Empty_ClientId_Then_Exception_Is_Thrown()
        {
            const string state = "state";
            var authorizationParameter = new AuthorizationParameter {State = state, Scope = "scope"};

            var exception = await _authorizationCodeGrantTypeParameterAuthEdpValidator.Validate(
                    authorizationParameter,
                    CancellationToken.None)
                .ConfigureAwait(false) as Option<Client>.Error;

            Assert.Equal(
                new Option<Client>.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidRequest,
                        Detail = string.Format(
                            Strings.MissingParameter,
                            CoreConstants.StandardAuthorizationRequestParameterNames.ClientIdName)
                    },
                    state),
                exception);
        }

        [Fact]
        public async Task When_Validating_Authorization_Parameter_With_Empty_RedirectUri_Then_Exception_Is_Thrown()
        {
            const string state = "state";
            var authorizationParameter = new AuthorizationParameter
            {
                State = state, Scope = "scope", ClientId = "clientId"
            };

            var exception = await _authorizationCodeGrantTypeParameterAuthEdpValidator.Validate(
                    authorizationParameter,
                    CancellationToken.None)
                .ConfigureAwait(false) as Option<Client>.Error;

            Assert.Equal(
                new Option<Client>.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidRequest,
                        Detail = string.Format(
                            Strings.MissingParameter,
                            CoreConstants.StandardAuthorizationRequestParameterNames.RedirectUriName)
                    },
                    state),
                exception);
        }

        [Fact]
        public async Task When_Validating_Authorization_Parameter_With_Empty_ResponseType_Then_Exception_Is_Thrown()
        {
            const string state = "state";
            var authorizationParameter = new AuthorizationParameter
            {
                State = state, Scope = "scope", ClientId = "clientId", RedirectUrl = new Uri("https://redirectUrl")
            };

            var exception = await _authorizationCodeGrantTypeParameterAuthEdpValidator.Validate(
                    authorizationParameter,
                    CancellationToken.None)
                .ConfigureAwait(false) as Option<Client>.Error;

            Assert.Equal(
                new Option<Client>.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidRequest,
                        Detail = string.Format(
                            Strings.MissingParameter,
                            CoreConstants.StandardAuthorizationRequestParameterNames.ResponseTypeName)
                    },
                    state),
                exception);
        }

        [Fact]
        public async Task When_Validating_Authorization_Parameter_With_InvalidResponseType_Then_Exception_Is_Thrown()
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

            var exception = await _authorizationCodeGrantTypeParameterAuthEdpValidator.Validate(
                    authorizationParameter,
                    CancellationToken.None)
                .ConfigureAwait(false) as Option<Client>.Error;

            Assert.Equal(
                new Option<Client>.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidRequest, Detail = Strings.AtLeastOneResponseTypeIsNotSupported
                    },
                    state),
                exception);
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

            var exception = await _authorizationCodeGrantTypeParameterAuthEdpValidator.Validate(
                    authorizationParameter,
                    CancellationToken.None)
                .ConfigureAwait(false) as Option<Client>.Error;

            Assert.Equal(
                new Option<Client>.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidRequest, Detail = Strings.AtLeastOnePromptIsNotSupported
                    },
                    state),
                exception);
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

            var exception = await _authorizationCodeGrantTypeParameterAuthEdpValidator.Validate(
                    authorizationParameter,
                    CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Equal(
                new Option<Client>.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidRequest, Detail = Strings.PromptParameterShouldHaveOnlyNoneValue
                    },
                    state),
                exception);
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
                .Returns(() => Task.FromResult((Client) null));

            var exception = await _authorizationCodeGrantTypeParameterAuthEdpValidator.Validate(
                    authorizationParameter,
                    CancellationToken.None)
                .ConfigureAwait(false) as Option<Client>.Error;

            Assert.Equal(
                new Option<Client>.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidRequest,
                        Detail = string.Format(Strings.ClientIsNotValid, clientId)
                    },
                    state),
                exception);
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

            var exception = await _authorizationCodeGrantTypeParameterAuthEdpValidator.Validate(
                    authorizationParameter,
                    CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Equal(
                new Option<Client>.Error(
                    new ErrorDetails
                    {
                        Title = ErrorCodes.InvalidRequest,
                        Detail = string.Format(Strings.RedirectUrlIsNotValid, redirectUri)
                    },
                    state),
                exception);
        }
    }
}
