namespace DotAuth.Stores.Marten.AcceptanceTests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Models;
using DotAuth.Stores.Marten;
using DotAuth.Stores.Marten.Containers;
using global::Marten;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Xunit.Abstractions;

public static class DbInitializer
{
    private static readonly SemaphoreSlim Semaphore = new(1);

    public static async Task<string> Init(
        ITestOutputHelper output,
        string connectionString,
        IEnumerable<Consent>? consents = null,
        IEnumerable<ResourceOwner>? users = null,
        IEnumerable<Client>? clients = null,
        IEnumerable<Scope>? scopes = null)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var connection = new NpgsqlConnection(connectionString);
        await using var _ = connection.ConfigureAwait(false);
        try
        {
            await Semaphore.WaitAsync().ConfigureAwait(false);
            for (var i = 1; i <= 10; i++)
            {
                try
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                    break;
                }
                catch (Exception e)
                {
                    output.WriteLine($"Failed to open connection {i} times");
                    output.WriteLine(e.Message);
                    await Task.Delay(TimeSpan.FromMilliseconds(i * 250)).ConfigureAwait(false);
                }
            }

            var schema = $"test_{DateTime.UtcNow.Ticks}";
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"CREATE SCHEMA {schema} AUTHORIZATION dotauth; ";
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            builder.SearchPath = schema;

            await Seed(builder.ConnectionString, schema, consents, users, clients, scopes).ConfigureAwait(false);
            return builder.ConnectionString;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    private static async Task Seed(
        string connectionString,
        string searchPath,
        IEnumerable<Consent>? consents,
        IEnumerable<ResourceOwner>? users,
        IEnumerable<Client>? clients,
        IEnumerable<Scope>? scopes)
    {
        var options = new DotAuthMartenOptions(
            connectionString,
            new MartenLoggerFacade(NullLogger<MartenLoggerFacade>.Instance),
            searchPath);
        await using var store = new DocumentStore(options);
        await using var session = store.LightweightSession();
        await using var _ = session.ConfigureAwait(false);
        if (consents != null) session.Store(consents.ToArray());
        if (users != null) session.Store(users.ToArray());
        if (clients != null) session.Store(clients.ToArray());
        if (scopes != null) session.Store(scopes.Select(ScopeContainer.Create).ToArray());
        await session.SaveChangesAsync().ConfigureAwait(false);
    }

    public static async Task Drop(string connectionString)
    {
        NpgsqlConnection.ClearAllPools();
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync().ConfigureAwait(false);
        var builder = new NpgsqlConnectionStringBuilder { ConnectionString = connectionString };
        var cmd = connection.CreateCommand();
        cmd.CommandText = $"DROP SCHEMA {builder.SearchPath} CASCADE;";
        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
    }
}
