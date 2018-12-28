namespace SimpleAuth.Uma.Stores
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;

    public interface ITicketStore
    {
        Task<bool> AddAsync(IEnumerable<Ticket> tickets);
        Task<bool> AddAsync(Ticket ticket);
        Task<bool> RemoveAsync(string ticketId);
        Task<Ticket> GetAsync(string ticketId);
    }
}
