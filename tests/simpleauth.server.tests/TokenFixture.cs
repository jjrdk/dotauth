namespace SimpleAuth.Server.Tests
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Client;
    using SimpleAuth.Shared.DTOs;
    using Xunit;

    public class TokenFixture : IDisposable
    {
        private const string BaseUrl = "http://localhost:5000";
        private const string WellKnownUma2Configuration = "/.well-known/uma2-configuration";
        private readonly UmaClient _umaClient;
        private readonly TestUmaServerFixture _server;

        public TokenFixture()
        {
            IdentityModelEventSource.ShowPII = true;
            _server = new TestUmaServerFixture();
            _umaClient = new UmaClient(_server.Client, new Uri(BaseUrl + WellKnownUma2Configuration));
        }

        [Fact]
        public async Task When_Ticket_Id_Does_Not_Exist_Then_Error_Is_Returned()
        {
            var tokenClient = new TokenClient(
                TokenCredentials.FromClientCredentials("resource_server", "resource_server"),
                _server.Client,
                new Uri(BaseUrl + WellKnownUma2Configuration));
            // Try to get the access token via "ticket_id" grant-type.
            var token = await tokenClient.GetToken(TokenRequest.FromTicketId("ticket_id", "")).ConfigureAwait(false);

            Assert.True(token.HasError);
            Assert.Equal("invalid_ticket", token.Error.Title);
            Assert.Equal("the ticket ticket_id doesn't exist", token.Error.Detail);
        }

        [Fact]
        public async Task When_Using_ClientCredentials_Grant_Type_Then_AccessToken_Is_Returned()
        {
            var tokenClient = new TokenClient(
                TokenCredentials.FromClientCredentials("resource_server", "resource_server"),
                _server.Client,
                new Uri(BaseUrl + WellKnownUma2Configuration));
            var result = await tokenClient.GetToken(TokenRequest.FromScopes("uma_protection", "uma_authorization"))
                .ConfigureAwait(false);

            Assert.NotEmpty(result.Content.AccessToken);
        }

        [Fact]
        public async Task When_Using_TicketId_Grant_Type_Then_AccessToken_Is_Returned()
        {
            var jwsPayload = new JwtPayload
            {
                {"iss", "http://server.example.com"},
                {"sub", "248289761001"},
                {"aud", "s6BhdRkqt3"},
                {"nonce", "n-0S6_WzA2Mj"},
                {"exp", "1311281970"},
                {"iat", "1311280970"}
            };
            var handler = new JwtSecurityTokenHandler();
            var set = new JsonWebKeySet();
            set.Keys.Add(_server.SharedUmaCtx.SignatureKey);
            var header = new JwtHeader(
                new SigningCredentials(set.GetSignKeys().First(), SecurityAlgorithms.HmacSha256));
            var securityToken = new JwtSecurityToken(header, jwsPayload);
            var jwt = handler.WriteToken(securityToken);

            var tc = new TokenClient(
                TokenCredentials.FromClientCredentials("resource_server", "resource_server"),
                _server.Client,
                new Uri(BaseUrl + WellKnownUma2Configuration));
            // Get PAT.
            var result = await tc.GetToken(TokenRequest.FromScopes("uma_protection", "uma_authorization"))
                .ConfigureAwait(false);
            var resource = await _umaClient.AddResource(
                    new ResourceSet {Name = "name", Scopes = new[] {"read", "write", "execute"}},
                    result.Content.AccessToken)
                .ConfigureAwait(false);

            var ticket = await _umaClient.RequestPermission(
                    new PermissionRequest // Add permission & retrieve a ticket id.
                    {
                        ResourceSetId = resource.Content.Id, Scopes = new[] {"read"}
                    },
                    "header")
                .ConfigureAwait(false);

            Assert.NotNull(ticket.Content);

            var tokenClient = new TokenClient(
                TokenCredentials.FromClientCredentials("resource_server", "resource_server"),
                _server.Client,
                new Uri(BaseUrl + WellKnownUma2Configuration));
            var token = await tokenClient.GetToken(TokenRequest.FromTicketId(ticket.Content.TicketId, jwt))
                .ConfigureAwait(false);

            var jwtToken = handler.ReadJwtToken(token.Content.AccessToken);
            Assert.NotNull(jwtToken.Claims);
        }

        public void Dispose()
        {
            _server?.Dispose();
        }
    }
}
