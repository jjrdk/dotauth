namespace SimpleAuth.Stores.Redis
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;
    using StackExchange.Redis;

    public class RedisTicketStore : ITicketStore
    {
        private readonly IDatabaseAsync _database;
        private readonly TimeSpan _expiry;

        public RedisTicketStore(IDatabaseAsync database, TimeSpan expiry = default)
        {
            _database = database;
            _expiry = expiry == default ? TimeSpan.FromMinutes(30) : expiry;
        }

        public Task<bool> Add(Ticket ticket, CancellationToken cancellationToken)
        {
            var json = JsonConvert.SerializeObject(ticket);
            return _database.StringSetAsync(ticket.Id, json, _expiry);
        }

        public Task<bool> Remove(string ticketId, CancellationToken cancellationToken)
        {
            return _database.KeyDeleteAsync(ticketId);
        }

        public async Task<Ticket> Get(string ticketId, CancellationToken cancellationToken)
        {
            var consent = await _database.StringGetAsync(ticketId).ConfigureAwait(false);
            return consent.HasValue
                ? JsonConvert.DeserializeObject<Ticket>(consent)
                : null;
        }

        /// <inheritdoc />
        public Task<IReadOnlyList<Ticket>> GetAll(string owner, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task Clean(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}