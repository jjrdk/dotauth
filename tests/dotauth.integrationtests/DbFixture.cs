namespace DotAuth.IntegrationTests;

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Stores.Marten;
using Marten;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class DbFixture : IDisposable
{
    private readonly DocumentStore _store;

    public DbFixture()
    {
        var connectionString = "Server=odin;Port=5432;Database=dotauth;User Id=dotauth;Password=dotauth;";
        _store = new DocumentStore(
            new DotAuthMartenOptions(
                connectionString,
                new MartenLoggerFacade(NullLogger<MartenLoggerFacade>.Instance)));
    }

    public async Task<ResourceOwner> GetUser()
    {
        var session = _store.LightweightSession();
        await using var _ = session.ConfigureAwait(false);
        {
            var existing = new ResourceOwner
            {
                Subject = "administrator",
                CreateDateTime = DateTime.UtcNow,
                IsLocalAccount = true,
                UpdateDateTime = DateTimeOffset.UtcNow,
                Password = "password".ToSha256Hash(string.Empty)
            };
            existing.Claims = existing.Claims.Concat(
                    new[]
                    {
                        new Claim("scope", "manager"),
                        new Claim("scope", "uma_protection"),
                        new Claim("role", "administrator"),
                    })
                .ToArray();
            session.Store(existing);
            await session.SaveChangesAsync().ConfigureAwait(false);

            return existing;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _store.Dispose();
    }
}
