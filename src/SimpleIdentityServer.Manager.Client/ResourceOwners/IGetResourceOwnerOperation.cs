namespace SimpleIdentityServer.Manager.Client.ResourceOwners
{
    using System;
    using System.Threading.Tasks;
    using Results;

    public interface IGetResourceOwnerOperation
    {
        Task<GetResourceOwnerResult> ExecuteAsync(Uri resourceOwnerUri, string authorizationHeaderValue = null);
    }
}