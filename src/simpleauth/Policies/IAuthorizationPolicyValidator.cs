namespace SimpleAuth.Shared.Policies
{
    using System.Threading.Tasks;
    using Models;
    using SimpleAuth.Parameters;

    public interface IAuthorizationPolicyValidator
    {
        Task<AuthorizationPolicyResult> IsAuthorized(
            Ticket validTicket,
            string clientId,
            ClaimTokenParameter claimTokenParameter);
    }
}
