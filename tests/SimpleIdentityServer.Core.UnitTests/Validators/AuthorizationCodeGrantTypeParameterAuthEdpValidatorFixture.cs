using Moq;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Validators
{
    using Shared.Models;
    using Shared.Repositories;
    using System;

    public sealed class AuthorizationCodeGrantTypeParameterAuthEdpValidatorFixture
    {
        private Mock<IParameterParserHelper> _parameterParserHelperFake;
        private Mock<IClientStore> _clientRepository;
        private Mock<IClientValidator> _clientValidatorFake;

        private IAuthorizationCodeGrantTypeParameterAuthEdpValidator
            _authorizationCodeGrantTypeParameterAuthEdpValidator;

        [Fact]
        public async Task When_Validating_Authorization_Parameter_With_Empty_Scope_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            const string state = "state";
            var authorizationParameter = new AuthorizationParameter
            {
                State = state
            };

            var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(
                    () => _authorizationCodeGrantTypeParameterAuthEdpValidator.ValidateAsync(authorizationParameter))
                .ConfigureAwait(false);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message ==
                        string.Format(ErrorDescriptions.MissingParameter,
                            CoreConstants.StandardAuthorizationRequestParameterNames.ScopeName));
            Assert.True(exception.State == state);
        }

        [Fact]
        public async Task When_Validating_Authorization_Parameter_With_Empty_ClientId_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            const string state = "state";
            var authorizationParameter = new AuthorizationParameter
            {
                State = state,
                Scope = "scope"
            };

            var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(
                    () => _authorizationCodeGrantTypeParameterAuthEdpValidator.ValidateAsync(authorizationParameter))
                .ConfigureAwait(false);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message ==
                        string.Format(ErrorDescriptions.MissingParameter,
                            CoreConstants.StandardAuthorizationRequestParameterNames.ClientIdName));
            Assert.True(exception.State == state);
        }

        [Fact]
        public async Task When_Validating_Authorization_Parameter_With_Empty_RedirectUri_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            const string state = "state";
            var authorizationParameter = new AuthorizationParameter
            {
                State = state,
                Scope = "scope",
                ClientId = "clientId"
            };

            var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(
                    () => _authorizationCodeGrantTypeParameterAuthEdpValidator.ValidateAsync(authorizationParameter))
                .ConfigureAwait(false);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message ==
                        string.Format(ErrorDescriptions.MissingParameter,
                            CoreConstants.StandardAuthorizationRequestParameterNames.RedirectUriName));
            Assert.True(exception.State == state);
        }

        [Fact]
        public async Task When_Validating_Authorization_Parameter_With_Empty_ResponseType_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            const string state = "state";
            var authorizationParameter = new AuthorizationParameter
            {
                State = state,
                Scope = "scope",
                ClientId = "clientId",
                RedirectUrl = new Uri("https://redirectUrl")
            };

            var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(
                    () => _authorizationCodeGrantTypeParameterAuthEdpValidator.ValidateAsync(authorizationParameter))
                .ConfigureAwait(false);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message ==
                        string.Format(ErrorDescriptions.MissingParameter,
                            CoreConstants.StandardAuthorizationRequestParameterNames.ResponseTypeName));
            Assert.True(exception.State == state);
        }

        [Fact]
        public async Task When_Vadidating_Authorization_Parameter_With_InvalidResponseType_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
            const string state = "state";
            var authorizationParameter = new AuthorizationParameter
            {
                State = state,
                Scope = "scope",
                ClientId = "clientId",
                RedirectUrl = new Uri("https://redirectUrl"),
                ResponseType = "invalid_response_type"
            };

            var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(() =>
                    _authorizationCodeGrantTypeParameterAuthEdpValidator.ValidateAsync(authorizationParameter))
                .ConfigureAwait(false);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == ErrorDescriptions.AtLeastOneResponseTypeIsNotSupported);
            Assert.True(exception.State == state);
        }

        [Fact]
        public async Task When_Validating_Authorization_Parameter_With_Invalid_Prompt_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
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

            var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(
                    () => _authorizationCodeGrantTypeParameterAuthEdpValidator.ValidateAsync(authorizationParameter))
                .ConfigureAwait(false);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == ErrorDescriptions.AtLeastOnePromptIsNotSupported);
            Assert.True(exception.State == state);
        }

        [Fact]
        public async Task
            When_Validating_Authorization_Parameter_With_NoneLoginPromptParameter_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
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
            _parameterParserHelperFake.Setup(p => p.ParsePrompts(It.IsAny<string>()))
                .Returns(new List<PromptParameter>
                {
                    PromptParameter.none,
                    PromptParameter.login
                });

            var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(
                    () => _authorizationCodeGrantTypeParameterAuthEdpValidator.ValidateAsync(authorizationParameter))
                .ConfigureAwait(false);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == ErrorDescriptions.PromptParameterShouldHaveOnlyNoneValue);
            Assert.True(exception.State == state);
        }

        //[Fact]
        //public async Task When_Validating_Authorization_Parameter_With_Not_Well_Formed_Uri_Then_Exception_Is_Thrown()
        //{
        //    // The redirect_uri is considered well-formed according to the RFC-3986
        //    InitializeFakeObjects();
        //    const string state = "state";
        //    var authorizationParameter = new AuthorizationParameter
        //    {
        //        State = state,
        //        Scope = "scope",
        //        ClientId = "clientId",
        //        RedirectUrl = new Uri("not_well_formed_uri"),
        //        ResponseType = "code",
        //        Prompt = "none"
        //    };
        //    _parameterParserHelperFake.Setup(p => p.ParsePrompts(It.IsAny<string>()))
        //        .Returns(new List<PromptParameter>
        //        {
        //            PromptParameter.none
        //        });

        //    var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(
        //            () => _authorizationCodeGrantTypeParameterAuthEdpValidator.ValidateAsync(authorizationParameter))
        //        .ConfigureAwait(false);
        //    Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
        //    Assert.True(exception.Message == ErrorDescriptions.TheRedirectionUriIsNotWellFormed);
        //    Assert.True(exception.State == state);
        //}

        [Fact]
        public async Task When_Validating_Authorization_Parameter_With_Invalid_ClientId_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
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
            _parameterParserHelperFake.Setup(p => p.ParsePrompts(It.IsAny<string>()))
                .Returns(new List<PromptParameter>
                {
                    PromptParameter.none
                });
            _clientRepository.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(() => Task.FromResult((Client)null));

            var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(() =>
                    _authorizationCodeGrantTypeParameterAuthEdpValidator.ValidateAsync(authorizationParameter))
                .ConfigureAwait(false);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.ClientIsNotValid, clientId));
            Assert.True(exception.State == state);
        }

        [Fact]
        public async Task
            When_Validating_Authorization_Parameter_With_RedirectUri_Not_Known_By_The_Client_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();
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
            _parameterParserHelperFake.Setup(p => p.ParsePrompts(It.IsAny<string>()))
                .Returns(new List<PromptParameter>
                {
                    PromptParameter.none
                });
            _clientRepository.Setup(c => c.GetById(It.IsAny<string>()))
                .Returns(Task.FromResult(client));
            _clientValidatorFake.Setup(c => c.GetRedirectionUrls(It.IsAny<Client>(), It.IsAny<Uri[]>()))
                .Returns(() => new Uri[0]);

            var exception = await Assert.ThrowsAsync<IdentityServerExceptionWithState>(() =>
                    _authorizationCodeGrantTypeParameterAuthEdpValidator.ValidateAsync(authorizationParameter))
                .ConfigureAwait(false);
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.Equal(string.Format(ErrorDescriptions.RedirectUrlIsNotValid, redirectUri), exception.Message);
            Assert.Equal(state, exception.State);
        }

        private void InitializeFakeObjects()
        {
            _parameterParserHelperFake = new Mock<IParameterParserHelper>();
            _clientValidatorFake = new Mock<IClientValidator>();
            _clientRepository = new Mock<IClientStore>();
            _authorizationCodeGrantTypeParameterAuthEdpValidator =
                new AuthorizationCodeGrantTypeParameterAuthEdpValidator(
                    _parameterParserHelperFake.Object,
                    _clientRepository.Object,
                    _clientValidatorFake.Object);
        }
    }
}
