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
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class TokenActionsFixture
    {
        private const string ClientId = "valid_client_id";
        private const string Clientsecret = "secret";
        private readonly TokenActions _tokenActions;

        public TokenActionsFixture()
        {
            var eventPublisher = new Mock<IEventPublisher>();
            const string scope = "valid_scope";
            var mock = new Mock<IClientStore>();
            mock.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new Client
                    {
                        JsonWebKeys =
                            "supersecretlongkey".CreateJwk(
                                    JsonWebKeyUseNames.Sig,
                                    KeyOperations.Sign,
                                    KeyOperations.Verify)
                                .ToSet(),
                        IdTokenSignedResponseAlg = SecurityAlgorithms.HmacSha256,
                        ClientId = ClientId,
                        Secrets = {new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = Clientsecret}},
                        AllowedScopes = new[] {new Scope {Name = scope}},
                        ResponseTypes = new[] {ResponseTypeNames.Token},
                        GrantTypes = new[] {GrantTypes.ClientCredentials}
                    });

            _tokenActions = new TokenActions(
                new RuntimeSettings(),
                new Mock<IAuthorizationCodeStore>().Object,
                mock.Object,
                new Mock<IScopeRepository>().Object,
                new IAuthenticateResourceOwnerService[0],
                eventPublisher.Object,
                new Mock<ITokenStore>().Object);
        }

        [Fact]
        public async Task When_Passing_No_Request_To_ResourceOwner_Grant_Type_Then_Exception_Is_Thrown()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _tokenActions.GetTokenByResourceOwnerCredentialsGrantType(
                        null,
                        null,
                        null,
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_No_Request_To_AuthorizationCode_Grant_Type_Then_Exception_Is_Thrown()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _tokenActions.GetTokenByAuthorizationCodeGrantType(
                        null,
                        null,
                        null,
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_No_Request_To_Refresh_Token_Grant_Type_Then_Exception_Is_Thrown()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _tokenActions.GetTokenByRefreshTokenGrantType(null, null, null, null, CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_To_ClientCredentials_GrantType_Then_Exception_Is_Thrown()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _tokenActions.GetTokenByClientCredentialsGrantType(
                        null,
                        null,
                        null,
                        null,
                        CancellationToken.None))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Getting_Token_Via_ClientCredentials_GrantType_Then_GrantedToken_Is_Returned()
        {
            const string scope = "valid_scope";
            const string clientId = "valid_client_id";
            var parameter = new ClientCredentialsGrantTypeParameter {Scope = scope};

            var authenticationHeader = new AuthenticationHeaderValue(
                "Basic",
                $"{clientId}:{Clientsecret}".Base64Encode());
            var result = await _tokenActions.GetTokenByClientCredentialsGrantType(
                    parameter,
                    authenticationHeader,
                    null,
                    null,
                    CancellationToken.None)
                .ConfigureAwait(false);

            Assert.Equal(clientId, result.ClientId);
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            await Assert
                .ThrowsAsync<ArgumentNullException>(
                    () => _tokenActions.RevokeToken(null, null, null, null, CancellationToken.None))
                .ConfigureAwait(false);
        }
    }
}
