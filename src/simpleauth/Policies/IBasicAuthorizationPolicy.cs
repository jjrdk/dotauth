namespace SimpleAuth.Policies
{
    using System.Threading;
    using System.Threading.Tasks;
    using Parameters;
    using Shared.Models;

    internal interface IBasicAuthorizationPolicy
    {
        Task<AuthorizationPolicyResult> Execute(
            TicketLineParameter ticket,
            Policy policy,
            ClaimTokenParameter claimTokenParameters,
            CancellationToken cancellationToken);
    }
}
