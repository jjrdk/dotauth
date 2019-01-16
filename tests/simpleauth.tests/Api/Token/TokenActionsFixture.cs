namespace SimpleAuth.Tests.Api.Token
{
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Models;
    using SimpleAuth;
    using SimpleAuth.Api.Token;
    using SimpleAuth.Authenticate;
    using SimpleAuth.Helpers;
    using SimpleAuth.JwtToken;
    using SimpleAuth.Validators;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class TokenActionsFixture
    {
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
            var eventPublisher = new Mock<IEventPublisher>();
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
                new OAuthConfigurationOptions(),
                new Mock<IClientCredentialsGrantTypeParameterValidator>().Object,
                new Mock<IAuthorizationCodeStore>().Object,
                mock.Object,
                new Mock<IResourceOwnerAuthenticateHelper>().Object,
                new Mock<IGrantedTokenGeneratorHelper>().Object,
                eventPublisher.Object,
                new Mock<ITokenStore>().Object,
                new Mock<IJwtGenerator>().Object,
                new Mock<IClientHelper>().Object,
                grantedTokenHelperMock.Object);
        }
    }
}
