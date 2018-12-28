namespace SimpleIdentityServer.Client.Operations
{
    using System;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Responses;

    public interface IGetDiscoveryOperation
    {
        Task<DiscoveryInformation> ExecuteAsync(Uri discoveryDocumentationUri);
    }
}