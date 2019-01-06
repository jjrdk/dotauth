namespace SimpleAuth.Uma.Tests
{
    using Client.Configuration;
    using Client.Permission;
    using Client.ResourceSet;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;
    using Shared.DTOs;
    using SimpleAuth.Client;
    using SimpleAuth.Client.Operations;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class TokenFixture : IDisposable
    {
        private const string BaseUrl = "http://localhost:5000";
        private ResourceSetClient _resourceSetClient;
        private PermissionClient _permissionClient;
        private readonly TestUmaServerFixture _server;

        public TokenFixture()
        {
            IdentityModelEventSource.ShowPII = true;
            _server = new TestUmaServerFixture();
        }

        [Fact]
        public async Task When_Ticket_Id_Does_Not_Exist_Then_Error_Is_Returned()
        {
            InitializeFakeObjects();

            var token = await new TokenClient(
                    TokenCredentials.FromClientCredentials("resource_server", "resource_server"),
                    TokenRequest.FromTicketId("ticket_id", ""),
                    _server.Client,
                    new GetDiscoveryOperation(_server
                        .Client)) // Try to get the access token via "ticket_id" grant-type.
                .ResolveAsync(BaseUrl + "/.well-known/uma2-configuration")
                .ConfigureAwait(false);

            Assert.True(token.ContainsError);
            Assert.Equal("invalid_ticket", token.Error.Error);
            Assert.Equal("the ticket ticket_id doesn't exist", token.Error.ErrorDescription);
        }

        [Fact]
        public async Task When_Using_ClientCredentials_Grant_Type_Then_AccessToken_Is_Returned()
        {
            InitializeFakeObjects();

            var result = await new TokenClient(
                    TokenCredentials.FromClientCredentials("resource_server", "resource_server"),
                    TokenRequest.FromScopes("uma_protection", "uma_authorization"),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(BaseUrl + "/.well-known/uma2-configuration")
                .ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.NotEmpty(result.Content.AccessToken);
        }

        [Fact]
        public async Task When_Using_TicketId_Grant_Type_Then_AccessToken_Is_Returned()
        {
            InitializeFakeObjects();

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
            set.Keys.Add(_server.SharedCtx.SignatureKey);
            var header =
                new JwtHeader(new SigningCredentials(set.GetSignKeys().First(), SecurityAlgorithms.HmacSha256));
            var securityToken = new JwtSecurityToken(header, jwsPayload);
            var jwt = handler.WriteToken(securityToken);
            //_jwsGenerator.Generate(jwsPayload, SecurityAlgorithms.RsaSha256, _server.SharedCtx.SignatureKey);

            var result = await new TokenClient(
                    TokenCredentials.FromClientCredentials("resource_server", "resource_server"), // Get PAT.
                    TokenRequest.FromScopes("uma_protection", "uma_authorization"),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(BaseUrl + "/.well-known/uma2-configuration")
                .ConfigureAwait(false);
            var resource = await _resourceSetClient.AddByResolution(new PostResourceSet // Add ressource.
            {
                Name = "name",
                Scopes = new List<string>
                        {
                            "read",
                            "write",
                            "execute"
                        }
            },
                    BaseUrl + "/.well-known/uma2-configuration",
                    result.Content.AccessToken)
                .ConfigureAwait(false);
            //var addPolicy = await _policyClient.AddByResolution(new PostPolicy // Add an authorization policy.
            //{
            //    Rules = new List<PostPolicyRule>
            //            {
            //                new PostPolicyRule
            //                {
            //                    IsResourceOwnerConsentNeeded = false,
            //                    Scopes = new List<string>
            //                    {
            //                        "read"
            //                    },
            //                    ClientIdsAllowed = new List<string>
            //                    {
            //                        "resource_server"
            //                    },
            //                    Claims = new List<PostClaim>
            //                    {
            //                        new PostClaim {Type = "sub", Value = "248289761001"}
            //                    }
            //                }
            //            },
            //    ResourceSetIds = new List<string>
            //            {
            //                resource.Content.Id
            //            }
            //},
            //        BaseUrl + "/.well-known/uma2-configuration",
            //        result.Content.AccessToken)
            //    .ConfigureAwait(false);
            var ticket = await _permissionClient.AddByResolution(
                    new PostPermission // Add permission & retrieve a ticket id.
                    {
                        ResourceSetId = resource.Content.Id,
                        Scopes = new List<string>
                        {
                            "read"
                        }
                    },
                    BaseUrl + "/.well-known/uma2-configuration",
                    "header")
                .ConfigureAwait(false);

            Assert.NotNull(ticket.Content);

            var token = await new TokenClient(
                    TokenCredentials.FromClientCredentials("resource_server",
                        "resource_server"), // Try to get the access token via "ticket_id" grant-type.
                    TokenRequest.FromTicketId(ticket.Content.TicketId, jwt),
                    _server.Client,
                    new GetDiscoveryOperation(_server.Client))
                .ResolveAsync(BaseUrl + "/.well-known/uma2-configuration")
                .ConfigureAwait(false);

            Assert.NotNull(token);
        }

        private void InitializeFakeObjects()
        {
            _resourceSetClient = new ResourceSetClient(_server.Client, new GetConfigurationOperation(_server.Client));
            _permissionClient = new PermissionClient(_server.Client, new GetConfigurationOperation(_server.Client));
        }

        public void Dispose()
        {
            _server?.Dispose();
        }
    }
}
