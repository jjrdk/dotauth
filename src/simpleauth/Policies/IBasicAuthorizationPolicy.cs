namespace SimpleAuth.Policies
{
    using System.Threading.Tasks;
    using Parameters;
    using Shared.Models;

    public interface IBasicAuthorizationPolicy
    {
        Task<AuthorizationPolicyResult> Execute(
            TicketLineParameter ticket,
            Policy policy,
            ClaimTokenParameter claimTokenParameters);
    }
}
