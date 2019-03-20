namespace SimpleAuth.Stores.Marten.AcceptanceTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Marten;
    using Npgsql;
    using SimpleAuth.Shared.Models;

    public static class DbInitializer
    {
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1);

        public static async Task<string> Init(
            string connectionString,
            IEnumerable<Consent> consents = null,
            IEnumerable<ResourceOwner> users = null,
            IEnumerable<Client> clients = null,
            IEnumerable<Scope> scopes = null)
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            using (var connection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    await Semaphore.WaitAsync().ConfigureAwait(false);

                    await connection.OpenAsync().ConfigureAwait(false);
                    var schema = $"test_{DateTime.UtcNow.Ticks.ToString()}";
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = $"CREATE SCHEMA {schema} AUTHORIZATION ithemba; ";
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
        }

        private static async Task Seed(
            string connectionString,
            string searchPath,
            IEnumerable<Consent> consents,
            IEnumerable<ResourceOwner> users,
            IEnumerable<Client> clients,
            IEnumerable<Scope> scopes)
        {
            using (var store = new DocumentStore(new SimpleAuthMartenOptions(connectionString, searchPath)))
            {
                using (var session = store.LightweightSession())
                {
                    if (consents != null) session.Store(consents.ToArray());
                    if (users != null) session.Store(users.ToArray());
                    if (clients != null) session.Store(clients.ToArray());
                    if (scopes != null) session.Store(scopes.ToArray());
                    await session.SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public static async Task Drop(string connectionString)
        {
            NpgsqlConnection.ClearAllPools();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                var builder = new NpgsqlConnectionStringBuilder { ConnectionString = connectionString };
                var cmd = connection.CreateCommand();
                cmd.CommandText = $"DROP SCHEMA {builder.SearchPath} CASCADE;";
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }
    }
}