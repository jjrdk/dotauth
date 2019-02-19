namespace SimpleAuth.Stores.Marten.AcceptanceTests.Features
{
    using global::Marten;
    using Npgsql;
    using SimpleAuth.Shared.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public static class DbInitializer
    {
        public static async Task<string> Init(
            string connectionString,
            IEnumerable<Consent> consents,
            IEnumerable<ResourceOwner> users,
            IEnumerable<Client> clients)
        {
            var builder = new NpgsqlConnectionStringBuilder { ConnectionString = connectionString };
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                var schema = $"satest_{DateTime.UtcNow.Ticks}";
                var cmd = connection.CreateCommand();
                cmd.CommandText = $"CREATE SCHEMA {schema} AUTHORIZATION ithemba; ";
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                builder.SearchPath = schema;

                await Seed(builder.ConnectionString, schema, consents, users, clients).ConfigureAwait(false);
                return builder.ConnectionString;
            }
        }

        private static async Task Seed(
            string connectionString,
            string searchPath,
            IEnumerable<Consent> consents,
            IEnumerable<ResourceOwner> users,
            IEnumerable<Client> clients)
        {
            using (var store = new DocumentStore(new SimpleAuthMartenOptions(connectionString, searchPath)))
            {
                using (var session = store.LightweightSession())
                {
                    session.Store(consents.ToArray());
                    session.Store(users.ToArray());
                    session.Store(clients.ToArray());
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