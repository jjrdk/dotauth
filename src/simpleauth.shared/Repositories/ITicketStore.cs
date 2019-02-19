namespace SimpleAuth.Shared.Repositories
{
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the ticket store interface.
    /// </summary>
    public interface ITicketStore
    {
        /// <summary>
        /// Adds the specified ticket.
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<bool> Add(Ticket ticket, CancellationToken cancellationToken);

        /// <summary>
        /// Removes the specified ticket identifier.
        /// </summary>
        /// <param name="ticketId">The ticket identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<bool> Remove(string ticketId, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the specified ticket identifier.
        /// </summary>
        /// <param name="ticketId">The ticket identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<Ticket> Get(string ticketId, CancellationToken cancellationToken);
    }
}
