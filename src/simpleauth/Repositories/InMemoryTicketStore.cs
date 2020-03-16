namespace SimpleAuth.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Shared.Models;
    using SimpleAuth.Shared.Repositories;

    /// <summary>
    /// Defines the in-memory ticket store.
    /// </summary>
    /// <seealso cref="ITicketStore" />
    public sealed class InMemoryTicketStore : ITicketStore
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private readonly Dictionary<string, Ticket> _tickets;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryTicketStore"/> class.
        /// </summary>
        public InMemoryTicketStore()
        {
            _tickets = new Dictionary<string, Ticket>();
        }

        /// <inheritdoc />
        public async Task<bool> Add(Ticket ticket, CancellationToken cancellationToken)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                _tickets.Add(ticket.Id, ticket);
                return true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <inheritdoc />
        public async Task<Ticket> Get(string ticketId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(ticketId))
            {
                throw new ArgumentNullException(nameof(ticketId));
            }

            try
            {
                await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                _tickets.TryGetValue(ticketId, out var ticket);
                return ticket;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Ticket>> GetAll(string owner, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(owner))
            {
                throw new ArgumentNullException(nameof(owner));
            }

            try
            {
                await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                var now = DateTimeOffset.UtcNow;
                bool Predicate(Ticket x) => x.Created <= now && x.Expires > now;
                return _tickets.Values.Where(Predicate).ToArray();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <inheritdoc />
        public async Task<bool> Remove(string ticketId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(ticketId))
            {
                throw new ArgumentNullException(nameof(ticketId));
            }

            try
            {
                await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                return _tickets.Remove(ticketId);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <inheritdoc />
        public async Task Clean(CancellationToken cancellationToken)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                var toRemove = _tickets
                    .Where(x => x.Value.Expires <= DateTimeOffset.UtcNow)
                    .Select(x => x.Key);
                foreach (var id in toRemove)
                {
                    _tickets.Remove(id);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
