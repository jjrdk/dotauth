namespace DotAuth.Shared.Policies;

using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Models;

/// <summary>
/// Defines the authorization policy to apply to resource requests.
/// </summary>
public interface IAuthorizationPolicy
{
    /// <summary>
    /// Executes the authorization policy on the passed resource.
    /// </summary>
    /// <param name="ticket"></param>
    /// <param name="claimTokenFormat"></param>
    /// <param name="requester"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="policy"></param>
    /// <returns></returns>
    Task<AuthorizationPolicyResult> Execute(
        TicketLineParameter ticket,
        string? claimTokenFormat,
        ClaimsPrincipal requester,
        CancellationToken cancellationToken,
        params PolicyRule[] policy);
}