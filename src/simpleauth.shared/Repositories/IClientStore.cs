namespace SimpleAuth.Shared.Repositories;

using System.Threading;
using System.Threading.Tasks;
using Models;

/// <summary>
/// Defines the client store interface.
/// </summary>
public interface IClientStore
{
    /// <summary>
    /// Gets the client by identifier.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<Client?> GetById(string clientId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all clients.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<Client[]> GetAll(CancellationToken cancellationToken);
}