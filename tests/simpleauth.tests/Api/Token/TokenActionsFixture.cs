namespace SimpleAuth.Tests.Api.Token
{
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Models;
    using SimpleAuth;
    using SimpleAuth.Api.Token;
    using SimpleAuth.Api.Token.Actions;
    using SimpleAuth.Authenticate;
    using SimpleAuth.Helpers;
    using SimpleAuth.Validators;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class TokenActionsFixture
    {
        private Mock<IGetTokenByResourceOwnerCredentialsGrantTypeAction>
            _getTokenByResourceOwnerCredentialsGrantTypeActionFake;
        private Mock<IGetTokenByAuthorizationCodeGrantTypeAction> _getTokenByAuthorizationCodeGrantTypeActionFake;
        private Mock<IGetTokenByRefreshTokenGrantTypeAction> _getTokenByRefreshTokenGrantTypeActionFake;
        private Mock<IClientCredentialsGrantTypeParameterValidator> _clientCredentialsGrantTypeParameterValidatorStub;
        private Mock<IRevokeTokenParameterValidator> _revokeTokenParameterValidator;
        private Mock<IRevokeTokenAction> _revokeTokenActionStub;
        private ITokenActions _tokenActions;

        [Fact]
        public async Task When_Passing_No_Request_To_ResourceOwner_Grant_Type_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _tokenActions.GetTokenByResourceOwnerCredentialsGrantType(null, null, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_No_Request_To_AuthorizationCode_Grant_Type_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _tokenActions.GetTokenByAuthorizationCodeGrantType(null, null, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_No_Request_To_Refresh_Token_Grant_Type_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _tokenActions.GetTokenByRefreshTokenGrantType(null, null, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_To_ClientCredentials_GrantType_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _tokenActions.GetTokenByClientCredentialsGrantType(null, null, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Getting_Token_Via_ClientCredentials_GrantType_Then_GrantedToken_Is_Returned()
        {
            InitializeFakeObjects();
            const string scope = "valid_scope";
            const string clientId = "valid_client_id";
            var parameter = new ClientCredentialsGrantTypeParameter
            {
                Scope = scope
            };

            var result = await _tokenActions.GetTokenByClientCredentialsGrantType(parameter, null, null, null).ConfigureAwait(false);

            Assert.True(result.ClientId == clientId);
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _tokenActions.RevokeToken(null, null, null, null)).ConfigureAwait(false);
        }

        private void InitializeFakeObjects()
        {
            _getTokenByResourceOwnerCredentialsGrantTypeActionFake = new Mock<IGetTokenByResourceOwnerCredentialsGrantTypeAction>();
            _getTokenByAuthorizationCodeGrantTypeActionFake = new Mock<IGetTokenByAuthorizationCodeGrantTypeAction>();
            _getTokenByRefreshTokenGrantTypeActionFake = new Mock<IGetTokenByRefreshTokenGrantTypeAction>();
            _clientCredentialsGrantTypeParameterValidatorStub = new Mock<IClientCredentialsGrantTypeParameterValidator>();
            _revokeTokenParameterValidator = new Mock<IRevokeTokenParameterValidator>();
            var eventPublisher = new Mock<IEventPublisher>();
            _revokeTokenActionStub = new Mock<IRevokeTokenAction>();
            const string scope = "valid_scope";
            const string clientId = "valid_client_id";
            var mock = new Mock<IAuthenticateClient>();
            mock.Setup(x => x.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), It.IsAny<string>()))
                .ReturnsAsync<AuthenticateInstruction, string, IAuthenticateClient, AuthenticationResult>((a, s) =>
                    new AuthenticationResult(
                        new Client
                        {
                            ClientId = clientId,
                            AllowedScopes = new[] { new Scope { Name = scope } },
                            ResponseTypes = new[] { ResponseTypeNames.Token },
                            GrantTypes = new List<GrantType> { GrantType.client_credentials }
                        },
                        null));
            //var scopeValidatorMock = new Mock<IScopeValidator>();
            //scopeValidatorMock.Setup(x => x.Check(It.IsAny<string>(), It.IsAny<Client>()))
            //    .Returns(new ScopeValidationResult(new[] { scope }));
            var grantedTokenHelperMock = new Mock<IGrantedTokenHelper>();
            grantedTokenHelperMock.Setup(x => x.GetValidGrantedTokenAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<JwtPayload>(),
                    It.IsAny<JwtPayload>()))
                .ReturnsAsync(new GrantedToken
                {
                    ClientId = clientId
                });
            _tokenActions = new TokenActions(
                _getTokenByResourceOwnerCredentialsGrantTypeActionFake.Object,
                _getTokenByAuthorizationCodeGrantTypeActionFake.Object,
                _getTokenByRefreshTokenGrantTypeActionFake.Object,
                _clientCredentialsGrantTypeParameterValidatorStub.Object,
                mock.Object,
                new Mock<IGrantedTokenGeneratorHelper>().Object,
                _revokeTokenParameterValidator.Object,
                _revokeTokenActionStub.Object,
                eventPublisher.Object,
                new Mock<ITokenStore>().Object,
                grantedTokenHelperMock.Object);
        }
    }
}
