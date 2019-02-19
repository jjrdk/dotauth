namespace SimpleAuth.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Shared.Models;
    using SimpleAuth.Shared.Repositories;

    internal sealed class InMemoryTicketStore : ITicketStore
    {
        private readonly List<Ticket> _tickets;

        public InMemoryTicketStore()
        {
            _tickets = new List<Ticket>();
        }

        public Task<bool> Add(params Ticket[] tickets)
        {
            _tickets.AddRange(tickets);
            return Task.FromResult(true);
        }

        public Task<bool> Add(Ticket ticket, CancellationToken cancellationToken)
        {
            _tickets.Add(ticket);
            return Task.FromResult(true);
        }

        public Task<Ticket> Get(string ticketId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(ticketId))
            {
                throw new ArgumentNullException(nameof(ticketId));
            }

            return Task.FromResult(_tickets.FirstOrDefault(t => t.Id == ticketId));
        }

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
