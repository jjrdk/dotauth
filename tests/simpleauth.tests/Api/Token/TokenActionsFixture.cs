using SimpleAuth.Services;
using SimpleAuth.Shared.Repositories;
using System.Net.Http.Headers;

namespace SimpleAuth.Tests.Api.Token
{
    using Microsoft.IdentityModel.Tokens;
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Models;
    using SimpleAuth;
    using SimpleAuth.Api.Token;
    using SimpleAuth.JwtToken;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class TokenActionsFixture
    {
        private const string clientId = "valid_client_id";
        private const string clientsecret = "secret";
        private ITokenActions _tokenActions;
        private Mock<ITokenStore> _tokenStore;

        public TokenActionsFixture()
        {
            InitializeFakeObjects();
        }

        [Fact]
        public async Task When_Passing_No_Request_To_ResourceOwner_Grant_Type_Then_Exception_Is_Thrown()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _tokenActions.GetTokenByResourceOwnerCredentialsGrantType(null, null, null, null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_No_Request_To_AuthorizationCode_Grant_Type_Then_Exception_Is_Thrown()
        {
            await Assert
                .ThrowsAsync<ArgumentNullException>(
                    () => _tokenActions.GetTokenByAuthorizationCodeGrantType(null, null, null, null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_No_Request_To_Refresh_Token_Grant_Type_Then_Exception_Is_Thrown()
        {
            await Assert
                .ThrowsAsync<ArgumentNullException>(
                    () => _tokenActions.GetTokenByRefreshTokenGrantType(null, null, null, null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_To_ClientCredentials_GrantType_Then_Exception_Is_Thrown()
        {
            await Assert
                .ThrowsAsync<ArgumentNullException>(
                    () => _tokenActions.GetTokenByClientCredentialsGrantType(null, null, null, null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Getting_Token_Via_ClientCredentials_GrantType_Then_GrantedToken_Is_Returned()
        {
            const string scope = "valid_scope";
            const string clientId = "valid_client_id";
            var parameter = new ClientCredentialsGrantTypeParameter { Scope = scope };

            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientsecret}".Base64Encode());
            var result = await _tokenActions
                .GetTokenByClientCredentialsGrantType(parameter, authenticationHeader, null, null)
                .ConfigureAwait(false);

            Assert.Equal(clientId, result.ClientId);
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _tokenActions.RevokeToken(null, null, null, null))
                .ConfigureAwait(false);
        }

        private void InitializeFakeObjects()
        {
            var eventPublisher = new Mock<IEventPublisher>();
            const string scope = "valid_scope";
            var mock = new Mock<IClientStore>();
            mock.Setup(x => x.GetById(It.IsAny<string>()))
                .ReturnsAsync(
                    new Client
                    {
                        JsonWebKeys = "supersecretlongkey".CreateJwk(JsonWebKeyUseNames.Sig, KeyOperations.Sign, KeyOperations.Verify).ToSet(),
                        IdTokenSignedResponseAlg = SecurityAlgorithms.HmacSha256,
                        ClientId = clientId,
                        Secrets = { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientsecret } },
                        AllowedScopes = new[] { new Scope { Name = scope } },
                        ResponseTypes = new[] { ResponseTypeNames.Token },
                        GrantTypes = new List<GrantType> { GrantType.client_credentials }
                    });

            _tokenStore = new Mock<ITokenStore>();
            //var grantedTokenGenerator = new Mock<IGrantedTokenGeneratorHelper>();
            //grantedTokenGenerator
            //    .Setup(
            //        x => x.GenerateToken(It.IsAny<Client>(), It.IsAny<string>(), It.IsAny<string>(), null, null, null))
            //    .ReturnsAsync(new GrantedToken { ClientId = clientId });
            _tokenActions = new TokenActions(
                new OAuthConfigurationOptions(),
                new Mock<IAuthorizationCodeStore>().Object,
                mock.Object,
                new IAuthenticateResourceOwnerService[0],
                eventPublisher.Object,
                _tokenStore.Object,
                new Mock<IJwtGenerator>().Object);
        }
    }
}
