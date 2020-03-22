namespace SimpleAuth.Shared.Repositories
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the ticket store interface.
    /// </summary>
    public interface ITicketStore : ICleanable
    {
        /// <summary>
        /// Adds the specified ticket.
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<bool> Add(Ticket ticket, CancellationToken cancellationToken = default);

        /// <summary>
        /// Approves the access request.
        /// </summary>
        /// <param name="ticketId">The ticket to approve.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        Task<bool> ApproveAccess(string ticketId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes the specified ticket identifier.
        /// </summary>
        /// <param name="ticketId">The ticket identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<bool> Remove(string ticketId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the specified ticket identifier.
        /// </summary>
        /// <param name="ticketId">The ticket identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<Ticket> Get(string ticketId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all tickets for the owner.
        /// </summary>
        /// <param name="owner">The owner of the tickets.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns>All tickets as a <see cref="Task{TResult}"/>.</returns>
        Task<IReadOnlyList<Ticket>> GetAll(string owner, CancellationToken cancellationToken = default);
    }
}
