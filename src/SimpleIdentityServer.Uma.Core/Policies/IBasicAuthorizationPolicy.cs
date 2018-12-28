namespace SimpleAuth.Uma.Policies
{
    using System.Threading.Tasks;
    using Models;
    using Parameters;

    public interface IBasicAuthorizationPolicy
    {
        Task<AuthorizationPolicyResult> Execute(TicketLineParameter ticket,
            Policy policy,
            ClaimTokenParameter claimTokenParameters);
    }
}