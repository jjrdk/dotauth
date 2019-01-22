namespace SimpleAuth.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Shared.Models;

    internal sealed class DefaultTicketStore : ITicketStore
    {
        public List<Ticket> _tickets;

        public DefaultTicketStore()
        {
            _tickets = new List<Ticket>();
        }

        public Task<bool> Add(IEnumerable<Ticket> tickets)
        {
            if (tickets == null)
            {
                throw new ArgumentNullException(nameof(tickets));
            }

            _tickets.AddRange(tickets);
            return Task.FromResult(true);
        }

        public Task<bool> Add(Ticket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException(nameof(ticket));
            }

            _tickets.Add(ticket);
            return Task.FromResult(true);
        }

        public Task<Ticket> Get(string ticketId)
        {
            if (string.IsNullOrWhiteSpace(ticketId))
            {
                throw new ArgumentNullException(nameof(ticketId));
            }

            return Task.FromResult(_tickets.FirstOrDefault(t => t.Id == ticketId));
        }

        public Task<bool> Remove(string ticketId)
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
