namespace SimpleAuth.IntegrationTests
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Marten;
    using Microsoft.IdentityModel.Tokens;
    using Npgsql;
    using SimpleAuth.Client;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Stores.Marten;
    using Xunit;

    public class DbFixture : IDisposable
    {
        private readonly DocumentStore _store;

        public DbFixture()
        {
            var connectionString = "Server=odin;Port=5432;Database=simpleauth;User Id=simpleauth;Password=simpleauth;";
            _store = new DocumentStore(new SimpleAuthMartenOptions(connectionString, new NulloMartenLogger()));
        }

        public async Task<ResourceOwner> GetUser()
        {
            using var session = _store.OpenSession();
            //var client = await session.LoadAsync<Client>("client").ConfigureAwait(false);
            //if (client == null)
            //{
            //    client = new Client
            //    {
            //        AllowedScopes = new[] { "read", "write" },
            //        ApplicationType = ApplicationTypes.Web,
            //        Claims = new Claim[0],
            //        ClientId = "client",
            //        ClientName = "client",
            //        GrantTypes = GrantTypes.All,
            //        RedirectionUrls = new[] { new Uri("http://localhost"), },
            //        ResponseTypes = ResponseTypeNames.All,
            //        Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = "secret" } },
            //        UserClaimsToIncludeInAuthToken = new Regex[0],
            //        UserInfoSignedResponseAlg = SecurityAlgorithms.RsaSha256,
            //        IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
            //        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost
            //    };

            //    session.Store(client);
            //    await session.SaveChangesAsync().ConfigureAwait(false);
            //}

            //var keys = await session.LoadAsync<JsonWebKey>("5").ConfigureAwait(false);
            //if (keys == null)
            //{
            //    using var rsa = new RSACryptoServiceProvider(2048);
            //    var signatureKey = rsa.CreateSignatureJwk("5", true);
            //    var modelSignatureKey = rsa.CreateSignatureJwk("6", false);

            //    using var rsa2 = new RSACryptoServiceProvider(2048);
            //    var encryptionKey = rsa2.CreateEncryptionJwk("7", true);
            //    var modelEncryptionKey = rsa2.CreateEncryptionJwk("8", false);

            //    session.Store(signatureKey, modelSignatureKey, encryptionKey, modelEncryptionKey);
            //    await session.SaveChangesAsync().ConfigureAwait(false);
            //}

            //const string subject = "user";
            //var existing = await session.LoadAsync<ResourceOwner>(subject).ConfigureAwait(false);
            //if (existing == null)
            {
                var existing = new ResourceOwner
                {
                    Subject = "administrator",
                    CreateDateTime = DateTime.UtcNow,
                    IsLocalAccount = true,
                    UpdateDateTime = DateTimeOffset.UtcNow,
                    Password = "password".ToSha256Hash()
                };
                existing.Claims = existing.Claims.Concat(new[]
                {
                    new Claim("scope", "manager"),
                    new Claim("scope", "uma_protection"),
                    new Claim("role", "administrator"),
                }).ToArray();
                session.Store(existing);
                await session.SaveChangesAsync().ConfigureAwait(false);

                return existing;
            }

            //return existing;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _store.Dispose();
        }
    }

    public class TokenTests : IClassFixture<DbFixture>
    {
        private readonly DbFixture _fixture;

        public TokenTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CanGetToken()
        {
            var client = new TokenClient(
                TokenCredentials.FromClientCredentials("client", "secret"),
                new HttpClient(),
                new Uri("http://localhost:8080/.well-known/openid-configuration"));
            await _fixture.GetUser().ConfigureAwait(false);
            //for (int i = 0; i < 100; i++)
            {
                var token = await client.GetToken(TokenRequest.FromPassword("user", "password", new[] { "read" }))
                    .ConfigureAwait(false);

                Assert.NotNull(token.Content);
            }
        }
    }
}
