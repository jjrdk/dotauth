namespace SimpleIdentityServer.Core.Api.Discovery
{
    using System.Threading.Tasks;
    using Shared.Responses;

    public interface IDiscoveryActions
    {
        Task<DiscoveryInformation> CreateDiscoveryInformation(string issuer, string scimEndpoint = null);
    }
}