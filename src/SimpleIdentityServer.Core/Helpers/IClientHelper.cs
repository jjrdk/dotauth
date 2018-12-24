namespace SimpleIdentityServer.Core.Helpers
{
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;

    public interface IClientHelper
    {
        Task<string> GenerateIdTokenAsync(string clientId, JwsPayload jwsPayload);
        Task<string> GenerateIdTokenAsync(Client client, JwsPayload jwsPayload);
    }
}