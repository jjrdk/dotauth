namespace SimpleAuth.IntegrationTests
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Marten;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Stores.Marten;

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
            {
                var existing = new ResourceOwner
                {
                    Subject = "administrator",
                    CreateDateTime = DateTime.UtcNow,
                    IsLocalAccount = true,
                    UpdateDateTime = DateTimeOffset.UtcNow,
                    Password = "password".ToSha256Hash(string.Empty)
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
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _store.Dispose();
        }
    }
}
