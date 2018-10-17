namespace SimpleIdentityServer.Core.WebSite.User
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Common.Models;
    using Parameters;

    public interface IUserActions
    {
        Task<IEnumerable<Common.Models.Consent>> GetConsents(ClaimsPrincipal claimsPrincipal);
        Task<bool> DeleteConsent(string consentId);
        Task<ResourceOwner> GetUser(ClaimsPrincipal claimsPrincipal);
        Task<bool> UpdateCredentials(string subject, string newPassword);
        Task<bool> UpdateClaims(string subject, IEnumerable<ClaimAggregate> claims);
        Task<bool> UpdateTwoFactor(string subject, string twoFactorAuth);
        Task<bool> AddUser(AddUserParameter addUserParameter, AuthenticationParameter authenticationParameter, string scimBaseUrl = null, bool addScimResource = false, string issuer = null);
    }
}