namespace SimpleIdentityServer.Client
{
    using System;
    using System.Threading.Tasks;
    using Shared.Responses;

    public interface IDiscoveryClient
    {
        /// <summary>
        /// Get information about open-id contract asynchronously.
        /// </summary>
        /// <param name="discoveryDocumentationUrl">Absolute URI of the open-id contract</param>
        /// <exception cref="ArgumentNullException">Thrown when parameter is null</exception>
        /// <returns>Open-id contract</returns>
        Task<DiscoveryInformation> GetDiscoveryInformationAsync(Uri discoveryDocumentationUrl);
    }
}