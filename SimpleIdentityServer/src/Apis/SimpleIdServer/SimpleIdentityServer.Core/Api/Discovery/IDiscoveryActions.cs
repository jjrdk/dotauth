namespace SimpleIdentityServer.Core.Api.Discovery
{
    using System.Threading.Tasks;
    using Common.DTOs.Responses;

    public interface IDiscoveryActions
    {
        Task<DiscoveryInformation> CreateDiscoveryInformation(string issuer, string scimEndpoint = null);
    }
}