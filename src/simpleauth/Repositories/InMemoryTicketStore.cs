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
    /// <seealso cref="SimpleAuth.Shared.Repositories.ITicketStore" />
    public sealed class InMemoryTicketStore : ITicketStore
    {
        private readonly List<Ticket> _tickets;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryTicketStore"/> class.
        /// </summary>
        public InMemoryTicketStore()
        {
            _tickets = new List<Ticket>();
        }

        /// <inheritdoc />
        public Task<bool> Add(Ticket ticket, CancellationToken cancellationToken)
        {
            _tickets.Add(ticket);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<Ticket> Get(string ticketId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(ticketId))
            {
                throw new ArgumentNullException(nameof(ticketId));
            }

            return Task.FromResult(_tickets.FirstOrDefault(t => t.Id == ticketId));
        }

        /// <inheritdoc />
        public Task<bool> Remove(string ticketId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(ticketId))
            {
                throw new ArgumentNullException(nameof(ticketId));
            }

            var ticket = _tickets.FirstOrDefault(t => t.Id == ticketId);
            if (ticket == null)
            {
                return Task.FromResult(false);
            }

            _tickets.Remove(ticket);
            return Task.FromResult(true);
        }
    }
}
