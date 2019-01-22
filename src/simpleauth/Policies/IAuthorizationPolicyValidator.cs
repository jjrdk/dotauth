namespace SimpleAuth.Policies
{
    using System.Threading.Tasks;
    using Parameters;
    using Shared.Models;

    public interface IAuthorizationPolicyValidator
    {
        Task<AuthorizationPolicyResult> IsAuthorized(
            Ticket validTicket,
            string clientId,
            ClaimTokenParameter claimTokenParameter);
    }
}
