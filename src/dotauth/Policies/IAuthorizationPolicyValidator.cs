namespace DotAuth.Policies;

using System.Threading;
using System.Threading.Tasks;
using DotAuth.Parameters;
using DotAuth.Shared.Models;
using DotAuth.Shared.Policies;

/// <summary>
/// Defines the authorization policy validator interface.
/// </summary>
public interface IAuthorizationPolicyValidator
{
    /// <summary>
    /// Gets whether the request is authorized.
    /// </summary>
    /// <param name="validTicket">The <see cref="Ticket"/> to validate against.</param>
    /// <param name="client">The requesting <see cref="Client"/>.</param>
    /// <param name="claimTokenParameter">The claim parameter.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async request.</param>
    /// <returns></returns>
    Task<AuthorizationPolicyResult> IsAuthorized(
        Ticket validTicket,
        Client client,
        ClaimTokenParameter claimTokenParameter,
        CancellationToken cancellationToken);
}
