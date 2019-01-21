using System.Net.Http.Headers;
using SimpleAuth.Shared.Repositories;

namespace SimpleAuth.Tests.Api.Token
{
    using Moq;
    using Parameters;
    using Shared;
    using Shared.Models;
    using SimpleAuth;
    using SimpleAuth.Api.Token;
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
        const string clientId = "valid_client_id";
        const string clientsecret = "secret";
        private ITokenActions _tokenActions;

        public TokenActionsFixture()
        {
            InitializeFakeObjects();
        }

        [Fact]
        public async Task When_Passing_No_Request_To_ResourceOwner_Grant_Type_Then_Exception_Is_Thrown()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _tokenActions.GetTokenByResourceOwnerCredentialsGrantType(null, null, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_No_Request_To_AuthorizationCode_Grant_Type_Then_Exception_Is_Thrown()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _tokenActions.GetTokenByAuthorizationCodeGrantType(null, null, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_No_Request_To_Refresh_Token_Grant_Type_Then_Exception_Is_Thrown()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _tokenActions.GetTokenByRefreshTokenGrantType(null, null, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_To_ClientCredentials_GrantType_Then_Exception_Is_Thrown()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _tokenActions.GetTokenByClientCredentialsGrantType(null, null, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Getting_Token_Via_ClientCredentials_GrantType_Then_GrantedToken_Is_Returned()
        {
            const string scope = "valid_scope";
            const string clientId = "valid_client_id";
            var parameter = new ClientCredentialsGrantTypeParameter
            {
                Scope = scope
            };

            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{clientsecret}".Base64Encode());
            var result = await _tokenActions.GetTokenByClientCredentialsGrantType(parameter, authenticationHeader, null, null).ConfigureAwait(false);

            Assert.Equal(clientId, result.ClientId);
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _tokenActions.RevokeToken(null, null, null, null)).ConfigureAwait(false);
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
                        ClientId = clientId,
                        Secrets = {new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientsecret} },
                        AllowedScopes = new[] {new Scope {Name = scope}},
                        ResponseTypes = new[] {ResponseTypeNames.Token},
                        GrantTypes = new List<GrantType> {GrantType.client_credentials}
                    });
            //mock.Setup(x => x.AuthenticateAsync(It.IsAny<AuthenticateInstruction>(), It.IsAny<string>()))
            //    .ReturnsAsync<AuthenticateInstruction, string, IAuthenticateClient, AuthenticationResult>((a, s) =>
            //        new AuthenticationResult(
            //
            //            null));

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
                grantedTokenHelperMock.Object);
        }
    }
}
