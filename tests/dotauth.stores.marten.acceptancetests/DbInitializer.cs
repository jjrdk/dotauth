#nullable enable
namespace DotAuth.Stores.Marten.AcceptanceTests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Models;
using DotAuth.Stores.Marten;
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
        IEnumerable<Consent> consents,
        IEnumerable<ResourceOwner> users,
        IEnumerable<Client> clients,
        IEnumerable<Scope> scopes)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
        }

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

            var schema = $"test_{DateTimeOffset.UtcNow.Ticks}";
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"CREATE SCHEMA {schema} AUTHORIZATION dotauth; ";
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

            var builder = new NpgsqlConnectionStringBuilder(connectionString)
            {
                Timezone = "UTC",
                SearchPath = schema
            };

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
        using var store = new DocumentStore(
            new DotAuthMartenOptions(
                connectionString,
                new MartenLoggerFacade(NullLogger<MartenLoggerFacade>.Instance),
                searchPath));
        var session = store.LightweightSession("test");
        await using var _ = session.ConfigureAwait(false);
        if (users != null) session.Store(users.ToArray());
        if (consents != null) session.Store(consents.ToArray());
        if (clients != null) session.Store(clients.ToArray());
        if (scopes != null) session.Store(scopes.ToArray());
        await session.SaveChangesAsync().ConfigureAwait(false);
    }

    public static async Task Drop(string connectionString, ITestOutputHelper outputHelper)
    {
        try
        {
            var connection = new NpgsqlConnection(connectionString);
            NpgsqlConnection.ClearPool(connection);
            await using var _ = connection.ConfigureAwait(false);
            await connection.OpenAsync().ConfigureAwait(false);
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"DROP SCHEMA {builder.SearchPath} CASCADE;";

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
        catch(Exception exception)
        {
            outputHelper.WriteLine(exception.Message);
        }
    }
}
#nullable disable