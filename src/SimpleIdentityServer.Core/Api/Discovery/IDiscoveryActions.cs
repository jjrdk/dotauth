namespace SimpleIdentityServer.Core.Api.Discovery
{
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Responses;

    public interface IDiscoveryActions
    {
        Task<DiscoveryInformation> CreateDiscoveryInformation(string issuer, string scimEndpoint = null);
    }
}