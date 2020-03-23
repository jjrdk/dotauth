namespace SimpleAuth.Policies
{
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Parameters;
    using Shared.Models;

    internal interface IAuthorizationPolicy
    {
        Task<AuthorizationPolicyResult> Execute(
            TicketLineParameter ticket,
            string claimTokenFormat,
            Claim[] claims,
            CancellationToken cancellationToken,
            params PolicyRule[] policy);
    }
}
