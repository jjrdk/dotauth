namespace SimpleAuth.Helpers
{
    using System.Threading.Tasks;
    using Shared;
    using Shared.Models;

    public interface IClientHelper
    {
        Task<string> GenerateIdTokenAsync(string clientId, JwsPayload jwsPayload);
        Task<string> GenerateIdTokenAsync(Client client, JwsPayload jwsPayload);
    }
}