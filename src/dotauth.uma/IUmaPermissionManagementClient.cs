namespace DotAuth.Uma;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Defines the interface for the permission management client.
/// </summary>
public interface IUmaPermissionManagementClient
{
    /// <summary>
    /// Gets the open permission requests for the current user's resource sets.
    /// </summary>
    /// <param name="subject">The user subject to retrieve requests for.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyList<AccessRequestDescription>> GetOpenRequests(string subject, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves the access request for a specific ticket.
    /// </summary>
    /// <param name="ticketId">The ticket id to approve.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<bool> Approve(string ticketId, CancellationToken cancellationToken = default);
}
