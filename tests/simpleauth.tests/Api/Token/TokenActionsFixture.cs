namespace SimpleAuth.Tests.Api.Token
{
    using Logging;
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
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class TokenActionsFixture
    {
        private Mock<IGetTokenByResourceOwnerCredentialsGrantTypeAction>
            _getTokenByResourceOwnerCredentialsGrantTypeActionFake;
        private Mock<IGetTokenByAuthorizationCodeGrantTypeAction> _getTokenByAuthorizationCodeGrantTypeActionFake;
        private Mock<IOAuthEventSource> _oauthEventSource;
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
        public async Task When_Requesting_Token_Via_Resource_Owner_Credentials_Grant_Type_Then_Events_Are_Logged()
        {
            InitializeFakeObjects();
            const string clientId = "clientId";
            const string userName = "userName";
            const string password = "password";
            const string accessToken = "accessToken";
            const string identityToken = "identityToken";
            var parameter = new ResourceOwnerGrantTypeParameter
            {
                ClientId = clientId,
                UserName = userName,
                Password = password,
                Scope = "fake"
            };
            var grantedToken = new GrantedToken
            {
                AccessToken = accessToken,
                IdToken = identityToken
            };
            _getTokenByResourceOwnerCredentialsGrantTypeActionFake.Setup(
                g => g.Execute(It.IsAny<ResourceOwnerGrantTypeParameter>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<X509Certificate2>(), null))
                .Returns(Task.FromResult(grantedToken));

            await _tokenActions.GetTokenByResourceOwnerCredentialsGrantType(parameter, null, null, null).ConfigureAwait(false);

            _oauthEventSource.Verify(s => s.StartGetTokenByResourceOwnerCredentials(clientId, userName, password));
            _oauthEventSource.Verify(s => s.EndGetTokenByResourceOwnerCredentials(accessToken, identityToken));
        }

        [Fact]
        public async Task When_Passing_No_Request_To_AuthorizationCode_Grant_Type_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _tokenActions.GetTokenByAuthorizationCodeGrantType(null, null, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Requesting_Token_Via_AuthorizationCode_Grant_Type_Then_Events_Are_Logged()
        {
            InitializeFakeObjects();
            const string clientId = "clientId";
            const string code = "code";
            const string accessToken = "accessToken";
            const string identityToken = "identityToken";
            var parameter = new AuthorizationCodeGrantTypeParameter
            {
                ClientId = clientId,
                Code = code,
                RedirectUri = new Uri("https://fake/")
            };
            var grantedToken = new GrantedToken
            {
                AccessToken = accessToken,
                IdToken = identityToken
            };
            _getTokenByAuthorizationCodeGrantTypeActionFake.Setup(
                g => g.Execute(It.IsAny<AuthorizationCodeGrantTypeParameter>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<X509Certificate2>(), null))
                .Returns(Task.FromResult(grantedToken));

            await _tokenActions.GetTokenByAuthorizationCodeGrantType(parameter, null, null, null).ConfigureAwait(false);

            _oauthEventSource.Verify(s => s.StartGetTokenByAuthorizationCode(clientId, code));
            _oauthEventSource.Verify(s => s.EndGetTokenByAuthorizationCode(accessToken, identityToken));
        }

        [Fact]
        public async Task When_Passing_No_Request_To_Refresh_Token_Grant_Type_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _tokenActions.GetTokenByRefreshTokenGrantType(null, null, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_Request_To_Refresh_Token_Grant_Type_Then_Events_Are_Logged()
        {
            InitializeFakeObjects();
            const string refreshToken = "refresh_token";
            const string accessToken = "accessToken";
            const string identityToken = "identityToken";
            var parameter = new RefreshTokenGrantTypeParameter
            {
                RefreshToken = refreshToken
            };
            var grantedToken = new GrantedToken
            {
                AccessToken = accessToken,
                IdToken = identityToken
            };
            _getTokenByRefreshTokenGrantTypeActionFake.Setup(
                g => g.Execute(It.IsAny<RefreshTokenGrantTypeParameter>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<X509Certificate2>(), null))
                .Returns(Task.FromResult(grantedToken));

            await _tokenActions.GetTokenByRefreshTokenGrantType(parameter, null, null, null).ConfigureAwait(false);

            _oauthEventSource.Verify(s => s.StartGetTokenByRefreshToken(refreshToken));
            _oauthEventSource.Verify(s => s.EndGetTokenByRefreshToken(accessToken, identityToken));
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
            //var grantedToken = new GrantedToken
            //{
            //    ClientId = clientId
            //};
            //_getTokenByClientCredentialsGrantTypeActionStub.Setup(g =>
            //        g.Execute(It.IsAny<ClientCredentialsGrantTypeParameter>(),
            //            It.IsAny<AuthenticationHeaderValue>(),
            //            It.IsAny<X509Certificate2>(),
            //            null))
            //    .Returns(Task.FromResult(grantedToken));

            var result = await _tokenActions.GetTokenByClientCredentialsGrantType(parameter, null, null, null).ConfigureAwait(false);

            _oauthEventSource.Verify(s => s.StartGetTokenByClientCredentials(scope));
            _oauthEventSource.Verify(s => s.EndGetTokenByClientCredentials(clientId, scope));
            Assert.NotNull(result);
            Assert.True(result.ClientId == clientId);
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _tokenActions.RevokeToken(null, null, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public void When_Revoking_Token_Then_Action_Is_Executed()
        {
            const string accessToken = "access_token";
            InitializeFakeObjects();

            _tokenActions.RevokeToken(new RevokeTokenParameter
            {
                Token = accessToken
            }, null, null, null);

            _oauthEventSource.Verify(s => s.StartRevokeToken(accessToken));
            _oauthEventSource.Verify(s => s.EndRevokeToken(accessToken));
        }

        private void InitializeFakeObjects()
        {
            _getTokenByResourceOwnerCredentialsGrantTypeActionFake = new Mock<IGetTokenByResourceOwnerCredentialsGrantTypeAction>();
            _getTokenByAuthorizationCodeGrantTypeActionFake = new Mock<IGetTokenByAuthorizationCodeGrantTypeAction>();
            _oauthEventSource = new Mock<IOAuthEventSource>();
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
                    It.IsAny<JwsPayload>(),
                    It.IsAny<JwsPayload>()))
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
                _oauthEventSource.Object,
                _revokeTokenActionStub.Object,
                eventPublisher.Object,
                new Mock<ITokenStore>().Object,
                grantedTokenHelperMock.Object);
        }
    }
}
