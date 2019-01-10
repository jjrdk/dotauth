namespace SimpleAuth.Shared.Policies
{
    using System.Threading.Tasks;
    using Models;
    using SimpleAuth.Parameters;

    public interface IBasicAuthorizationPolicy
    {
        Task<AuthorizationPolicyResult> Execute(
            TicketLineParameter ticket,
            Policy policy,
            ClaimTokenParameter claimTokenParameters);
    }
}
