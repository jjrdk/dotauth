namespace SimpleIdentityServer.Core.Helpers
{
    using System.Threading.Tasks;
    using Common.Models;
    using Parameters;

    public interface IConsentHelper
    {
        Task<Consent> GetConfirmedConsentsAsync(string subject, AuthorizationParameter authorizationParameter);
    }
}