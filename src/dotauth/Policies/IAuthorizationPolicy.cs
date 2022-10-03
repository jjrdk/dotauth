namespace DotAuth.Policies;

using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Models;

internal interface IAuthorizationPolicy
{
    Task<AuthorizationPolicyResult> Execute(
        TicketLineParameter ticket,
        string? claimTokenFormat,
        ClaimsPrincipal requester,
        CancellationToken cancellationToken,
        params PolicyRule[] policy);
}