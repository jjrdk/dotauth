namespace SimpleAuth.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Shared.Models;

    public interface ITicketStore
    {
        Task<bool> Add(IEnumerable<Ticket> tickets);
        Task<bool> Add(Ticket ticket);
        Task<bool> Remove(string ticketId);
        Task<Ticket> Get(string ticketId);
    }
}
