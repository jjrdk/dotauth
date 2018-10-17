namespace SimpleIdentityServer.Client.Operations
{
    using System;
    using System.Threading.Tasks;
    using Core.Common.DTOs.Responses;

    public interface IGetDiscoveryOperation
    {
        Task<DiscoveryInformation> ExecuteAsync(Uri discoveryDocumentationUri);
    }
}