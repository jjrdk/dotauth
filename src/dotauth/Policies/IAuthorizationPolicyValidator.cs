namespace DotAuth.Policies;

using System.Threading;
using System.Threading.Tasks;
using DotAuth.Parameters;
using DotAuth.Shared.Models;

/// <summary>
/// Defines the authorization policy validator interface.
/// </summary>
public interface IAuthorizationPolicyValidator
{
    /// <summary>
    /// Gets whether the request is authorized.
    /// </summary>
    /// <param name="validTicket"></param>
    /// <param name="client"></param>
    /// <param name="claimTokenParameter"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<AuthorizationPolicyResult> IsAuthorized(
        Ticket validTicket,
        Client client,
        ClaimTokenParameter claimTokenParameter,
        CancellationToken cancellationToken);
}