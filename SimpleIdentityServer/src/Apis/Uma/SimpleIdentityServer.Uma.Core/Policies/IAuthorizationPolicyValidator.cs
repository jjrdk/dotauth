namespace SimpleIdentityServer.Uma.Core.Policies
{
    using System.Threading.Tasks;
    using Models;
    using Parameters;

    public interface IAuthorizationPolicyValidator
    {
        Task<AuthorizationPolicyResult> IsAuthorized(Ticket validTicket, string clientId, ClaimTokenParameter claimTokenParameter);
    }
}