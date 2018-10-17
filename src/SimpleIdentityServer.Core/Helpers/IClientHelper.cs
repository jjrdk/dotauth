namespace SimpleIdentityServer.Core.Helpers
{
    using System.Threading.Tasks;
    using Common;

    public interface IClientHelper
    {
        Task<string> GenerateIdTokenAsync(string clientId, JwsPayload jwsPayload);
        Task<string> GenerateIdTokenAsync(Common.Models.Client client, JwsPayload jwsPayload);
    }
}